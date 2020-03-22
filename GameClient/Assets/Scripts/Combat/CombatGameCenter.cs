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

    private List<Quest> m_Quest = new List<Quest>();

    public void ResetQuest()
    {
        m_Quest.Clear();
        for (int i = 0; i < GameConst.COUNT_QUEST; i++)
        {
            Int32 key = m_Random.Next();
            m_Quest.Add(Quest.RandomQuest(key));
        }
    }
    public Quest GetQuest(Int32 index)
    {
        if (index < 0 || index >= m_Quest.Count)
            return Quest.empty;
        return m_Quest[index];
    }

    public struct Quest
    {
        public String name { get; private set; }
        public List<Material> materals { get; private set; }
        public Int32 processSecond{ get; private set; }
        public Int32 reward { get; private set; }

        public static readonly List<Quest> m_PresetQuests = new List<Quest>
        {
            new Quest
            {
                name = "木马",
                materals = new List<Material>
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
                materals = new List<Material>
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
                materals = new List<Material>
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
                materals = new List<Material>
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
            materals = null,
            processSecond = 0,
            reward = 0,
        };

        public static bool IsEmpty(Quest quest)
        {
            return quest.name == "error";
        }
        public static Quest RandomQuest(Int32 randomKey)
        {
            Int32 index = randomKey % m_PresetQuests.Count;
            return m_PresetQuests[index];
        }

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
}


