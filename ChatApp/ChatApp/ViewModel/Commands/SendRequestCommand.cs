﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChatApp.ViewModel.Commands
{
    class SendRequestCommand : ICommand

    {
        MainWindowViewModel _mainWindowViewModel;
        
        public SendRequestCommand(MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;
        }

        public event EventHandler? CanExecuteChanged;

        
        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            _mainWindowViewModel.SendRequest();
        }
    }
}
