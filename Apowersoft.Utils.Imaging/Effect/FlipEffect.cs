using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Apowersoft.Utils.Imaging {
    /// <summary>
    /// 翻转
    /// </summary>
    public class FlipEffect : IEffect {

        public enum FlipEffectType {
            HorizontalFlip, VerticalFlip
        };

        public FlipEffect(FlipEffectType type) {
            EffectType = type;
        }

        public bool CanApplyPartly { get { return true; } }

        private FlipEffectType EffectType { get; set; }

        public Image Apply(Image sourceImage, out Point offsetChange) {
            offsetChange = Point.Empty;

            RotateFlipType flipType = EffectType == FlipEffectType.HorizontalFlip
                ? RotateFlipType.RotateNoneFlipX : RotateFlipType.RotateNoneFlipY;

            return ImageOperator.RotateFlip(sourceImage, flipType);
        }
    }
}
