using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace FreeNet
{
    internal class BufferManager
    {
        private byte[] total_buffer;
        private int total_buffer_size;
        private int buffer_size;

        private Stack<int> buffer_index_pool = new Stack<int>();
        private int current_index = 0;

        public BufferManager(int total_buffer_size, int buffer_size)
        {
            this.total_buffer_size = total_buffer_size;
            this.buffer_size = buffer_size;

            total_buffer = new byte[total_buffer_size];
        }
        public void SetBuffer(SocketAsyncEventArgs args)
        {
            if(buffer_index_pool.Count > 0)
            {
                args.SetBuffer(total_buffer, buffer_index_pool.Pop(), buffer_size);
            }
            else
            {
                if(total_buffer_size < current_index + buffer_size)
                {
                    throw new ArgumentNullException("할당된 total_buffer 보다 더 많은 버퍼를 사용하려 시도했습니다");
                    // 기존 참조한 교재 코드에는, public bool SetBuffer로, 여기에 return false를 사용하는 구문이었지만, bool자료를 사용하는 코드가 하나도 없었기 때문에, void로 바꾸고, throw new ArgumentNullExexcption으로 대체
                }
                args.SetBuffer(total_buffer, current_index, buffer_size);
                current_index += buffer_size;
            }

        }
        public void FreeBuffer(SocketAsyncEventArgs args)
        {
            buffer_index_pool.Push(args.Offset);
            args.SetBuffer(null, 0, 0);
        }
    }
}
