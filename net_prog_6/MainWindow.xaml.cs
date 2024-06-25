
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace net_prog_6.Server
{
    public partial class MainWindow : Window
    {
        private const int Port = 8888;
        private UdpClient udpServer;
        private List<IPEndPoint> connectedClients = new List<IPEndPoint>();
        private Dictionary<IPEndPoint, int> clientRequestCount = new Dictionary<IPEndPoint, int>();
        private Dictionary<IPEndPoint, DateTime> lastActivityTime = new Dictionary<IPEndPoint, DateTime>();
        private readonly object clientLock = new object();
        private readonly TimeSpan inactiveTimeout = TimeSpan.FromMinutes(10);

        public MainWindow()
        {
            InitializeComponent();
            StartServer();
        }

        private void StartServer()
        {
            udpServer = new UdpClient(new IPEndPoint(IPAddress.Parse("10.0.0.139"), Port));
            Log("Server started. Listening on port " + Port);

            Task.Run(() => ReceiveMessages());
            Task.Run(() => CheckInactiveClients());
        }

        private async Task ReceiveMessages()
        {
            while (true)
            {
                try
                {
                    UdpReceiveResult result = await udpServer.ReceiveAsync();
                    IPEndPoint clientEndPoint = result.RemoteEndPoint;
                    byte[] clientMessage = result.Buffer;
                    string message = Encoding.ASCII.GetString(clientMessage);

                    
                    if (!connectedClients.Contains(clientEndPoint))
                    {
                        connectedClients.Add(clientEndPoint);
                        Log($"Client connected: {clientEndPoint}");
                    }

                    
                    string response = ProcessRequest(message);
                    byte[] responseData = Encoding.ASCII.GetBytes(response);
                    await udpServer.SendAsync(responseData, responseData.Length, clientEndPoint);

                    Log($"Response sent to {clientEndPoint}: {response}");

                   
                    TrackClientRequest(clientEndPoint);
                }
                catch (Exception ex)
                {
                    Log($"Error: {ex.Message}");
                }
            }
        }

        private string ProcessRequest(string request)
        {
           
            return $"Server received: {request}";
        }

        private void TrackClientRequest(IPEndPoint clientEndPoint)
        {
            lock (clientLock)
            {
                if (!clientRequestCount.ContainsKey(clientEndPoint))
                {
                    clientRequestCount[clientEndPoint] = 1;
                }
                else
                {
                    clientRequestCount[clientEndPoint]++;
                }
            }
        }

        private async Task CheckInactiveClients()
        {
            while (true)
            {
                await Task.Delay(1000);

                lock (clientLock)
                {
                    List<IPEndPoint> disconnectedClients = new List<IPEndPoint>();
                    foreach (var clientEndPoint in lastActivityTime.Keys)
                    {
                        if ((DateTime.Now - lastActivityTime[clientEndPoint]) > inactiveTimeout)
                        {
                            disconnectedClients.Add(clientEndPoint);
                        }
                    }

                    foreach (var clientEndPoint in disconnectedClients)
                    {
                        DisconnectClient(clientEndPoint);
                    }
                }
            }
        }

        private void DisconnectClient(IPEndPoint clientEndPoint)
        {
            if (connectedClients.Contains(clientEndPoint))
            {
                connectedClients.Remove(clientEndPoint);
                Log($"Client disconnected due to inactivity: {clientEndPoint}");

                
                clientRequestCount.Remove(clientEndPoint);
                lastActivityTime.Remove(clientEndPoint);
            }
        }

        private void Log(string message)
        {
            Dispatcher.Invoke(() =>
            {
                txtLog.AppendText($"{DateTime.Now}: {message}\n");
            });
        }
    }
}
