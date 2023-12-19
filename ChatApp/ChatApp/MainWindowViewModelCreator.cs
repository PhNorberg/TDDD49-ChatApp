using ChatApp.Model;
using ChatApp.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp
{
    class MainWindowViewModelCreator
    {
        public MainWindowViewModelCreator() { }
        public static MainWindowViewModel Create()
        {
            var _networkHandler = new NetworkHandler();
            return new MainWindowViewModel(_networkHandler);
        }
    }
}
