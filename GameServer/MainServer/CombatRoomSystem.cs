using System;
using System.Collections.Generic;
using System.Net.Sockets;
using HamPig;
using HamPig.Network;
using AuctionWar;

namespace MainServer
{
    public class CombatRoomSystem
    {
        private HashSet<Socket> m_MatchingSet = new HashSet<Socket>();
        private Dictionary<UInt32, CombatRoom> m_RoomSet = new Dictionary<UInt32, CombatRoom>();
        private Dictionary<Socket, UInt32> m_MapPlayerToRoom = new Dictionary<Socket, UInt32>();
        private IdAllocator m_RoomAllocator = new IdAllocator(1, 10000);

        public void Update()
        {
            this.Matching();
        }
        public void JoinCombatMatch(Socket cfd)
        {
            if (m_MatchingSet.Contains(cfd)) return;
            Console.WriteLine("One client is going to match.");
            m_MatchingSet.Add(cfd);
        }
        public void Matching()
        {
            /* 两两匹配 */
            var resList = new List<MatchingResult>();
            var players = new List<Socket>();
            foreach (var player in m_MatchingSet)
            {
                players.Add(player);
                if(players.Count >= CombatRoom.MAX_PLAYER)
                {
                    var res = new MatchingResult
                    {
                        roomId = m_RoomAllocator.GetId(),
                        players = players,
                    };
                    players = new List<Socket>();
                    resList.Add(res);
                }
            }

            /* 创建房间 */
            foreach (var res in resList)
            {
                var room = new CombatRoom(res.roomId);
                foreach (var player in res.players)
                {
                    m_MatchingSet.Remove(player);
                    room.AddPlayer(player);
                    m_MapPlayerToRoom.Add(player, res.roomId);
                    ServerNetManager.Send(player, (Int16)ProtocType.CombatMatchRes, new CombatMatchRes
                    {
                        RoomId = res.roomId,
                    });
                }
                m_RoomSet.Add(res.roomId, room);
            }
        }
        public void CancelCombatMatch(Socket cfd)
        {
            /* 若匹配队列没有该玩家，证明已经有匹配结果，不需要额外发送别的信息。 */
            if (m_MatchingSet.Contains(cfd))
            {
                Console.WriteLine("One client cancel matching.");
                m_MatchingSet.Remove(cfd);
                ServerNetManager.Send(cfd, (Int16)ProtocType.CombatMatchRes, new CombatMatchRes
                {
                    RoomId = CommonConst.ROOM_ERROR,
                });
            }
        }
        public void QuitCombat(Socket cfd)
        {
            if (!m_MapPlayerToRoom.ContainsKey(cfd)) return;
            var roomId = m_MapPlayerToRoom[cfd];
            var room = m_RoomSet[roomId];
        }

        public class MatchingResult
        {
            public UInt32 roomId;
            public List<Socket> players;
        }

        public class CombatRoom
        {
            public const Int32 MAX_PLAYER = 2;
            private List<Socket> m_PLayerList = new List<Socket>();
            public UInt32 roomId { get; private set; }
            public CombatRoom(UInt32 roomId)
            {
                this.roomId = roomId;
            }
            public void AddPlayer(Socket cfd)
            {
                if (m_PLayerList.Count >= MAX_PLAYER) return;
                m_PLayerList.Add(cfd);
            }
            public void ForEachPlayer(Action<Socket> func)
            {
                foreach (var player in m_PLayerList)
                {
                    func(player);
                }
            }
        }
    }
}
