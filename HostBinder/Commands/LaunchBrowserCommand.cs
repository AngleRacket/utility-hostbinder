using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using HostBinder.Helpers;

namespace HostBinder.Commands
{
    internal class LaunchBrowserCommand : ICommand
    {
        private readonly ViewModels.MainWindowViewModel _viewModel;

        public LaunchBrowserCommand(ViewModels.MainWindowViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return (_viewModel.SelectedHostItem!=null);
        }

        public void Execute(object parameter)
        {
            var browserName = parameter as string;

            var browser = _viewModel.Browsers.FirstOrDefault(b => b.Name == browserName);

            if (browser != null)
            {
                var command = browser.Command;
                var url = "\"http://"+_viewModel.SelectedHostItem.Name+"/\"";

                Process.Start(command, url);
            }
            
            _viewModel.SelectedTargetTag = null;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
