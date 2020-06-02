using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Apowersoft.Utils.WinAPI;
using log4net;

namespace Apowersoft.Utils.Imaging {
    /// <summary>
    /// 阴影
    /// </summary>
    public class DropShadowEffect : IEffect {
        private ILog log = LogManager.GetLogger(typeof(DropShadowEffect));

        public DropShadowEffect() {
            Darkness = 0.6f;
            ShadowSize = 7;
            ShadowOffset = new Point(-1, -1);
        }

        public bool CanApplyPartly { get { return false; } }
        public float Darkness { get; set; }
        public int ShadowSize { get; set; }
        public Point ShadowOffset { get; set; }

        public virtual Image Apply(Image sourceImage, out Point offsetChange) {
            return CreateShadow(sourceImage, Darkness, ShadowSize, ShadowOffset, out offsetChange, PixelFormat.Format32bppArgb);
        }

        // 由指定bitmap创建一个新有带阴影的bitmap
        protected Bitmap CreateShadow(Image sourceBitmap, float darkness, int shadowSize, Point shadowOffset,
            out Point offset, PixelFormat targetPixelformat) {
            // Create a new "clean" image
            offset = shadowOffset;
            offset.X += shadowSize - 1;
            offset.Y += shadowSize - 1;
            Bitmap returnImage = ImageOperator.CreateEmptyBitmap(sourceBitmap.Width + (shadowSize * 2), sourceBitmap.Height + (shadowSize * 2),
                targetPixelformat, Color.Empty, sourceBitmap.HorizontalResolution, sourceBitmap.VerticalResolution);
            // Make sure the shadow is odd, there is no reason for an even blur!
            if ((shadowSize & 1) == 0) {
                shadowSize++;
            }
            bool useGDIBlur = GDIplus.IsBlurPossible(shadowSize);
            // Create "mask" for the shadow
            ColorMatrix maskMatrix = new ColorMatrix();
            maskMatrix.Matrix00 = 0;
            maskMatrix.Matrix11 = 0;
            maskMatrix.Matrix22 = 0;
            if (useGDIBlur) {
                maskMatrix.Matrix33 = darkness + 0.1f;
            }
            else {
                maskMatrix.Matrix33 = darkness;
            }
            Rectangle shadowRectangle = new Rectangle(new Point(shadowSize, shadowSize), sourceBitmap.Size);
            ImageOperator.ApplyColorMatrix((Bitmap)sourceBitmap, Rectangle.Empty, returnImage, shadowRectangle, maskMatrix);

            // blur "shadow", apply to whole new image
            if (useGDIBlur) {
                // Use GDI Blur
                Rectangle newImageRectangle = new Rectangle(0, 0, returnImage.Width, returnImage.Height);
                if (!GDIplus.ApplyBlur(returnImage, newImageRectangle, shadowSize + 1, false)) {
                    log.Error("Failed to ApplyBlur in GDIplus.");
                }
            }
            else {
                // try normal software blur
                ApplyBoxBlur(returnImage, shadowSize);
            }

            // Draw the original image over the shadow
            using (Graphics graphics = Graphics.FromImage(returnImage)) {
                // Make sure we draw with the best quality!
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                // draw original with a TextureBrush so we have nice antialiasing!
                using (Brush textureBrush = new TextureBrush(sourceBitmap, WrapMode.Clamp)) {
                    // We need to do a translate-tranform otherwise the image is wrapped
                    graphics.TranslateTransform(offset.X, offset.Y);
                    graphics.FillRectangle(textureBrush, 0, 0, sourceBitmap.Width, sourceBitmap.Height);
                }
            }
            return returnImage;
        }

        private void ApplyBoxBlur(Bitmap destinationBitmap, int range) {
            // We only need one fastbitmap as we use it as source and target (the reading is done for one line H/V, writing after "parsing" one line H/V)
            using (IFastBitmap fastBitmap = FastBitmap.Create(destinationBitmap)) {
                FastBitmapOperator.ApplyBoxBlur(fastBitmap, range);
            }
        }
    }
}
