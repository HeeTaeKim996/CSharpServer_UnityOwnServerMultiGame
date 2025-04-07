using FreeNet;


namespace GameServer
{
    class Program
    {
        private static List<CGameUser> userList = new List<CGameUser>();
        private static object cs_userList = new object();
        public static CGameServer cGameServer = new CGameServer();

        static void Main(string[] args)
        {
            CPacketBufferManager.Initialize(2_000);

            CNetworkService cNetworkService = new CNetworkService();
            cNetworkService.session_created_callback += On_session_created;
            cNetworkService.Initialize();
            cNetworkService.Listen("0.0.0.0", 7979, 100);

            Console.WriteLine("Server Started");
            while (true)
            {
                Thread.Sleep(1_000);
            }
        }

        private static void On_session_created(CUserToken token)
        {
            CGameUser cGameUser = new CGameUser(token);
            lock (cs_userList)
            {
                userList.Add(cGameUser);
            }
            cGameServer.Enter_lobby(cGameUser);
        }

        public static void Remove_user(CGameUser cGameUser)
        {
            lock (cs_userList)
            {
                userList.Remove(cGameUser);
            }
        }
    }
}

