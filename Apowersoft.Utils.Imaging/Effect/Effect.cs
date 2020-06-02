using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;

namespace Apowersoft.Utils.Imaging {
    public interface IEffect {
        /// <summary>
        /// 是否可以应用到部分图片
        /// </summary> 
        bool CanApplyPartly { get; }
        /// <summary>
        ///  应用
        /// </summary>
        /// <param name="sourceImage"></param>
        /// <param name="offsetChange"></param>
        /// <returns></returns>
        Image Apply(Image sourceImage, out Point offsetChange);
    }
}