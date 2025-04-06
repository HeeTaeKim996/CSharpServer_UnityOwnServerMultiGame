using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace FreeNet
{
    internal class SocketAsyncEventArgsPool
    {
        private Stack<SocketAsyncEventArgs> args_pool;
        private object cs_args_pool = new object();


        public SocketAsyncEventArgsPool(int pool_capacity)
        {
            args_pool = new Stack<SocketAsyncEventArgs>(pool_capacity);
        }
        public SocketAsyncEventArgs Pop()
        {
            lock (cs_args_pool)
            {
                return args_pool.Pop();
            }
        }
        public void Push(SocketAsyncEventArgs args)
        {
            if(args == null)
            {
                throw new ArgumentNullException("SocketAsyncEventArgsPool : args는 null일 수 없습니다");
            }
            lock (cs_args_pool)
            {
                args_pool.Push(args);
            }
        }
        public int Count
        {
            get
            {
                return args_pool.Count;
            }
        }
    }
}
