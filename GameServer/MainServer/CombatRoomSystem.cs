using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Google.Protobuf;
using HamPig;
using HamPig.Network;
using AuctionWar;

namespace MainServer
{
    public class CombatRoomSystem
    {
        private Random m_Random = new Random();

        private HashSet<Socket> m_MatchingSet = new HashSet<Socket>();
        private Dictionary<UInt32, CombatRoom> m_RoomSet = new Dictionary<UInt32, CombatRoom>();
        private Dictionary<Socket, UInt32> m_MapPlayerToRoom = new Dictionary<Socket, UInt32>();
        private IdAllocator m_RoomAllocator = new IdAllocator(1, 10000);

        public void Awake()
        {
            CombatCommandListener.instance.AddListener(this.AddCombatCommand);
            CombatReadyListener.instance.AddListener(this.PlayerReady);
        }
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
                Int32 seed = m_Random.Next();
                Int32 playerId = 1;
                foreach (var player in res.players)
                {
                    m_MatchingSet.Remove(player);
                    room.AddPlayer(player);
                    m_MapPlayerToRoom.Add(player, res.roomId);
                    ServerNetManager.Send(player, (Int16)ProtocType.CombatMatchRes, new CombatMatchRes
                    {
                        RoomId = res.roomId,
                        Seed = seed,
                        SelfId = playerId,
                    });
                    playerId++;
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
            var room = this.GetPlayerRoom(cfd);
            if (room == null) return;
            Console.WriteLine(String.Format("房间 {0}：{1}号玩家退出对战", room.roomId, room.GetPlayerId(cfd)));
            room.Clear();
            m_RoomSet.Remove(room.roomId);
            m_MapPlayerToRoom.Remove(cfd);
        }

        private void AddCombatCommand(Socket cfd, CombatCommand combatCommand)
        {
            var room = this.GetPlayerRoom(cfd);
            if (room == null) return;
            room.AddCommand(cfd, combatCommand.Id, combatCommand.Parm);
        }
        private void PlayerReady(Socket cfd, CombatReady combatReady)
        {
            var room = this.GetPlayerRoom(cfd);
            if (room == null) return;
            room.ReadyPlayer(cfd);
            if (room.IsAllReady())
            {
                room.Run();
            }
        }
        private CombatRoom GetPlayerRoom(Socket cfd)
        {
            if (!m_MapPlayerToRoom.ContainsKey(cfd)) return null;
            var roomId = m_MapPlayerToRoom[cfd];
            if (!m_RoomSet.ContainsKey(roomId)) return null;
            return m_RoomSet[roomId];
        }

        public class MatchingResult
        {
            public UInt32 roomId;
            public List<Socket> players;
        }

        public class CombatRoom
        {
            public const Int32 MAX_PLAYER = 1;
            public const Int32 ID_PLAYER_ERROR = 0;

            public UInt32 roomId { get; private set; }
            public CombatRoom(UInt32 roomId)
            {
                this.roomId = roomId;
                this.m_NextId = 1;
                this.m_NextSeq = 0;
                this.m_ReadyCount = 0;
            }
            public void Run()
            {
                // TODO 所有玩家准备完毕即可开始
                Timer.CallInterval(CommonConst.FRAME_INTERVAL, this.SendFrame);
            }
            public void Clear()
            {
                foreach (var pair in m_Players)
                {
                    var player = pair.Value;
                    ServerNetManager.Send(player.cfd, (Int16)ProtocType.CombatResult, new CombatResult { });
                }
                m_Players.Clear();
                m_NextId = 1;
            }

            private Int32 m_NextId;
            private Dictionary<Socket, Player> m_Players = new Dictionary<Socket, Player>();
            public void AddPlayer(Socket cfd)
            {
                if (m_Players.Count >= MAX_PLAYER) return;
                m_Players.Add(cfd, new Player
                {
                    cfd = cfd,
                    id = m_NextId,
                });
                m_NextId++;
            }
            public Int32 GetPlayerId(Socket cfd)
            {
                if (!m_Players.ContainsKey(cfd)) return ID_PLAYER_ERROR;
                return m_Players[cfd].id;
            }

            private Int32 m_ReadyCount;
            public void ReadyPlayer(Socket cfd)
            {
                if (!m_Players.ContainsKey(cfd)) return;
                m_Players[cfd].isReady = true;
                m_ReadyCount++;
            }
            public bool IsAllReady() { return (m_ReadyCount == m_Players.Count); }

            private List<Command> m_Commands = new List<Command>();
            public void AddCommand(Socket cfd, Int32 cmdId, ByteString parm)
            {
                if (!m_Players.ContainsKey(cfd)) return;
                var player = m_Players[cfd];
                m_Commands.Add(new Command
                {
                    playerId = player.id,
                    cmdId = cmdId,
                    parm = parm,
                });
            }

            private Int32 m_NextSeq;
            private void SendFrame()
            {
                // 打包帧包
                var frame = new FramePackage
                {
                    Seq = m_NextSeq,
                };
                m_NextSeq++;
                foreach (var cmd in m_Commands)
                {
                    frame.Data.Add(new FramePackage.Types.Command
                    {
                        PlayerId = cmd.playerId,
                        CommandId = cmd.cmdId,
                        Parameter = cmd.parm,
                    });
                }
                // 广播帧包
                foreach (var pair in m_Players)
                {
                    var cfd = pair.Key;
                    ServerNetManager.Send(cfd, (Int16)ProtocType.FramePackage, frame);
                }
                // 清空缓存的命令
                m_Commands.Clear();
            }

            public class Player
            {
                public Socket cfd;
                public Int32 id;
                public bool isReady;
            }

            public class Command
            {
                public Int32 playerId;
                public Int32 cmdId;
                public ByteString parm;
            }
        }
    }
}
