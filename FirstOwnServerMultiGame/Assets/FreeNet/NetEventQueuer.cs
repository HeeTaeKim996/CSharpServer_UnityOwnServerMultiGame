using System.Collections.Generic;
using System.Diagnostics;
using FreeNet;

namespace FreeNetUnity
{
    public enum NetworkEvent
    {
        connected,

        disconnected
    }


    public class NetEventQueuer
    {
        private Queue<NetworkEvent> network_events = new Queue<NetworkEvent>();
        private Queue<CPacket> network_messages = new Queue<CPacket>();
        private object cs_network_queue = new object();
        
        public bool Has_network_event()
        {
            lock (cs_network_queue)
            {
                return network_events.Count > 0;
            }
        }
        public void Enqueue_network_event(NetworkEvent net_event)
        {
            lock (cs_network_queue)
            {
                network_events.Enqueue(net_event);
            }
        }
        public NetworkEvent Dequeue_network_event()
        {
            lock (cs_network_queue)
            {
                return network_events.Dequeue();
            }
        }


        public bool Has_network_message()
        {
            lock (cs_network_queue)
            {
                return network_messages.Count > 0;
            }
        }
        public void Enqueue_network_message(CPacket msg)
        {
            lock (cs_network_queue)
            {
                network_messages.Enqueue(msg);
            }
        }
        public CPacket Dequeue_network_message()
        {
            lock (cs_network_queue)
            {
                return network_messages.Dequeue();
            }
        }
    }
}

