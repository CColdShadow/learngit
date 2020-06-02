using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Apowersoft.Utils.Imaging {
    /// <summary>
    /// 撕裂边缘
    /// </summary>
    public class TornEdgeEffect : DropShadowEffect {
        public TornEdgeEffect()
            : base() {
            ShadowSize = 7;
            ToothHeight = 12;
            HorizontalToothRange = 20;
            VerticalToothRange = 20;
        }

        public int ToothHeight { get; set; }
        public int HorizontalToothRange { get; set; }
        public int VerticalToothRange { get; set; }

        public override Image Apply(Image sourceImage, out Point offsetChange) {
            using (Image tmpTornImage = CreateTornEdge(sourceImage, ToothHeight, HorizontalToothRange, VerticalToothRange)) {
                return CreateShadow(tmpTornImage, Darkness, ShadowSize, ShadowOffset, out offsetChange, PixelFormat.Format32bppArgb);
            }
        }

        private Image CreateTornEdge(Image sourceImage, int toothHeight, int horizontalToothRange, int verticalToothRange) {
            Image returnImage = ImageOperator.CreateEmptyBitmap(sourceImage.Width, sourceImage.Height, PixelFormat.Format32bppArgb, Color.Empty, sourceImage.HorizontalResolution, sourceImage.VerticalResolution);
            using (GraphicsPath path = new GraphicsPath()) {
                Random random = new Random();
                int HorizontalRegions = (int)(sourceImage.Width / horizontalToothRange);
                int VerticalRegions = (int)(sourceImage.Height / verticalToothRange);

                // Start
                Point previousEndingPoint = new Point(horizontalToothRange, random.Next(1, toothHeight));
                Point newEndingPoint;
                // Top
                for (int i = 0; i < HorizontalRegions; i++) {
                    int x = (int)previousEndingPoint.X + horizontalToothRange;
                    int y = random.Next(1, toothHeight);
                    newEndingPoint = new Point(x, y);
                    path.AddLine(previousEndingPoint, newEndingPoint);
                    previousEndingPoint = newEndingPoint;
                }

                // Right
                for (int i = 0; i < VerticalRegions; i++) {
                    int x = sourceImage.Width - random.Next(1, toothHeight);
                    int y = (int)previousEndingPoint.Y + verticalToothRange;
                    newEndingPoint = new Point(x, y);
                    path.AddLine(previousEndingPoint, newEndingPoint);
                    previousEndingPoint = newEndingPoint;
                }

                // Bottom
                for (int i = 0; i < HorizontalRegions; i++) {
                    int x = (int)previousEndingPoint.X - horizontalToothRange;
                    int y = sourceImage.Height - random.Next(1, toothHeight);
                    newEndingPoint = new Point(x, y);
                    path.AddLine(previousEndingPoint, newEndingPoint);
                    previousEndingPoint = newEndingPoint;
                }

                // Left
                for (int i = 0; i < VerticalRegions; i++) {
                    int x = random.Next(1, toothHeight);
                    int y = (int)previousEndingPoint.Y - verticalToothRange;
                    newEndingPoint = new Point(x, y);
                    path.AddLine(previousEndingPoint, newEndingPoint);
                    previousEndingPoint = newEndingPoint;
                }
                path.CloseFigure();

                // Draw the created figure with the original image by using a TextureBrush so we have anti-aliasing
                using (Graphics graphics = Graphics.FromImage(returnImage)) {
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    using (Brush brush = new TextureBrush(sourceImage)) {
                        // Imporant note: If the target wouldn't be at 0,0 we need to translate-transform!!
                        graphics.FillPath(brush, path);
                    }
                }
            }
            return returnImage;
        }
    }
}
