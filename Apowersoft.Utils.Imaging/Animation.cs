using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;

namespace Apowersoft.Utils.Imaging {

    /// <summary>
    /// Interface for passing base type
    /// </summary>
    public interface IAnimator {
        bool HasNext { get; }
        int FramesCount { get; }
        int CurrentFrameNum { get; }
    }

    // This class is used to store a animation leg
    internal class AnimationLeg<T> {
        public T Destination { get; set; }
        public int FramesCount { get; set; }
        public EasingType EasingType { get; set; }
        public EasingMode EasingMode { get; set; }
    }

    /// <summary>
    /// Base class for the animation logic, this only implements Properties and a constructor
    /// </summary>
    /// <typeparam name="T">Type for the animation, like Point/Rectangle/Size</typeparam>
    public abstract class AnimatorBase<T> : IAnimator {
        protected T first;
        protected T last;
        protected T current;
        private Queue<AnimationLeg<T>> queue = new Queue<AnimationLeg<T>>();
        protected int framesCount;
        protected int currentFrameNum = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="first"></param>
        /// <param name="last"></param>
        /// <param name="frames"></param>
        /// <param name="easingType"></param>
        /// <param name="easingMode"></param>
        public AnimatorBase(T first, T last, int framesCount, EasingType easingType, EasingMode easingMode) {
            this.first = first;
            this.last = last;
            this.framesCount = framesCount;
            this.current = first;
            this.EasingType = easingType;
            this.EasingMode = easingMode;
        }

        /// <summary>
        /// The amount of frames
        /// </summary>
        public int FramesCount { get { return framesCount; } }

        /// <summary>
        /// Current frame number
        /// </summary>
        public int CurrentFrameNum { get { return currentFrameNum; } }

        /// <summary>
        /// First animation value
        /// </summary>
        public T First { get { return first; } }

        /// <summary>
        /// Last animation value, of this "leg"
        /// </summary>
        public T Last { get { return last; } }

        /// <summary>
        /// Final animation value, this is including the legs
        /// </summary>
        public T Final {
            get {
                if (queue.Count == 0) {
                    return last;
                }
                return queue.ToArray()[queue.Count - 1].Destination;
            }
        }

        /// <summary>
        /// This restarts the current animation and changes the last frame
        /// </summary>
        /// <param name="newDestination"></param>
        public void ChangeDestination(T newDestination) {
            ChangeDestination(newDestination, framesCount);
        }

        /// <summary>
        /// This restarts the current animation and changes the last frame
        /// </summary>
        /// <param name="newDestination"></param>
        /// <param name="frames"></param>
        public void ChangeDestination(T newDestination, int framesCount) {
            queue.Clear();
            this.first = current;
            this.currentFrameNum = 0;
            this.framesCount = framesCount;
            this.last = newDestination;
        }

        /// <summary>
        /// Queue the destination, it will be used to continue at the last frame
        /// All values will stay the same
        /// </summary>
        /// <param name="queuedDestination"></param>
        public void QueueDestinationLeg(T queuedDestination) {
            QueueDestinationLeg(queuedDestination, FramesCount, EasingType, EasingMode);
        }

        /// <summary>
        /// Queue the destination, it will be used to continue at the last frame
        /// </summary>
        /// <param name="queuedDestination"></param>
        /// <param name="frames"></param>
        public void QueueDestinationLeg(T queuedDestination, int framesCount) {
            QueueDestinationLeg(queuedDestination, framesCount, EasingType, EasingMode);
        }

        /// <summary>
        /// Queue the destination, it will be used to continue at the last frame
        /// </summary>
        /// <param name="queuedDestination"></param>
        /// <param name="frames"></param>
        /// <param name="easingType">EasingType</param>
        public void QueueDestinationLeg(T queuedDestination, int framesCount, EasingType easingType) {
            QueueDestinationLeg(queuedDestination, framesCount, easingType, EasingMode);
        }

        /// <summary>
        /// Queue the destination, it will be used to continue at the last frame
        /// </summary>
        /// <param name="queuedDestination"></param>
        /// <param name="frames"></param>
        /// <param name="easingType"></param>
        /// <param name="easingMode"></param>
        public void QueueDestinationLeg(T queuedDestination, int framesCount, EasingType easingType, EasingMode easingMode) {
            AnimationLeg<T> leg = new AnimationLeg<T>();
            leg.Destination = queuedDestination;
            leg.FramesCount = framesCount;
            leg.EasingType = easingType;
            leg.EasingMode = easingMode;
            queue.Enqueue(leg);
        }

        /// <summary>
        /// The EasingType to use for the animation
        /// </summary>
        public EasingType EasingType { get; set; }

        /// <summary>
        /// The EasingMode to use for the animation
        /// </summary>
        public EasingMode EasingMode { get; set; }

        /// <summary>
        /// Get the easing value, which is from 0-1 and depends on the frame
        /// </summary>
        protected double EasingValue {
            get {
                switch (EasingMode) {
                    case EasingMode.EaseOut:
                        return Easing.EaseOut((double)currentFrameNum / (double)framesCount, EasingType);
                    case EasingMode.EaseInOut:
                        return Easing.EaseInOut((double)currentFrameNum / (double)framesCount, EasingType);
                    case EasingMode.EaseIn:
                    default:
                        return Easing.EaseIn((double)currentFrameNum / (double)framesCount, EasingType);
                }
            }
        }

        /// <summary>
        /// Get the current (previous) frame object
        /// </summary>
        public virtual T Current { get { return current; } }

        /// <summary>
        /// Returns if there are any frame left, and if this is the case than the frame is increased.
        /// </summary>
        public virtual bool NextFrame {
            get {
                if (currentFrameNum < framesCount) {
                    currentFrameNum++;
                    return true;
                }
                if (queue.Count > 0) {
                    this.first = current;
                    this.currentFrameNum = 0;
                    AnimationLeg<T> nextLeg = queue.Dequeue();
                    this.last = nextLeg.Destination;
                    this.framesCount = nextLeg.FramesCount;
                    this.EasingType = nextLeg.EasingType;
                    this.EasingMode = nextLeg.EasingMode;
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Are there more frames to animate?
        /// </summary>
        public virtual bool HasNext {
            get {
                if (currentFrameNum < framesCount) {
                    return true;
                }
                return queue.Count > 0;
            }
        }

        /// <summary>
        /// Get the next animation frame value object
        /// </summary>
        /// <returns></returns>
        public abstract T Next();
    }

    /// <summary>
    /// Implementation of the RectangleAnimator
    /// </summary>
    public class RectangleAnimator : AnimatorBase<Rectangle> {
        public RectangleAnimator(Rectangle first, Rectangle last, int frames)
            : base(first, last, frames, EasingType.Linear, EasingMode.EaseIn) {
        }
        public RectangleAnimator(Rectangle first, Rectangle last, int frames, EasingType easingType)
            : base(first, last, frames, easingType, EasingMode.EaseIn) {
        }

        public RectangleAnimator(Rectangle first, Rectangle last, int frames, EasingType easingType, EasingMode easingMode)
            : base(first, last, frames, easingType, easingMode) {
        }

        /// <summary>
        /// Calculate the next frame object
        /// </summary>
        /// <returns>Rectangle</returns>
        public override Rectangle Next() {
            if (NextFrame) {
                double easingValue = EasingValue;
                double dx = last.X - first.X;
                double dy = last.Y - first.Y;

                int x = first.X + (int)(easingValue * dx);
                int y = first.Y + (int)(easingValue * dy);
                double dw = last.Width - first.Width;
                double dh = last.Height - first.Height;
                int width = first.Width + (int)(easingValue * dw);
                int height = first.Height + (int)(easingValue * dh);
                current = new Rectangle(x, y, width, height);
            }
            return current;
        }
    }

    /// <summary>
    /// Implementation of the PointAnimator
    /// </summary>
    public class PointAnimator : AnimatorBase<Point> {
        public PointAnimator(Point first, Point last, int frames)
            : base(first, last, frames, EasingType.Linear, EasingMode.EaseIn) {
        }
        public PointAnimator(Point first, Point last, int frames, EasingType easingType)
            : base(first, last, frames, easingType, EasingMode.EaseIn) {
        }
        public PointAnimator(Point first, Point last, int frames, EasingType easingType, EasingMode easingMode)
            : base(first, last, frames, easingType, easingMode) {
        }

        /// <summary>
        /// Calculate the next frame value
        /// </summary>
        /// <returns>Point</returns>
        public override Point Next() {
            if (NextFrame) {
                double easingValue = EasingValue;
                double dx = last.X - first.X;
                double dy = last.Y - first.Y;

                int x = first.X + (int)(easingValue * dx);
                int y = first.Y + (int)(easingValue * dy);
                current = new Point(x, y);
            }
            return current;
        }
    }

    /// <summary>
    /// Implementation of the SizeAnimator
    /// </summary>
    public class SizeAnimator : AnimatorBase<Size> {
        public SizeAnimator(Size first, Size last, int frames)
            : base(first, last, frames, EasingType.Linear, EasingMode.EaseIn) {
        }
        public SizeAnimator(Size first, Size last, int frames, EasingType easingType)
            : base(first, last, frames, easingType, EasingMode.EaseIn) {
        }
        public SizeAnimator(Size first, Size last, int frames, EasingType easingType, EasingMode easingMode)
            : base(first, last, frames, easingType, easingMode) {
        }

        /// <summary>
        /// Calculate the next frame values
        /// </summary>
        /// <returns>Size</returns>
        public override Size Next() {
            if (NextFrame) {
                double easingValue = EasingValue;
                double dw = last.Width - first.Width;
                double dh = last.Height - first.Height;
                int width = first.Width + (int)(easingValue * dw);
                int height = first.Height + (int)(easingValue * dh);
                current = new Size(width, height);
            }
            return current;
        }
    }

    /// <summary>
    /// Implementation of the ColorAnimator
    /// </summary>
    public class ColorAnimator : AnimatorBase<Color> {
        public ColorAnimator(Color first, Color last, int frames)
            : base(first, last, frames, EasingType.Linear, EasingMode.EaseIn) {
        }
        public ColorAnimator(Color first, Color last, int frames, EasingType easingType)
            : base(first, last, frames, easingType, EasingMode.EaseIn) {
        }
        public ColorAnimator(Color first, Color last, int frames, EasingType easingType, EasingMode easingMode)
            : base(first, last, frames, easingType, easingMode) {
        }

        /// <summary>
        /// Calculate the next frame values
        /// </summary>
        /// <returns>Color</returns>
        public override Color Next() {
            if (NextFrame) {
                double easingValue = EasingValue;
                double da = last.A - first.A;
                double dr = last.R - first.R;
                double dg = last.G - first.G;
                double db = last.B - first.B;
                int a = first.A + (int)(easingValue * da);
                int r = first.R + (int)(easingValue * dr);
                int g = first.G + (int)(easingValue * dg);
                int b = first.B + (int)(easingValue * db);
                current = Color.FromArgb(a, r, g, b);
            }
            return current;
        }
    }

    /// <summary>
    /// Implementation of the IntAnimator
    /// </summary>
    public class IntAnimator : AnimatorBase<int> {
        public IntAnimator(int first, int last, int frames)
            : base(first, last, frames, EasingType.Linear, EasingMode.EaseIn) {
        }
        public IntAnimator(int first, int last, int frames, EasingType easingType)
            : base(first, last, frames, easingType, EasingMode.EaseIn) {
        }
        public IntAnimator(int first, int last, int frames, EasingType easingType, EasingMode easingMode)
            : base(first, last, frames, easingType, easingMode) {
        }

        /// <summary>
        /// Calculate the next frame values
        /// </summary>
        /// <returns>int</returns>
        public override int Next() {
            if (NextFrame) {
                double easingValue = EasingValue;
                double delta = last - first;
                current = first + (int)(easingValue * delta);
            }
            return current;
        }
    }

    /// <summary>
    /// Easing logic, to make the animations more "fluent"
    /// </summary>
    public static class Easing {
        public static double Ease(double linearStep, double acceleration, EasingType type) {
            double easedStep = acceleration > 0 ? EaseIn(linearStep, type) : acceleration < 0 ? EaseOut(linearStep, type) : (double)linearStep;
            return ((easedStep - linearStep) * Math.Abs(acceleration) + linearStep);
        }

        public static double EaseIn(double linearStep, EasingType type) {
            switch (type) {
                case EasingType.Step:
                    return linearStep < 0.5 ? 0 : 1;
                case EasingType.Linear:
                    return linearStep;
                case EasingType.Sine:
                    return Sine.EaseIn(linearStep);
                case EasingType.Quadratic:
                    return Power.EaseIn(linearStep, 2);
                case EasingType.Cubic:
                    return Power.EaseIn(linearStep, 3);
                case EasingType.Quartic:
                    return Power.EaseIn(linearStep, 4);
                case EasingType.Quintic:
                    return Power.EaseIn(linearStep, 5);
            }
            throw new NotImplementedException();
        }

        public static double EaseOut(double linearStep, EasingType type) {
            switch (type) {
                case EasingType.Step:
                    return linearStep < 0.5 ? 0 : 1;
                case EasingType.Linear:
                    return linearStep;
                case EasingType.Sine:
                    return Sine.EaseOut(linearStep);
                case EasingType.Quadratic:
                    return Power.EaseOut(linearStep, 2);
                case EasingType.Cubic:
                    return Power.EaseOut(linearStep, 3);
                case EasingType.Quartic:
                    return Power.EaseOut(linearStep, 4);
                case EasingType.Quintic:
                    return Power.EaseOut(linearStep, 5);
            }
            throw new NotImplementedException();
        }

        public static double EaseInOut(double linearStep, EasingType easeInType, EasingType easeOutType) {
            return linearStep < 0.5 ? EaseInOut(linearStep, easeInType) : EaseInOut(linearStep, easeOutType);
        }

        public static double EaseInOut(double linearStep, EasingType type) {
            switch (type) {
                case EasingType.Step:
                    return linearStep < 0.5 ? 0 : 1;
                case EasingType.Linear:
                    return linearStep;
                case EasingType.Sine:
                    return Sine.EaseInOut(linearStep);
                case EasingType.Quadratic:
                    return Power.EaseInOut(linearStep, 2);
                case EasingType.Cubic:
                    return Power.EaseInOut(linearStep, 3);
                case EasingType.Quartic:
                    return Power.EaseInOut(linearStep, 4);
                case EasingType.Quintic:
                    return Power.EaseInOut(linearStep, 5);
            }
            throw new NotImplementedException();
        }

        static class Sine {
            public static double EaseIn(double s) {
                return Math.Sin(s * (Math.PI / 2) - (Math.PI / 2)) + 1;
            }
            public static double EaseOut(double s) {
                return Math.Sin(s * (Math.PI / 2));
            }
            public static double EaseInOut(double s) {
                return Math.Sin(s * Math.PI - (Math.PI / 2) + 1) / 2;
            }
        }

        static class Power {
            public static double EaseIn(double s, int power) {
                return Math.Pow(s, power);
            }
            public static double EaseOut(double s, int power) {
                var sign = power % 2 == 0 ? -1 : 1;
                return sign * (Math.Pow(s - 1, power) + sign);
            }
            public static double EaseInOut(double s, int power) {
                s *= 2;
                if (s < 1) {
                    return EaseIn(s, power) / 2;
                }
                var sign = power % 2 == 0 ? -1 : 1;
                return (sign / 2.0 * (Math.Pow(s - 2, power) + sign * 2));
            }
        }
    }

    public enum EasingType { Step, Linear, Sine, Quadratic, Cubic, Quartic, Quintic }

    public enum EasingMode { EaseIn, EaseOut, EaseInOut }
}