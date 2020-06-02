using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Apowersoft.Utils.Imaging {
    /// <summary>
    /// 边框
    /// </summary>
    public class BorderEffect : IEffect {
        public BorderEffect() {
            Width = 2;
            Color = Color.Black;
        }

        public bool CanApplyPartly { get { return false; } }

        public Color Color { get; set; }
        public int Width { get; set; }

        public Image Apply(Image sourceImage, out Point offsetChange) {
            Image ret = CreateBorder(sourceImage, Width, Color, sourceImage.PixelFormat, out offsetChange);
            offsetChange = Point.Empty;
            return ret;
        }

        private Image CreateBorder(Image sourceImage, int borderSize, Color borderColor, PixelFormat targetPixelformat, out Point offset) {
            offset = new Point(borderSize, borderSize);

            Bitmap newImage = ImageOperator.CreateEmptyBitmap(sourceImage.Width + (borderSize * 2), sourceImage.Height + (borderSize * 2), targetPixelformat, Color.Empty, sourceImage.HorizontalResolution, sourceImage.VerticalResolution);
            using (Graphics graphics = Graphics.FromImage(newImage)) {
                // Make sure we draw with the best quality!
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                using (GraphicsPath path = new GraphicsPath()) {
                    path.AddRectangle(new Rectangle(borderSize >> 1, borderSize >> 1, newImage.Width - (borderSize), newImage.Height - (borderSize)));
                    using (Pen pen = new Pen(borderColor, borderSize)) {
                        pen.LineJoin = LineJoin.Round;
                        pen.StartCap = LineCap.Round;
                        pen.EndCap = LineCap.Round;
                        graphics.DrawPath(pen, path);
                    }
                }
                // draw original with a TextureBrush so we have nice antialiasing!
                using (Brush textureBrush = new TextureBrush(sourceImage, WrapMode.Clamp)) {
                    // We need to do a translate-tranform otherwise the image is wrapped
                    graphics.TranslateTransform(offset.X, offset.Y);
                    graphics.FillRectangle(textureBrush, 0, 0, sourceImage.Width, sourceImage.Height);
                }
            }
            return newImage;
        }

    }
}
