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
        private ConsoleAsync m_Console;
        public bool isShutdown { get; private set; }

        public void Start()
        {
            Console.WriteLine("Server running...");
            m_Console = new ConsoleAsync();
            ServerNetManager.Bind("127.0.0.1", 8888);
            isShutdown = false;
            ListenProtoc();
        }

        public void Update()
        {
            string cmd = m_Console.TryReadLine();
            ParseCommand(cmd);
            ServerNetManager.Update();
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

        private void ListenProtoc()
        {
            CombatMatchListener.instance.AddListener(delegate (Socket cfd, CombatMatch combatMatch)
            {
                var res = new CombatMatchRes
                {
                    RoomId = 1,
                };
                ServerNetManager.Send(cfd, (Int16)ProtocType.CombatMatchRes, res);
            });
        }
    }
}
