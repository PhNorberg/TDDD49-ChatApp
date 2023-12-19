using ChatApp.Model;
using ChatApp.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp
{
    class ChatWindowViewModelCreator
    {
        public ChatWindowViewModelCreator() { }
        public static ChatWindowViewModel Create(NetworkHandler networkHandler, ChatHistoryHandler chatHistoryHandler, ChatRole chatrole, String username, String friendUsername)
        {

            return new ChatWindowViewModel(networkHandler, chatHistoryHandler, chatrole, username, friendUsername);
        }
    }
}
