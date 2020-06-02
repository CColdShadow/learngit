using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Apowersoft.Utils.Imaging {
    /// <summary>
    /// 修改图片大小
    /// </summary>
    public class ResizeEffect : IEffect {
        public bool CanApplyPartly { get { return false; } }

        public ResizeEffect(int width, int height, bool maintainAspectRatio) {
            Width = width;
            Height = height;
            MaintainAspectRatio = maintainAspectRatio;
        }

        public int Width { get; set; }
        public int Height { get; set; }
        public bool MaintainAspectRatio { get; set; }

        public Image Apply(Image sourceImage, out Point offsetChange) {
            offsetChange = Point.Empty;
            return ImageOperator.ResizeImage(sourceImage, MaintainAspectRatio, false,
                Color.Empty, Width, Height, out offsetChange);
        }
    }
}
