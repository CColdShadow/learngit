using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Apowersoft.Utils.Imaging {
    /// <summary>
    /// 旋转
    /// </summary>
    public class RotateEffect : IEffect {
        public RotateEffect(int angle) {
            Angle = angle;
        }

        public bool CanApplyPartly { get { return true; } }

        public int Angle { get; set; }

        public Image Apply(Image sourceImage, out Point offsetChange) {
            offsetChange = Point.Empty;
            RotateFlipType flipType;
            if (Angle == 90) {
                flipType = RotateFlipType.Rotate90FlipNone;
            }
            else if (Angle == -90 || Angle == 270) {
                flipType = RotateFlipType.Rotate270FlipNone;
            }
            else {
                throw new NotSupportedException("Currently only an angle of 90 or -90 (270) is supported.");
            }
            return ImageOperator.RotateFlip(sourceImage, flipType);
        }
    }
}
