using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuctionData
{
    public const Int32 auctionItemCount = 4;
    private List<GameItem> m_Items = new List<GameItem>();
    private List<Int32> m_Prices = new List<Int32>();

    public Int32 m_CurrIdx { get; private set; } = 0;
    public CombatGameCenter.Player currentCaller { get; private set; }
    
    public void Reset(Int32 seed)
    {
        m_Items.Clear();
        m_Prices.Clear();
        System.Random random = new System.Random(seed);
        // 添加非胜利道具的拍卖品
        Int32 itemCount = 0;
        Int32 targetCount = auctionItemCount - 1;
        while(itemCount < targetCount)
        {
            var item = GameItem.Random(random.Next());
            if(item.type != GameItem.ItemType.Win)
            {
                itemCount++;
                m_Items.Add(item);
                m_Prices.Add(GetUpsetPrice(item.type));
            }
        }
        // 添加胜利道具拍卖品
        var winItem = new GameItem { type = GameItem.ItemType.Win };
        m_Items.Add(winItem);
        m_Prices.Add(GetUpsetPrice(winItem.type));

        m_CurrIdx = 0;
        currentCaller = null;
    }
    public void Clear()
    {
        m_Items.Clear();
        m_Prices.Clear();
    }
    public Int32 GetUpsetPrice(GameItem.ItemType type)
    {
        if (m_TblUpsetPrice.ContainsKey(type))
        {
            return m_TblUpsetPrice[type];
        }
        else
        {
            return 0;
        }
    }
    public Int32 GetPrice(Int32 idx)
    {
        if(Utility.IsInRange(idx, 0, m_Prices.Count - 1))
        {
            return m_Prices[idx];
        }
        return 0;
    }
    public GameItem GetItem(Int32 idx)
    {
        if(Utility.IsInRange(idx, 0, m_Items.Count - 1))
        {
            return m_Items[idx];
        }
        return GameItem.empty;
    }
    public bool IsEnd()
    {
        return (m_CurrIdx >= m_Items.Count);
    }
    public GameItem GetCurrentItem()
    {
        return GetItem(m_CurrIdx);
    }
    public Int32 GetCurrentPrice()
    {
        return GetPrice(m_CurrIdx);
    }
    public void Next()
    {
        m_CurrIdx = Math.Min(m_CurrIdx + 1, m_Items.Count);
        currentCaller = null;
    }
    public bool RisePrice(CombatGameCenter.Player caller, Int32 value)
    {
        if (IsEnd()) return false;    // 无拍卖品
        if (currentCaller != null && caller.id == currentCaller.id) return false;   // 重复叫价
        m_Prices[m_CurrIdx] += value;
        currentCaller = caller;
        return true;
    }

    private static readonly Dictionary<GameItem.ItemType, Int32> m_TblUpsetPrice = new Dictionary<GameItem.ItemType, Int32>
    {
        { GameItem.ItemType.Win, 3000 },
        { GameItem.ItemType.Storehouse, 30 },
    };
}

public struct GameItem
{
    public ItemType type;

    public static GameItem empty {
        get
        {
            return new GameItem
            {
                type = ItemType.LastItem
            };
        }
    }
    private static Int32 ItemTypeCount()
    {
        return (Int32)ItemType.LastItem;
    }
    public static GameItem Random(Int32 key)
    {
        ItemType type = (ItemType)(key % ItemTypeCount());
        return new GameItem
        {
            type = type,
        };
    }

    public bool IsEmpty()
    {
        return (type == ItemType.LastItem);
    }
    public string GetName()
    {
        if (m_TblItemName.ContainsKey(type))
        {
            return m_TblItemName[type];
        }
        return m_TblItemName[ItemType.LastItem];
    }

    private static readonly Dictionary<ItemType, string> m_TblItemName = new Dictionary<ItemType, string>
    {
        { ItemType.Win, "古老皇冠" },
        { ItemType.Storehouse, "仓库" },
        { ItemType.LastItem, "未知" },
    };

    public enum ItemType : Int32
    {
        Win,
        Storehouse,
        LastItem,
    }
}


