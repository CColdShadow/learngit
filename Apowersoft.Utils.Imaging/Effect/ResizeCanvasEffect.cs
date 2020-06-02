using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Apowersoft.Utils.Imaging {
    /// <summary>
    /// 修改画布大小
    /// </summary>
    public class ResizeCanvasEffect : IEffect {
        public ResizeCanvasEffect(int left, int right, int top, int bottom) {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
            BackgroundColor = Color.Empty;	// Uses the default background color depending on the format
        }

        public bool CanApplyPartly { get { return false; } }

        public int Left { get; set; }
        public int Right { get; set; }
        public int Top { get; set; }
        public int Bottom { get; set; }
        public Color BackgroundColor { get; set; }

        public Image Apply(Image sourceImage, out Point offsetChange) {
            // Make sure the elements move according to the offset the effect made the bitmap move
            offsetChange = new Point(Left, Top);
            return ImageOperator.ResizeCanvas(sourceImage, BackgroundColor, Left, Right, Top, Bottom);
        }
    }
}
