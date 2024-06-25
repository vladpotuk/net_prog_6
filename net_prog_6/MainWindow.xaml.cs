using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace net_prog_6
{
    public partial class MainWindow : Window
    {
        private const int Port = 8888;
        private UdpClient udpServer;

        public MainWindow()
        {
            InitializeComponent();
            StartServer();
        }

        private void StartServer()
        {
            udpServer = new UdpClient(Port);
            udpServer.BeginReceive(ReceiveCallback, null);
            Log("Server started. Listening on port " + Port);
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, Port);
            byte[] clientMessage = udpServer.EndReceive(ar, ref clientEndPoint);
            string message = Encoding.ASCII.GetString(clientMessage);

            

           
            string response = "Recipe: Salad\nIngredients: Lettuce, Tomato, Cucumber";
            byte[] responseData = Encoding.ASCII.GetBytes(response);
            udpServer.Send(responseData, responseData.Length, clientEndPoint);

            Log($"Response sent to {clientEndPoint}: {response}");

           
            udpServer.BeginReceive(ReceiveCallback, null);
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
