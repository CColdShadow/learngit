using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Apowersoft.Utils.Imaging {
    /// <summary>
    /// Adjust, not implemented
    /// </summary>
    class AdjustEffect : IEffect {
        public AdjustEffect()
            : base() {
            Contrast = 1f;
            Brightness = 1f;
            Gamma = 1f;
        }

        public bool CanApplyPartly { get { return true; } }

        public float Contrast { get; set; }
        public float Brightness { get; set; }
        public float Gamma { get; set; }

        public Image Apply(Image sourceImage, out Point offsetChange) {
            throw new NotImplementedException();
        }
    }
}
