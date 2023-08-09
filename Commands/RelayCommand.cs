using System;
using System.Windows.Input;

namespace File_Explorer.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _action;

        public RelayCommand(Action<object> action) => _action = action;

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object parameter) => _action(parameter);
    }
}
