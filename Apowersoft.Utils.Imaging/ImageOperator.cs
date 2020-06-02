using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using Apowersoft.Utils.WinAPI;
using Apowersoft.Utils.ScreenRecorder.Imaging.Fill;
using log4net;

namespace Apowersoft.Utils.Imaging {
    public class ImageOperator {
        private static ILog log = LogManager.GetLogger(typeof(ImageOperator));

        public static Image LoadImage(string filename) {
            if (string.IsNullOrEmpty(filename)) {
                return null;
            }
            if (!File.Exists(filename)) {
                return null;
            }
            Image fileImage = null;
            // Fixed lock problem Bug #3431881
            using (Stream imageFileStream = File.OpenRead(filename)) {
                // And fixed problem that the bitmap stream is disposed... by Cloning the image
                // This also ensures the bitmap is correctly created

                if (filename.EndsWith(".ico")) {
                    // Icon logic, try to get the Vista icon, else the biggest possible
                    try {
                        using (Image tmpImage = ExtractVistaIcon(imageFileStream)) {
                            if (tmpImage != null) {
                                fileImage = Clone(tmpImage);
                            }
                        }
                    }
                    catch (Exception vistaIconException) {
                        log.Debug("Failed to extract vista icon, " + vistaIconException.Message);
                    }
                    if (fileImage == null) {
                        try {
                            // No vista icon, try normal icon
                            imageFileStream.Position = 0;
                            // We create a copy of the bitmap, so everything else can be disposed
                            using (Icon tmpIcon = new Icon(imageFileStream, new Size(1024, 1024))) {
                                using (Image tmpImage = tmpIcon.ToBitmap()) {
                                    fileImage = Clone(tmpImage);
                                }
                            }
                        }
                        catch (Exception iconException) {
                            log.Debug("Failed to create new icon, " + iconException.Message);
                        }
                    }
                }
                if (fileImage == null) {
                    // We create a copy of the bitmap, so everything else can be disposed
                    imageFileStream.Position = 0;
                    using (Image tmpImage = Image.FromStream(imageFileStream, true, true)) {
                        fileImage = Clone(tmpImage);
                    }
                }
            }

            bool needExifOrientate = true;
            if (fileImage != null && needExifOrientate) {
                EixfProcessor.Orientate(fileImage);
            }
            return fileImage;
        }

        // Extract a bitmap with the Vista Icon (256x256) from a (icon) file stream
        private static Bitmap ExtractVistaIcon(Stream iconStream) {
            const int SizeICONDIR = 6;
            const int SizeICONDIRENTRY = 16;
            Bitmap bmpPngExtracted = null;
            try {
                byte[] srcBuf = new byte[iconStream.Length];
                iconStream.Read(srcBuf, 0, (int)iconStream.Length);
                int iCount = BitConverter.ToInt16(srcBuf, 4);
                for (int iIndex = 0; iIndex < iCount; iIndex++) {
                    int iWidth = srcBuf[SizeICONDIR + SizeICONDIRENTRY * iIndex];
                    int iHeight = srcBuf[SizeICONDIR + SizeICONDIRENTRY * iIndex + 1];
                    int iBitCount = BitConverter.ToInt16(srcBuf, SizeICONDIR + SizeICONDIRENTRY * iIndex + 6);
                    if (iWidth == 0 && iHeight == 0) {
                        int iImageSize = BitConverter.ToInt32(srcBuf, SizeICONDIR + SizeICONDIRENTRY * iIndex + 8);
                        int iImageOffset = BitConverter.ToInt32(srcBuf, SizeICONDIR + SizeICONDIRENTRY * iIndex + 12);
                        using (MemoryStream destStream = new MemoryStream()) {
                            destStream.Write(srcBuf, iImageOffset, iImageSize);
                            destStream.Seek(0, System.IO.SeekOrigin.Begin);
                            bmpPngExtracted = new Bitmap(destStream);
                        }
                        break;
                    }
                }
            }
            catch {
                return null;
            }
            return bmpPngExtracted;
        }

        /// <summary>
        /// 支持的图片格式
        /// </summary>
        /// <param name="pixelformat"></param>
        /// <returns>支持32bppArgb，32bppPArgb，32bppRgb和24bppRgb</returns>
        public static bool SupportsPixelFormat(PixelFormat pixelformat) {
            return (pixelformat.Equals(PixelFormat.Format32bppArgb) ||
                    pixelformat.Equals(PixelFormat.Format32bppPArgb) ||
                    pixelformat.Equals(PixelFormat.Format32bppRgb) ||
                    pixelformat.Equals(PixelFormat.Format24bppRgb));
        }

        #region copy_crop

        /// <summary>
        /// Image clone
        /// </summary>
        /// <param name="sourceImage"></param>
        /// <returns></returns>
        public static Image Clone(Image sourceImage) {
            if (sourceImage == null) {
                return null;
            }

            if (sourceImage is Metafile) {
                return (Image)sourceImage.Clone();
            }
            return CloneArea(sourceImage, Rectangle.Empty, PixelFormat.DontCare);
        }

        /// <summary>
        /// 复制完整图片
        /// </summary>
        /// <param name="sourceBitmap"></param>
        /// <param name="targetFormat"></param>
        /// <returns>成功返回新的Bitmap对象</returns>
        public static Bitmap Clone(Image sourceBitmap, PixelFormat targetFormat) {
            if (sourceBitmap == null) {
                return null;
            }
            return CloneArea(sourceBitmap, Rectangle.Empty, targetFormat);
        }

        /// <summary>
        /// 复制图片上的矩形区域
        /// </summary>
        /// <param name="sourceImage"></param>
        /// <param name="sourceRect"></param>
        /// <returns>
        /// sourceImage为null，且sourceRect的宽度或者调试为0时，返回null；
        /// 仅sourceImage为null，或者仅sourceRect的宽度或者调试为0时，返回一个空白的Bitmap对象；
        /// 其它返回相应的Bitmap对象。
        /// </returns>
        public static Bitmap CloneArea(Image sourceImage, Rectangle sourceRect) {
            return CloneArea(sourceImage, sourceRect,
                sourceImage != null ? sourceImage.PixelFormat : PixelFormat.Format32bppArgb);
        }

        /// <summary>
        /// 复制图片上的矩形区域
        /// </summary>
        /// <param name="sourceImage"></param>
        /// <param name="sourceRect"></param>
        /// <param name="targetFormat"></param>
        /// <returns>
        /// sourceImage为null，且sourceRect的宽度或者调试为0时，返回null；
        /// 仅sourceImage为null，或者仅sourceRect的宽度或者调试为0时，返回一个空白的Bitmap对象；
        /// 其它返回相应的Bitmap对象。
        /// </returns>
        public static Bitmap CloneArea(Image sourceImage, Rectangle sourceRect, PixelFormat targetFormat) {
            Rectangle bitmapRect = new Rectangle(0, 0, sourceImage.Width, sourceImage.Height);
            sourceRect.Intersect(bitmapRect);

            Size dstImageSize = GetNewImageSizeForClone(sourceImage, sourceRect);
            if (dstImageSize.Width == 0 || dstImageSize.Height == 0) {
                return null;
            }

            targetFormat = GetNewPixelFormatForClone(sourceImage, targetFormat);
            if (sourceImage == null) {
                return new Bitmap(dstImageSize.Width, dstImageSize.Height, targetFormat);
            }

            Bitmap newImage = new Bitmap(dstImageSize.Width, dstImageSize.Height, targetFormat);
            newImage.SetResolution(sourceImage.HorizontalResolution, sourceImage.VerticalResolution);

            using (Graphics graphics = Graphics.FromImage(newImage)) {
                bool fromTransparentToNon = !Image.IsAlphaPixelFormat(targetFormat)
                    && Image.IsAlphaPixelFormat(sourceImage.PixelFormat);
                if (fromTransparentToNon) {
                    graphics.Clear(Color.White);
                }

                try {
                    if (dstImageSize == sourceImage.Size) {
                        graphics.DrawImageUnscaled(sourceImage, 0, 0);
                    }
                    else {
                        graphics.DrawImage(sourceImage, 0, 0, sourceRect, GraphicsUnit.Pixel);
                    }
                }
                catch (Exception ex) {
                    log.ErrorFormat("Error in CloneArea, {0}.", ex.ToString());
#if DEBUG
                    throw ex;
#endif
                }
            }

            return newImage;
        }
        
        /// <summary>
        /// 复制图片上的区域
        /// </summary>
        /// <param name="sourceImage">源图片</param>
        /// <param name="sourceRegion">源区域</param>
        /// <returns>失败返回null，成功返回新的Bitmap对象</returns>
        public static Bitmap CloneArea(Image sourceImage, Region sourceRegion) {
            if (sourceImage == null || sourceRegion == null) {
                return null;
            }

            using (Graphics graphics = System.Drawing.Graphics.FromImage(sourceImage)) {
                Rectangle validRect = Rectangle.Round(sourceRegion.GetBounds(graphics));
                if (validRect.Width == 0 || validRect.Height == 0) {
                    return null;
                }

                // 对区域进行平移
                Bitmap dstImage = new Bitmap(validRect.Width, validRect.Height, sourceImage.PixelFormat);
                using (Graphics dstGraphics = Graphics.FromImage(dstImage)) {
                    graphics.SmoothingMode = SmoothingMode.HighQuality;

                    // 设置剪辑区域
                    Region validRegion = sourceRegion.Clone();
                    validRegion.Translate(-validRect.X, -validRect.Y);
                    dstGraphics.SetClip(validRegion, CombineMode.Replace);

                    // 绘图
                    Rectangle dstRect = new Rectangle(Point.Empty, validRect.Size);
                    dstGraphics.DrawImage(sourceImage, dstRect, validRect, GraphicsUnit.Pixel);
                }
                return dstImage;
            }
        }

        // 裁剪原图片到指定的矩形
        public static bool Crop(ref Image image, ref Rectangle cropRectangle) {
            Image returnImage = null;
            if (image != null && (image is Bitmap) && image.Width > 0 && image.Height > 0) {
                cropRectangle.Intersect(new Rectangle(0, 0, image.Width, image.Height));
                if (cropRectangle.Width != 0 || cropRectangle.Height != 0) {
                    returnImage = CloneArea(image, cropRectangle);
                    if (returnImage != null) {
                        image = returnImage;
                        return true;
                    }
                }
            }
            return false;
        }

        // 复制图片时，新图片的大小的判定
        private static Size GetNewImageSizeForClone(Image sourceImage, Rectangle sourceRect) {
            if (sourceRect.Width != 0 && sourceRect.Height != 0) {
                return sourceRect.Size;
            }
            if (sourceImage != null) {
                if (sourceImage.Width != 0 && sourceImage.Height != 0) {
                    return sourceImage.Size;
                }
            }
            return Size.Empty;
        }

        // 复制图片时，新图片的像素格式的判定
        private static PixelFormat GetNewPixelFormatForClone(Image sourceImage, PixelFormat targetFormat) {
            if (SupportsPixelFormat(targetFormat)) {
                return targetFormat;
            }
            if (PixelFormat.DontCare != targetFormat && PixelFormat.Undefined != targetFormat) {
                return PixelFormat.Format32bppArgb;
            }
            if (sourceImage == null) {
                return PixelFormat.Format32bppArgb;
            }
            if (SupportsPixelFormat(sourceImage.PixelFormat)) {
                return sourceImage.PixelFormat;
            }
            return Image.IsAlphaPixelFormat(sourceImage.PixelFormat)
                ? PixelFormat.Format32bppArgb : PixelFormat.Format24bppRgb;
        }

        #endregion

        #region effects

        // 应用特效
        public static Image ApplyEffect(Image sourceImage, IEffect effect) {
            Point offsetNotCare;
            List<IEffect> effects = new List<IEffect>();
            effects.Add(effect);
            return ApplyEffects(sourceImage, effects, out offsetNotCare);
        }
        
        // 应用特效
        public static Image ApplyEffect(Image sourceImage, IEffect effect, out Point offset) {
            // Default out value for the offset, will be modified there where needed
            offset = new Point(0, 0);
            if (sourceImage == null) {
                return null;
            }

            List<IEffect> effects = new List<IEffect>();
            effects.Add(effect);
            return ApplyEffects(sourceImage, effects, out offset);
        }

        // 应用特效
        public static Image ApplyEffects(Image sourceImage, List<IEffect> effects, out Point offset) {
            // Default out value for the offset, will be modified there where needed
            offset = new Point(0, 0);
            if (sourceImage == null) {
                return null;
            }

            Image currentImage = sourceImage;
            bool disposeImage = false;
            Point tmpPoint;
            foreach (IEffect effect in effects) {
                Image tmpImage = effect.Apply(currentImage, out tmpPoint);
                if (tmpImage != null) {
                    offset.Offset(tmpPoint);
                    if (disposeImage) {
                        currentImage.Dispose();
                    }
                    currentImage = tmpImage;
                    tmpImage = null;
                    // Make sure the "new" image is disposed
                    disposeImage = true;
                }
            }
            return currentImage;
        }

        #endregion

        #region create_new

        public static Bitmap CreateEmptyFromExist(Image sourceImage, Color backgroundColor) {
            PixelFormat pixelFormat = sourceImage.PixelFormat;
            if (backgroundColor.A < 255) {
                pixelFormat = PixelFormat.Format32bppArgb;
            }
            return CreateEmptyBitmap(sourceImage.Width, sourceImage.Height, pixelFormat, backgroundColor, sourceImage.HorizontalResolution, sourceImage.VerticalResolution);
        }

        public static Bitmap CreateEmptyBitmap(int width, int height, PixelFormat format,
            Color backgroundColor, float horizontalResolution, float verticalResolution) {
            // Create a new "clean" image
            Bitmap newImage = new Bitmap(width, height, format);
            newImage.SetResolution(horizontalResolution, verticalResolution);
            if (format != PixelFormat.Format8bppIndexed) {
                using (Graphics graphics = Graphics.FromImage(newImage)) {
                    // 空色和透明都是透明的意思
                    if (Color.Empty.Equals(backgroundColor) || Color.Transparent.Equals(backgroundColor))
                        if (Image.IsAlphaPixelFormat(format)) {
                            graphics.Clear(Color.Transparent);
                        }
                        else {
                            graphics.Clear(Color.White);

                        }
                    else {
                        graphics.Clear(backgroundColor);
                    }
                }
            }
            return newImage;
        }
        
        #endregion

        #region modify_exist

        // 旋转
        public static Image RotateFlip(Image sourceImage, RotateFlipType rotateFlipType) {
            Image returnImage = Clone(sourceImage);
            returnImage.RotateFlip(rotateFlipType);
            return returnImage;
        }

        public static Image ResizeImage(Image sourceImage, bool maintainAspectRatio,
            bool canvasUseNewSize, Color backgroundColor, int newWidth, int newHeight, out Point offset) {
            int destX = 0;
            int destY = 0;

            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)newWidth / (float)sourceImage.Width);
            nPercentH = ((float)newHeight / (float)sourceImage.Height);
            if (maintainAspectRatio) {
                if (nPercentW == 1) {
                    nPercentW = nPercentH;
                    if (canvasUseNewSize) {
                        destX = Math.Max(0, System.Convert.ToInt32((newWidth - (sourceImage.Width * nPercentW)) / 2));
                    }
                }
                else if (nPercentH == 1) {
                    nPercentH = nPercentW;
                    if (canvasUseNewSize) {
                        destY = Math.Max(0, System.Convert.ToInt32((newHeight - (sourceImage.Height * nPercentH)) / 2));
                    }
                }
                else if (nPercentH != 0 && nPercentH < nPercentW) {
                    nPercentW = nPercentH;
                    if (canvasUseNewSize) {
                        destX = Math.Max(0, System.Convert.ToInt32((newWidth - (sourceImage.Width * nPercentW)) / 2));
                    }
                }
                else {
                    nPercentH = nPercentW;
                    if (canvasUseNewSize) {
                        destY = Math.Max(0, System.Convert.ToInt32((newHeight - (sourceImage.Height * nPercentH)) / 2));
                    }
                }
            }

            offset = new Point(destX, destY);

            int destWidth = (int)(sourceImage.Width * nPercentW);
            int destHeight = (int)(sourceImage.Height * nPercentH);
            if (newWidth == 0) {
                newWidth = destWidth;
            }
            if (newHeight == 0) {
                newHeight = destHeight;
            }
            Image newImage = null;
            if (maintainAspectRatio && canvasUseNewSize) {
                newImage = CreateEmptyBitmap(newWidth, newHeight, sourceImage.PixelFormat, backgroundColor, sourceImage.HorizontalResolution, sourceImage.VerticalResolution);
            }
            else {
                newImage = CreateEmptyBitmap(destWidth, destHeight, sourceImage.PixelFormat, backgroundColor, sourceImage.HorizontalResolution, sourceImage.VerticalResolution);
            }

            using (Graphics graphics = Graphics.FromImage(newImage)) {
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(sourceImage, new Rectangle(destX, destY, destWidth, destHeight), new Rectangle(0, 0, sourceImage.Width, sourceImage.Height), GraphicsUnit.Pixel);
            }
            return newImage;
        }

        public static Image ResizeCanvas(Image sourceImage, Color backgroundColor, int left, int right, int top, int bottom) {
            Bitmap newBitmap = CreateEmptyBitmap(sourceImage.Width + left + right, sourceImage.Height + top + bottom, sourceImage.PixelFormat, backgroundColor, sourceImage.HorizontalResolution, sourceImage.VerticalResolution);
            using (Graphics graphics = Graphics.FromImage(newBitmap)) {
                graphics.DrawImageUnscaled(sourceImage, left, top);
            }
            return newBitmap;
        }

        // 转换为灰度暗图像：转为灰度，并将颜色变暗
        public static Bitmap CreateGrayscaleDark(Bitmap originalBitmap) {
            Bitmap newBitmap = new Bitmap(originalBitmap.Width, originalBitmap.Height);
            using (Graphics g = Graphics.FromImage(newBitmap)) {
                //create the grayscale ColorMatrix
                ColorMatrix colorMatrix = new ColorMatrix(
                    new float[][] 
                    {
                       new float[] {.3f, .3f, .3f, 0, 0},
                       new float[] {.59f, .59f, .59f, 0, 0},
                       new float[] {.11f, .11f, .11f, 0, 0},
                       new float[] {0, 0, 0, 1, 0},
                       new float[] {-0.30f, -0.30f, -0.30f, 0, 1}
                   });

                //create some image attributes
                ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(colorMatrix);
                Rectangle rect = new Rectangle(0, 0, originalBitmap.Width, originalBitmap.Height);
                ApplyImageAttributes(originalBitmap, rect, newBitmap, rect, attributes);
            }
            return newBitmap;
        }

        public static Bitmap FillColor(Bitmap bmp, Point location, Color fillColor) {
            if (bmp == null || fillColor == null) {
                log.Error("Failed to fill color, image or color is null.");
                return null;
            }

            try {
                FloodFiller filler = new FloodFiller();
                filler.FillColor = fillColor;
                return filler.FloodFill(bmp, location);
            }
            catch (Exception ex) {
                log.ErrorFormat("Failed to fill color, image size {0}, location {1}, color {2}, error info {3}.",
                   bmp.Size.ToString(), location.ToString(), fillColor.ToString(), ex.Message);
                return null;
            }
        }

        #endregion

        #region apply_matrix__attributes

        public static void ApplyColorMatrix(Bitmap source, ColorMatrix colorMatrix) {
            ApplyColorMatrix(source, Rectangle.Empty, source, Rectangle.Empty, colorMatrix);
        }

        public static void ApplyColorMatrix(Bitmap source, Rectangle sourceRect, Bitmap dest, Rectangle destRect, ColorMatrix colorMatrix) {
            ImageAttributes imageAttributes = new ImageAttributes();
            imageAttributes.ClearColorMatrix();
            imageAttributes.SetColorMatrix(colorMatrix);
            ApplyImageAttributes(source, sourceRect, dest, destRect, imageAttributes);
        }

        public static void ApplyColorMatrix(Bitmap source, ImageAttributes imageAttributes) {
            ApplyImageAttributes(source, Rectangle.Empty, source, Rectangle.Empty, imageAttributes);
        }

        public static void ApplyImageAttributes(Bitmap source, Rectangle sourceRect, Bitmap dest, Rectangle destRect, ImageAttributes imageAttributes) {
            if (sourceRect == Rectangle.Empty) {
                sourceRect = new Rectangle(0, 0, source.Width, source.Height);
            }
            if (dest == null) {
                dest = source;
            }
            if (destRect == Rectangle.Empty) {
                destRect = new Rectangle(0, 0, dest.Width, dest.Height);
            }
            using (Graphics graphics = Graphics.FromImage(dest)) {
                // Make sure we draw with the best quality!
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.CompositingMode = CompositingMode.SourceCopy;

                graphics.DrawImage(source, destRect, sourceRect.X, sourceRect.Y, sourceRect.Width, sourceRect.Height, GraphicsUnit.Pixel, imageAttributes);
            }
        }

        #endregion

        #region auto_corp

        public static Rectangle FindAutoCropRectangle(Image image, int cropDifference) {
            Rectangle cropRectangle = Rectangle.Empty;
            Rectangle currentRectangle = Rectangle.Empty;
            List<Point> checkPoints = new List<Point>();
            // Top Left
            checkPoints.Add(new Point(0, 0));
            // Bottom Left
            checkPoints.Add(new Point(0, image.Height - 1));
            // Top Right
            checkPoints.Add(new Point(image.Width - 1, 0));
            // Bottom Right
            checkPoints.Add(new Point(image.Width - 1, image.Height - 1));
            using (IFastBitmap fastBitmap = FastBitmap.Create((Bitmap)image)) {
                // find biggest area
                foreach (Point checkPoint in checkPoints) {
                    currentRectangle = FastBitmapOperator.FindAutoCropRectangle(fastBitmap, checkPoint, cropDifference);
                    if (currentRectangle.Width * currentRectangle.Height > cropRectangle.Width * cropRectangle.Height) {
                        cropRectangle = currentRectangle;
                    }
                }
            }
            return cropRectangle;
        }

        #endregion
    }
}