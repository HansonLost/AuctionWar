using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using HamPig.Network;
using AuctionWar;

namespace MainServer
{
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

    public class IdAllocator
    {
        private HashSet<UInt32> m_IdSet = new HashSet<UInt32>();
        private UInt32 m_MinId;
        private UInt32 m_MaxId;
        private UInt32 m_MaxCount;
        private UInt32 m_NextId;

        public IdAllocator(UInt32 min, UInt32 max)
        {
            this.m_MinId = min;
            this.m_MaxId = max;
            this.m_MaxCount = max - min + 1;
            this.m_NextId = min;
        }

        public bool IsFull() { return m_IdSet.Count >= m_MaxCount; }

        public UInt32 GetId()
        {
            if (this.IsFull()) return 0;
            while (m_IdSet.Contains(m_NextId))
            {
                m_NextId = (m_NextId - m_MinId + 1) % m_MaxCount + m_MinId;
            }
            m_IdSet.Add(m_NextId);
            return m_NextId;
        }
        public void RestoreId(UInt32 id)
        {
            if (m_IdSet.Contains(id))
            {
                m_IdSet.Remove(id);
            }
        }
    }

    public class CombatRoomSystem
    {
        private Queue<Socket> m_MatchingSet;
        private Dictionary<UInt32, CombatRoom> m_RoomSet;
        private IdAllocator m_RoomAllocator = new IdAllocator(1, 10000);
        
        public CombatRoomSystem()
        {
            m_MatchingSet = new Queue<Socket>();
            m_RoomSet = new Dictionary<UInt32, CombatRoom>();
        }

        public void JoinCombatMatch(Socket cfd)
        {
            m_MatchingSet.Enqueue(cfd);
            if(m_MatchingSet.Count >= CombatRoom.MAX_PLAYER)
            {
                var roomId = m_RoomAllocator.GetId();
                var room = new CombatRoom(roomId);
                for (int i = 0; i < CombatRoom.MAX_PLAYER; i++)
                {
                    var player = m_MatchingSet.Dequeue();
                    room.AddPlayer(player);
                }
                m_RoomSet.Add(roomId, room);

                for (int i = 0; i < CombatRoom.MAX_PLAYER; i++)
                {
                    room.ForEachPlayer(delegate (Socket player)
                    {
                        Int16 key = (Int16)ProtocType.CombatMatchRes;
                        var msg = new CombatMatchRes
                        {
                            RoomId = roomId,
                        };
                        ServerNetManager.Send(player, key, msg);
                    });
                }
            }
        }
    }
}
