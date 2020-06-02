using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Apowersoft.Utils.Imaging {
    /// <summary>
    /// Monochrome, not implemented
    /// </summary>
    public class MonochromeEffect : IEffect {
        private byte threshold;

        public bool CanApplyPartly { get { return true; } }

        public MonochromeEffect(byte threshold) {
            this.threshold = threshold;
        }

        public Image Apply(Image sourceImage, out Point offsetChange) {
            throw new NotImplementedException();
        }
    }
}
