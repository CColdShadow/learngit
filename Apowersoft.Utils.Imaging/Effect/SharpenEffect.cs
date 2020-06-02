using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace Apowersoft.Utils.Imaging {
    /// <summary>
    /// 锐化
    /// </summary>
    public class SharpenEffect : IEffect {
        private log4net.ILog log = log4net.LogManager.GetLogger(typeof(SharpenEffect));

        public SharpenEffect(float depth) {
            Depth = depth;
        }

        public bool CanApplyPartly { get { return true; } }

        public float Depth { get; set; }

        public Image Apply(Image sourceImage, out Point offsetChange) {
            offsetChange = Point.Empty;
            return Sharpen(sourceImage, Depth);
        }

        public Image Sharpen(Image sourceImage, float depth) {
            Bitmap sourceBitmap = sourceImage.Clone() as Bitmap;
            if (sourceBitmap == null) {
                return null;
            }

            Bitmap newImage = ImageOperator.CreateEmptyBitmap(sourceBitmap.Width, sourceBitmap.Height, sourceBitmap.PixelFormat,
                Color.Empty, sourceBitmap.HorizontalResolution, sourceBitmap.VerticalResolution);

            Rectangle rect = new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height);
            BitmapData bitmapdata = sourceBitmap.LockBits(rect, ImageLockMode.ReadWrite, sourceBitmap.PixelFormat);

            int bytePerPixelColor = GetBytePerPixelColoer(bitmapdata.PixelFormat);
            if (bytePerPixelColor != 3 && bytePerPixelColor != 4) {
                log.Error("Pixel format not supported for sharpen:" + bitmapdata.PixelFormat.ToString());
                return sourceImage;
            }

            //获取像素地址值  
            IntPtr ptr = bitmapdata.Scan0;
            //每个图片需要记录长乘以宽*3（RGB，实际顺序应该是BGR）  
            int bytes = sourceBitmap.Width * sourceBitmap.Height * bytePerPixelColor;
            byte[] rgbvalues = new byte[bytes];

            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbvalues, 0, bytes);//Marshal使用  
            for (int i = 0; i < sourceBitmap.Width; i++) {
                for (int j = 0; j < sourceBitmap.Height; j++) {
                    int ltIndex = ((i == 0 ? i : i - 1) + (j == 0 ? j : j - 1) * sourceBitmap.Width) * bytePerPixelColor;
                    int curIndex = (i + j * sourceBitmap.Width) * bytePerPixelColor;

                    int blueValueLT = rgbvalues[ltIndex];
                    int blueValue = rgbvalues[curIndex];
                    int newValue = (int)(blueValue + depth * (blueValue - blueValueLT));
                    if (newValue < 0) {
                        newValue = 0;
                    }
                    if (newValue > 255) {
                        newValue = 255;
                    }
                    rgbvalues[curIndex] = (byte)newValue;

                    int greenValueLT = rgbvalues[ltIndex + 1];
                    int greenValue = rgbvalues[curIndex + 1];
                    newValue = (int)(greenValue + depth * (greenValue - greenValueLT));
                    if (newValue < 0) {
                        newValue = 0;
                    }
                    if (newValue > 255) {
                        newValue = 255;
                    }
                    rgbvalues[curIndex + 1] = (byte)newValue;

                    int redValueLT = rgbvalues[ltIndex + 2];
                    int redValue = rgbvalues[curIndex + 2];
                    newValue = (int)(redValue + depth * (redValue - redValueLT));
                    if (newValue < 0) {
                        newValue = 0;
                    }
                    if (newValue > 255) {
                        newValue = 255;
                    }
                    rgbvalues[curIndex + 2] = (byte)newValue;
                }
            }
            sourceBitmap.UnlockBits(bitmapdata);

            BitmapData newBitmapdata = newImage.LockBits(rect, ImageLockMode.ReadWrite, sourceBitmap.PixelFormat);
            System.Runtime.InteropServices.Marshal.Copy(rgbvalues, 0, newBitmapdata.Scan0, bytes);//Marshal  
            newImage.UnlockBits(newBitmapdata);

            return newImage;
        }

        private int GetBytePerPixelColoer(PixelFormat pixelFormat) {
            int ret = 0;
            switch (pixelFormat) {
                case PixelFormat.Format8bppIndexed:
                    ret = 1;
                    break;
                case PixelFormat.Format16bppArgb1555:
                case PixelFormat.Format16bppGrayScale:
                case PixelFormat.Format16bppRgb555:
                case PixelFormat.Format16bppRgb565:
                case PixelFormat.Format24bppRgb:
                    ret = 2;
                    break;
                case PixelFormat.Canonical:
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                case PixelFormat.Format32bppRgb:
                    ret = 4;
                    break;
                case PixelFormat.Format48bppRgb:
                    ret = 6;
                    break;
                case PixelFormat.Format64bppArgb:
                case PixelFormat.Format64bppPArgb:
                    ret = 8;
                    break;

                default:
                    break;

            }
            return ret;
        }
    }
}
