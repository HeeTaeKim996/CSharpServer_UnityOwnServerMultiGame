using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace FreeNet
{
    public class CNetworkService
    {
        private int connected_count;

        private CListener client_listener;
        private BufferManager bufferManager;
        private SocketAsyncEventArgsPool recv_args_pool;
        private SocketAsyncEventArgsPool send_args_pool;

        public delegate void SessionHandler(CUserToken token);
        public SessionHandler session_created_callback;

        

        public void Initialize()
        {
            connected_count = 0;
            bufferManager = new BufferManager(StaticValues.max_connections * StaticValues.bufferSize * StaticValues.pre_alloc_count, StaticValues.bufferSize);
            recv_args_pool = new SocketAsyncEventArgsPool(StaticValues.max_connections);
            send_args_pool = new SocketAsyncEventArgsPool(StaticValues.max_connections);


            SocketAsyncEventArgs args;
            for(int i = 0; i < StaticValues.max_connections; i++)
            {
                CUserToken token = new CUserToken();

                // recv_args
                {
                    args = new SocketAsyncEventArgs();
                    args.Completed += On_receive_completed;
                    bufferManager.SetBuffer(args);
                    args.UserToken = token;

                    recv_args_pool.Push(args);
                }
                // send_args
                {
                    args = new SocketAsyncEventArgs();
                    args.Completed += On_send_completed;
                    bufferManager.SetBuffer(args);
                    args.UserToken = token;

                    send_args_pool.Push(args);
                }
            }
        }
        private void On_receive_completed(object sender, SocketAsyncEventArgs e)
        {
            if(e.LastOperation == SocketAsyncOperation.Receive)
            {
                Process_receive(e);
            }
            else
            {
                throw new ArgumentNullException("마지막으로 받은 소켓오퍼레이션이 SocketAsyncOperation.Receive 가 아님");
            }
        }
        private void On_send_completed(object sender, SocketAsyncEventArgs e)
        {
            CUserToken token = e.UserToken as CUserToken;
            token.Process_send(e);
        }
        public void Listen(string host, int port, int backLog)
        {
            client_listener = new CListener();
            client_listener.callback_on_newClient += On_newClient;
            client_listener.Start(host, port, backLog);
        }
        private void On_newClient(Socket client_socket, object sender)
        {
            Interlocked.Increment(ref connected_count);
            Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId} 클라이언트 연결 핸들 : {client_socket.Handle}. 연결된 총 클라이언트 : {this.connected_count}");

            SocketAsyncEventArgs recv_args = recv_args_pool.Pop();
            SocketAsyncEventArgs send_args = send_args_pool.Pop();

            Begin_receive(client_socket, recv_args, send_args);

            CUserToken token = recv_args.UserToken as CUserToken;
            session_created_callback?.Invoke(token);
        }
        public void On_cConnector_connect_completed(Socket socket, CUserToken token)
        {
            SocketAsyncEventArgs recv_args = new SocketAsyncEventArgs();
            recv_args.Completed += On_receive_completed;
            recv_args.SetBuffer(new byte[StaticValues.bufferSize], 0, StaticValues.bufferSize);
            recv_args.UserToken = token;

            SocketAsyncEventArgs send_args = new SocketAsyncEventArgs();
            send_args.Completed += On_send_completed;
            send_args.SetBuffer(new byte[StaticValues.bufferSize], 0, StaticValues.bufferSize);
            send_args.UserToken = token;

            Begin_receive(socket, recv_args, send_args);
        }

        private void Begin_receive(Socket socket, SocketAsyncEventArgs recv_args, SocketAsyncEventArgs send_args)
        {
            CUserToken token = recv_args.UserToken as CUserToken;
            token.socket = socket;
            token.Set_args(recv_args, send_args);

            bool pending = socket.ReceiveAsync(recv_args);
            if (!pending)
            {
                Process_receive(recv_args);
            }
        }
        private void Process_receive(SocketAsyncEventArgs e)
        {
            CUserToken token = e.UserToken as CUserToken;

            if(e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                token.On_receive_tcp(e.Buffer, e.Offset, e.BytesTransferred);

                bool pending = token.socket.ReceiveAsync(token.recv_args);
                if (!pending)
                {
                    On_receive_completed(null, token.recv_args);
                }
            }
            else
            {
                Console.WriteLine($"CNetowkrService __  Error : {e.SocketError}, Transferred : {e.BytesTransferred}");
                close_clientSocket(token);
            }
        }
        private void close_clientSocket(CUserToken token)
        {
            token.On_removed();
            if(recv_args_pool != null)
            {
                recv_args_pool.Push(token.recv_args);
            }
            if(send_args_pool != null)
            {
                send_args_pool.Push(token.send_args);
            }
        }
    }
}
