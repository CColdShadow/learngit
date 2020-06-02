using System;
using System.Collections.Generic;
using System.Drawing;

namespace Apowersoft.Utils.Imaging {
    public static class Colors {
        public static bool IsVisible(Color c) {
            return c != null && !c.Equals(Color.Empty) && !c.Equals(Color.Transparent) && c.A > 0;
        }

        public static Color Mix(List<Color> colors) {
            int a = 0;
            int r = 0;
            int g = 0;
            int b = 0;
            int count = 0;
            foreach (Color color in colors) {
                if (!color.Equals(Color.Empty)) {
                    a += color.A;
                    r += color.R;
                    g += color.G;
                    b += color.B;
                    count++;
                }
            }
            if (count == 0) {
                return Color.Empty;
            }
            return Color.FromArgb(a / count, r / count, g / count, b / count);
        }
    }
}