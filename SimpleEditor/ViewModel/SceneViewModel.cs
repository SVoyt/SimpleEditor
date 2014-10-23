using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using MVVM;
using SimpleEditor.Model;

namespace SimpleEditor.ViewModel
{
    /// <summary>
    /// View model for scene class
    /// </summary>
    public class SceneViewModel:ViewModelBase
    {
        private Scene _scene;

        #region commands

        /// <summary>
        /// Gets command to add new layer to scene
        /// </summary>
        public ICommand AddLayerCommand { get; private set; }

        /// <summary>
        /// Gets command to remove layer from scene
        /// </summary>
        public ICommand RemoveLayerCommand { get; private set; }

        /// <summary>
        /// Gets command to move up layer in layer list
        /// </summary>
        public ICommand UpLayerCommand { get; private set; }

        /// <summary>
        /// Gets command to move down layer in layer list
        /// </summary>
        public ICommand DownLayerCommand { get; private set; }

        #endregion

        #region public properties

        /// <summary>
        /// Gets layers viewmodel observable collection
        /// </summary>
        public ObservableCollection<LayerViewModel> Layers { get; private set; }

        /// <summary>
        /// Gets or sets selected layer index
        /// </summary>
        public int SelectedLayerIndex
        {
            get { return Scene.SelectedLayerIndex; }
            set
            { 
                if (value == -1)
                    return;
                Scene.SelectedLayerIndex = value;
                RaisePropertyChanged("SelectedLayerIndex");
                RaisePropertyChanged("SelectedLayerViewModel");
            }
        }

        /// <summary>
        /// Gets selected layer view model
        /// </summary>
        public LayerViewModel SelectedLayerViewModel
        {
            get
            {
                return Layers[SelectedLayerIndex];
            }
        }


        /// <summary>
        /// Gets or sets color for scene brush
        /// </summary>
        public Color Color
        {
            get { return Color.FromRgb(Scene.Color.R, Scene.Color.G, Scene.Color.B); ; }
            set
            {
                Scene.Color = System.Drawing.Color.FromArgb(value.R, value.G, value.B);
                RaisePropertyChanged("Color");
                RaisePropertyChanged("ColorString");
            }
        }

        /// <summary>
        /// Gets color in string format
        /// </summary>
        public string ColorString
        {
            get
            {
                var cc = new ColorConverter();
                return cc.ConvertToString(Color);
            }
        }

        /// <summary>
        /// Gets or sets pen thickness
        /// </summary>
        public int Thickness
        {
            get { return Scene.Thickness; }
            set
            {
                Scene.Thickness = value;
                RaisePropertyChanged("Thickness");
            }
        }

        /// <summary>
        /// Gets or sets true if pan mode is on
        /// </summary>
        public bool PanMode
        {
            get { return Scene.PanMode; }
            set
            {
                Scene.PanMode = value;

                RaisePropertyChanged("PanMode");
            }
        }

        /// <summary>
        /// Scene
        /// </summary>
        public Scene Scene
        {
            get { return _scene; }
            set
            {
                if (value==null)
                    throw new ArgumentNullException("Scene can not be null");
                if (_scene!=null)
                    _scene.Dispose();
                _scene = value;
                _scene.LayersOrderChanged += _scene_LayersOrderChanged;
                InvalidateLayerList();
                RaisePropertyChanged("Scene");
            }
        }


        #endregion

        public SceneViewModel(Scene scene)
        {
            Layers = new ObservableCollection<LayerViewModel>();
            Scene = scene;
            Color = Color.FromRgb(Scene.Color.R, Scene.Color.G, Scene.Color.B);
            Thickness = Scene.Thickness;
          
            AddLayerCommand = new RelayCommand(param => Scene.AddNewLayer() );
            RemoveLayerCommand = new RelayCommand(param => Scene.RemoveSelectedLayer(),param=> Scene.CanRemoveSelectedLayer());
            UpLayerCommand = new RelayCommand(param => Scene.MoveSelectedLayerUp(), param => Scene.CanMoveSelectedLayerUp());
            DownLayerCommand = new RelayCommand(param => Scene.MoveSelectedLayerDown(), param=>Scene.CanMoveSelectedLayerDown());
        }


        private void _scene_LayersOrderChanged(object sender, EventArgs e)
        {
            InvalidateLayerList();   
        }

        private void InvalidateLayerList()
        {
            Layers.Clear();
            for (var i = 0; i < Scene.Layers.Count; i++)
            {
                Layers.Add(new LayerViewModel(Scene.Layers[i]) { Name = String.Format("Layer {0}", i+1) });
            }

            SelectedLayerIndex = Scene.SelectedLayerIndex;
        }

    }
}
