using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Text.Json;

namespace ChatApp.Model
{
    internal class NetworkHandler : INotifyPropertyChanged
    {
        private NetworkStream stream;
        private TcpListener _server;
        private TcpClient _communicationChannel;
        private bool _keepListening = true;
        public event Action<string> ClientConnected;
        public event Action<Data> NetworkEvent;
        public event PropertyChangedEventHandler? PropertyChanged;

        public NetworkStream Stream { get { return stream; } }

        public Task<bool> StartServer(IPAddress address, int port)
        {
            
            return Task.Factory.StartNew(() =>
            {
                var _ipEndPoint = new IPEndPoint(address, port);
                _server = new TcpListener(_ipEndPoint);
                try
                {
                    _server.Start();
                    System.Diagnostics.Debug.WriteLine("Server start successful");
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            });
        }

        public Task StartListeningForClients()
        {
            return Task.Run(async () =>
            {
                try
                {
                    _communicationChannel = _server.AcceptTcpClient(); // Is on hold waiting for incoming connection and turns into communication channel object when connected

                    stream = _communicationChannel.GetStream();
                    
                    // Initial logic just to capture requester's Name
                    Data initialData = await ReadMessageAsync();
                    ClientConnected?.Invoke(initialData.Name);
                    _keepListening = true;
                    // Start continuously listening for messages (Fire-and-forget)
                    _ = ListenForMessages();
                }
                catch (Exception ex)
                {
                    // Exception logic not obligatory
                }
            });
        }

        public Task<bool> StartClient(IPAddress address, int port)
        {
            return Task.Run( async () =>
            {
                var _ipEndPoint = new IPEndPoint(address, port);
                _communicationChannel = new TcpClient();

                try
                {
                    await _communicationChannel.ConnectAsync(_ipEndPoint);
                    stream = _communicationChannel.GetStream();
                    // Start continuously listening for messages (Fire-and-forget)
                    _keepListening = true;
                    _ = ListenForMessages();
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            });
        }
       
        public async Task SendMessage(Data data)
        {
            byte[] jsonString = JsonSerializer.SerializeToUtf8Bytes(data);

            int messageLength = jsonString.Length;
            byte[] lengthPrefix = BitConverter.GetBytes(messageLength);

            await stream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length);

            await stream.WriteAsync(jsonString, 0, jsonString.Length);

        }

        public async Task<Data> ReadMessageAsync()
        {
            var lengthBuffer = new byte[4];
            await stream.ReadAsync(lengthBuffer, 0, 4);
            int messageLength = BitConverter.ToInt32(lengthBuffer, 0);

            var message = new byte[messageLength];
            int recieved = 0;
            while (recieved < messageLength)
            {
                int bytesRead = await stream.ReadAsync(message, 0, messageLength);
                recieved += bytesRead;
            }
            
            var jsonString = Encoding.UTF8.GetString(message, 0, recieved);

            Data data = JsonSerializer.Deserialize<Data>(jsonString);
            return data;
        }

        public async Task ListenForMessages()
        {
            while (_keepListening)
            {
                try
                {
                    Data data = await ReadMessageAsync();
                    if (data != null)
                    {
                        // Process data
                        NetworkEvent?.Invoke(data);
                    }
                }
                catch (Exception ex)
                {
                    // Exception handling not needed
                }
            }
        }

        public async Task CloseConnection()
        {
            _keepListening = false;
            stream.Dispose();
            stream = null;
            _communicationChannel.Close();
            _communicationChannel = null;
        }

    }
}
