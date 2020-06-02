using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.ComponentModel;

namespace Apowersoft.Utils.Imaging {
    public enum RenderMode { Edit, Export };
    public enum EditStatus { Undrawn, Drawing, Moving, Resizing, Idle };
    public enum HeadEndStyle { NoneHead, StartArrow, EndArrow, BothArrow, StartDot, EndDot, BothDot, Custom };

    public interface IDrawableContainer : INotifyPropertyChanged, IDisposable {
        ISurface Parent { get; }
        bool Selected { get; set; }
        bool DrawOnClick { get; }
        bool CanMove { get; }
        bool CanCopy { get; }
        bool AutoFlatten { get; }
        float ShowScale { get; set; }

        int Left { get; set; }
        int Top { get; set; }
        int Width { get; set; }
        int Height { get; set; }

        Point Location { get; }
        Size Size { get; }
        Rectangle Bounds { get; }
        Rectangle DrawingBounds { get; }

        bool HasFilters { get; }

        EditStatus Status { get; set; }

        void AlignToParent(HAlignment horizontalAlignment, VAlignment verticalAlignment);
        void Invalidate();
        bool ClickableAt(int x, int y);
        void HideGrippers();
        void ShowGrippers();
        void MoveBy(int x, int y);
        bool HandleMouseDown(int x, int y);
        void HandleMouseUp(int x, int y);
        bool HandleMouseMove(int x, int y);
        void OnLocateChanged();
        bool InitContent();
        void MakeBoundsChangeUndoable(bool allowMerge);
    }

    public interface ITextContainer : IDrawableContainer {
        string Text { get; set; }
        void ChangeText(string newText, bool allowUndoable);
    }

    public interface IImageContainer : IDrawableContainer {
        Image Image { get; set; }
        void Load(string filename);
    }

    public interface ICursorContainer : IDrawableContainer {
        Cursor Cursor { get; set; }
        void Load(string filename);
    }

    public interface IIconContainer : IDrawableContainer {
        Icon Icon { get; set; }
        void Load(string filename);
    }

    public interface IMetafileContainer : IDrawableContainer {
        Metafile Metafile { get; set; }
        void Load(string filename);
    }
}