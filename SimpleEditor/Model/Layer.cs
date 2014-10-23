using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;


namespace SimpleEditor.Model
{
    /// <summary>
    /// Layer class. Contains drawing logic
    /// </summary>
    public class Layer : IDisposable
    {
        #region private fields

        private bool _isVisible = true;
        private Point _position;
        private Size _size;
        private Bitmap _bitmap;
        private Bitmap _bufferBitmap;

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets visibility of the layer
        /// </summary>
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                _isVisible = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets layer position
        /// </summary>
        public Point Position
        {
            get { return _position; }
            set
            {
                _position = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets layer size
        /// </summary>
        public Size Size
        {
            get { return _size; }
            private set
            {
                _size = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets bitmap with already drawed figures, 
        /// does not contain now drawing figure
        /// </summary>
        public Bitmap Bitmap
        {
            get { return _bitmap; }
            private set
            {
                if (_bitmap!=null)
                    _bitmap.Dispose();
                _bitmap = value;
            }
        }

        /// <summary>
        /// Gets bitmap of now drawing figure
        /// </summary>
        public Bitmap BufferBitmap
        {
            get { return _bufferBitmap; }
            private set
            {
                if (_bufferBitmap != null)
                    _bufferBitmap.Dispose();
                _bufferBitmap = value;
            }
        }

        #endregion

        /// <summary>
        /// Fires when something changed in layer 
        /// </summary>
        public event EventHandler LayerChanged;

        #region constructors

        /// <summary>
        /// Creates layer with 1 on 1 canvas
        /// </summary>
        public Layer()
        {
            Size = new Size(1,1);
            Bitmap = new Bitmap(Size.Width, Size.Height, PixelFormat.Format32bppArgb);            
        }

        /// <summary>
        /// Creates layer from layer bundle
        /// </summary>
        /// <param name="layerBundle">layer bundle</param>
        public Layer(LayerBundle layerBundle)
        {
            if (layerBundle==null)
                throw new ArgumentNullException("layerBundle");
            Bitmap = layerBundle.Bitmap;
            Position = layerBundle.Position;
            Size = layerBundle.Size;
            IsVisible = layerBundle.IsVisible;
        }

#endregion

        #region public methods
        /// <summary>
        /// Changes poisition and size of layer
        /// </summary>
        /// <param name="newPosition">New position</param>
        /// <param name="newSize">New size</param>
        public void ChangeSizeAndPosition(Point newPosition, Size newSize)
        {
            var newBitmap = new Bitmap(newSize.Width,newSize.Height,PixelFormat.Format32bppArgb);

            using (var gr = Graphics.FromImage(newBitmap))
            {
                var dx = Position.X - newPosition.X;
                var dy = Position.Y - newPosition.Y;
                if (dx < 0)
                    dx = 0;
                if (dy < 0)
                    dy = 0;
                gr.DrawImage(Bitmap,dx,dy);
            }
            Bitmap = newBitmap;
            Position = newPosition;
            Size = newSize;
            Invalidate();
        }

        /// <summary>
        /// Draw lines by points
        /// </summary>
        /// <param name="pen">Pen to draw</param>
        /// <param name="points">Lines points</param>
        public void DrawLines(Pen pen, Point[] points)
        {
            if (pen == null)
                throw new ArgumentNullException("pen");

            BufferBitmap = new Bitmap(Bitmap.Width, Bitmap.Height, PixelFormat.Format32bppArgb);

            using (var gr = Graphics.FromImage(BufferBitmap))
            {
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.DrawLines(pen, points);
            }

            Invalidate();
        }

        /// <summary>
        /// Draw point 
        /// </summary>
        /// <param name="brush">Brush to draw</param>
        /// <param name="thickness">Diameter of point</param>
        /// <param name="p">Point</param>
        public void DrawPoint(Brush brush, float thickness, Point p)
        {
            if (brush == null)
                throw new ArgumentNullException("brush");

            BufferBitmap = new Bitmap(Bitmap.Width,Bitmap.Height,PixelFormat.Format32bppArgb);

            using (var gr = Graphics.FromImage(BufferBitmap))
            {
                gr.SmoothingMode = SmoothingMode.HighQuality;
                var rect = new RectangleF(p.X - thickness/2, p.Y - thickness/2, thickness, thickness);
                gr.FillEllipse(brush, rect);
            }

            Invalidate();
        }

        /// <summary>
        /// Applies now drawing figure to Bitmap
        /// </summary>
        public void Apply()
        {
            using (var gr = Graphics.FromImage(Bitmap))
            {
                gr.DrawImage(BufferBitmap,0,0);
            }
            BufferBitmap = null;
            Invalidate();
        }

        /// <summary>
        /// Offset layer
        /// </summary>
        /// <param name="dx">X offset (adds to x)</param>
        /// <param name="dy">Y offset (adds to y)</param>
        public void Offset(int dx, int dy)
        {
            _position.X += dx;
            _position.Y += dy;
            Invalidate();
        }

        /// <summary>
        /// Invalidating layer
        /// </summary>
        public void Invalidate()
        {
            if (LayerChanged!=null)
                LayerChanged(this,new EventArgs());
        }

        /// <summary>
        /// Disposing layer ( dispose bitmaps )
        /// </summary>
        public void Dispose()
        {
            Bitmap = null;
            BufferBitmap = null;
        }

        #endregion
    }
}
