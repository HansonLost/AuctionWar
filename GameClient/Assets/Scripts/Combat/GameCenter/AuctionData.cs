using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuctionData
{
    public const Int32 auctionItemCount = 4;
    private List<CombatProp> m_Props = new List<CombatProp>();
    private List<Int32> m_Prices = new List<Int32>();
    private HashSet<Int32> m_PassPlayers = new HashSet<Int32>();

    public Int32 m_CurrIdx { get; private set; } = 0;
    public CombatGameCenter.Player currentCaller { get; private set; }
    
    public void Reset(Int32 seed)
    {
        Clear();
        System.Random random = new System.Random(seed);
        Int32 winPropId = 1;    // 胜利道具ID
        // 添加非胜利道具的拍卖品
        Int32 itemCount = 0;
        Int32 targetCount = auctionItemCount - 1;
        while(itemCount < targetCount)
        {
            var prop = CombatPropHelper.RandomProp(random.Next());
            if(prop.GetId() != winPropId)
            {
                itemCount++;
                m_Props.Add(prop);
                m_Prices.Add(prop.GetUpsetPrice());
            }
        }
        // 添加胜利道具拍卖品
        var winProp = CombatPropHelper.CreateProp(winPropId);
        m_Props.Add(winProp);
        m_Prices.Add(winProp.GetUpsetPrice());

        m_CurrIdx = 0;
        currentCaller = null;
    }
    public void Clear()
    {
        m_Props.Clear();
        m_Prices.Clear();
    }
    public CombatProp GetProp(Int32 idx)
    {
        if(Utility.IsInRange(idx, 0, m_Props.Count - 1))
        {
            return m_Props[idx];
        }
        return null;
    }
    public CombatProp GetCurrentProp()
    {
        return GetProp(m_CurrIdx);
    }
    public bool IsEnd()
    {
        return (m_CurrIdx >= m_Props.Count);
    }
    public Int32 GetPrice(Int32 idx)
    {
        if (Utility.IsInRange(idx, 0, m_Prices.Count - 1))
        {
            return m_Prices[idx];
        }
        return 0;
    }
    public Int32 GetCurrentPrice()
    {
        return GetPrice(m_CurrIdx);
    }
    public void Next()
    {
        m_CurrIdx = Math.Min(m_CurrIdx + 1, m_Props.Count);
        m_PassPlayers.Clear();
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
    public void Pass(Int32 playerId)
    {
        if (m_PassPlayers.Contains(playerId)) return;
        m_PassPlayers.Add(playerId);
    }
    public Int32 GetPassPlayerCount() { return m_PassPlayers.Count; }
}


