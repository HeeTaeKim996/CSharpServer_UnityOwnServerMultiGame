using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeNet;
using Microsoft.VisualBasic.FileIO;

namespace GameServer
{
    public class NetObjectPool
    {
        private static short id;

        private List<short> pool = new List<short>();


        public bool Has_id(short id)
        {
            return pool.Contains(id);
        }

        public short Add()
        {
            id++;
            pool.Add(id);
            return id;
        }
        public bool Delete(short id)
        {
            if (!pool.Contains(id))
            {
                return false;
            }
            else
            {
                pool.Remove(id);
                return true;
            }
        }
    }


    public class NetObjectPoolManager
    {
        private Dictionary<NetObjectCode, NetObjectPool> pools_pool = new Dictionary<NetObjectCode, NetObjectPool>();
        private CGameRoom cGameRoom;

        public NetObjectPoolManager(CGameRoom cGameRoom)
        {
            this.cGameRoom = cGameRoom;
        }

        public short Add_object(NetObjectCode netObjectCode)
        {
            if (!pools_pool.ContainsKey(netObjectCode))
            {
                pools_pool[netObjectCode] = new NetObjectPool();
                cGameRoom.On_new_objectPool_instantaited(netObjectCode);
            }
            return pools_pool[netObjectCode].Add();
        }
        public bool Remove_object(NetObjectCode netObjectCode, short id)
        {
            if (pools_pool.ContainsKey(netObjectCode))
            {
                return pools_pool[netObjectCode].Delete(id);
            }
            else
            {
                return false;
            }
        }
    }
}
