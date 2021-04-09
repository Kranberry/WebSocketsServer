using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Windows.UI.Popups;
using System.Threading;

namespace SuperAmazingClient
{
    public class SocketClient
    {
        private byte[] DataBuffer = new byte[1024];

        private int Port { get; }
        //private string ServerAdress { get; }
        private Socket Client { get; set; }
        private IPHostEntry IpHostInfo { get; }
        private IPAddress IpAdress { get; }
        private IPEndPoint Server { get; }
        private bool Connected { get; set; }

        /// <summary>
        /// A wrapper for the Socket class.
        /// </summary>
        /// <param name="port">Port of the socket server</param>
        public SocketClient(/*string serverAdress,*/ int port)
        {
            Port = port;

            // Establish the local endpoint for the socket.
            IpHostInfo = Dns.GetHostEntry(Dns.GetHostName());   // Dns.GetHostName() represents this machine
            IpAdress = IpHostInfo.AddressList[0];
            Server = new IPEndPoint(IpAdress, Port);
            // Create a tcp/IP socket
            Client = new Socket(IpAdress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        /// <summary>
        /// Create a new connection to the server. Will create a new socket if already connected
        /// </summary>
        public async void Connect()
        {
            try
            {
                if (Connected) // Make sure to not connect multiple sockets from the same instance
                {
                    Disconnect();   // Disconnect the old one
                    Client = new Socket(IpAdress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);   // Create a new one
                }

                Client.Connect(Server); // Connect the new one to the server
                Connected = true;
                
            }
            catch(SocketException e)
            {
                MessageDialog msgBox = new MessageDialog("A connection to the host could not be made: " + e.ToString());
                await msgBox.ShowAsync();
            }
        }

        /// <summary>
        /// Send a disconnect message to the server, and close and dispose the client socket
        /// </summary>
        public async void Disconnect()
        {
            Connected = false;
            await SendRequest("Disconnecting");
            // Release the socket
            Client.Shutdown(SocketShutdown.Both);   // Disable the socket from both sending and recieving data
            Client.Close(); // Close and dispose of the object
            Client.Dispose();   // Release every resource of the current socket instance
        }

        public async Task<string> SendRequest(string request)
        {
            // The encoded buffer to send to the server
            byte[] sendBuffer = Encoding.ASCII.GetBytes(request);

            try
            {
                // Send our data to the server
                int sentBuffer = Client.Send(sendBuffer);
                // Recieve the response
                int recievedBufferCount = Client.Receive(DataBuffer);
                string recievedMessage = Encoding.ASCII.GetString(DataBuffer, 0, recievedBufferCount);
                return recievedMessage ?? "Server happy!";
            }
            catch(Exception e)
            {
                MessageDialog msgBox = new MessageDialog("A request to the server could not be done: " + e.ToString());
                await msgBox.ShowAsync();
                return e.Message;
            }
        }
    }
}
