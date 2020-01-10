using System;
using System.Linq;
using System.Windows.Input;
using HostBinder.Helpers;

namespace HostBinder.Commands
{
    internal class ClearSelectedTargetTagCommand : ICommand
    {
        private readonly ViewModels.MainWindowViewModel _viewModel;

        public ClearSelectedTargetTagCommand(ViewModels.MainWindowViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return (_viewModel.SelectedTargetTag!=null);
        }

        public void Execute(object parameter)
        {
            _viewModel.SelectedTargetTag = null;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
