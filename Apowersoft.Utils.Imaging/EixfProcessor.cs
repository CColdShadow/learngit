using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using log4net;

namespace Apowersoft.Utils.Imaging {
    internal enum ExifOrientations : byte {
        Unknown = 0,
        TopLeft = 1,
        TopRight = 2,
        BottomRight = 3,
        BottomLeft = 4,
        LeftTop = 5,
        RightTop = 6,
        RightBottom = 7,
        LeftBottom = 8,
    }

    public class EixfProcessor {
        private const int EXIF_ORIENTATION_ID = 0x0112;

        // 适正
        public static void Orientate(Image image) {
            try {
                // Get the index of the orientation property.
                int orientation_index = Array.IndexOf(image.PropertyIdList, EXIF_ORIENTATION_ID);
                // If there is no such property, return Unknown.
                if (orientation_index < 0) {
                    return;
                }
                PropertyItem item = image.GetPropertyItem(EXIF_ORIENTATION_ID);

                ExifOrientations orientation = (ExifOrientations)item.Value[0];
                // Orient the image.
                switch (orientation) {
                    case ExifOrientations.Unknown:
                    case ExifOrientations.TopLeft:
                        break;
                    case ExifOrientations.TopRight:
                        image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        break;
                    case ExifOrientations.BottomRight:
                        image.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        break;
                    case ExifOrientations.BottomLeft:
                        image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                        break;
                    case ExifOrientations.LeftTop:
                        image.RotateFlip(RotateFlipType.Rotate90FlipX);
                        break;
                    case ExifOrientations.RightTop:
                        image.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        break;
                    case ExifOrientations.RightBottom:
                        image.RotateFlip(RotateFlipType.Rotate90FlipY);
                        break;
                    case ExifOrientations.LeftBottom:
                        image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        break;
                }
                // Set the orientation to be normal, as we rotated the image.
                item.Value[0] = (byte)ExifOrientations.TopLeft;
                image.SetPropertyItem(item);
            }
            catch (Exception orientEx) {
                log.Error("Failed to Orientate, " + orientEx.Message);
            }
        }

        private static ILog log = LogManager.GetLogger(typeof(EixfProcessor));
    }
}