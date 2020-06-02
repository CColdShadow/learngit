using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Collections.Generic;

namespace Apowersoft.Utils.Imaging {
    public enum HAlignment { LEFT, CENTER, RIGHT };
    public enum VAlignment { TOP, CENTER, BOTTOM };

    public enum SurfaceMessageType {
        FileSaved, Error, Info, UploadedUri,
        ImageRectChanged, CanvasRectChanged,
        CanvasResizingBegin, CanvasResizing, CanvasResizingEnd,
        RightClickInsideCanvas, RightClickOutsideCanvas,
        LeftDoubleClickInsideCanvas, LeftDoubleClickOutsideCanvas,
        ModifiedChanged, SelectedPartChanged,
        SelectedElementsChanged, UndrawnElementCreated, 
        UndoRedoChanged
    };

    public enum DrawingModes {
        None, Select, Rect, Ellipse, Text, AutoText, Line, Arrow, Highlight, BrushHighlight,
        Obfuscate, Pixelate, Bitmap, Path, ArrowPath, Brush, ProEraser, Fill, Bezier,
        RoundedRect, Bubble, Diamond, Step, FreeEraser, RectEraser, BrushObfuscate, BrushPixelate
    };

    public class SurfaceMessageEventArgs : EventArgs {
        public SurfaceMessageType MessageType { get; set; }
        public string Message { get; set; }
        public ISurface Surface { get; set; }
    }

    public class SurfaceElementEventArgs : EventArgs {
        public IList<IDrawableContainer> Elements { get; set; }
    }

    public class SurfaceDrawingModeEventArgs : EventArgs {
        public DrawingModes DrawingMode { get; set; }
    }

    public delegate void SurfaceMessageEventHandler(object sender, SurfaceMessageEventArgs eventArgs);
    public delegate void SurfaceDrawingModeEventHandler(object sender, SurfaceDrawingModeEventArgs eventArgs);

    public interface ISurface : IDisposable {
        event SurfaceMessageEventHandler SurfaceMessage;
        event SurfaceDrawingModeEventHandler DrawingModeChanged;

        Guid ID { get; set; }
        bool Modified { get; set; }
        Image Image { get; set; }
        string ImageName { get; }

        Image GetImageForExport();

        long SaveElementsToStream(Stream stream);
        void LoadElementsFromStream(Stream stream);

        bool HasSelectedElements { get; }

        void RemoveSelectedElements();
        void CutSelectedElements();
        void CopySelectedElements();
        void PasteElementFromClipboard();
        void DuplicateSelectedElements();
        void DeselectElement(IDrawableContainer container);
        void DeselectAllElements();
        void SelectElement(IDrawableContainer container);
        void FlattenElement(IDrawableContainer container);

        void Invalidate(Rectangle rectangleToInvalidate);
        void Invalidate();

        void RemoveElement(IDrawableContainer elementToRemove, bool makeUndoable);
        void SendMessageEvent(object source, SurfaceMessageType messageType, string message);
        void ApplyBitmapEffect(IEffect effect);
    }
}