using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace Apowersoft.Utils.Imaging {
    /// <summary>
    /// Invert
    /// </summary>
    public class InvertEffect : IEffect {
        public bool CanApplyPartly { get { return true; } }

        public Image Apply(Image sourceImage, out Point offsetChange) {
            offsetChange = Point.Empty;
            return CreateNegative(sourceImage);
        }

        private Bitmap CreateNegative(Image sourceImage) {
            Bitmap clone = ImageOperator.Clone(sourceImage) as Bitmap;
            ColorMatrix invertMatrix = new ColorMatrix(new float[][] { 
				new float[] {-1, 0, 0, 0, 0}, 
				new float[] {0, -1, 0, 0, 0}, 
				new float[] {0, 0, -1, 0, 0}, 
				new float[] {0, 0, 0, 1, 0}, 
				new float[] {1, 1, 1, 1, 1} 
			});

            ImageOperator.ApplyColorMatrix(clone, invertMatrix);
            return clone;
        }
    }
}
