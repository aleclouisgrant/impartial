using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace ImpartialUI.Commands
{
    public class SelectPathCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public SelectPathCommand()
        {
            
        }

        public void Execute(object parameter)
        {
            throw new NotImplementedException();
        }
    }
}
