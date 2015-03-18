using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using MVVM;
using SimpleEditor.Model;

namespace SimpleEditor.ViewModel
{
    /// <summary>
    /// Main view logic class
    /// </summary>
    public class MainViewModel: ViewModelBase
    {
        #region constants

        private const string FileFilter = "Simple Editor files|*.se";
        private const string ExportFilter = "PNG|*.jpg";

        private const string SaveErrorMessage = "Error occurred on saving:\r\n{0}";
        private const string LoadErrorMessage = "Error occurred on opening:\r\n{0}";
        private const string ExportErrorMessage = "Error occurred on saving to PNG:\r\n{0}";

        #endregion

        public SceneViewModel SceneViewModel { get; private set; }
        public Size VisibleSize { get; set; }

        public ICommand NewCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand LoadCommand { get; private set; }
        public ICommand ExportCommand { get; private set; }

        public MainViewModel()
        {
            var scene = new Scene();
            scene.AddNewLayer();

            SceneViewModel = new SceneViewModel(scene);
            NewCommand = new RelayCommand(param=> New());
            LoadCommand = new RelayCommand(param=> Load());
            SaveCommand = new RelayCommand(param=> Save());
            ExportCommand = new RelayCommand(param=> Export());
        }

        #region private methods

        private void New()
        {
            var scene = new Scene();
            scene.AddNewLayer();
            LoadScene(scene);
        }

        private void LoadScene(Scene scene)
        {
            SceneViewModel.Scene = scene;
        }

        private void Load()
        {
            try
            {
                var ofd = new OpenFileDialog { Filter = FileFilter };
                if (ofd.ShowDialog().Value)
                {
                    LoadScene(Scene.Load(ofd.FileName));
                }
            }
            catch (Exception e)
            {
                Error(LoadErrorMessage, e);
            }
        }

        private void Save()
        {
            try
            {
                var sfd = new SaveFileDialog { Filter = FileFilter, AddExtension = true };
                if (sfd.ShowDialog().Value)
                {
                    SceneViewModel.Scene.Save(sfd.FileName);
                }
            }
            catch (Exception e)
            {
                Error(SaveErrorMessage, e);
            }
        }

        private void Export()
        {
            try
            {
                var sfd = new SaveFileDialog { Filter = ExportFilter, AddExtension = true };
                if (sfd.ShowDialog().Value)
                {
                    SceneViewModel.Scene.Export(sfd.FileName, new System.Drawing.Size((int)VisibleSize.Width, (int)VisibleSize.Height));
                }
            }
            catch (Exception e)
            {
                Error(ExportErrorMessage,e);
            }
        }

        private void Error(string message, Exception e)
        {
            MessageBox.Show(string.Format(message, e.Message));
        }

        #endregion

    }
}
