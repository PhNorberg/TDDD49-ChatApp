using ChatApp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChatApp.ViewModel.Commands
{
    class ViewChatHistoryCommand : ICommand
    {
        ChatWindowViewModel _chatWindowViewModel;

        public ViewChatHistoryCommand(ChatWindowViewModel chatWindowViewModel)
        {
            _chatWindowViewModel = chatWindowViewModel;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) {
            return true;
        }

        public void Execute(object? parameter)
        {
            if (parameter is ChatSummary chatSummary)
            {
                _chatWindowViewModel.LoadChatHistory(chatSummary);
            }
            
        }
    }
}
