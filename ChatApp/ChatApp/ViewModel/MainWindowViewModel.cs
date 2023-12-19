using ChatApp.ViewModel.Commands;
using ChatApp.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.Net;
using ChatApp.View;

namespace ChatApp.ViewModel
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        private string _username;
        private string _your_portnumber;
        private string _your_friends_portnumber;
        private string _your_ip;
        private string _errorMessage;
        private Visibility _errorMessageVisibility = Visibility.Collapsed;
        public Action CloseAction { get; set; }
        private NetworkHandler _networkHandler;
        public StartListeningCommand StartListeningCommand { get; private set; }
        public SendRequestCommand SendRequestCommand { get; private set; }

        public MainWindowViewModel(NetworkHandler networkHandler)
        {
            _networkHandler = networkHandler;
            _networkHandler.NetworkEvent += OnNetworkEvent;
            StartListeningCommand = new StartListeningCommand(this);
            SendRequestCommand = new SendRequestCommand(this);
        }

        public string Username
        {
            get { return _username; }
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged(nameof(Username));
                }
            }
        }

        public string YourPortNumber
        {
            get { return _your_portnumber; }
            set
            {
                if (_your_portnumber != value)
                {
                    _your_portnumber = value;
                    OnPropertyChanged(nameof(YourPortNumber));
                }
            }
        }

        public string YourFriendsPortNumber
        {
            get { return _your_friends_portnumber; }
            set
            {
                if (_your_friends_portnumber != value)
                {
                    _your_friends_portnumber = value;
                    OnPropertyChanged(nameof(YourFriendsPortNumber));
                }
            }
        }

        public string YourIp
        {
            get { return _your_ip; }
            set
            {
                if (_your_ip != value)
                {
                    _your_ip = value;
                    OnPropertyChanged(nameof(YourIp));
                }
            }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
                ErrorMessageVisibility = string.IsNullOrEmpty(_errorMessage) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public Visibility ErrorMessageVisibility
        {
            get { return _errorMessageVisibility; }
            set
            {
                if (_errorMessageVisibility != value)
                {
                    _errorMessageVisibility = value;
                    OnPropertyChanged(nameof(ErrorMessageVisibility));
                }
            }
        }

        private void CloseWindow()
        {
            CloseAction();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new
                PropertyChangedEventArgs(propertyName));
        }

        public async void StartListening()
        {
            ErrorMessage = string.Empty;

            var errors = ValidateInput();

            int port;
            bool isValidPort = int.TryParse(YourPortNumber, out port);
            if (string.IsNullOrWhiteSpace(YourPortNumber) || !isValidPort || port < 0 || port > 65535)
            {
                errors.Add("Please enter your valid portnumber.");
            }

            if (!errors.Any())
            {
                bool _connectionSuccess = await _networkHandler.StartServer(IPAddress.Parse(YourIp), port);

                if (_connectionSuccess)
                {
                    OpenChatWindow(true);
                }
                else
                {
                    ErrorMessage = "Portnumber busy, try another";
                }

            }
            else
            {
                ErrorMessage = string.Join(Environment.NewLine, errors);
            }
        }

        public async void SendRequest()
        {
            ErrorMessage = string.Empty;

            var errors = ValidateInput();

            int port;
            bool isValidPort = int.TryParse(YourFriendsPortNumber, out port);

            if (string.IsNullOrWhiteSpace(YourFriendsPortNumber) || !isValidPort || port < 0 || port > 65535)
            {
                errors.Add("Please enter your friends valid portnumber.");
            }

            if (!errors.Any())
            {
                bool _connectionSuccess = await _networkHandler.StartClient(IPAddress.Parse(YourIp), port);
                if (_connectionSuccess)
                {
                    var data = new Data
                    {
                        Message = "",
                        RequestType = RequestType.None,
                        Name = _username,
                        Date = DateTime.Now

                    };
                    System.Diagnostics.Debug.WriteLine("1: Client sending data...ofc this works");
                    await _networkHandler.SendMessage(data);
                }
                else
                {
                    ErrorMessage = "Port number not in use, try another";
                }

            }
            else
            {
                ErrorMessage = string.Join(Environment.NewLine, errors);
            }
        }


        private void OpenChatWindow(bool isServer, string friendUsername = "")
        {
            Application.Current.Dispatcher.Invoke(() => ShowChatWindow(isServer, friendUsername));
        }

        private void ShowChatWindow(bool isServer, string friendUsername) // We're coming from the UI thread when we're the server, but from a background thread when we're the "client"
        {
            _networkHandler.NetworkEvent -= OnNetworkEvent;
            ChatRole chatRole = isServer ? ChatRole.Server : ChatRole.Client;
            var chatViewModel = ChatWindowViewModelCreator.Create(_networkHandler, new ChatHistoryHandler(), chatRole, _username, friendUsername);
            var chatWindow = new ChatWindow
            {
                DataContext = chatViewModel
            };
            chatWindow.Show();
            CloseWindow();
        }

        private List<string> ValidateInput()
        {

            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(Username))
            {
                errors.Add("Please enter a username.");
            }

            IPAddress _ip;
            bool isValidIp = IPAddress.TryParse(YourIp, out _ip); // xxx.xxx.xxx.xxx 0-255

            if (string.IsNullOrWhiteSpace(YourIp) || !isValidIp)
            {
                errors.Add("Please enter valid IP.");
            }

            return errors;
        }

        private void OnNetworkEvent(Data data)
        {
            if (data.RequestType == RequestType.ConnectionRequestAccepted)
            {
                System.Diagnostics.Debug.WriteLine("Opening chat window from client side...");
                OpenChatWindow(false, data.Name); // Pass the name of the server we just got accepted to chat with
            }
            else if (data.RequestType == RequestType.ConnectionRequestDenied)
            {
                ErrorMessage = "Friend denied your chat request!";
                _networkHandler.CloseConnection();
            }
        }
    }
}
