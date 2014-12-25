using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using SimpleEditor.Extentions;
using Brush = System.Drawing.Brush;
using Color = System.Drawing.Color;
using Pen = System.Drawing.Pen;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Point = System.Drawing.Point;

namespace SimpleEditor.Model
{
    
    /// <summary>
    /// Scene manage layer logic. 
    /// Inherited from IDisposable, because has Pen and Brush (GDI unmanaged objects) and layers inside has unmanaged resources
    /// </summary>
    public class Scene:IDisposable
    {
        #region private fields

        private bool _pressed;
        private Point _lastPoint;
        private int _selectedLayerIndex;
        private List<Point> _points;
        private Pen _pen;
        private Brush _brush;
        private int _thickness;
        private Color _color;

        #endregion

        private Layer SelectedLayer
        {
            get
            {
                if (SelectedLayerIndex == -1)
                    return null;

                return Layers[SelectedLayerIndex];
            }
        }

        #region public properties

        /// <summary>
        /// Gets or sets current brush color
        /// </summary>
        public Color Color
        {
            get
            {
                return _color;
            }
            set
            {
                _color = value;
                InvalidateBrushAndPen();
            }
        }

        /// <summary>
        /// Gets or sets current pen thickness (width)
        /// </summary>
        public int Thickness
        {
            get
            {
                return _thickness;
            }
            set
            {
                _thickness = value;
                InvalidateBrushAndPen();
            }
        }

        /// <summary>
        /// Gets or sets current selected layer index,
        /// if scene has layers, selected index should be positive, 
        /// if has no layers SelectedLayerIndex=-1
        /// </summary>
        public int SelectedLayerIndex
        {
            get { return _selectedLayerIndex; }
            set
            {
                if (HasNoLayers)
                    return;
                if (value<0)
                    throw new ArgumentException("SelectedLayerIndex must be greater then 0");
                if (value>= Layers.Count)
                    throw new ArgumentOutOfRangeException("SelectedLayerIndex must be < layers array count");

                _selectedLayerIndex = value;
            }
        }

        /// <summary>
        /// Gets layer list
        /// </summary>
        public List<Layer> Layers { get; private set; }

        /// <summary>
        /// Returns true if scene has no layers
        /// </summary>
        public bool HasNoLayers
        {
            get { return Layers.Count == 0; }
        }

        /// <summary>
        /// Gets or sets value
        /// </summary>
        public bool PanMode
        {
            get;
            set;
        }

        #endregion

        #region public events

        /// <summary>
        /// Fires if something changed in scene
        /// </summary>
        public event EventHandler SceneChanged;

        /// <summary>
        /// Fires when order of layers is changed (moving layer up\down, deleting)
        /// </summary>
        public event EventHandler LayersOrderChanged;

        #endregion

        /// <summary>
        /// Contructor
        /// </summary>
        public Scene()
        {
            _points = new List<Point>();
            _selectedLayerIndex = -1;

            Layers = new List<Layer>();        
            Color = Color.Black;
            Thickness = 5;     
        }

        #region public methods

        /// <summary>
        /// Save current scene to file
        /// </summary>
        /// <param name="filename">Filename</param>
        public void Save(string filename)
        {
            var lst = Layers.Select(layer => new LayerBundle()
            {
                Bitmap = layer.Bitmap, 
                Position = layer.Position, 
                Size = layer.Size,
                IsVisible = layer.IsVisible
            }).ToList();
            var dcs = new DataContractSerializer(typeof(List<LayerBundle>));
            dcs.WriteObject(File.Create(filename), lst);
        }

        /// <summary>
        /// Export scene to PNG
        /// </summary>
        /// <param name="filename">Filename</param>
        /// <param name="size">Canvas size</param>
        public void Export(string filename, Size size)
        {
            var bmp = DrawToBitmap(size.Width, size.Height);           
            bmp.Save(filename,ImageFormat.Png);
        }

        /// <summary>
        /// Load scene from file
        /// </summary>
        /// <param name="filename">Filename</param>
        /// <returns>Loaded scene</returns>
        public static Scene Load(string filename)
        {
            var dcs = new DataContractSerializer(typeof(List<LayerBundle>));
            var layerBundles = (List<LayerBundle>)dcs.ReadObject(File.OpenRead(filename));
            var scene = new Scene();
            scene.LayersFromBundle(layerBundles);
          
            return scene;
        }

        /// <summary>
        /// Draw scene to bitmap
        /// </summary>
        /// <param name="sceneWidth">Width of bitmap</param>
        /// <param name="sceneHeight">Height of bitmap</param>
        /// <returns>Bitmap with drawed scene</returns>
        public Bitmap DrawToBitmap(int sceneWidth,int sceneHeight)
        {
            var bitmap = new Bitmap(sceneWidth, sceneHeight, PixelFormat.Format32bppArgb);
            using (var gr = Graphics.FromImage(bitmap))
            {
                gr.Clear(Color.White);
                foreach (var layer in Layers)
                {
                    if (layer.IsVisible)
                    {
                        gr.DrawImage(layer.Bitmap, layer.Position);
                        if (layer.BufferBitmap != null)
                            gr.DrawImage(layer.BufferBitmap, layer.Position);
                    }
                }
            }
            return bitmap;
        }

        /// <summary>
        /// Adding new layer to scene
        /// </summary>
        public void AddNewLayer()
        {
            var layer = new Layer();
            layer.LayerChanged += layer_LayerChanged;
            Layers.Add(layer);
            if (Layers.Count == 1)
                SelectedLayerIndex = 0;
            Invalidate();
            InvalidateLayersOrder();
        }

        /// <summary>
        /// Return true if layer can be deleted
        /// </summary>
        /// <returns></returns>
        public bool CanRemoveSelectedLayer()
        {
            return SelectedLayerIndex != -1;
        }

        /// <summary>
        /// Remove selected layer from scene
        /// </summary>
        public void RemoveSelectedLayer()
        {
            if (HasNoLayers)
                return;
            SelectedLayer.Dispose();
            Layers.RemoveAt(SelectedLayerIndex);
            if (HasNoLayers)
                _selectedLayerIndex = -1;
            else
                _selectedLayerIndex = 0;

            Invalidate();
            InvalidateLayersOrder();
        }

        /// <summary>
        /// Returns true if layer can be moved up
        /// </summary>
        /// <returns></returns>
        public bool CanMoveSelectedLayerUp()
        {
            return SelectedLayerIndex > 0;
        }

        /// <summary>
        /// Move selected layer up in layer list (move up in drawing order, move down in canvas )
        /// </summary>
        public void MoveSelectedLayerUp()
        {
            if (HasNoLayers)
                return;

            if (SelectedLayerIndex == 0)
                return;
            var layer = SelectedLayer;

            Layers.RemoveAt(SelectedLayerIndex);
            Layers.Insert(SelectedLayerIndex-1,layer);
            SelectedLayerIndex--;

            Invalidate();
            InvalidateLayersOrder();
        }

        /// <summary>
        /// Returns true if layer can be moved down
        /// </summary>
        /// <returns></returns>
        public bool CanMoveSelectedLayerDown()
        {
            return SelectedLayerIndex < Layers.Count - 1;
        }

        /// <summary>
        /// Move selected layer down in layer list (move down in drawing order, move up in canvas )
        /// </summary>
        public void MoveSelectedLayerDown()
        {
            if (HasNoLayers)
                return;

            if (SelectedLayerIndex==Layers.Count-1)
                return;

            var layer = SelectedLayer;

            Layers.RemoveAt(SelectedLayerIndex);
            Layers.Insert(SelectedLayerIndex+1, layer);
            SelectedLayerIndex++;
            Invalidate();
            InvalidateLayersOrder();
        }

        /// <summary>
        /// Start user interaction logic, like MouseDown on control
        /// If not pan mode draw point
        /// </summary>
        /// <param name="p">Start point</param>
        public void PressDown(Point p)
        {
            if ((HasNoLayers)||(!SelectedLayer.IsVisible))
                return;

            _pressed = true;
            _lastPoint = p;

            if (!PanMode)
            {
                CheckLayerPostionAndSize(new[] {new Point(p.X, p.Y)});
                var normalized = p.Normalize(SelectedLayer.Position);
                SelectedLayer.DrawPoint(_brush, Thickness, normalized);
                _points = new List<Point>();
                _points.Add(p);
            }          
        }

        /// <summary>
        /// Draw line or move layer (depends on pan mode)
        /// </summary>
        /// <param name="p">Point to move</param>
        public void Move(Point p)
        {
            if ((((HasNoLayers) | (!_pressed))) || (!SelectedLayer.IsVisible))
                return;

            if (PanMode)
            {
                var dx = p.X - _lastPoint.X ;
                var dy = p.Y - _lastPoint.Y;
                SelectedLayer.Offset(dx, dy);
            }
            else
            {
                CheckLayerPostionAndSize(new[] { new Point(p.X, p.Y), new Point(_lastPoint.X,_lastPoint.Y)  });
                _points.Add(p);
                SelectedLayer.DrawLines(_pen,_points.Select(c=>c.Normalize(SelectedLayer.Position)).ToArray());
            }

            _lastPoint = p;
        }

        /// <summary>
        /// Ends user intercation logic, like mouseup
        /// </summary>
        /// <param name="p">Last point</param>
        public void PressUp(Point p)
        {
            if ((((HasNoLayers)|(!_pressed))) || (!SelectedLayer.IsVisible))
                return;

            _pressed = false;

            if (PanMode)
                return;
            SelectedLayer.Apply();
            _points.Clear();
        }

        /// <summary>
        /// Dispose scene
        /// </summary>
        public void Dispose()
        {
            _pen.Dispose();
            _brush.Dispose();
            foreach (var layer in Layers)
            {
                layer.Dispose();
            }
        }

        #endregion

        #region private methods
        /// <summary>
        /// Add layers from bundles
        /// </summary>
        /// <param name="layerBundles">Collection of layer bundles</param>
        private void LayersFromBundle(IEnumerable<LayerBundle> layerBundles)
        {
            foreach (var layerBundle in layerBundles)
            {
                var layer = new Layer(layerBundle);
                layer.LayerChanged += layer_LayerChanged;
                Layers.Add(layer);
            }
            if (Layers.Count > 0)
                SelectedLayerIndex = 0;
        }

        /// <summary>
        /// Checks that drawed point or line in layer bounds, 
        /// if not increase bounds to contains drawing
        /// </summary>
        /// <param name="points">Points of drawing</param>
        private void CheckLayerPostionAndSize(Point[] points)
        {
            if (HasNoLayers)
                return;

            var needToChangeSize = false;
            var halfThickness = Thickness/2;
            var minX = points.Min(c => c.X) - halfThickness;
            var minY = points.Min(c => c.Y) - halfThickness;
            var maxX = points.Max(c => c.X) + halfThickness;
            var maxY = points.Max(c => c.Y) + halfThickness;

            var newPosition = SelectedLayer.Position;
            var newSize = SelectedLayer.Size;

            if (minX < SelectedLayer.Position.X)
            {
                newPosition.X = minX;
                needToChangeSize = true;
            }
            if (minY < SelectedLayer.Position.Y)
            {
                newPosition.Y = minY;
                needToChangeSize = true;
            }

            if (maxX >= SelectedLayer.Position.X + SelectedLayer.Size.Width)
            {
                newSize.Width = maxX - newPosition.X;
                needToChangeSize = true;
            }

            if (maxY >= SelectedLayer.Position.Y + SelectedLayer.Size.Height)
            {
                newSize.Height = maxY - newPosition.Y;
                needToChangeSize = true;
            }

            if (needToChangeSize)
            {
                var dx = SelectedLayer.Position.X - newPosition.X;
                var dy = SelectedLayer.Position.Y - newPosition.Y;
                newSize.Width += dx;
                newSize.Height += dy;
                SelectedLayer.ChangeSizeAndPosition(newPosition, newSize);
            }
        }

        private void layer_LayerChanged(object sender, System.EventArgs e)
        {
            Invalidate();
        }

        private void Invalidate()
        {
            if (SceneChanged != null)
                SceneChanged(this, new EventArgs());
        }

        private void InvalidateLayersOrder()
        {
            if (LayersOrderChanged!=null)
                LayersOrderChanged(this,new EventArgs());
        }

        private void InvalidateBrushAndPen()
        {
            if(_pen!=null)
                _pen.Dispose();
            if (_brush!=null)
                _brush.Dispose();

            _brush = new SolidBrush(_color);
            _pen = new Pen(_brush,Thickness)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round            
            };
        }
        #endregion 

    }
}
