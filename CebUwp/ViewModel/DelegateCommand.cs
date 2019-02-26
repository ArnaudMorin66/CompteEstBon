using System;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace CompteEstBon
{
    public sealed class DelegateCommand<T> : ICommand
    {
        private readonly Action<T> _command;

        public DelegateCommand(Action<T> command)
        {
            _command = command;
        }

        public void Execute(object parameter)
        {
            _command((T)parameter);
        }

        bool ICommand.CanExecute(object parameter)
        {
            return true;
        }

        event EventHandler ICommand.CanExecuteChanged {
            add { }
            remove { }
        }
    }
}