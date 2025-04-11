using System;
using System.Net;
using UnityEngine;
using FreeNet;
using UnityEngine.SceneManagement;

namespace FreeNetUnity 
{
    public class CNetUnityService : MonoBehaviour
    {
        private CNetworkManager cNetworkManager;
        private CNetworkService cNetworkService;
        private NetEventQueuer netEventQueuer = new NetEventQueuer();
        private IPeer cRemoteServerPeer;


        private void Awake()
        {
            CPacketBufferManager.Initialize(20);
            cNetworkManager = GetComponent<CNetworkManager>();
        }

        public void Connect(string host, int port)
        {
            if(cNetworkService != null)
            {
                Debug.Log("CNetUnityService : 이미 서버와 연결됐습니다");
                return;
            }

            cNetworkService = new CNetworkService();
            CConnector cConnector = new CConnector(cNetworkService);
            cConnector.callback_on_connected += On_connected_server;
            IPEndPoint remote_endPoint = new IPEndPoint(IPAddress.Parse(host), port);
            cConnector.Connect(remote_endPoint);
        }
        private void On_connected_server(CUserToken token)
        {
            cRemoteServerPeer = new CRemoteServerPeer(token);
            ((CRemoteServerPeer)cRemoteServerPeer).Set_netEventQueuer(netEventQueuer);
            netEventQueuer.Enqueue_network_event(NetworkEvent.connected);
        }

        private void Update()
        {
            while(netEventQueuer.Has_network_event())
            {
                cNetworkManager.On_status_changed(netEventQueuer.Dequeue_network_event());
            }
            while(netEventQueuer.Has_network_message())
            {
                CPacket msg = netEventQueuer.Dequeue_network_message();
                cNetworkManager.On_message(msg);
                CPacket.Push_back(msg);
            }
        }
        public void Send(CPacket msg)
        {
            try
            {
                cRemoteServerPeer.Send(msg);
                CPacket.Push_back(msg);

                //Debug
                {   
                    //Debug.Log(CPacketBufferManager.Count);
                }
            }
            catch(Exception e)
            {
                Debug.Log($"CNetUnityService : {e.Message}");
            }
        }
        public bool is_connected()
        {
            return cNetworkService! != null;
        }
        private void OnApplicationQuit()
        {
            ((CRemoteServerPeer)cRemoteServerPeer).token.Disconnect();
        }

    }
}



