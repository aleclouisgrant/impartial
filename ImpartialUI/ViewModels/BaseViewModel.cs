using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ImpartialUI.ViewModels
{
    public abstract class BaseViewModel : INotifyPropertyChanged, IDisposable
    {
        private Exception _exception;
        public Exception Exception
        {
            get
            {
                return _exception;
            }
            set
            {
                _exception = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        public void OnPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ClearException()
        {
            Exception = null;
        }

        public virtual void Dispose() { }
    }
}
