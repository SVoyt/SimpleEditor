using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SimpleEditor.Extentions;
using SimpleEditor.Model;
using Point = System.Drawing.Point;

namespace SimpleEditor.Controls
{
    /// <summary>
    /// Control to draw Scene object
    /// </summary>
    public class DrawControl:Control
    {
        #region dependency properties

        /// <summary>
        /// Dependency property. Scene for drawing control
        /// </summary>
        public static readonly DependencyProperty SceneProperty =
            DependencyProperty.Register("Scene", typeof (Scene), typeof (DrawControl),
                new PropertyMetadata(new PropertyChangedCallback(SceneChanged)));

        /// <summary>
        /// Dependency property. Actual size of drawing
        /// </summary>
        public static readonly DependencyProperty ActualSizeProperty =
            DependencyProperty.Register("ActualSize", typeof (Size), typeof (DrawControl));

        #endregion

        #region public properties

        /// <summary>
        /// Scene for drawing control
        /// </summary>
        public Scene Scene
        {
            get
            {
                return (Scene)GetValue(SceneProperty);
            }
            set
            {
                SetValue(SceneProperty, value);
            }
        }

        /// <summary>
        /// Actual size of drawing
        /// </summary>
        public Size ActualSize
        {
            get
            {
                return (Size)GetValue(ActualSizeProperty);
            }
            set
            {
                SetValue(ActualSizeProperty, value);
            }
        }

        #endregion

        static void SceneChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue!=null)
                ((Scene)e.NewValue).SceneChanged += ((DrawControl)sender).InvalidateScene;
            ((DrawControl)sender).InvalidateVisual();
        }

        /// <summary>
        /// Invalidate scene
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void InvalidateScene(object sender, EventArgs e)
        {
            this.InvalidateVisual();
        }

        #region protected methods

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            var mousePos = e.GetPosition(this);
            Scene.PressDown(new Point((int)mousePos.X,(int)mousePos.Y));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var mousePos = e.GetPosition(this);
            Scene.Move(new Point((int)mousePos.X, (int)mousePos.Y));
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            var mousePos = e.GetPosition(this);
            Scene.PressUp(new Point((int)mousePos.X, (int)mousePos.Y));
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            var mousePos = e.GetPosition(this);
            Scene.PressUp(new Point((int)mousePos.X, (int)mousePos.Y));
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if ((e.Property == ActualHeightProperty) | (e.Property == ActualWidthProperty))
            {
                ActualSize = new Size(ActualWidth,ActualHeight);
            }
        }

        /// <summary>
        /// Draw scene bitmap in context
        /// </summary>
        /// <param name="drawingContext">Drawing context</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);          
            drawingContext.DrawRectangle(new SolidColorBrush(Colors.White), null, new Rect(0, 0, this.ActualWidth, this.ActualHeight));
            if (Scene == null)
                return;
            
            using (var resultBitmap = Scene.DrawToBitmap((int) ActualWidth, (int) ActualHeight))
            {
                drawingContext.DrawImage(resultBitmap.ToWpfBitmap(),new Rect(0,0,ActualWidth,ActualHeight));
            }
        }

        #endregion

    }
}
