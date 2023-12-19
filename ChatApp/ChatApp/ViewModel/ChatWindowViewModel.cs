using ChatApp.Model;
using ChatApp.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ChatApp.ViewModel
{
    class ChatWindowViewModel : INotifyPropertyChanged
    {
        private string _infoMessage;
        private string _username;
        private string _friendUsername;
        private string _messageText;
        private string _searchText;
        private ObservableCollection<Data> _activeChatMessages;
        private ObservableCollection<ChatSummary> _historicChatSummary;
        private ObservableCollection<ChatSummary> _filteredHistoricChatSummary; // this to be bound to view
        private NetworkHandler _networkHandler;
        private ChatHistoryHandler _chatHistoryHandler;
        private ChatRole _chatRole;
        private Visibility _isLiveChatVisibility;
        private Visibility _incomingRequestVisibility = Visibility.Collapsed;
        private Visibility _disconnectButtonVisibility = Visibility.Collapsed;
        public AcceptRequestCommand AcceptRequestCommand { get; set; }
        public DenyRequestCommand DenyRequestCommand { get; set; }
        public SendMessageCommand SendMessageCommand { get; set; }
        public ViewChatHistoryCommand ViewChatHistoryCommand { get; set; }
        public SendBuzzCommand SendBuzzCommand { get; set; }
        public event Action RequestBuzz;
        public ChatWindowViewModel(NetworkHandler networkHandler, ChatHistoryHandler chatHistoryHandler, ChatRole chatRole, String username, String friendUsername)
        {
            _networkHandler = networkHandler;
            _networkHandler.ClientConnected += OnClientConnected; // Subscribe to event Action in networkhandler
            _networkHandler.NetworkEvent += OnNetworkEvent;
            _chatHistoryHandler = chatHistoryHandler;
            _chatRole = chatRole;
            _username = username;
            _friendUsername = friendUsername;
            AcceptRequestCommand = new AcceptRequestCommand(this);
            DenyRequestCommand = new DenyRequestCommand(this);
            SendMessageCommand = new SendMessageCommand(this);
            ViewChatHistoryCommand = new ViewChatHistoryCommand(this);
            SendBuzzCommand = new SendBuzzCommand(this);
            IsLiveChatVisibility = _chatRole == ChatRole.Server ? Visibility.Collapsed : Visibility.Visible;
            _activeChatMessages = new ObservableCollection<Data>();
            _historicChatSummary = new ObservableCollection<ChatSummary>();
            StartupLogic();

        }

        public string InfoMessage
        {
            get { return _infoMessage; }
            set
            {
                if (_infoMessage != value)
                {
                    _infoMessage = value;
                    OnPropertyChanged(nameof(InfoMessage));
                }
            }
        }

        public string Username
        {
            get { return _username; }
        }

        public string MessageText
        {
            get { return _messageText; }
            set
            {
                if (_messageText != value)
                {
                    _messageText = value;
                    OnPropertyChanged(nameof(MessageText));
                }
            }
        }

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                    FilterHistoricChats();
                }
            }
        }

        public ObservableCollection<Data> ActiveChatMessages
        {
            get { return _activeChatMessages; }
            set
            {
                _activeChatMessages = value;
                OnPropertyChanged(nameof(ActiveChatMessages));
            }
        }

        public ObservableCollection<ChatSummary> HistoricChatSummary
        {
            get { return _historicChatSummary; }
        }

        public ObservableCollection<ChatSummary> FilteredHistoricChatSummary
        {
            get { return _filteredHistoricChatSummary; }

        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Visibility IncomingRequestVisibility
        {
            get { return _incomingRequestVisibility; }
            set
            {
                if (_incomingRequestVisibility != value)
                {
                    _incomingRequestVisibility = value;
                    OnPropertyChanged(nameof(IncomingRequestVisibility));
                }
            }
        }

        public Visibility IsLiveChatVisibility
        {
            get { return _isLiveChatVisibility; }
            set
            {
                if (_isLiveChatVisibility != value)
                {
                    _isLiveChatVisibility = value;
                    OnPropertyChanged(nameof(IsLiveChatVisibility));
                }
            }
        }

        private void StartupLogic()
        {
            LoadHistoricalChats();

            if (_chatRole == ChatRole.Server)
            {
                IncomingRequestVisibility = Visibility.Collapsed;
                InfoMessage = "Listening for client requests...";
                _networkHandler.StartListeningForClients();
            }
            else if (_chatRole == ChatRole.Client)
            {
                InfoMessage = "You're chatting with " + _friendUsername + ". To disconnect, click a historic chat or exit the window.";
            }
        }

        private void OnClientConnected(string name)
        {
            _friendUsername = name;
            InfoMessage = "Client connection request from..." + _friendUsername;
            IncomingRequestVisibility = Visibility.Visible;
        }

        public async Task AcceptRequest()
        {
            var data = new Data
            {
                Message = "",
                RequestType = RequestType.ConnectionRequestAccepted,
                Name = _username,
                Date = DateTime.Now,
            };

            if (_activeChatMessages.Count > 0)
            {
                _activeChatMessages.Clear();
            }

            IsLiveChatVisibility = Visibility.Visible;
            await _networkHandler.SendMessage(data);
            InfoMessage = "You're chatting with " + _friendUsername + ". To disconnect, click a historic chat or exit the window.";
            IncomingRequestVisibility = Visibility.Collapsed;
        }


        public async Task DenyRequest()
        {
            var data = new Data
            {
                Message = "",
                RequestType = RequestType.ConnectionRequestDenied,
                Name = _username,
                Date = DateTime.Now
            };
            await _networkHandler.SendMessage(data);

            // Message is sent to client telling him that we denied, now we need to close the connection aswell.
            await _networkHandler.CloseConnection();

            StartListeningAgain(false); // Server should start showing "Listening for client requests..." and also kick off a new listener for clients
        }

        private async Task LoadHistoricalChats() // Loads left column of Chat Summaries
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _historicChatSummary.Clear();

                List<ChatSummary> sortedSummaries = _chatHistoryHandler.LoadHistoricalChats(Username);
                _historicChatSummary = new ObservableCollection<ChatSummary>(sortedSummaries);
                _filteredHistoricChatSummary = new ObservableCollection<ChatSummary>(sortedSummaries); // Also add to filtered

                OnPropertyChanged(nameof(FilteredHistoricChatSummary));
            });
        }


        private void StartListeningAgain(bool disconnected)
        {

            InfoMessage = disconnected ? _friendUsername + " disconnected on you. Listening for new client requests..." : "Listening for new client requests...";
            _networkHandler.StartListeningForClients();
        }


        private async void OnNetworkEvent(Data data)
        {
            if (data.RequestType == RequestType.CloseConnection)
            {
                // different logic depending on if youre server or client. If Server, you shut down tcpclient and keep listening on TcpListener (same logic as DenyRequest)
                // and if youre client, you have no TCPListener to fall back on, so its over.
                if (_chatRole == ChatRole.Server)
                {
                    IsLiveChatVisibility = Visibility.Collapsed;
                    SaveMessages();
                    await _networkHandler.CloseConnection();
                    StartListeningAgain(true);
                }
                else
                {
                    IsLiveChatVisibility = Visibility.Collapsed;
                    SaveMessages();
                    await _networkHandler.CloseConnection();
                    InfoMessage = _friendUsername + " disconnected on you.";
                    // If the client has to turn into a server, logic should go here. Else just assume that the conversation is over and client just reads his other conversations and cannot take on incoming requests
                }
            }
            else if (data.RequestType == RequestType.ChatMessage)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ActiveChatMessages.Add(data);
                });
            }

            else if (data.RequestType == RequestType.Buzz)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    TriggerBuzz();
                });
            }
        }


        public async Task SendMessage()
        {
            var data = new Data
            {
                Message = _messageText,
                RequestType = RequestType.ChatMessage,
                Name = _username,
                Date = DateTime.Now
            };

            await _networkHandler.SendMessage(data);

            Application.Current.Dispatcher.Invoke(() => // Bang on the UI thread
            {
                ActiveChatMessages.Add(data);
            });

            MessageText = "";
        }

        private void SaveMessages()
        {
            if (ActiveChatMessages.Count == 0)
            {
                return;
            }
            var jsonString = JsonSerializer.Serialize(_activeChatMessages);

            string documentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string chatHistoryFolder = "TDDD49-chats";
            string timestamp = _activeChatMessages[_activeChatMessages.Count - 1].Date.ToString("yyyyMMddHHmmss");
            string fileName = $"{_username}_and_{_friendUsername}_{timestamp}.json";

            string dirPath = Path.Combine(documentPath, chatHistoryFolder, fileName);

            File.WriteAllText(dirPath, jsonString);

            LoadHistoricalChats(); // Update historic chats column

        }

        public async Task LoadChatHistory(ChatSummary chatSummary) // Updates big box of conversations
        {
            // We gotta update ChatSummary buttons left column

            if (_networkHandler.Stream != null)
            {
                await HandleClosing();
                await LoadHistoricalChats();

                if (_chatRole == ChatRole.Server)
                {
                    StartListeningAgain(false);
                }
                else
                {
                    InfoMessage = "You cut off the connection. Session over";
                }
            }

            var chatHistoryList = await _chatHistoryHandler.LoadChatHistoryAsync(chatSummary);
            _activeChatMessages.Clear();

            foreach (var message in chatHistoryList)
            {
                _activeChatMessages.Add(message);
            }

        }

        private void FilterHistoricChats()
        {
            // If searchbox is empty, we simply show all historic chats
            if (string.IsNullOrEmpty(SearchText))
            {
                _filteredHistoricChatSummary = new ObservableCollection<ChatSummary>(_historicChatSummary);
                OnPropertyChanged(nameof(FilteredHistoricChatSummary));
            }
            // Or else we just show the ones that SearchText is substring of
            else
            {
                // Use LINQ
                var filteredResults = (from chatSummary in _historicChatSummary
                                       where chatSummary.FriendUsername.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                                       select chatSummary).ToList();
                _filteredHistoricChatSummary = new ObservableCollection<ChatSummary>(filteredResults);
                OnPropertyChanged(nameof(FilteredHistoricChatSummary));
            }
        }

        public async Task HandleClosing()
        {
            IsLiveChatVisibility = Visibility.Collapsed;

            var data = new Data
            {
                Message = "",
                RequestType = RequestType.CloseConnection,
                Name = _username,
                Date = DateTime.Now
            };

            await _networkHandler.SendMessage(data);
            await _networkHandler.CloseConnection();

        }


        public async Task SendBuzz()
        {
            var data = new Data
            {
                Message = "",
                RequestType = RequestType.Buzz,
                Name = _username,
                Date = DateTime.Now
            };

            await _networkHandler.SendMessage(data);
        }

        public void TriggerBuzz()
        {
            RequestBuzz?.Invoke();
        }



    }

}
