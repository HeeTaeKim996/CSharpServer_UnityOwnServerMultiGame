using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace FreeNet
{
    internal class CListener
    {
        private Socket listen_socket;
        private SocketAsyncEventArgs accept_args;
        private AutoResetEvent autoResetEvent;

        public delegate void NewClientHandler(Socket accept_socket, object token);
        public NewClientHandler callback_on_newClient;


        public void Start(string host, int port, int backLog)
        {
            listen_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress address;
            if(host == "0.0.0.0")
            {
                address = IPAddress.Any;
            }
            else
            {
                address = IPAddress.Parse(host);
            }
            IPEndPoint endPoint = new IPEndPoint(address, port);
            listen_socket.Bind(endPoint);
            listen_socket.Listen(backLog);

            try
            {
                accept_args = new SocketAsyncEventArgs();
                accept_args.Completed += On_accept_completed;

                Thread listen_thread = new Thread(DoListen);
                listen_thread.Start();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }


        }
        private void On_accept_completed(object sender, SocketAsyncEventArgs e)
        {
            autoResetEvent.Set();
            if(e.SocketError == SocketError.Success)
            {
                Socket accept_socket = e.AcceptSocket;
                callback_on_newClient?.Invoke(accept_socket, e.UserToken);
            }
            else
            {
                Console.WriteLine($"CListener : SocketACcept Failed : {e.SocketError}");
            }
        }
        private void DoListen()
        {
            autoResetEvent = new AutoResetEvent(false);

            while (true)
            {
                accept_args.AcceptSocket = null;

                try
                {
                    bool pending = listen_socket.AcceptAsync(accept_args);
                    if (!pending)
                    {
                        On_accept_completed(null, accept_args);
                    }

                    autoResetEvent.WaitOne();
                }
                catch(Exception e)
                {
                    Console.WriteLine($"CListener : SocketAccepting Failed : {e.Message}");
                }
            }
        }

    }
}
