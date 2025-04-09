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
        private byte id_index = 0;

        private List<byte> pool = new List<byte>();


        public bool Has_id(byte id)
        {
            return pool.Contains(id);
        }
        public bool is_addable()
        {
            return id_index < 224;
        }

        public byte Add()
        {
            id_index++;
            if(id_index == 225)
            {
                return 0;
            }
            pool.Add(id_index);
            return id_index;
        }
        public bool Delete(byte id)
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
        private Dictionary<byte, NetObjectPool> pools_pool = new Dictionary<byte, NetObjectPool>();
        private CGameRoom cGameRoom;
        private byte pool_index = 1;

        public NetObjectPoolManager(CGameRoom cGameRoom)
        {
            pools_pool[pool_index] = new NetObjectPool();
            this.cGameRoom = cGameRoom;
        }

        public (byte, byte) Add_object()
        {
            if (!pools_pool[pool_index].is_addable())
            {
                pool_index++;
                pools_pool[pool_index] = new NetObjectPool();
            }
            return (pool_index, pools_pool[pool_index].Add());
        }
        public bool Remove_object(byte pool_code, byte id)
        {
            if (pools_pool.ContainsKey(pool_code))
            {
                return pools_pool[pool_code].Delete(id);
            }
            else
            {
                return false;
            }
        }
    }
}
