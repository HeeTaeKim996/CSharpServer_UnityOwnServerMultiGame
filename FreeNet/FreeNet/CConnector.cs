using System;
using System.Net;
using System.Net.Sockets;

namespace FreeNet
{
    public class CConnector
    {
        private CNetworkService cNetworkService;
        private Socket client_socket;

        public delegate void ConnectHandler(CUserToken token);
        public ConnectHandler callback_on_connected;

        public CConnector(CNetworkService cNetworkService)
        {
            this.cNetworkService = cNetworkService;
        }
        public void Connect(IPEndPoint remote_endPoint)
        {
            client_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            SocketAsyncEventArgs connect_args = new SocketAsyncEventArgs();
            connect_args.Completed += On_connect_completed;
            connect_args.RemoteEndPoint = remote_endPoint;

            bool pending = client_socket.ConnectAsync(connect_args);
            if (!pending)
            {
                On_connect_completed(null, connect_args);
            }
        }
        private void On_connect_completed(object sender, SocketAsyncEventArgs e)
        {
            if(e.SocketError == SocketError.Success)
            {
                CUserToken token = new CUserToken();
                cNetworkService.On_cConnector_connect_completed(client_socket, token);
                callback_on_connected?.Invoke(token);
            }
            else
            {
                Console.WriteLine($"CConnector : {e.SocketError}");
            }
        }

    }
}
