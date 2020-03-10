using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HamPig.Network;
using HamPig;
using AuctionWar;
using System.Net.Sockets;

namespace MainServer
{
    public class GameServer
    {
        private ConsoleAsync m_Console = new ConsoleAsync();
        private Dictionary<Socket, Client> m_Clients = new Dictionary<Socket, Client>();
        private CombatRoomSystem m_SysCombatRoom = new CombatRoomSystem();
        public bool isShutdown { get; private set; }

        public void Start()
        {
            Console.WriteLine("Server running...");
            isShutdown = false;
            this.AwakeServer();
            this.ListenProtoc();
        }

        public void Update()
        {
            string cmd = m_Console.TryReadLine();
            ParseCommand(cmd);
            ServerNetManager.Update();
            Timer.Update();
            this.RemoveDeadClient();
        }

        public void Shutdown()
        {

        }

        private void ParseCommand(String cmd)
        {
            if (cmd == null) return;
            if(cmd == "exit")
            {
                isShutdown = true;
            }
        }

        private void RemoveDeadClient()
        {
            List<Socket> listRemove = new List<Socket>();
            foreach (var item in m_Clients)
            {
                // TODO 暂时写死 8 秒为死亡秒数
                var client = item.Value;
                if(Timer.time - client.lastBeat >= 8.0f)
                {
                    listRemove.Add(item.Key);
                }
            }
            foreach (var key in listRemove)
            {
                m_Clients.Remove(key);
                Console.WriteLine("one client has dead by heart beat.");
            }
        }

        private void AwakeServer()
        {
            ServerNetManager.onAccept += delegate (Socket cfd)
            {
                if (m_Clients.ContainsKey(cfd))
                {
                    Console.WriteLine("client has been in online list.");
                    return;
                }
                Console.WriteLine("new client has connected to server.");
                m_Clients.Add(cfd, new Client { cfd = cfd, lastBeat = Timer.time });
            };
            ServerNetManager.Bind("127.0.0.1", 8888);
        }

        private void ListenProtoc()
        {
            CombatMatchListener.instance.AddListener(this.CombatMatch);
            HeartbeatListener.instance.AddListener(this.AnswerHeartBeat);
        }

        private void CombatMatch(Socket cfd, CombatMatch combatMatch)
        {
            m_SysCombatRoom.JoinCombatMatch(cfd);
        }

        private void AnswerHeartBeat(Socket cfd, Heartbeat heartbeat)
        {
            ServerNetManager.Send(cfd, (Int16)ProtocType.Heartbeat, new Heartbeat());
            if (m_Clients.ContainsKey(cfd))
            {
                m_Clients[cfd].lastBeat = Timer.time;
            }
        }

        public class Client
        {
            public Socket cfd;
            public float lastBeat;
        }
    }
}
