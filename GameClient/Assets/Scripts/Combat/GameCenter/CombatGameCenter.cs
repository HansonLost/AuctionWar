using System.Collections;
using System.Collections.Generic;
using System;
using HamPig.Network;
using AuctionWar;

// 网络同步数据
public partial class CombatGameCenter
{
    public PlayerSet playerSet { get; private set; } = new PlayerSet();
    public QuestMarket questMarket { get; private set; } = new QuestMarket();
    public MaterialMarket materialMarket { get; private set; } = new MaterialMarket();
    public AuctionData auction { get; private set; } = new AuctionData();

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
        public string name { get; private set; }
        public Int32 count { get; private set; }

        private static Dictionary<CombatGameCenter.Material.Type, String> m_TypeToName = new Dictionary<CombatGameCenter.Material.Type, string>
        {
            { Type.Wood,            "木材" },
            { Type.Iron,            "钢铁" },
            { Type.Stone,           "石头" },
            { Type.Fuel,            "燃料" },
            { Type.Clay,            "粘土" },
            { Type.Cotton,          "棉花" },
            { Type.Plastic,         "塑料" },
            { Type.LastMaterial,    "虚无" },
        };
        public static readonly Material empty = new Material(Type.LastMaterial, 0);

        public Material(Type type, Int32 count)
        {
            this.type = type;
            this.count = count;
            this.name = m_TypeToName[type];
        }
        public bool IsEmpty() { return this.type == Type.LastMaterial; }
        public enum Type : Int32
        {
            Wood,   // 木材
            Iron,   // 钢铁
            Stone,  // 石头
            Fuel,   // 燃料
            Clay,   // 粘土
            Cotton, // 棉花
            Plastic,// 塑料
            LastMaterial,    // 中止类型
        }
    }

    public class Player
    {
        public const Int32 maxQuest = 2;
        public Int32 id { get; private set; }
        public Int32 money { get; private set; }
        public ProcessFactory processFactory { get; private set; } = new ProcessFactory();
        public Int32 storehouseCapacity { get; private set; } = GameConst.COUNT_STOREHOUSE_BEGIN;

        private List<CombatProp> m_PropBag = new List<CombatProp>();
        private List<Wholesale> m_Wholesaler = new List<Wholesale>();
        private List<Quest> m_Quests = new List<Quest>();
        private List<Material> m_Storehouses = new List<Material>();

        public Action<Int32> onChangeMoney;
        public Action<Quest> onAddQuest;
        public Action<Material> onAddMaterial;

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
        public bool IsFullStorehouse()
        {
            return m_Storehouses.Count >= storehouseCapacity;
        }
        public Material GetStorehouse(Int32 idx)
        {
            if(Utility.IsInRange(idx, 0, m_Storehouses.Count - 1))
            {
                return m_Storehouses[idx];
            }
            return Material.empty;
        }
        public void SetStorehouseCapacity(Int32 value)
        {
            storehouseCapacity = value;
        }
        public void SetStorehouse(Int32 idx, Material mat)
        {
            if (Utility.IsInRange(idx, 0, m_Storehouses.Count - 1) &&
                mat.type != Material.Type.LastMaterial &&
                mat.count > 0)
            {
                m_Storehouses[idx] = mat;
            }
        }
        public void RemoveStorehouse(Int32 idx)
        {
            if(Utility.IsInRange(idx, 0, m_Storehouses.Count - 1))
            {
                m_Storehouses.RemoveAt(idx);
            }
        }
        public void AddStorehouse(Material mat)
        {
            if (!IsFullStorehouse())
            {
                m_Storehouses.Add(mat);
            }
        }
        
        public void AddMaterial(Material mat)
        {
            if (IsFullStorehouse()) return;
            m_Storehouses.Add(mat);
            if(onAddMaterial != null)
            {
                onAddMaterial.Invoke(mat);
            }
        }
        public void ForEachQuest(Action<Quest> action)
        {
            foreach (var quest in m_Quests)
            {
                action.Invoke(quest);
            }
        }
        public void ForEachMaterial(Action<Material> action)
        {
            foreach (var mat in m_Storehouses)
            {
                action.Invoke(mat);
            }
        }

        /* 需求操作 */
        public void ClearQuest()
        {
            m_Quests.Clear();
        }
        public Quest GetQuest(Int32 idx)
        {
            if (Utility.IsInRange(idx, 0, m_Quests.Count - 1))
            {
                return m_Quests[idx];
            }
            return Quest.empty;
        }
        public void AddQuest(Quest quest)
        {
            if (IsFullQuest()) return;
            m_Quests.Add(quest);
            processFactory.AddQuest(quest);
            onAddQuest?.Invoke(quest);
        }
        public void RemoveQuest(Int32 idx)
        {
            if (Utility.IsInRange(idx, 0, m_Quests.Count - 1))
            {
                m_Quests.RemoveAt(idx);
            }
        }

        /* 批发商操作 */
        public void ClearWholesaler()
        {
            m_Wholesaler.Clear();
        }
        public void AddWholesale(Material mat, Int32 price)
        {
            m_Wholesaler.Add(new Wholesale
            {
                material = mat,
                price = price,
                isSellout = false,
            });
        }
        public Int32 WholesaleCount()
        {
            return m_Wholesaler.Count;
        }
        public bool IsFullWholesale()
        {
            return m_Wholesaler.Count >= GameConst.COUNT_WHOLESALE;
        }
        public Wholesale GetWholesale(Int32 index)
        {
            if(Utility.IsInRange(index, 0, m_Wholesaler.Count - 1))
            {
                return m_Wholesaler[index];
            }
            else
            {
                return new Wholesale
                {
                    material = Material.empty,
                    price = 0,
                    isSellout = true,
                };
            }
        }
        public void BuyWholesale(Int32 index)
        {
            var wholesale = GetWholesale(index);
            if (!wholesale.isSellout)
            {
                wholesale.isSellout = true;
                m_Wholesaler[index] = wholesale;
            }
        }
        public void ForEachWholesaler(Action<Material, Int32, bool> action)
        {
            foreach (var wholesale in m_Wholesaler)
            {
                action.Invoke(wholesale.material, wholesale.price, wholesale.isSellout);
            }
        }

        /* 道具背包操作 */
        public void AddProp(CombatProp prop)
        {
            m_PropBag.Add(prop);
        }
        public void RemoveProp(CombatProp prop)
        {
            prop.OnDiscard();
            m_PropBag.Remove(prop);
        }
        public void ForEachProp(Action<CombatProp> action)
        {
            foreach (var prop in m_PropBag)
            {
                action?.Invoke(prop);
            }
        }

        public struct Wholesale
        {
            public Material material;
            public Int32 price;
            public bool isSellout;
        }
    }

    public class PlayerSet
    {
        private Dictionary<Int32, Player> m_Players = new Dictionary<int, Player>();

        public void ResetPlayer()
        {
            m_Players.Clear();
            for (int i = 1; i <= MatchSystem.instance.playerCount; i++)
            {
                m_Players.Add(i, new Player(i));
            }
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
        public void ForEachPlayer(Action<Player> action)
        {
            foreach (var pair in m_Players)
            {
                action.Invoke(pair.Value);
            }
        }
    }

    public class QuestMarket
    {
        private readonly Int32 m_MaxCountQuest = GameConst.COUNT_QUEST;
        private List<Quest> m_Quests = new List<Quest>();
        private List<QuestStateType> m_QuestStates = new List<QuestStateType>();

        public Action<Int32, Quest> onHandOutQuest;

        public void RefreshQuest(Int32 seed)
        {
            Random random = new Random(seed);
            m_Quests.Clear();
            m_QuestStates.Clear();
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

    public class MaterialMarket
    {
        public const Int32 maxCountMarket = GameConst.COUNT_MARKET;
        private List<Material> m_Materials = new List<Material>();
        private List<StateType> m_MatStates = new List<StateType>();


        /// <summary>
        /// 买了第[0]个，花了[1]金币，材料为[3]
        /// </summary>
        public Action<Int32, Int32, Material> onBuyMaterial { get; set; }

        public void RefreshMarket(Int32 playerId, Int32 seed)
        {
            // 原料市场是独立的，只刷新自己的市场
            if (MatchSystem.instance.selfId != playerId) return;

            Random random = new Random(seed);
            m_Materials.Clear();
            m_MatStates.Clear();
            for (int i = 0; i < maxCountMarket; i++)
            {
                Material.Type type = (Material.Type)(random.Next() % (Int32)Material.Type.LastMaterial);
                Int32 count = random.Next() % 10 + 1;   // 1 - 10
                m_Materials.Add(new Material(type, count));
                m_MatStates.Add(StateType.Sell);
            }
        }
        public Material GetMaterial(Int32 idx)
        {
            return (IsValidIndex(idx) ? m_Materials[idx] : Material.empty);
        }
        public Int32 GetPrice(Material mat)
        {
            // 所有材料统一单价 1 金币
            return mat.count * 1;
        }
        public Int32 GetPrice(Int32 idx)
        {
            return (IsValidIndex(idx) ? GetPrice(m_Materials[idx]) : 0);
        }
        public StateType GetState(Int32 idx)
        {
            return (IsValidIndex(idx) ? m_MatStates[idx] : StateType.None);
        }
        public Material BuyMaterial(Int32 idx)
        {
            if(IsValidIndex(idx)&& m_MatStates[idx] == StateType.Sell)
            {
                m_MatStates[idx] = StateType.SellOut;
                var mat = m_Materials[idx];
                if (onBuyMaterial != null)
                    onBuyMaterial.Invoke(idx, GetPrice(mat), mat);
                return mat;
            }
            return Material.empty;
        }

        private bool IsValidIndex(Int32 idx) { return (idx >= 0 && idx < m_Materials.Count); }

        public enum StateType
        {
            Sell,       // 出售中
            SellOut,    // 售空
            None,
        }
    }
}


