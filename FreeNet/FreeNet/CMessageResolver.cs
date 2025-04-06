using System;

namespace FreeNet
{
    internal class CMessageResolver
    {
        private byte[] message_buffer = new byte[StaticValues.bufferSize];
        private int remain_bytes;
        private int quantity_to_read = 0;
        private int current_readed = 0;

        public delegate void CompleteMessageCallBack(Const_buffer buffer);
        

        public void On_receiv_tcp(byte[] tcp_buffer, int offset, int transferred, CompleteMessageCallBack callback)
        {
            remain_bytes = transferred;
            int src_position = offset;

            while(remain_bytes > 0)
            {
                bool completed;

                if(current_readed < StaticValues.HeaderSize)
                {
                    quantity_to_read = StaticValues.HeaderSize;

                    completed = Read_until(tcp_buffer, ref src_position, offset, transferred);
                    if (!completed) return;

                    quantity_to_read += Get_body_size();   
                }

                completed = Read_until(tcp_buffer, ref src_position, offset, transferred);
                if (completed)
                {
                    callback(new Const_buffer(message_buffer, current_readed));
                    Clear_buffer();
                }
            }
        }
        
        private bool Read_until(byte[] tcp_buffer, ref int src_position, int offset, int transferred)
        {
            if (src_position >= offset + transferred) return false;

            int read_size = quantity_to_read - current_readed;
            if(remain_bytes < read_size)
            {
                read_size = remain_bytes;
            }

            Buffer.BlockCopy(tcp_buffer, src_position, message_buffer, current_readed, read_size);

            remain_bytes -= read_size;
            src_position += read_size;
            current_readed += read_size;


            if (current_readed < quantity_to_read) return false;
            return true;
        }

        private short Get_body_size()
        {
            return BitConverter.ToInt16(message_buffer);

            // ○ 기존 교재 코드 내용은 하단과 같다. HEADERSIZE를 2( HeaderBuffer를 short로 push) vs 4( HEaderBuffer를 int로 push) 에 따라 다르게 읽는 것으로 처리했던 것으로 보인다)
            //Type type = Defines.HEADERSIZE.GetType();

            //if (type.Equals(typeof(Int16)))
            //{
            //    return BitConverter.ToInt16(this.message_buffer);
            //}
            //else
            //{
            //    return BitConverter.ToInt32(this.message_buffer);
            //}
        }
        private void Clear_buffer()
        {
            Array.Clear(message_buffer, 0, message_buffer.Length);
            current_readed = 0;
        }
    }
}
