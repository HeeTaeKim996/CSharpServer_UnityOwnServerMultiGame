using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Threading;

namespace FreeNet
{
    public class CUserToken
    {
        public Socket socket { get; set; }

        private IPeer peer = null;
        public SocketAsyncEventArgs recv_args { get; private set; }
        public SocketAsyncEventArgs send_args { get; private set; }



        private CMessageResolver cMessageResolver = new CMessageResolver();


        private Queue<CPacket> sending_queue = new Queue<CPacket>();
        private object cs_sending_queue = new object();




        public void Set_peer(IPeer peer)
        {
            this.peer = peer;
        }
        public void Set_args(SocketAsyncEventArgs recv_args, SocketAsyncEventArgs send_args)
        {
            this.recv_args = recv_args;
            this.send_args = send_args;
        }




        public void On_receive_tcp(byte[] buffer, int offset, int transferred)
        {
            cMessageResolver.On_receiv_tcp(buffer, offset, transferred, On_message);
        }
        private void On_message(Const_buffer const_buffer)
        {
            peer.On_message(const_buffer);
        }



        public void Send(CPacket msg)
        {
            CPacket copy_packet = CPacket.Pop_forCopy_send(msg, msg.position);

            lock (cs_sending_queue)
            {
                if(this.sending_queue.Count <= 0)
                {
                    sending_queue.Enqueue(copy_packet);

                    Start_send();
                }
                else
                {
                    sending_queue.Enqueue(copy_packet);
                    Console.WriteLine("sending_queue가 아직 비지 않았습니다. 메세지를 Enqueue 합니다");
                }
            }
        }
        private void Start_send()
        {
            lock (cs_sending_queue)
            {
                CPacket peek_packet = sending_queue.Peek();
                peek_packet.Record_size();

                send_args.SetBuffer(0, peek_packet.position);

                Buffer.BlockCopy(peek_packet.buffer, 0, send_args.Buffer, 0, peek_packet.position);
                // 주의. 여기서 peek_packet.Push_bacak 처리하면 안된다. process_send에서 재확인 후 처리함

                bool pending = socket.SendAsync(send_args);
                if (!pending)
                {
                    Process_send(send_args);
                }

            }
        }


        private static int sent_count = 0;
        public void Process_send(SocketAsyncEventArgs e)
        {
            lock (cs_sending_queue)
            {
                if (e.BytesTransferred <= 0 || e.SocketError != SocketError.Success) return;

                if (sending_queue.Peek().position != e.BytesTransferred)
                {
                    Console.WriteLine($"CUserToken : 송신한 패킷의 길이와, 예정된 패킷의 길이가 다릅니다. Transferred : {e.BytesTransferred}, peek_size : {sending_queue.Peek().position}");
                    return;
                }


                if (sending_queue.Count <= 0)
                {
                    throw new Exception("sneding_queue.Coun 가 0보다 작습니다. (소스코드를 보니 이럴 확률은 없다 보면 되지만, 혹시나 해서 넣어놓은듯 싶음)");
                }


                Interlocked.Increment(ref sent_count);
                //Console.WriteLine($"보낸 소켓 : {e.SocketError}, Transferred : {e.BytesTransferred}, sent_count : {sent_count}");


                CPacket.Push_back(sending_queue.Dequeue());


                if(sending_queue.Count > 0)
                {
                    Start_send();
                }
            }
        }

        public void On_removed()
        {
            sending_queue.Clear();
            if(peer != null)
            {
                peer.On_removed();
            }
        }

        public void Disconnect()
        {
            try
            {
                socket.Shutdown(SocketShutdown.Send);
            }
            catch (Exception e) { }

            socket.Close();
        }


        public void Start_keep_alive()
        {
            Timer kepp_alive_timer = new Timer((object e) =>
            {
                CPacket send_packet = CPacket.Pop_forCreate();
                send_packet.Push(0);
                Send(send_packet);
            }, null, 0, 3_000);
        }

    }
}
