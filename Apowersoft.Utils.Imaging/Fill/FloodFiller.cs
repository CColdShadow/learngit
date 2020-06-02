using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace Apowersoft.Utils.ScreenRecorder.Imaging.Fill {
    internal class FloodFiller {
        private EditableBitmap bitmap;
        private byte[] tolerance = new byte[] { 5, 5, 5 };
        private Color fillColor = Color.White;
        private bool fillDiagonally = false;

        FloodFillRangeQueue ranges = new FloodFillRangeQueue();

        //cached bitmap properties
        private int bitmapWidth = 0;
        private int bitmapHeight = 0;
        private int bitmapStride = 0;
        private int bitmapPixelFormatSize = 0;
        private byte[] bitmapBits = null;

        //internal, initialized per fill
        //private BitArray pixelsChecked;
        private bool[] pixelsChecked;
        private byte[] byteFillColor;
        private byte[] startColor;
        //private int stride;

        public FloodFiller() {
        }
        
        public Color FillColor {
            get { return fillColor; }
            set { fillColor = value; }
        }

        public bool FillDiagonally {
            get { return fillDiagonally; }
            set { fillDiagonally = value; }
        }

        public byte[] Tolerance {
            get { return tolerance; }
            set { tolerance = value; }
        }

        public Bitmap FloodFill(Bitmap bmp, Point pt) {
            if (bmp == null) {
                return null;
            }

            Rectangle imageRect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            if (!imageRect.Contains(pt)) {
                return null;
            }

            EditableBitmap editBmp = new EditableBitmap(bmp);
            this.bitmap = editBmp;
            PrepareForFloodFill(pt);
            DoFloodFill(pt);
            this.bitmap = null;
            return editBmp.Bitmap;
        }

        private void PrepareForFloodFill(Point pt) {
            //cache data in member variables to decrease overhead of property calls
            //this is especially important with Width and Height, as they call
            //GdipGetImageWidth() and GdipGetImageHeight() respectively in gdiplus.dll - 
            //which means major overhead.
            byteFillColor = new byte[] { fillColor.B, fillColor.G, fillColor.R, fillColor.A };
            bitmapStride = bitmap.Stride;
            bitmapPixelFormatSize = bitmap.PixelFormatSize;
            bitmapBits = bitmap.Bits;
            bitmapWidth = bitmap.Bitmap.Width;
            bitmapHeight = bitmap.Bitmap.Height;

            pixelsChecked = new bool[bitmapBits.Length / bitmapPixelFormatSize];
        }

        private void DoFloodFill(Point pt) {
            ranges = new FloodFillRangeQueue(((bitmapWidth + bitmapHeight) / 2) * 5);//new Queue<FloodFillRange>();

            //***Get starting color.
            int x = pt.X; int y = pt.Y;
            int idx = CoordsToByteIndex(ref x, ref y);
            startColor = new byte[] { bitmap.Bits[idx], bitmap.Bits[idx + 1], bitmap.Bits[idx + 2] };

            bool[] pixelsChecked = this.pixelsChecked;

            //***Do first call to floodfill.
            LinearFill(ref x, ref y);

            //***Call floodfill routine while floodfill ranges still exist on the queue
            while (ranges.Count > 0) {
                //**Get Next Range Off the Queue
                FloodFillRange range = ranges.Dequeue();

                //**Check Above and Below Each Pixel in the Floodfill Range
                int downPxIdx = (bitmapWidth * (range.Y + 1)) + range.StartX;//CoordsToPixelIndex(lFillLoc,y+1);
                int upPxIdx = (bitmapWidth * (range.Y - 1)) + range.StartX;//CoordsToPixelIndex(lFillLoc, y - 1);
                int upY = range.Y - 1;//so we can pass the y coord by ref
                int downY = range.Y + 1;
                int tempIdx;
                for (int i = range.StartX; i <= range.EndX; i++) {
                    //*Start Fill Upwards
                    //if we're not above the top of the bitmap and the pixel above this one is within the color tolerance
                    tempIdx = CoordsToByteIndex(ref i, ref upY);
                    if (range.Y > 0 && (!pixelsChecked[upPxIdx]) && CheckPixel(ref tempIdx))
                        LinearFill(ref i, ref upY);

                    //*Start Fill Downwards
                    //if we're not below the bottom of the bitmap and the pixel below this one is within the color tolerance
                    tempIdx = CoordsToByteIndex(ref i, ref downY);
                    if (range.Y < (bitmapHeight - 1) && (!pixelsChecked[downPxIdx]) && CheckPixel(ref tempIdx))
                        LinearFill(ref i, ref downY);
                    downPxIdx++;
                    upPxIdx++;
                }
            }
        }

        /// <summary>
        /// Finds the furthermost left and right boundaries of the fill area
        /// on a given y coordinate, starting from a given x coordinate, filling as it goes.
        /// Adds the resulting horizontal range to the queue of floodfill ranges,
        /// to be processed in the main loop.
        /// </summary>
        /// <param name="x">The x coordinate to start from.</param>
        /// <param name="y">The y coordinate to check at.</param>
        private void LinearFill(ref int x, ref int y) {

            //cache some bitmap and fill info in local variables for a little extra speed
            byte[] bitmapBits = this.bitmapBits;
            bool[] pixelsChecked = this.pixelsChecked;
            byte[] byteFillColor = this.byteFillColor;
            int bitmapPixelFormatSize = this.bitmapPixelFormatSize;
            int bitmapWidth = this.bitmapWidth;

            //***Find Left Edge of Color Area
            int lFillLoc = x; //the location to check/fill on the left
            int idx = CoordsToByteIndex(ref x, ref y); //the byte index of the current location
            int pxIdx = (bitmapWidth * y) + x;//CoordsToPixelIndex(x,y);
            while (true) {
                //**fill with the color
                //bitmapBits[idx] = byteFillColor[0];
                //bitmapBits[idx + 1] = byteFillColor[1];
                //bitmapBits[idx + 2] = byteFillColor[2];
                // fill the color with alpha blend 
                bitmapBits[idx] = (byte)BlendColor(byteFillColor[0], byteFillColor[3], bitmapBits[idx], bitmapBits[idx + 3]);
                bitmapBits[idx + 1] = (byte)BlendColor(byteFillColor[1], byteFillColor[3], bitmapBits[idx + 1], bitmapBits[idx + 3]);
                bitmapBits[idx + 2] = (byte)BlendColor(byteFillColor[2], byteFillColor[3], bitmapBits[idx + 2], bitmapBits[idx + 3]);
                bitmapBits[idx + 3] = (byte)BlendAlpha(byteFillColor[3], bitmapBits[idx + 3]);
                //**indicate that this pixel has already been checked and filled
                pixelsChecked[pxIdx] = true;
                //**de-increment
                lFillLoc--;     //de-increment counter
                pxIdx--;        //de-increment pixel index
                idx -= bitmapPixelFormatSize;//de-increment byte index
                //**exit loop if we're at edge of bitmap or color area
               // if (lFillLoc <= 0 || (pixelsChecked[pxIdx]) || !CheckPixel(ref idx))
                if (lFillLoc <= 0 || (pixelsChecked[pxIdx]) || !CheckPixel(ref idx)) {
                    break;
                }

            }
            lFillLoc++;

            //***Find Right Edge of Color Area
            int rFillLoc = x; //the location to check/fill on the left
            idx = CoordsToByteIndex(ref x, ref y);
            pxIdx = (bitmapWidth * y) + x;
            while (true) {
                //fill with the color
                //bitmapBits[idx] = byteFillColor[0];
                //bitmapBits[idx + 1] = byteFillColor[1];
                //bitmapBits[idx + 2] = byteFillColor[2];
                // fill the color with alpha blend 
                bitmapBits[idx] = (byte)BlendColor(byteFillColor[0], byteFillColor[3], bitmapBits[idx], bitmapBits[idx + 3]);
                bitmapBits[idx + 1] = (byte)BlendColor(byteFillColor[1], byteFillColor[3], bitmapBits[idx + 1], bitmapBits[idx + 3]);
                bitmapBits[idx + 2] = (byte)BlendColor(byteFillColor[2], byteFillColor[3], bitmapBits[idx + 2], bitmapBits[idx + 3]);
                bitmapBits[idx + 3] = (byte)BlendAlpha(byteFillColor[3], bitmapBits[idx + 3]);
                //**indicate that this pixel has already been checked and filled
                pixelsChecked[pxIdx] = true;
                //**increment
                rFillLoc++;     //increment counter
                pxIdx++;        //increment pixel index
                idx += bitmapPixelFormatSize;//increment byte index
                //**exit loop if we're at edge of bitmap or color area
                if (rFillLoc >= bitmapWidth || pixelsChecked[pxIdx] || !CheckPixel(ref idx))
                    break;

            }
            rFillLoc--;

            //add range to queue
            FloodFillRange r = new FloodFillRange(lFillLoc, rFillLoc, y);
            ranges.Enqueue(ref r);
        }

        ///<summary>Sees if a pixel is within the color tolerance range.</summary>
        ///<param name="px">The byte index of the pixel to check, passed by reference to increase performance.</param>
        private bool CheckPixel(ref int px) {
            //tried a 'for' loop but it adds an 8% overhead to the floodfill process
            /*bool ret = true;
            for (byte i = 0; i < 3; i++)
            {
                ret &= (bitmap.Bits[px] >= (startColor[i] - tolerance[i])) && bitmap.Bits[px] <= (startColor[i] + tolerance[i]);
                px++;
            }
            return ret;*/

            return (bitmapBits[px] >= (startColor[0] - tolerance[0])) && bitmapBits[px] <= (startColor[0] + tolerance[0]) &&
                (bitmapBits[px + 1] >= (startColor[1] - tolerance[1])) && bitmapBits[px + 1] <= (startColor[1] + tolerance[1]) &&
                (bitmapBits[px + 2] >= (startColor[2] - tolerance[2])) && bitmapBits[px + 2] <= (startColor[2] + tolerance[2]);
        }

        ///<summary>Calculates and returns the byte index for the pixel (x,y).</summary>
        ///<param name="x">The x coordinate of the pixel whose byte index should be returned.</param>
        ///<param name="y">The y coordinate of the pixel whose byte index should be returned.</param>
        private int CoordsToByteIndex(ref int x, ref int y) {
            return (bitmapStride * y) + (x * bitmapPixelFormatSize);
        }

        /// <summary>
        /// Returns the linear index for a pixel, given its x and y coordinates.
        /// </summary>
        /// <param name="x">The x coordinate of the pixel.</param>
        /// <param name="y">The y coordinate of the pixel.</param>
        /// <returns></returns>
        private int CoordsToPixelIndex(int x, int y) {
            return (bitmapWidth * y) + x;
        }

        /// <summary>
        /// Blend a color value of R, G or B
        /// </summary>
        /// <param name="foreColorValue"></param>
        /// <param name="foreAlphaValue"></param>
        /// <param name="backColorValue"></param>
        /// <param name="backAlphaValue"></param>
        /// <returns></returns>
        private float BlendColor(float foreColorValue, float foreAlphaValue,
            float backColorValue, float backAlphaValue) {
            return (foreColorValue * foreAlphaValue / 255f
                + backColorValue * backAlphaValue / 255f * (1 - foreAlphaValue / 255f));
        }

        /// <summary>
        /// Blend the alpha value
        /// </summary>
        /// <param name="foreAlphaValue"></param>
        /// <param name="backAlphaValue"></param>
        /// <returns></returns>
        private float BlendAlpha(float foreAlphaValue, float backAlphaValue) {
            return (1 - (1 - foreAlphaValue / 255f) * (1 - backAlphaValue / 255f)) * 255f;
        }

    }
}
