using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Apowersoft.Utils.Imaging {
    /// <summary>
    /// 模糊
    /// </summary>
    public class BlurEffect : IEffect {
        public BlurEffect(int radius) {
            Radius = radius;
        }

        public bool CanApplyPartly { get { return true; } }

        public int Radius { get; set; }

        public Image Apply(Image sourceImage, out Point offsetChange) {
            offsetChange = Point.Empty;
            return Blur(sourceImage, Radius);
        }

        public Image Blur(Image sourceImage, int radius) {
            if (sourceImage == null) {
                return sourceImage;
            }

            using (IFastBitmap fastBitmap = FastBitmap.CreateCloneOf(sourceImage)) {
                FastBitmapOperator.ApplyBoxBlur(fastBitmap, radius);
                Bitmap bmp = new Bitmap(sourceImage.Width, sourceImage.Height, sourceImage.PixelFormat);
                using (Graphics graphics = Graphics.FromImage(bmp)) {
                    fastBitmap.DrawTo(graphics, Point.Empty);
                }
                return bmp;
            }
        }
    }
}
