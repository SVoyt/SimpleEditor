using System;
using System.Windows.Media.Imaging;
using MVVM;
using SimpleEditor.Extentions;
using SimpleEditor.Model;

namespace SimpleEditor.ViewModel
{
    /// <summary>
    /// View model for layer
    /// </summary>
    public class LayerViewModel:ViewModelBase
    {
        private readonly Layer _layer;
        private string _name;

        /// <summary>
        /// Gets image from layer
        /// </summary>
        public BitmapSource Image
        {
            get
            {
                return _layer.Bitmap.ToWpfBitmap();
            }
        }

        /// <summary>
        /// Gets or sets layer name
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            { 
                _name = value; 
                RaisePropertyChanged("Name");
            }
        }

        /// <summary>
        /// Gets or sets visibility of the layer
        /// </summary>
        public bool IsVisible
        {
            set { _layer.IsVisible = value; }
            get { return _layer.IsVisible; }
        }


        public LayerViewModel(Layer layer)
        {
            if (layer==null)
                throw new ArgumentNullException("layer");
            _layer = layer;
            _layer.LayerChanged += _layer_LayerChanged;
        }

        private void _layer_LayerChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged("Image");
        }
    }
}
