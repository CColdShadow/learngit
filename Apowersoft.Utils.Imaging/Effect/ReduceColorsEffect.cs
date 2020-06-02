using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Apowersoft.Utils.Imaging {
    /// <summary>
    /// ReduceColors, not implemented
    /// </summary>
    class ReduceColorsEffect : IEffect {
        public ReduceColorsEffect()
            : base() {
            Colors = 256;
        }

        public bool CanApplyPartly { get { return true; } }

        public int Colors { get; set; }

        public Image Apply(Image sourceImage, out Point offsetChange) {
            throw new NotImplementedException();
        }
    }
}
