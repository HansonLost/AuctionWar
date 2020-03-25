using System.Collections;
using System.Collections.Generic;
using System;
using HamPig.Network;
using AuctionWar;

// 网络同步数据
public class CombatGameCenter
{
    private readonly Random m_Random;
    public CombatGameCenter(Int32 seed)
    {
        m_Random = new Random(seed);
    }
    public void Awake()
    {
        questMarket.Reset(m_Random.Next());
        ResetPlayer();
    }

    public QuestMarket questMarket { get; private set; } = new QuestMarket();

    private Dictionary<Int32, Player> m_Players = new Dictionary<int, Player>();
    private Int32 m_SelfKey;
    private void ResetPlayer()
    {
        m_Players.Clear();
        for (int i = 1; i <= 2; i++)
        {
            m_Players.Add(i, new Player(i));
        }
        m_SelfKey = MatchSystem.instance.selfId;
    }
    public Player GetPlayer(Int32 id)
    {
        m_Players.TryGetValue(id, out Player res);
        return res;
    }
    public Player GetSelfPlayer()
    {
        return GetPlayer(MatchSystem.instance.selfId);
    }

    public struct Quest
    {
        public String name { get; private set; }
        public List<Material> materials { get; private set; }
        public Int32 processSecond{ get; private set; }
        public Int32 reward { get; private set; }

        public static readonly List<Quest> m_PresetQuests = new List<Quest>
        {
            new Quest
            {
                name = "木马",
                materials = new List<Material>
                {
                    new Material(Material.Type.Wood, 3),
                    new Material(Material.Type.Iron, 2),
                },
                processSecond = 5,
                reward = 25,
            },
            new Quest
            {
                name = "石砖",
                materials = new List<Material>
                {
                    new Material(Material.Type.Stone, 4),
                    new Material(Material.Type.Fuel, 4),
                },
                processSecond = 8,
                reward = 64,
            },
            new Quest
            {
                name = "花瓶",
                materials = new List<Material>
                {
                    new Material(Material.Type.Clay, 6),
                    new Material(Material.Type.Fuel, 3),
                },
                processSecond = 4,
                reward = 36,
            },
            new Quest
            {
                name = "床",
                materials = new List<Material>
                {
                    new Material(Material.Type.Iron, 4),
                    new Material(Material.Type.Cotton, 5),
                    new Material(Material.Type.Plastic, 2),
                },
                processSecond = 10,
                reward = 110,
            },
        };
        public static readonly Quest empty = new Quest
        {
            name = "error",
            materials = null,
            processSecond = 0,
            reward = 0,
        };

        public static bool IsEmpty(Quest quest)
        {
            return quest.name == "error";
        }
        public static Quest RandomQuest(Int32 seed)
        {
            Int32 index = seed % m_PresetQuests.Count;
            return m_PresetQuests[index];
        }

        public bool IsEmpty() { return Quest.IsEmpty(this); }

    }

    public struct Material
    {
        public Type type { get; private set; }
        public Int32 count { get; private set; }
        public Material(Type type, Int32 count)
        {
            this.type = type;
            this.count = count;
        }
        public enum Type
        {
            Wood,   // 木材
            Iron,   // 钢铁
            Stone,  // 石头
            Fuel,   // 燃料
            Clay,   // 粘土
            Cotton, // 棉花
            Plastic,// 塑料
        }
    }

    public class Player
    {
        public const Int32 maxQuest = 2;
        public Int32 id { get; private set; }
        public Int32 money { get; private set; }
        private List<Quest> m_Quests = new List<Quest>();
        private List<Material> m_Stores = new List<Material>();

        public Action<Int32> onChangeMoney;
        public Action<Quest> onAddQuest;

        public Player(Int32 id)
        {
            this.id = id;
            money = 0;
        }

        public void Invoke()
        {
            onChangeMoney.Invoke(this.money);
        }
        public void SetMoney(Int32 value)
        {
            money = value;
            if (onChangeMoney != null)
                onChangeMoney.Invoke(value);
        }
        public bool IsFullQuest()
        {
            return m_Quests.Count >= maxQuest;
        }
        public void AddQuest(Quest quest)
        {
            if (IsFullQuest()) return;
            m_Quests.Add(quest);
            if (onAddQuest != null)
            {
                onAddQuest.Invoke(quest);
            }
        }
        public void ForEachQuest(Action<Quest> action)
        {
            foreach (var quest in m_Quests)
            {
                action.Invoke(quest);
            }
        }
    }

    public class QuestMarket
    {
        private readonly Int32 m_MaxCountQuest = GameConst.COUNT_QUEST;
        private List<Quest> m_Quests = new List<Quest>();
        private List<QuestStateType> m_QuestStates = new List<QuestStateType>();

        public Action<Int32, Quest> onHandOutQuest;

        public void Reset(Int32 seed)
        {
            Random random = new Random(seed);
            m_Quests.Clear();
            for (int i = 0; i < m_MaxCountQuest; i++)
            {
                Int32 key = random.Next();
                m_Quests.Add(Quest.RandomQuest(key));
                m_QuestStates.Add(QuestStateType.Normal);
            }
        }
        public Quest GetQuest(Int32 index)
        {
            return (IsValidIndex(index) ? m_Quests[index] : Quest.empty);
        }
        public QuestStateType GetQuestState(Int32 index)
        {
            return (IsValidIndex(index) ? m_QuestStates[index] : QuestStateType.None);
        }
        public Quest HandOutQuest(Int32 index)
        {
            if(IsValidIndex(index) && m_QuestStates[index] == QuestStateType.Normal)
            {
                m_QuestStates[index] = QuestStateType.HandOut;
                if (onHandOutQuest != null)
                    onHandOutQuest.Invoke(index, m_Quests[index]);
                return m_Quests[index];
            }
            
            return Quest.empty;
        }

        private bool IsValidIndex(Int32 index) { return (index >= 0 || index < m_Quests.Count); }

        public enum QuestStateType
        {
            None,
            Normal,
            HandOut,
            Finish,
        };
    }
}


