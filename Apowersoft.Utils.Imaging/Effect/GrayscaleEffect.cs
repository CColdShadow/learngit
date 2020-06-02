using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace Apowersoft.Utils.Imaging {
    /// <summary>
    /// 灰度
    /// </summary>
    public class GrayscaleEffect : IEffect {
        public bool CanApplyPartly { get { return true; } }

        public Image Apply(Image sourceImage, out Point offsetChange) {
            offsetChange = Point.Empty;
            return CreateGrayscale(sourceImage as Bitmap);
        }

        // 转换为灰度图像
        private Bitmap CreateGrayscale(Bitmap originalBitmap) {
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
                       new float[] {0, 0, 0, 0, 1}
                   });

                //create some image attributes
                ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(colorMatrix);
                Rectangle rect = new Rectangle(0, 0, originalBitmap.Width, originalBitmap.Height);
                ImageOperator.ApplyImageAttributes(originalBitmap, rect, newBitmap, rect, attributes);
            }
            return newBitmap;
        }
    }
}
