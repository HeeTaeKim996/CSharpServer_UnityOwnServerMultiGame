using System;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Schema;

namespace FreeNet
{
    public class CPacket
    {
        #region 공부정리
        // □ 
        // - 기존 교재의 코드와 변경된 내용이 많다. 변경된 내용을 나열하면, 1) CPacket직접 생성은 CPacketBufferManager의 풀로만. 송신 수신 때 모두 풀을 활용 2) 생성 코드 관련 코드들 대폭 수정 3) 패킷을 풀로 돌려보낼 때, this.owner = null; 처리
        //  - 1) 오브젝트 풀 활용 
        //   - 기존 교재의 코드는, 오브젝트의 풀을 패킷을 보내기 위해 만들 때. 한번 사용했다. 설명에 앞서, 기존 코드에서 CPacket을 생성(풀 또는 직접생성)해서 처리한 경우는,
        //    1. CMessgaeResolver에서 리턴한 Const<byte[]> buffer 를 CPacket 으로 만들어 담아, 읽기 처리할 때 (CPacket을 직접 생성)
        //    2. 패킷을 보내기 위해, CPacket을 create로 풀에서 생성할 때(풀 활용)
        //    3. 2에서 만들어진 패킷을, sendingSocket의 queue에 담기 위해, 2패킷을 복사할 때(CPacket 직접 생성)
        //    이었다. 위 내용의 1번은, Pop_forCopy_read 로 처리하여 풀로 관리한다. 2번은, Pop_forCreate로 기존 내용과 동일하게(일부만 수정) 만들었다. 3번은, Pop_forCopy_send 로 처리하여 풀로 관리한다
        //  - 2) 생성 코드 관련 대폭 수정
        //     - 자세한 내용은 생략하고, 코드의 범용성은 떨어졌다. 다만 명확성은 올라간 듯 싶다. 특히, 위 2.에서, Copy 와 overwrite를 없애고 하나의 매서드로 단일화했다. 여기서 기존 overwrite에서는, set_protocol_id로 protocol_id 멤버값을 할당하고, 프로토콜 버퍼도 BlockCopy
        //       한 후에, 다시 전체 버퍼를 카피 해서 오버라이드 했는데, 아마 교재 작성자의 실수인 듯 싶다. 
        //  - 3) this.owner = null;
        //     - 패킷 풀을 보면, 기존 사용한 패킷을 초기화하지 않는다. ( owner, buffer, position, protocol_id ). 어차피 byte[1024]로 크기는 고정이며, 생성과 수신 처리는 모두 코드로 관리하기 때문에, buffer내용, position, protocol_id는 초기화할 필요가 없다.
        //     - ( 예를 들어, 1024 중 900 까지 작성 및 처리하고, 풀로 들어갔다가, 다시 pop 되어 700 까지 작성했다 치자. 그래도 이 패킷을 수신한 곳에서는 아래 코드의 Pop, Push를 정해진대로 하기 때문에, 수신자는 700 까지만 읽게 된다) 
        //       다만, owner는 초기화 작업을 push_back 때 추가하였다. owner가 사용되는 경우는, 서버에서 메세지를 수신할 때, 메인 로직에 CPacket을 enqueue하고, 메인로직에서 다시 dequeue해서, 이 패킷을 실행할 주체를 다시 찾기 위해 필요한 것 하나 뿐이다.
        //       따라서 사실 owner도 초기화를 안해도, 문제가 되지는 않는다. 메세지를 수신할 때 항상 매서드 Set_buffer_owner에서 처리하기 때문에. 
        //       하지만, 1) owner로 참조된 객체인 IPeer 구현 객체가 사라져도, CPacket이 owner로 참조하고 있으면, GC가 해당 사라진 객체를 처리하지 않는다 한다.
        //               2) 혹시나 추후 owner를 참조하는 경우가 추가로 생길 경우에 대비하여,
        //       push_back 때 owner = null 작업을 추가하였다.


        //        ☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
        //      ☆ Const<T> 의 T Value 와, CPacket 오브젝트 풀 활용
        //       - 기존 교재의 위 1. 3. 의 경우 직접 생성했기 때문에, Const<byte[]>(buffeR) 를 받아서, CPacket msg = new CPacket(const(instance).Value, this); 로 처리했었다. Const<T>의 T가 참조형 (byte[] 등) 이면, 얕은 복사로 인해 문제가 발생할 수 있는데,
        //         기존 방법에서 문제가 생기지 않았떤 이유는, 1. 3. 의 경우에만 Const<byte[]>로 받아서  T.Value를 직접 대입했기 때문. 
        //         하지만 1.3. 에 풀링을 사용하면서, Const<byte[]> 의 T.Value를 CPacket에 그대로 사용한다면, 얕은 복사로 인해, 문제가 생긴다. 따라서, 1.3도 풀링을 한다면, CPacket에 할당하는 버퍼를, T.Value로 바로 넣으면 안되고,
        //         Buffer.BlockCopy(T.Value, 0, copy_buffer, 0, T.Value.Length); (하단의 방법인) CPacket msg = CPacket.Pop_forCopy_read(copy_buffer, this); 로 처리해야 한다. 3)방법도 마찬가지

        #endregion

        public IPeer owner { get; private set; }
        public byte[] buffer { get; private set; } = new byte[StaticValues.bufferSize];
        public int position { get; private set; }
        public Int16 protocol_id { get; private set; }


        public static CPacket Pop_forCopy_for_serverQueue(Const_buffer const_buffer, IPeer owner)
        {
            CPacket return_packet = CPacketBufferManager.Pop();
            return_packet.Copy_buffer(const_buffer.Value, const_buffer.position);
            return_packet.Set_position(StaticValues.HeaderSize);

            return_packet.Set_owner(owner);
            return return_packet;
        }
        public static CPacket Pop_forCopy_for_clientRead(Const_buffer const_buffer)
        {
            CPacket return_packet = CPacketBufferManager.Pop();
            return_packet.Copy_buffer(const_buffer.Value, const_buffer.position);
            return_packet.Set_position(StaticValues.HeaderSize);
            return return_packet;
        }


        public static CPacket Pop_forCopy_send(CPacket copiedPacket, int position)
        {
            CPacket return_packet = CPacketBufferManager.Pop();
            return_packet.Copy_buffer(copiedPacket.buffer, position);
            return_packet.Set_position(position);
            return return_packet;
        }


        public static CPacket Pop_forCreate()
        {
            CPacket return_packet = CPacketBufferManager.Pop();
            return_packet.Set_position(StaticValues.HeaderSize);
            return return_packet;
        }


        public static void Push_back(CPacket packet)
        {
            packet.owner = null;
            CPacketBufferManager.Push(packet);
        }

        public void Copy_buffer(byte[] copied_buffer, int position)
        {
            Buffer.BlockCopy(copied_buffer, 0, buffer, 0, position); // byte[] 는 struct가 아닌 참조형이기 때문에, this.buffer = copied_buffer.buffer 로 할시, 얕은 복사됨

            //Console.WriteLine($"CPacket 테스트 {position}");
        }
        public void Set_owner(IPeer owner)
        {
            this.owner = owner;
        }
        public void Set_position(int position)
        {
            this.position = position;
        }


        public byte Pop_byte()
        {
            byte data = (byte)BitConverter.ToInt16(buffer, position);
            position += sizeof(byte);
            return data;
        }
        public Int16 Pop_int16()
        {
            Int16 data = BitConverter.ToInt16(buffer, position);
            position += sizeof(Int16);
            return data;
        }
        public Int32 Pop_int32()
        {
            Int32 data = BitConverter.ToInt32(buffer, position);
            position += sizeof(Int32);
            return data;
        }
        public string Pop_string()
        {
            Int16 len = BitConverter.ToInt16(buffer, position);
            position += sizeof(Int16);

            string data = Encoding.UTF8.GetString(buffer, position, len);
            position += len;

            return data;
        }
        public float Pop_float()
        {
            float data = BitConverter.ToSingle(buffer, position);
            position += sizeof(Single);
            return data;
        }

        public void Push(byte data)
        {
            byte[] push_buffer = new byte[] { data };
            Buffer.BlockCopy(push_buffer, 0, buffer, position, sizeof(byte));
            position += sizeof(byte);
        }
        public void Push(Int16 data)
        {
            byte[] push_buffer = BitConverter.GetBytes(data);
            Buffer.BlockCopy(push_buffer, 0, buffer, position, sizeof(Int16));
            position += sizeof(Int16);
        }
        public void Push(Int32 data)
        {
            byte[] push_buffer = BitConverter.GetBytes(data);
            Buffer.BlockCopy(push_buffer, 0, buffer, position, sizeof(Int32));
            position += sizeof(Int32);
        }
        public void Push(string data)
        {
            byte[] data_buffer = Encoding.UTF8.GetBytes(data);
            Int16 len = (short)data_buffer.Length;
            byte[] len_buffer = BitConverter.GetBytes(len);

            Buffer.BlockCopy(len_buffer, 0, buffer, position, sizeof(Int16));
            position += sizeof(Int16);

            Buffer.BlockCopy(data_buffer, 0, buffer, position, len);
            position += len;
        }
        public void Push(float data)
        {
            byte[] push_buffer = BitConverter.GetBytes(data);
            Buffer.BlockCopy(push_buffer, 0, buffer, position, sizeof(Single));
            position += sizeof(Single);
        }

        public void Record_size()
        {
            Int16 data_length = (Int16)(position - StaticValues.HeaderSize);
            byte[] header_buffer = BitConverter.GetBytes(data_length);

            Buffer.BlockCopy(header_buffer, 0, buffer, 0, StaticValues.HeaderSize);
        }
    }
}
