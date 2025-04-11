using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FreeNet
{
    public class CPacketBufferManager
    {
        private static int pool_capacity;
        private static Stack<CPacket> cPacket_pool;
        private static object cs_cPacket_pool = new object();

        public static void Initialize(int capacity)
        {
            pool_capacity = capacity;
            cPacket_pool = new Stack<CPacket>(capacity);
            Allocate();
        }
        private static void Allocate()
        {
            lock (cs_cPacket_pool)
            {
                for (int i = 0; i < pool_capacity; i++)
                {
                    cPacket_pool.Push(new CPacket());
                }
            }
        }
        public static void Push(CPacket packet)
        {
            lock (cs_cPacket_pool)
            {
                cPacket_pool.Push(packet);
            }

            // Debug 1 
            {
                Console.WriteLine(cPacket_pool.Count);
            }

            // Debug 2
            {
                //packet.Set_position(2);
                //Console.WriteLine($"{cPacket_pool.Count} // {packet.Pop_byte()} // {packet.Pop_byte()} // {packet.Pop_byte()} // {packet.Pop_byte()} ");
            }
        }
        public static CPacket Pop()
        {
            if(cPacket_pool.Count <= 0)
            {
                Allocate();
                Console.WriteLine("CPAcketBufferManager : ALlocate 처리됨");
            }
            lock (cs_cPacket_pool)
            {
                return cPacket_pool.Pop();
            }
        }

        public static int Count
        {
            get
            {
                return cPacket_pool.Count;
            }
        }
    }
}
