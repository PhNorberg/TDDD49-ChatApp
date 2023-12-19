using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using ChatApp.ViewModel;

namespace ChatApp.View
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        public ChatWindow()
        {
            InitializeComponent();
            
            this.Loaded += Window1_Loaded;
        }

        private void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ChatWindowViewModel viewModel)
            {
                viewModel.RequestBuzz += Buzz;
            }
        }

        private async void Buzz()
        {
            var left = this.Left;
            var top = this.Top;
            var random = new Random();

            for (var i = 0; i < 100; i++)
            {
                this.Left = left + random.Next(-10, 10);
                this.Top = top + random.Next(-10, 10);

                await Task.Delay(5);
            }

            this.Left = left;
            this.Top = top;
        }
        private void ChatWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is ChatWindowViewModel viewModel)
            {
                viewModel.HandleClosing();
                viewModel.RequestBuzz -= Buzz;
            }
        }
    }
}
