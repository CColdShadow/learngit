using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Apowersoft.Utils.Imaging {
    /// <summary>
    /// 像素化
    /// </summary>
    public class PixelateEffect : IEffect {
        public PixelateEffect(int pixelSize) {
            PixelSize = pixelSize;
        }

        public bool CanApplyPartly { get { return true; } }

        public int PixelSize { get; set; }

        public Image Apply(Image sourceImage, out Point offsetChange) {
            offsetChange = Point.Empty;
            return Pixelate(sourceImage, PixelSize);
        }

        public Image Pixelate(Image sourceImage, int pixelSize) {
            if (sourceImage == null || !(sourceImage is Bitmap)) {
                return sourceImage;
            }

            if (sourceImage.Width < pixelSize) {
                pixelSize = sourceImage.Width;
            }
            if (sourceImage.Height < pixelSize) {
                pixelSize = sourceImage.Height;
            }

            using (IFastBitmap dest = FastBitmap.CreateCloneOf(sourceImage)) {
                using (IFastBitmap src = FastBitmap.Create(sourceImage as Bitmap)) {
                    FastBitmap.MixColors(dest, src, pixelSize);
                }

                Bitmap bmp = new Bitmap(dest.Width, dest.Height, sourceImage.PixelFormat);
                using (Graphics graphics = Graphics.FromImage(bmp)) {
                    dest.DrawTo(graphics, Point.Empty);
                }
                return bmp;
            }
        }
    }
}
