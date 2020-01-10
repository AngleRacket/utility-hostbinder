using System;
using System.Linq;
using System.Windows.Input;
using HostBinder.Helpers;

namespace HostBinder.Commands
{
    internal class SelectedHostItemBindingChangedCommand : ICommand
    {
        private readonly ViewModels.MainWindowViewModel _viewModel;

        public SelectedHostItemBindingChangedCommand(ViewModels.MainWindowViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is int)
            {
                var lineNumber = (int) parameter;
                var target = _viewModel.Targets.FirstOrDefault(t => t.LineNumber == lineNumber);
                if (target != null)
                {
                    var newHost = target.Address + "\t" + _viewModel.SelectedHostItem.Name +  "\t" +
                                  _viewModel.SelectedHostItem.Comment;
                    _viewModel.HostsFileContents = ParseTextHelper.ReplaceLine(_viewModel.HostsFileContents, _viewModel.SelectedHostItem.LineNumber, newHost);
                }
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
