using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Diagnostics;
using System.Security.AccessControl;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using log4net;

namespace Apowersoft.Utils.Imaging {

    public static class ImageOutput {

        private static ILog log = LogManager.GetLogger(typeof(ImageOutput)); 

        /// <summary>
        /// Save a surface to a stream
        /// </summary>
        /// <param name="surface"></param>
        /// <param name="stream"></param>
        /// <param name="outputSettings"></param>
        /// <returns></returns>
        public static bool SaveToStream(ISurface surface, Stream stream,
            OutputSettings outputSettings) {
            Image imageToSave = null;
            bool disposeImage = CreateImageFromSurface(surface, outputSettings, out imageToSave);
            bool ret = SaveToStream(imageToSave, stream, outputSettings);
            if (disposeImage && imageToSave != null) {
                // cleanup if needed
                imageToSave.Dispose();
            }
            return ret;
        }

        /// <summary>
        /// Save the region of a surface to a stream
        /// </summary>
        /// <param name="surface"></param>
        /// <param name="region"></param>
        /// <param name="stream"></param>
        /// <param name="outputSettings"></param>
        /// <returns></returns>
        public static bool SaveToStream(ISurface surface, Region region,
            Stream stream, OutputSettings outputSettings) {
            Image imageToSave = null;
            bool disposeImage = CreateImageFromSurface(surface, outputSettings, out imageToSave);
            bool ret = SaveToStream(ImageOperator.CloneArea(imageToSave, region), stream, outputSettings);
            if (disposeImage && imageToSave != null) {
                // cleanup if needed
                imageToSave.Dispose();
            }
            return ret;
        }

        /// <summary>
        /// Save an image to a stream
        /// </summary>
        /// <param name="imageToSave"></param>
        /// <param name="stream"></param>
        /// <param name="outputSettings"></param>
        /// <returns></returns>
        public static bool SaveToStream(Image imageToSave, Stream stream, OutputSettings outputSettings) {
            if (imageToSave == null || stream == null) {
                return false;
            }

            if (outputSettings.Format == OutputFormat.pdf) {
                return SaveImageToPdf(imageToSave, stream);
            }

            return SaveNormalImageFormat(imageToSave, stream, outputSettings);
        }
        
        public static bool Save(ISurface surface, string fullPath, bool allowOverwrite,
            OutputSettings outputSettings) {
            try {
                string path = Path.GetDirectoryName(fullPath);
                // check whether path exists - if not create it
                DirectoryInfo di = new DirectoryInfo(path);
                if (!di.Exists) {
                    Directory.CreateDirectory(di.FullName);
                }

                if (!allowOverwrite && File.Exists(fullPath)) {
                    log.Error("File '" + fullPath + "' already exists.");
                    return false;
                }
            }
            catch (Exception ex) {
                log.ErrorFormat("Failed to get directory, {0}, {1}.", fullPath, ex.Message);
                return false;
            }

            // Create the stream and call SaveToStream
            try {
                using (FileStream stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write)) {
                    return SaveToStream(surface, stream, outputSettings);
                }
            }
            catch (Exception ex) {
                log.ErrorFormat("Failed to save to stream, {0}, {1}.", fullPath, ex.Message);
                return false;
            }
        }             
        
        private static void GetImageFormat(Stream stream, OutputFormat outputFormat,
            ref ImageFormat imageFormat, ref bool useMemoryStream) {
            switch (outputFormat) {
                case OutputFormat.bmp:
                    imageFormat = ImageFormat.Bmp;
                    break;
                case OutputFormat.gif:
                    imageFormat = ImageFormat.Gif;
                    break;
                case OutputFormat.jpg:
                    imageFormat = ImageFormat.Jpeg;
                    break;
                case OutputFormat.tiff:
                    imageFormat = ImageFormat.Tiff;
                    break;
                case OutputFormat.png:
                default:
                    if (!stream.CanSeek) {
                        int majorVersion = Environment.OSVersion.Version.Major;
                        int minorVersion = Environment.OSVersion.Version.Minor;
                        if (majorVersion < 6 || (majorVersion == 6 && minorVersion == 0)) {
                            useMemoryStream = true;
                        }
                    }
                    imageFormat = ImageFormat.Png;
                    break;
            }
        }

        private static bool SaveImageToPdf(Image imageToSave, Stream stream) {
            try {
                using (PdfDocument doc = new PdfDocument()) {
                    PdfPage page = new PdfPage();
                    doc.Pages.Add(page);
                    XGraphics xgr = XGraphics.FromPdfPage(page);
                    XImage img = XImage.FromGdiPlusImage(imageToSave);
                    if (img.PointWidth / img.PointHeight >= (double)page.Width / (double)page.Width) {
                        xgr.DrawImage(img, 0, 0, page.Width, img.PixelHeight * page.Width / img.PixelWidth);
                    }
                    else {
                        xgr.DrawImage(img, 0, 0, img.PixelWidth * page.Height / img.PixelHeight, page.Height);
                    }
                    doc.Save(stream, true);
                    doc.Close();
                }
                return true;
            }
            catch (Exception ex) {
                log.Error("Failed to save image to pdf, " + ex.Message);
            }
            return false;
        }

        private static bool SaveNormalImageFormat(Image imageToSave, Stream stream, OutputSettings outputSettings) {
            bool ret = false;

            ImageFormat imageFormat = ImageFormat.Bmp;
            bool useMemoryStream = false;
            GetImageFormat(stream, outputSettings.Format, ref imageFormat, ref useMemoryStream);
            MemoryStream memoryStream = null;
            try {
                // Check if we want to use a memory stream, to prevent a issue which happens with Windows before "7".
                // The save is made to the targetStream, this is directed to either the MemoryStream or the original
                Stream targetStream = stream;
                if (useMemoryStream) {
                    memoryStream = new MemoryStream();
                    targetStream = memoryStream;
                }

                if (imageFormat == ImageFormat.Jpeg) {
                    ret = SaveJpeg(imageToSave, imageFormat, outputSettings.JPGQuality, targetStream);
                }
                else {
                    ret = SaveNonJpeg(imageToSave, imageFormat, targetStream);
                }

                // If we used a memory stream, we need to stream the memory stream to the original stream.
                if (useMemoryStream) {
                    memoryStream.WriteTo(stream);
                }
            }
            catch (Exception ex) {
                log.Error("Failed to save, " + ex.Message);
            }
            finally {
                if (memoryStream != null) {
                    memoryStream.Dispose();
                }
            }

            return ret;
        }

        private static bool SaveJpeg(Image imageToSave, ImageFormat imageFormat, int jpegQuality, Stream stream) {
            bool foundEncoder = false;
            foreach (ImageCodecInfo imageCodec in ImageCodecInfo.GetImageEncoders()) {
                if (imageCodec.FormatID != imageFormat.Guid) {
                    continue;
                }

                foundEncoder = true;
                try {
                    EncoderParameters parameters = new EncoderParameters(1);
                    parameters.Param[0] = new EncoderParameter(Encoder.Quality, jpegQuality);
                    // Removing transparency if it's not supported in the output
                    if (Image.IsAlphaPixelFormat(imageToSave.PixelFormat)) {
                        Image nonAlphaImage = ImageOperator.Clone(imageToSave, PixelFormat.Format24bppRgb);
                        nonAlphaImage.Save(stream, imageCodec, parameters);
                        nonAlphaImage.Dispose();
                        nonAlphaImage = null;
                    }
                    else {
                        imageToSave.Save(stream, imageCodec, parameters);
                    }
                    return true;
                }
                catch (Exception ex) {
                    log.Error("Failed to save jpeg, " + ex.Message);
                }
            }

            if (!foundEncoder) {
                log.Error("No JPG encoder found.");
            }
            return false;
        }

        private static bool SaveNonJpeg(Image imageToSave, ImageFormat imageFormat, Stream stream) {
            bool needsDispose = false;
            // Removing transparency if it's not supported in the output
            if (imageFormat != ImageFormat.Png && Image.IsAlphaPixelFormat(imageToSave.PixelFormat)) {
                imageToSave = ImageOperator.Clone(imageToSave, PixelFormat.Format24bppRgb);
                needsDispose = true;
            }

            try {
                imageToSave.Save(stream, imageFormat);
                return true;
            }
            catch (Exception ex) {
                log.Error("Failed to save image, " + ex.Message);
                return false;
            }
            finally {
                if (needsDispose && imageToSave != null) {
                    imageToSave.Dispose();
                    imageToSave = null;
                }
            }
        }
        
        private static bool CreateImageFromSurface(ISurface surface, OutputSettings outputSettings,
            out Image imageToSave) {
            try {
                imageToSave = surface.GetImageForExport();
            }
            catch (Exception ex) {
                imageToSave = null;
                log.Error("Failed to GetImageForExport of surface, " + ex.Message);
            }

            if (imageToSave == null) {
                return false;
            }

            if (outputSettings.Effects != null && outputSettings.Effects.Count > 0) {
                // apply effects, if there are any
                Image tmpImage;
                Point ignoreOffset;
                try {
                    tmpImage = ImageOperator.ApplyEffects((Bitmap)imageToSave,
                        outputSettings.Effects, out ignoreOffset);
                }
                catch (Exception ex) {
                    tmpImage = null;
                    log.Error("Failed to apply output effect, " + ex.Message);
                }

                if (tmpImage != null) {
                    imageToSave.Dispose();
                    imageToSave = tmpImage;
                }
            }

            return true;
        }

    }
}