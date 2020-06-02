using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Apowersoft.Utils.Imaging {
    public static class RectangleHelper {

        /// <summary>
        /// Get a rectangle from a string "Left,Top,Width,Height"
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static Rectangle RectangleFromString(string str) {
            if (string.IsNullOrEmpty(str)) {
                return Rectangle.Empty;
            }

            char[] splitter = {','};
            string[] numStrs = str.Split(splitter);
            if (numStrs.Length < 4) {
                return Rectangle.Empty;
            }

            int[] nums = new int[4];
            for (int i = 0; i < 4; ++i) {
                int n = 0;
                if (int.TryParse(numStrs[i], out n)) {
                    nums[i] = n;
                }
                else {
                    return Rectangle.Empty;
                }
            }

            return new Rectangle(nums[0], nums[1], nums[2], nums[3]);
        }

    }
}
