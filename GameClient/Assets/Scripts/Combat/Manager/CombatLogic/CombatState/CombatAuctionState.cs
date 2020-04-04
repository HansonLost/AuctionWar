using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AuctionWar;

public class AuctionState : CombatManager.IState
{
    public const Int32 NOBODY_CALL_TIME = 10;
    public const Int32 ANYBODY_CALL_TIME = 3;

    public CombatGameCenter gameCenter { get { return CombatManager.instance.gameCenter; } }
    private GameObject m_View;
    private Int32 m_StartSeq;

    /// <summary>
    /// 逻辑事件 - 更新状态。float - 剩余时间
    /// </summary>
    public Action<float> onUpdate;
    /// <summary>
    /// 逻辑事件 - 切换拍卖品。
    /// </summary>
    public Action onNextAuctionItem;
    /// <summary>
    /// 操作事件 - 叫价。Int32 - 叫价玩家ID
    /// </summary>
    public Action<Int32> onRisePrice;

    // --- system --- //
    public void Reset(Int32 seq)
    {
        m_StartSeq = seq;
        gameCenter.auction.Reset(MatchSystem.instance.random.Next());
        onNextAuctionItem?.Invoke();
        return;
    }
    public void LoadResource()
    {
        if (m_View == null)
        {
            m_View = CanvasManager.instance.CreatePanel(CanvasManager.PanelLevelType.Normal, "Combat/PnlAuctionView");
        }
        BindCommand();
    }
    public void ReleaseResource()
    {
        if (m_View == null) return;
        GameObject.Destroy(m_View);
        m_View = null;
        RemoveCommand();
    }
    public CombatManager.StateType LogicUpdate(Int32 seq)
    {
        CombatManager.StateType nextState = CombatManager.StateType.Auction;
        AuctionData auction = gameCenter.auction;
        float dt = CombatFrameManager.GetIntervalTime(m_StartSeq, seq);
        float leftTime = 0;
        if(auction.currentCaller == null)
        {
            // 无人叫价
            leftTime = Math.Max(NOBODY_CALL_TIME - dt, 0);
            if(dt >= NOBODY_CALL_TIME)
            {
                // 流拍
                auction.Next();
                m_StartSeq = seq;
                onNextAuctionItem?.Invoke();
            }
        }
        else
        {
            // 已有人叫价
            leftTime = Math.Max(ANYBODY_CALL_TIME - dt, 0);
            if(dt >= ANYBODY_CALL_TIME)
            {
                // 竞拍成功
                var player = auction.currentCaller;
                var price = auction.GetCurrentPrice();
                player.SetMoney(player.money - price);
                ReceiveAuctionItem(player.id, auction.GetCurrentItem());
                auction.Next();
                m_StartSeq = seq;
                onNextAuctionItem?.Invoke();
            }
        }
        if (auction.IsEnd())
        {
            nextState = CombatManager.StateType.Operation;
        }
        onUpdate?.Invoke(leftTime);
        return nextState;
    }

    private void BindCommand()
    {
        CmdAuctionRisePriceListener.instance.AddListener(this.RisePrice);
    }
    private void RemoveCommand()
    {
        CmdAuctionRisePriceListener.instance.RemoveListener(this.RisePrice);
    }

    // --- command --- //
    public void TryPass()
    {
        Pass(MatchSystem.instance.selfId);
    }
    public void TryRisePrice(Int32 gap)
    {
        CombatFrameManager.instance.SendCommand(
             GameConst.CommandType.AuctionRisePrice,
             MatchSystem.instance.selfId,
             new CmdAuctionRisePrice
             {
                 Gap = gap,
             });
    }

    // --- callback --- //
    private void Pass(Int32 playerId)
    {
        Debug.Log("PASS 功能未实现");
    }
    private void RisePrice(Int32 playerId, CmdAuctionRisePrice param)
    {
        var player = gameCenter.playerSet.GetPlayer(playerId);
        var auction = gameCenter.auction;
        bool isSucceed = auction.RisePrice(player, param.Gap);
        if (isSucceed)
        {
            m_StartSeq = CombatFrameManager.instance.seq;
            onRisePrice?.Invoke(playerId);
        }
    }
    
    // --- other --- //
    private void ReceiveAuctionItem(Int32 playerId, GameItem item)
    {
        Debug.Log(String.Format("玩家{0}获得道具{1}", playerId, item.GetName()));
    }
}



