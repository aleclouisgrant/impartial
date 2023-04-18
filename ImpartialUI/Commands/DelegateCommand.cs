using System;
using System.Windows.Input;

namespace ImpartialUI.Commands
{
    public class DelegateCommand : ICommand
    {
        readonly Action<object> _execute;
        private bool _canExecute;

        public DelegateCommand(Action<object> OnExecute, bool CanExecute = true)
        {
            _execute = OnExecute;
            _canExecute = CanExecute;
        }

        public DelegateCommand(Action OnExecute, bool CanExecute = true)
        {
            _execute = O => OnExecute?.Invoke();
            _canExecute = CanExecute;
        }

        public bool CanExecute(object Parameter)
        {
            //if (Parameter == null)
            //    Parameter = _canExecute;

            if (Parameter is bool)
                _canExecute = (bool)Parameter;

            return _canExecute;
        }

        public void Execute(object Parameter) => _execute?.Invoke(Parameter);

        public void RaiseCanExecuteChanged(bool CanExecute)
        {
            if (_canExecute != CanExecute)
            {
                _canExecute = CanExecute;
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                });
            }
        }
        public event EventHandler CanExecuteChanged;
    }
}
