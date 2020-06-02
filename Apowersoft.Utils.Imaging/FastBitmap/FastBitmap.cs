using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace Apowersoft.Utils.Imaging {
    public interface IFastBitmap : IDisposable {
        /// <summary>
        /// Get the color at x,y
        /// The returned Color object depends on the underlying pixel format
        /// </summary>
        /// <param name="x">int x</param>
        /// <param name="y">int y</param>
        /// <returns>Color</returns>
        Color GetColorAt(int x, int y);

        /// <summary>
        /// Set the color at the specified location
        /// </summary>
        /// <param name="x">int x</param>
        /// <param name="y">int y</param>
        /// <param name="color">Color</param>
        void SetColorAt(int x, int y, Color color);

        /// <summary>
        /// Get the color at x,y
        /// The returned byte[] color depends on the underlying pixel format
        /// </summary>
        /// <param name="x">int x</param>
        /// <param name="y">int y</par
        void GetColorAt(int x, int y, byte[] color);

        /// <summary>
        /// Set the color at the specified location
        /// </summary>
        /// <param name="x">int x</param>
        /// <param name="y">int y</param>
        /// <param name="color">byte[] color</param>
        void SetColorAt(int x, int y, byte[] color);

        /// <summary>
        /// Lock the bitmap
        /// </summary>
        void Lock();

        /// <summary>
        /// Unlock the bitmap
        /// </summary>
        void Unlock();

        /// <summary>
        /// Unlock the bitmap and get the underlying bitmap in one call
        /// </summary>
        /// <returns></returns>
        Bitmap UnlockAndReturnBitmap();

        /// <summary>
        /// Size of the underlying image
        /// </summary>
        Size Size {
            get;
        }

        /// <summary>
        /// Height of the image area that this fastbitmap covers
        /// </summary>
        int Height {
            get;
        }

        /// <summary>
        /// Width of the image area that this fastbitmap covers
        /// </summary>
        int Width {
            get;
        }

        /// <summary>
        /// Top of the image area that this fastbitmap covers
        /// </summary>
        int Top {
            get;
        }

        /// <summary>
        /// Left of the image area that this fastbitmap covers
        /// </summary>
        int Left {
            get;
        }

        /// <summary>
        /// Right of the image area that this fastbitmap covers
        /// </summary>
        int Right {
            get;
        }

        /// <summary>
        /// Bottom of the image area that this fastbitmap covers
        /// </summary>
        int Bottom {
            get;
        }

        /// <summary>
        /// Does the underlying image need to be disposed
        /// </summary>
        bool NeedsDispose {
            get;
            set;
        }

        /// <summary>
        /// Returns if this FastBitmap has an alpha channel
        /// </summary>
        bool hasAlphaChannel {
            get;
        }

        /// <summary>
        /// Draw the stored bitmap to the destionation bitmap at the supplied point
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="destination">Point with location</param>
        void DrawTo(Graphics graphics, Point destination);

        /// <summary>
        /// Draw the stored Bitmap on the Destination bitmap with the specified rectangle
        /// Be aware that the stored bitmap will be resized to the specified rectangle!!
        /// </summary>
        /// <param name="graphics">Graphics</param>
        /// <param name="destinationRect">Rectangle with destination</param>
        void DrawTo(Graphics graphics, Rectangle destinationRect);

        /// <summary>
        /// Return true if the coordinates are inside the FastBitmap
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        bool Contains(int x, int y);

        /// <summary>
        /// Set the bitmap resolution
        /// </summary>
        /// <param name="horizontal"></param>
        /// <param name="vertical"></param>
        void SetResolution(float horizontal, float vertical);
    }

    public interface IFastBitmapWithOffset : IFastBitmap {
        /// <summary>
        /// Return true if the coordinates are inside the FastBitmap
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        new bool Contains(int x, int y);

        /// <summary>
        /// Set the color at the specified location, using offsetting so the original coordinates can be used
        /// </summary>
        /// <param name="x">int x</param>
        /// <param name="y">int y</param>
        /// <param name="color">Color color</param>
        new void SetColorAt(int x, int y, Color color);

        /// <summary>
        /// Set the color at the specified location, using offsetting so the original coordinates can be used
        /// </summary>
        /// <param name="x">int x</param>
        /// <param name="y">int y</param>
        /// <param name="color">byte[] color</param>
        new void SetColorAt(int x, int y, byte[] color);

        /// <summary>
        /// Get the color at x,y
        /// The returned Color object depends on the underlying pixel format
        /// </summary>
        /// <param name="x">int x</param>
        /// <param name="y">int y</param>
        /// <returns>Color</returns>
        new Color GetColorAt(int x, int y);

        /// <summary>
        /// Get the color at x,y, using offsetting so the original coordinates can be used
        /// The returned byte[] color depends on the underlying pixel format
        /// </summary>
        /// <param name="x">int x</param>
        /// <param name="y">int y</par
        new void GetColorAt(int x, int y, byte[] color);

        new int Left {
            get;
            set;
        }

        new int Top {
            get;
            set;
        }
    }

    public interface IFastBitmapWithClip : IFastBitmap {
        Rectangle Clip {
            get;
            set;
        }

        bool InvertClip {
            get;
            set;
        }

        /// <summary>
        /// Set the color at the specified location, this doesn't do anything if the location is excluded due to clipping
        /// </summary>
        /// <param name="x">int x</param>
        /// <param name="y">int y</param>
        /// <param name="color">Color color</param>
        new void SetColorAt(int x, int y, Color color);

        /// <summary>
        /// Set the color at the specified location, this doesn't do anything if the location is excluded due to clipping
        /// </summary>
        /// <param name="x">int x</param>
        /// <param name="y">int y</param>
        /// <param name="color">byte[] color</param>
        new void SetColorAt(int x, int y, byte[] color);

        /// <summary>
        /// Return true if the coordinates are inside the FastBitmap and not clipped
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        new bool Contains(int x, int y);
    }

    public interface IFastBitmapWithBlend : IFastBitmap {
        Color BackgroundBlendColor {
            get;
            set;
        }
        Color GetBlendedColorAt(int x, int y);
    }

    public unsafe abstract class FastBitmap : IFastBitmap, IFastBitmapWithClip, IFastBitmapWithOffset {
        protected const int PIXELFORMAT_INDEX_A = 3;
        protected const int PIXELFORMAT_INDEX_R = 2;
        protected const int PIXELFORMAT_INDEX_G = 1;
        protected const int PIXELFORMAT_INDEX_B = 0;

        public const int COLOR_INDEX_R = 0;
        public const int COLOR_INDEX_G = 1;
        public const int COLOR_INDEX_B = 2;
        public const int COLOR_INDEX_A = 3;

        protected Rectangle area = Rectangle.Empty;

        public bool NeedsDispose { get; set; }
        public Rectangle Clip { get; set; }
        public bool InvertClip { get; set; }

        protected Bitmap bitmap = null;

        protected BitmapData bmData;
        protected int stride; /* bytes per pixel row */
        protected bool bitsLocked = false;
        protected byte* pointer = (byte*)0;

        private static log4net.ILog log = log4net.LogManager.GetLogger(typeof(FastBitmap));

        public static IFastBitmap Create(Bitmap source) {
            return Create(source, Rectangle.Empty);
        }

        public void SetResolution(float horizontal, float vertical) {
            bitmap.SetResolution(horizontal, vertical);
        }

        public static IFastBitmap Create(Bitmap source, Rectangle area) {
            switch (source.PixelFormat) {
                case PixelFormat.Format8bppIndexed:
                    return new FastChunkyBitmap(source, area);
                case PixelFormat.Format24bppRgb:
                    return new Fast24RGBBitmap(source, area);
                case PixelFormat.Format32bppRgb:
                    return new Fast32RGBBitmap(source, area);
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                    return new Fast32ARGBBitmap(source, area);
                default:
                    throw new NotSupportedException(string.Format("Not supported Pixelformat {0}", source.PixelFormat));
            }
        }

        public static IFastBitmap CreateCloneOf(Image source) {
            return CreateCloneOf(source, source.PixelFormat, Rectangle.Empty);
        }

        public static IFastBitmap CreateCloneOf(Image source, PixelFormat pixelFormat) {
            return CreateCloneOf(source, pixelFormat, Rectangle.Empty);
        }

        public static IFastBitmap CreateCloneOf(Image source, Rectangle area) {
            return CreateCloneOf(source, PixelFormat.DontCare, area);
        }


        public static IFastBitmap CreateCloneOf(Image source, PixelFormat pixelFormat, Rectangle area) {
            Bitmap destination = ImageOperator.CloneArea(source, area, pixelFormat);
            if (destination != null) {
                try {
                    FastBitmap fastBitmap = Create(destination) as FastBitmap;
                    fastBitmap.NeedsDispose = true;
                    fastBitmap.Left = area.Left;
                    fastBitmap.Top = area.Top;
                    return fastBitmap;
                }
                catch (Exception ex) {
                    log.Error("Failed to create FastBitmap, " + ex.Message);
                }
            }
            return null;
        }

        public static IFastBitmap CreateEmpty(Size newSize, PixelFormat pixelFormat, Color backgroundColor) {
            Bitmap destination = ImageOperator.CreateEmptyBitmap(newSize.Width, newSize.Height, pixelFormat, backgroundColor, 96f, 96f);
            IFastBitmap fastBitmap = Create(destination);
            fastBitmap.NeedsDispose = true;
            return fastBitmap;
        }

        public static void MixColors(IFastBitmap dest, IFastBitmap src, int pixelSize) {
            List<Color> colors = new List<Color>();
            int halbPixelSize = pixelSize / 2;
            for (int y = src.Top - halbPixelSize; y < src.Bottom + halbPixelSize; y = y + pixelSize) {
                for (int x = src.Left - halbPixelSize; x <= src.Right + halbPixelSize; x = x + pixelSize) {
                    colors.Clear();
                    for (int yy = y; yy < y + pixelSize; yy++) {
                        if (yy >= src.Top && yy < src.Bottom) {
                            for (int xx = x; xx < x + pixelSize; xx++) {
                                if (xx >= src.Left && xx < src.Right) {
                                    colors.Add(src.GetColorAt(xx, yy));
                                }
                            }
                        }
                    }
                    Color currentAvgColor = Colors.Mix(colors);
                    for (int yy = y; yy <= y + pixelSize; yy++) {
                        if (yy >= src.Top && yy < src.Bottom) {
                            for (int xx = x; xx <= x + pixelSize; xx++) {
                                if (xx >= src.Left && xx < src.Right) {
                                    dest.SetColorAt(xx, yy, currentAvgColor);
                                }
                            }
                        }
                    }
                }
            }
        }

        protected FastBitmap(Bitmap bitmap, Rectangle area) {
            this.bitmap = bitmap;
            Rectangle bitmapArea = new Rectangle(Point.Empty, bitmap.Size);
            if (area != Rectangle.Empty) {
                area.Intersect(bitmapArea);
                this.area = area;
            }
            else {
                this.area = bitmapArea;
            }
            // As the lock takes care that only the specified area is made available we need to calculate the offset
            this.Left = area.Left;
            this.Top = area.Top;
            // Default cliping is done to the area without invert
            this.Clip = this.area;
            this.InvertClip = false;
            // Always lock, so we don't need to do this ourselves
            Lock();
        }

        public Size Size {
            get {
                if (area == Rectangle.Empty) {
                    return bitmap.Size;
                }
                return area.Size;
            }
        }

        public int Width {
            get {
                if (area == Rectangle.Empty) {
                    return bitmap.Width;
                }
                return area.Width;
            }
        }

        public int Height {
            get {
                if (area == Rectangle.Empty) {
                    return bitmap.Height;
                }
                return area.Height;
            }
        }

        private int left;
        public int Left {
            get {
                return 0;
            }
            set {
                left = value;
            }
        }

        int IFastBitmapWithOffset.Left {
            get {
                return left;
            }
            set {
                left = value;
            }
        }

        private int top;
        public int Top {
            get {
                return 0;
            }
            set {
                top = value;
            }
        }

        int IFastBitmapWithOffset.Top {
            get {
                return top;
            }
            set {
                top = value;
            }
        }

        public int Right {
            get {
                return Left + Width;
            }
        }

        public int Bottom {
            get {
                return Top + Height;
            }
        }

        public Bitmap UnlockAndReturnBitmap() {
            if (bitsLocked) {
                Unlock();
            }
            NeedsDispose = false;
            return bitmap;
        }

        public virtual bool hasAlphaChannel {
            get {
                return false;
            }
        }

        ~FastBitmap() {
            Dispose(false);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            Unlock();
            if (disposing) {
                if (bitmap != null && NeedsDispose) {
                    bitmap.Dispose();
                }
            }
            bitmap = null;
            bmData = null;
            pointer = null;
        }

        public void Lock() {
            if (Width > 0 && Height > 0 && !bitsLocked) {
                bmData = bitmap.LockBits(area, ImageLockMode.ReadWrite, bitmap.PixelFormat);
                bitsLocked = true;

                IntPtr Scan0 = bmData.Scan0;
                pointer = (byte*)(void*)Scan0;
                stride = bmData.Stride;
            }
        }

        public void Unlock() {
            if (bitsLocked) {
                bitmap.UnlockBits(bmData);
                bitsLocked = false;
            }
        }

        public void DrawTo(Graphics graphics, Point destination) {
            DrawTo(graphics, new Rectangle(destination, area.Size));
        }

        public void DrawTo(Graphics graphics, Rectangle destinationRect) {
            // Make sure this.bitmap is unlocked, if it was locked
            bool isLocked = bitsLocked;
            if (isLocked) {
                Unlock();
            }
            graphics.DrawImage(this.bitmap, destinationRect, area, GraphicsUnit.Pixel);
        }

        public bool Contains(int x, int y) {
            return area.Contains(x - Left, y - Top);
        }

        public abstract Color GetColorAt(int x, int y);
        public abstract void SetColorAt(int x, int y, Color color);
        public abstract void GetColorAt(int x, int y, byte[] color);
        public abstract void SetColorAt(int x, int y, byte[] color);

        #region IFastBitmapWithClip

        bool IFastBitmapWithClip.Contains(int x, int y) {
            bool contains = Clip.Contains(x, y);
            if (InvertClip) {
                return !contains;
            }
            else {
                return contains;
            }
        }

        void IFastBitmapWithClip.SetColorAt(int x, int y, byte[] color) {
            bool contains = Clip.Contains(x, y);
            if ((InvertClip && contains) || (!InvertClip && !contains)) {
                return;
            }
            SetColorAt(x, y, color);
        }

        void IFastBitmapWithClip.SetColorAt(int x, int y, Color color) {
            bool contains = Clip.Contains(x, y);
            if ((InvertClip && contains) || (!InvertClip && !contains)) {
                return;
            }
            SetColorAt(x, y, color);
        }

        #endregion

        #region IFastBitmapWithOffset

        bool IFastBitmapWithOffset.Contains(int x, int y) {
            return area.Contains(x - Left, y - Top);
        }

        Color IFastBitmapWithOffset.GetColorAt(int x, int y) {
            x -= left;
            y -= top;
            return GetColorAt(x, y);
        }
        void IFastBitmapWithOffset.GetColorAt(int x, int y, byte[] color) {
            x -= left;
            y -= top;
            GetColorAt(x, y, color);
        }

        void IFastBitmapWithOffset.SetColorAt(int x, int y, byte[] color) {
            x -= left;
            y -= top;
            SetColorAt(x, y, color);
        }

        void IFastBitmapWithOffset.SetColorAt(int x, int y, Color color) {
            x -= left;
            y -= top;
            SetColorAt(x, y, color);
        }

        #endregion
    }

    public unsafe class FastChunkyBitmap : FastBitmap {
        // Used for indexed images
        private Color[] colorEntries;
        private Dictionary<Color, byte> colorCache = new Dictionary<Color, byte>();

        public FastChunkyBitmap(Bitmap source, Rectangle area)
            : base(source, area) {
            colorEntries = bitmap.Palette.Entries;
        }

        public override Color GetColorAt(int x, int y) {
            int offset = x + (y * stride);
            byte colorIndex = pointer[offset];
            return colorEntries[colorIndex];
        }

        public override void GetColorAt(int x, int y, byte[] color) {
            throw new NotImplementedException("No performance gain!");
        }

        public override void SetColorAt(int x, int y, byte[] color) {
            throw new NotImplementedException("No performance gain!");
        }

        public byte GetColorIndexAt(int x, int y) {
            int offset = x + (y * stride);
            return pointer[offset];
        }

        public void SetColorIndexAt(int x, int y, byte colorIndex) {
            int offset = x + (y * stride);
            pointer[offset] = colorIndex;
        }

        public override void SetColorAt(int x, int y, Color color) {
            int offset = x + (y * stride);
            byte colorIndex;
            if (!colorCache.TryGetValue(color, out colorIndex)) {
                bool foundColor = false;
                for (colorIndex = 0; colorIndex < colorEntries.Length; colorIndex++) {
                    if (color == colorEntries[colorIndex]) {
                        colorCache.Add(color, colorIndex);
                        foundColor = true;
                        break;
                    }
                }
                if (!foundColor) {
                    throw new ArgumentException("No such color!");
                }
            }
            pointer[offset] = colorIndex;
        }
    }

    public unsafe class Fast24RGBBitmap : FastBitmap {
        public Fast24RGBBitmap(Bitmap source, Rectangle area)
            : base(source, area) {
        }

        public override Color GetColorAt(int x, int y) {
            int offset = (x * 3) + (y * stride);
            return Color.FromArgb(255, pointer[PIXELFORMAT_INDEX_R + offset], pointer[PIXELFORMAT_INDEX_G + offset], pointer[PIXELFORMAT_INDEX_B + offset]);
        }

        public override void SetColorAt(int x, int y, Color color) {
            int offset = (x * 3) + (y * stride);
            pointer[PIXELFORMAT_INDEX_R + offset] = color.R;
            pointer[PIXELFORMAT_INDEX_G + offset] = color.G;
            pointer[PIXELFORMAT_INDEX_B + offset] = color.B;
        }

        public override void GetColorAt(int x, int y, byte[] color) {
            int offset = (x * 3) + (y * stride);
            color[PIXELFORMAT_INDEX_R] = pointer[PIXELFORMAT_INDEX_R + offset];
            color[PIXELFORMAT_INDEX_G] = pointer[PIXELFORMAT_INDEX_G + offset];
            color[PIXELFORMAT_INDEX_B] = pointer[PIXELFORMAT_INDEX_B + offset];
        }

        public override void SetColorAt(int x, int y, byte[] color) {
            int offset = (x * 3) + (y * stride);
            pointer[PIXELFORMAT_INDEX_R + offset] = color[PIXELFORMAT_INDEX_R];
            pointer[PIXELFORMAT_INDEX_G + offset] = color[PIXELFORMAT_INDEX_G];
            pointer[PIXELFORMAT_INDEX_B + offset] = color[PIXELFORMAT_INDEX_B];
        }
    }

    public unsafe class Fast32RGBBitmap : FastBitmap {
        public Fast32RGBBitmap(Bitmap source, Rectangle area)
            : base(source, area) {
        }

        public override Color GetColorAt(int x, int y) {
            int offset = (x * 4) + (y * stride);
            return Color.FromArgb(255, pointer[PIXELFORMAT_INDEX_R + offset], pointer[PIXELFORMAT_INDEX_G + offset], pointer[PIXELFORMAT_INDEX_B + offset]);
        }

        public override void SetColorAt(int x, int y, Color color) {
            int offset = (x * 4) + (y * stride);
            pointer[PIXELFORMAT_INDEX_R + offset] = color.R;
            pointer[PIXELFORMAT_INDEX_G + offset] = color.G;
            pointer[PIXELFORMAT_INDEX_B + offset] = color.B;
        }

        public override void GetColorAt(int x, int y, byte[] color) {
            int offset = (x * 4) + (y * stride);
            color[COLOR_INDEX_R] = pointer[PIXELFORMAT_INDEX_R + offset];
            color[COLOR_INDEX_G] = pointer[PIXELFORMAT_INDEX_G + offset];
            color[COLOR_INDEX_B] = pointer[PIXELFORMAT_INDEX_B + offset];
        }

        public override void SetColorAt(int x, int y, byte[] color) {
            int offset = (x * 4) + (y * stride);
            pointer[PIXELFORMAT_INDEX_R + offset] = color[COLOR_INDEX_R];	// R
            pointer[PIXELFORMAT_INDEX_G + offset] = color[COLOR_INDEX_G];
            pointer[PIXELFORMAT_INDEX_B + offset] = color[COLOR_INDEX_B];
        }
    }

    public unsafe class Fast32ARGBBitmap : FastBitmap, IFastBitmapWithBlend {
        public override bool hasAlphaChannel {
            get {
                return true;
            }
        }

        public Color BackgroundBlendColor {
            get;
            set;
        }
        public Fast32ARGBBitmap(Bitmap source, Rectangle area)
            : base(source, area) {
            BackgroundBlendColor = Color.White;
        }

        public override Color GetColorAt(int x, int y) {
            int offset = (x * 4) + (y * stride);
            return Color.FromArgb(pointer[PIXELFORMAT_INDEX_A + offset], pointer[PIXELFORMAT_INDEX_R + offset], pointer[PIXELFORMAT_INDEX_G + offset], pointer[PIXELFORMAT_INDEX_B + offset]);
        }

        public override void SetColorAt(int x, int y, Color color) {
            int offset = (x * 4) + (y * stride);
            pointer[PIXELFORMAT_INDEX_A + offset] = color.A;
            pointer[PIXELFORMAT_INDEX_R + offset] = color.R;
            pointer[PIXELFORMAT_INDEX_G + offset] = color.G;
            pointer[PIXELFORMAT_INDEX_B + offset] = color.B;
        }

        public override void GetColorAt(int x, int y, byte[] color) {
            int offset = (x * 4) + (y * stride);
            color[COLOR_INDEX_R] = pointer[PIXELFORMAT_INDEX_R + offset];
            color[COLOR_INDEX_G] = pointer[PIXELFORMAT_INDEX_G + offset];
            color[COLOR_INDEX_B] = pointer[PIXELFORMAT_INDEX_B + offset];
            color[COLOR_INDEX_A] = pointer[PIXELFORMAT_INDEX_A + offset];
        }

        public override void SetColorAt(int x, int y, byte[] color) {
            int offset = (x * 4) + (y * stride);
            pointer[PIXELFORMAT_INDEX_R + offset] = color[COLOR_INDEX_R];	// R
            pointer[PIXELFORMAT_INDEX_G + offset] = color[COLOR_INDEX_G];
            pointer[PIXELFORMAT_INDEX_B + offset] = color[COLOR_INDEX_B];
            pointer[PIXELFORMAT_INDEX_A + offset] = color[COLOR_INDEX_A];
        }

        public Color GetBlendedColorAt(int x, int y) {
            int offset = (x * 4) + (y * stride);
            int a = pointer[PIXELFORMAT_INDEX_A + offset];
            int red = pointer[PIXELFORMAT_INDEX_R + offset];
            int green = pointer[PIXELFORMAT_INDEX_G + offset];
            int blue = pointer[PIXELFORMAT_INDEX_B + offset];

            if (a < 255) {
                // As the request is to get without alpha, we blend.
                int rem = 255 - a;
                red = (red * a + BackgroundBlendColor.R * rem) / 255;
                green = (green * a + BackgroundBlendColor.G * rem) / 255;
                blue = (blue * a + BackgroundBlendColor.B * rem) / 255;
            }
            return Color.FromArgb(255, red, green, blue);
        }
    }
}