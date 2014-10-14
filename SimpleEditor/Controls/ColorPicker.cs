using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SimpleEditor.Controls
{
    /// <summary>
    /// Color picker control. Draws line of colors (ColorCollection)
    /// </summary>
    public class ColorPicker : Control
    {
        private static readonly Color[] DEFAULT_COLORS =
        {
            Colors.Black, Colors.Blue, Colors.Yellow, Colors.Red,
            Colors.Cyan, Colors.Green, Colors.White
        };

        private bool _pressed;

        #region dependency properties

        /// <summary>
        /// Dependency property. Colors for color line
        /// </summary>
        public static readonly DependencyProperty ColorCollectionProperty =
            DependencyProperty.Register(
                "ColorCollection",
                typeof(IEnumerable<Color>),
                typeof(ColorPicker),
                new FrameworkPropertyMetadata(DEFAULT_COLORS)
                );

        /// <summary>
        /// Dependency property. Selected by user color
        /// </summary>
        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register(
                "SelectedColor",
                typeof(Color),
                typeof(ColorPicker),
                new FrameworkPropertyMetadata(Colors.Black)
                );

        #endregion

        #region public properties

        /// <summary>
        /// Colors for color line
        /// </summary>
        public IEnumerable<Color> ColorCollection
        {
            get
            {
                return (IEnumerable<Color>)GetValue(ColorCollectionProperty);
            }
            set
            {
                SetValue(ColorCollectionProperty, value);
            }
        }

        /// <summary>
        /// Selected by user color
        /// </summary>
        public Color SelectedColor
        {
            get
            {
                return (Color)GetValue(SelectedColorProperty);
            }
            set
            {
                SetValue(SelectedColorProperty, value);
            }
        }

        #endregion

        #region protected methods

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            MouseEvent(e.GetPosition(this));
            _pressed = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_pressed)
                MouseEvent(e.GetPosition(this));
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            _pressed = false;
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            _pressed = false;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            var colorArray = ColorCollection.ToArray();
            for (var i = 0; i < ActualWidth; i++)
            {
                var upperPen = new Pen(new SolidColorBrush(GetColorByPos(i, 0, (int)ActualWidth, colorArray)), 1);
                drawingContext.DrawLine(upperPen, new Point(i, 0), new Point(i, (int)ActualHeight / 2));
                var lowerPen = new Pen(
                    new SolidColorBrush(colorArray[GetColorNumber(i, (int)ActualWidth, colorArray)]), 1);
                drawingContext.DrawLine(lowerPen, new Point(i, (int)ActualHeight / 2), new Point(i, (int)ActualHeight));
            }
        }

        #endregion

        #region private methods

        private void MouseEvent(Point p)
        {
            var col = GetColorByPos((int)p.X, (int)p.Y, (int)ActualWidth, ColorCollection.ToArray());
            if (!col.Equals(SelectedColor))
            {
                SelectedColor = col;
            }
        }

        private int GetColorNumber(int pos, int width, Color[] colorArray)
        {
            var sectionWidth = (double)width / (colorArray.Length - 1);
            return (int)(pos / sectionWidth);
        }

        private Color GetColorByPos(int posX, int posY, int width, Color[] colorArray)
        {
            var sectionWidth = (double)width / (colorArray.Length - 1);
            var colorNumber = (int)((posX) / sectionWidth);
            var delta = ((colorNumber + 1) * sectionWidth - posX) / (sectionWidth);
            if ((posY > ActualHeight / 2) | (colorNumber == colorArray.Length - 1))
                return colorArray[GetColorNumber(posX, width, colorArray)];
            else
                return GetColorByDelta(colorArray[colorNumber], colorArray[colorNumber + 1], delta);
        }

        private Color GetColorByDelta(Color colorA, Color colorB, double delta)
        {
            var r = (byte)(colorA.R * (delta) + colorB.R * (1 - delta));
            var g = (byte)(colorA.G * (delta) + colorB.G * (1 - delta));
            var b = (byte)(colorA.B * (delta) + colorB.B * (1 - delta));
            return Color.FromRgb(r, g, b);
        }

        #endregion
    }
}
