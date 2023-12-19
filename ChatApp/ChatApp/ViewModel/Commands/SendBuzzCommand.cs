using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChatApp.ViewModel.Commands
{
    class SendBuzzCommand : ICommand
    {
        ChatWindowViewModel _chatWindowViewModel;

        public SendBuzzCommand(ChatWindowViewModel chatWindowViewModel)
        {
            _chatWindowViewModel = chatWindowViewModel;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) {
            return true;
        }
        public void Execute(object? parameter)
        {
            _chatWindowViewModel.SendBuzz();
        }
    }
}
