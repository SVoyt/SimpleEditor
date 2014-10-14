using System.ComponentModel;

namespace MVVM
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        //basic ViewModelBase
        internal void RaisePropertyChanged(string prop)
        {
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(prop)); }
        }
        public event PropertyChangedEventHandler PropertyChanged;

    }
}
