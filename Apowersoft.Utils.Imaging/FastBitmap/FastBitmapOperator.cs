using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Apowersoft.Utils.Imaging {
    /// <summary>
    /// FastBitmap操作类
    /// </summary>
    public class FastBitmapOperator {
        public static void ApplyBoxBlur(IFastBitmap fastBitmap, int range) {
            // Range must be odd!
            if ((range & 1) == 0) {
                range++;
            }
            if (range <= 1) {
                return;
            }

            // Box blurs are frequently used to approximate a Gaussian blur.
            // By the central limit theorem, if applied 3 times on the same image, a box blur approximates the Gaussian kernel to within about 3%, yielding the same result as a quadratic convolution kernel.
            // This might be true, but the GDI+ BlurEffect doesn't look the same, a 2x blur is more simular and we only make 2x Box-Blur.
            // (Might also be a mistake in our blur, but for now it looks great)
            if (fastBitmap.hasAlphaChannel) {
                BoxBlurHorizontalAlpha(fastBitmap, range);
                BoxBlurVerticalAlpha(fastBitmap, range);
                BoxBlurHorizontalAlpha(fastBitmap, range);
                BoxBlurVerticalAlpha(fastBitmap, range);
            }
            else {
                BoxBlurHorizontal(fastBitmap, range);
                BoxBlurVertical(fastBitmap, range);
                BoxBlurHorizontal(fastBitmap, range);
                BoxBlurVertical(fastBitmap, range);
            }
        }

        private static void BoxBlurHorizontal(IFastBitmap targetFastBitmap, int range) {
            if (targetFastBitmap.hasAlphaChannel) {
                throw new NotSupportedException("BoxBlurHorizontal should NOT be called for bitmaps with alpha channel");
            }
            int halfRange = range / 2;
            Color[] newColors = new Color[targetFastBitmap.Width];
            byte[] tmpColor = new byte[3];
            for (int y = targetFastBitmap.Top; y < targetFastBitmap.Bottom; y++) {
                int hits = 0;
                int r = 0;
                int g = 0;
                int b = 0;
                for (int x = targetFastBitmap.Left - halfRange; x < targetFastBitmap.Right; x++) {
                    int oldPixel = x - halfRange - 1;
                    if (oldPixel >= targetFastBitmap.Left) {
                        targetFastBitmap.GetColorAt(oldPixel, y, tmpColor);
                        r -= tmpColor[FastBitmap.COLOR_INDEX_R];
                        g -= tmpColor[FastBitmap.COLOR_INDEX_G];
                        b -= tmpColor[FastBitmap.COLOR_INDEX_B];
                        hits--;
                    }

                    int newPixel = x + halfRange;
                    if (newPixel < targetFastBitmap.Right) {
                        targetFastBitmap.GetColorAt(newPixel, y, tmpColor);
                        r += tmpColor[FastBitmap.COLOR_INDEX_R];
                        g += tmpColor[FastBitmap.COLOR_INDEX_G];
                        b += tmpColor[FastBitmap.COLOR_INDEX_B];
                        hits++;
                    }

                    if (x >= targetFastBitmap.Left) {
                        newColors[x - targetFastBitmap.Left] = Color.FromArgb(255, (byte)(r / hits), (byte)(g / hits), (byte)(b / hits));
                    }
                }
                for (int x = targetFastBitmap.Left; x < targetFastBitmap.Right; x++) {
                    targetFastBitmap.SetColorAt(x, y, newColors[x - targetFastBitmap.Left]);
                }
            }
        }

        private static void BoxBlurHorizontalAlpha(IFastBitmap targetFastBitmap, int range) {
            if (!targetFastBitmap.hasAlphaChannel) {
                throw new NotSupportedException("BoxBlurHorizontalAlpha should be called for bitmaps with alpha channel");
            }
            int halfRange = range / 2;
            Color[] newColors = new Color[targetFastBitmap.Width];
            byte[] tmpColor = new byte[4];
            for (int y = targetFastBitmap.Top; y < targetFastBitmap.Bottom; y++) {
                int hits = 0;
                int a = 0;
                int r = 0;
                int g = 0;
                int b = 0;
                for (int x = targetFastBitmap.Left - halfRange; x < targetFastBitmap.Right; x++) {
                    int oldPixel = x - halfRange - 1;
                    if (oldPixel >= targetFastBitmap.Left) {
                        targetFastBitmap.GetColorAt(oldPixel, y, tmpColor);
                        a -= tmpColor[FastBitmap.COLOR_INDEX_A];
                        r -= tmpColor[FastBitmap.COLOR_INDEX_R];
                        g -= tmpColor[FastBitmap.COLOR_INDEX_G];
                        b -= tmpColor[FastBitmap.COLOR_INDEX_B];
                        hits--;
                    }

                    int newPixel = x + halfRange;
                    if (newPixel < targetFastBitmap.Right) {
                        targetFastBitmap.GetColorAt(newPixel, y, tmpColor);
                        a += tmpColor[FastBitmap.COLOR_INDEX_A];
                        r += tmpColor[FastBitmap.COLOR_INDEX_R];
                        g += tmpColor[FastBitmap.COLOR_INDEX_G];
                        b += tmpColor[FastBitmap.COLOR_INDEX_B];
                        hits++;
                    }

                    if (x >= targetFastBitmap.Left) {
                        newColors[x - targetFastBitmap.Left] = Color.FromArgb((byte)(a / hits), (byte)(r / hits), (byte)(g / hits), (byte)(b / hits));
                    }
                }
                for (int x = targetFastBitmap.Left; x < targetFastBitmap.Right; x++) {
                    targetFastBitmap.SetColorAt(x, y, newColors[x - targetFastBitmap.Left]);
                }
            }
        }

        private static void BoxBlurVertical(IFastBitmap targetFastBitmap, int range) {
            if (targetFastBitmap.hasAlphaChannel) {
                throw new NotSupportedException("BoxBlurVertical should NOT be called for bitmaps with alpha channel");
            }
            int w = targetFastBitmap.Width;
            int halfRange = range / 2;
            Color[] newColors = new Color[targetFastBitmap.Height];
            int oldPixelOffset = -(halfRange + 1) * w;
            int newPixelOffset = (halfRange) * w;
            byte[] tmpColor = new byte[4];
            for (int x = targetFastBitmap.Left; x < targetFastBitmap.Right; x++) {
                int hits = 0;
                int r = 0;
                int g = 0;
                int b = 0;
                for (int y = targetFastBitmap.Top - halfRange; y < targetFastBitmap.Bottom; y++) {
                    int oldPixel = y - halfRange - 1;
                    if (oldPixel >= targetFastBitmap.Top) {
                        targetFastBitmap.GetColorAt(x, oldPixel, tmpColor);
                        r -= tmpColor[FastBitmap.COLOR_INDEX_R];
                        g -= tmpColor[FastBitmap.COLOR_INDEX_G];
                        b -= tmpColor[FastBitmap.COLOR_INDEX_B];
                        hits--;
                    }

                    int newPixel = y + halfRange;
                    if (newPixel < targetFastBitmap.Bottom) {
                        targetFastBitmap.GetColorAt(x, newPixel, tmpColor);
                        r += tmpColor[FastBitmap.COLOR_INDEX_R];
                        g += tmpColor[FastBitmap.COLOR_INDEX_G];
                        b += tmpColor[FastBitmap.COLOR_INDEX_B];
                        hits++;
                    }

                    if (y >= targetFastBitmap.Top) {
                        newColors[y - targetFastBitmap.Top] = Color.FromArgb(255, (byte)(r / hits), (byte)(g / hits), (byte)(b / hits));
                    }
                }

                for (int y = targetFastBitmap.Top; y < targetFastBitmap.Bottom; y++) {
                    targetFastBitmap.SetColorAt(x, y, newColors[y - targetFastBitmap.Top]);
                }
            }
        }

        private static void BoxBlurVerticalAlpha(IFastBitmap targetFastBitmap, int range) {
            if (!targetFastBitmap.hasAlphaChannel) {
                throw new NotSupportedException("BoxBlurVerticalAlpha should be called for bitmaps with alpha channel");
            }

            int w = targetFastBitmap.Width;
            int halfRange = range / 2;
            Color[] newColors = new Color[targetFastBitmap.Height];
            int oldPixelOffset = -(halfRange + 1) * w;
            int newPixelOffset = (halfRange) * w;
            byte[] tmpColor = new byte[4];
            for (int x = targetFastBitmap.Left; x < targetFastBitmap.Right; x++) {
                int hits = 0;
                int a = 0;
                int r = 0;
                int g = 0;
                int b = 0;
                for (int y = targetFastBitmap.Top - halfRange; y < targetFastBitmap.Bottom; y++) {
                    int oldPixel = y - halfRange - 1;
                    if (oldPixel >= targetFastBitmap.Top) {
                        targetFastBitmap.GetColorAt(x, oldPixel, tmpColor);
                        a -= tmpColor[FastBitmap.COLOR_INDEX_A];
                        r -= tmpColor[FastBitmap.COLOR_INDEX_R];
                        g -= tmpColor[FastBitmap.COLOR_INDEX_G];
                        b -= tmpColor[FastBitmap.COLOR_INDEX_B];
                        hits--;
                    }

                    int newPixel = y + halfRange;
                    if (newPixel < targetFastBitmap.Bottom) {
                        //int colorg = pixels[index + newPixelOffset];
                        targetFastBitmap.GetColorAt(x, newPixel, tmpColor);
                        a += tmpColor[FastBitmap.COLOR_INDEX_A];
                        r += tmpColor[FastBitmap.COLOR_INDEX_R];
                        g += tmpColor[FastBitmap.COLOR_INDEX_G];
                        b += tmpColor[FastBitmap.COLOR_INDEX_B];
                        hits++;
                    }

                    if (y >= targetFastBitmap.Top) {
                        newColors[y - targetFastBitmap.Top] = Color.FromArgb((byte)(a / hits), (byte)(r / hits), (byte)(g / hits), (byte)(b / hits));
                    }
                }

                for (int y = targetFastBitmap.Top; y < targetFastBitmap.Bottom; y++) {
                    targetFastBitmap.SetColorAt(x, y, newColors[y - targetFastBitmap.Top]);
                }
            }
        }

        public static Rectangle FindAutoCropRectangle(IFastBitmap fastBitmap, Point colorPoint, int cropDifference) {
            Rectangle cropRectangle = Rectangle.Empty;
            Color referenceColor = fastBitmap.GetColorAt(colorPoint.X, colorPoint.Y);
            Point min = new Point(int.MaxValue, int.MaxValue);
            Point max = new Point(int.MinValue, int.MinValue);

            if (cropDifference > 0) {
                for (int y = 0; y < fastBitmap.Height; y++) {
                    for (int x = 0; x < fastBitmap.Width; x++) {
                        Color currentColor = fastBitmap.GetColorAt(x, y);
                        int diffR = Math.Abs(currentColor.R - referenceColor.R);
                        int diffG = Math.Abs(currentColor.G - referenceColor.G);
                        int diffB = Math.Abs(currentColor.B - referenceColor.B);
                        if (((diffR + diffG + diffB) / 3) > cropDifference) {
                            if (x < min.X) min.X = x;
                            if (y < min.Y) min.Y = y;
                            if (x > max.X) max.X = x;
                            if (y > max.Y) max.Y = y;
                        }
                    }
                }
            }
            else {
                for (int y = 0; y < fastBitmap.Height; y++) {
                    for (int x = 0; x < fastBitmap.Width; x++) {
                        Color currentColor = fastBitmap.GetColorAt(x, y);
                        if (referenceColor.Equals(currentColor)) {
                            if (x < min.X) min.X = x;
                            if (y < min.Y) min.Y = y;
                            if (x > max.X) max.X = x;
                            if (y > max.Y) max.Y = y;
                        }
                    }
                }
            }

            if (!(Point.Empty.Equals(min) && max.Equals(new Point(fastBitmap.Width - 1, fastBitmap.Height - 1)))) {
                if (!(min.X == int.MaxValue || min.Y == int.MaxValue || max.X == int.MinValue || min.X == int.MinValue)) {
                    cropRectangle = new Rectangle(min.X, min.Y, max.X - min.X + 1, max.Y - min.Y + 1);
                }
            }
            return cropRectangle;
        }
    }
}
