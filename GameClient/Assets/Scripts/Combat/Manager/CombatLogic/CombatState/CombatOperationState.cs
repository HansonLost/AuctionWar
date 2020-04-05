using System;
using System.Collections.Generic;
using UnityEngine;
using AuctionWar;

public partial class CombatManager
{
    public class OperationState : IState
    {
        protected CombatGameCenter gameCenter { get { return CombatManager.instance.gameCenter; } }
        private GameObject m_View;

        // 基本流程
        private Int32 m_StartSeq;
        private readonly Int32 m_FreezenTime = 3;
        private bool m_IsFreezen;
        private readonly Int32 m_OpTime = GameConst.COMBAT_OPERATION_TIME;
        private Int32 m_CurrTime;

        public Action<Int32> onChangeFreezenTime;
        public Action<Int32> onChangeCountdownTime;

        /// <summary>
        /// 操作事件 - 玩家购买材料。Int32 - 玩家ID; Int32 - 采购栏索引
        /// </summary>
        public Action<Int32, Int32> onBuyMaterial;
        /// <summary>
        /// 操作事件 - 玩家把材料放入加工模块中。Int32 - 玩家ID
        /// </summary>
        public Action<Int32> onPutInMaterial;
        /// <summary>
        /// 操作事件 - 玩家刷新材料商店。Int32 - 玩家ID
        /// </summary>
        public Action<Int32> onRefreshMarket;
        /// <summary>
        /// 逻辑事件 - 加工完成。Int32 - 玩家ID
        /// </summary>
        public Action<Int32> onFinishProcess;
        /// <summary>
        /// 逻辑事件 - 更改加工完成时间。Int32 - 玩家ID
        /// </summary>
        public Action<Int32> onChangeProcess;
        /// <summary>
        /// 操作事件 - 玩家出售材料。Int32 - 玩家ID
        /// </summary>
        public Action<Int32> onSellMaterial;

        // --- system --- //
        public void Reset(Int32 seq)
        {
            m_StartSeq = seq;
            m_IsFreezen = true;
            m_CurrTime = 0;

            RefreshGameCenter();
        }
        public void LoadResource()
        {
            if (m_View != null) return;
            m_View = CanvasManager.instance.CreatePanel(CanvasManager.PanelLevelType.Normal, "Combat/PnlOperationView");
            BindCommand();
        }
        public void ReleaseResource()
        {
            if (m_View == null) return;
            GameObject.Destroy(m_View);
            m_View = null;
            RemoveCommand();
        }
        public StateType LogicUpdate(Int32 seq)
        {
            UpdateOperationTime(seq);
            UpdateProcessTime(seq);
            return (m_CurrTime >= m_OpTime ? StateType.Auction : StateType.Operation);
        }

        private void BindCommand()
        {
            CmdBuyMaterialListener.instance.AddListener(this.BuyMaterial);
            CmdPutInMaterialListener.instance.AddListener(this.PutInMaterial);
            CmdRefreshMarketListener.instance.AddListener(this.RefreshMarket);
            CmdSellMaterialListener.instance.AddListener(this.SellMaterial);
        }
        private void RemoveCommand()
        {
            CmdBuyMaterialListener.instance.RemoveListener(this.BuyMaterial);
            CmdPutInMaterialListener.instance.RemoveListener(this.PutInMaterial);
            CmdRefreshMarketListener.instance.RemoveListener(this.RefreshMarket);
            CmdSellMaterialListener.instance.RemoveListener(this.SellMaterial);
        }

        // --- command --- //
        public void TryBuyMaterial(Int32 idx)
        {
            Int32 selfId = MatchSystem.instance.selfId;
            bool isValid = CanPlayerBuyMaterial(selfId, idx);

            if (isValid)
            {
                CombatFrameManager.instance.SendCommand(
                    GameConst.CommandType.BuyMaterial,
                    selfId,
                    new CmdBuyMaterial
                    {
                        Index = idx,
                    });
            }
        }
        public void TryPutInMaterial(Int32 storehouseIdx, Int32 processBlockIdx)
        {
            CombatFrameManager.instance.SendCommand(
                GameConst.CommandType.PutInMaterial,
                MatchSystem.instance.selfId,
                new CmdPutInMaterial
                {
                    Storehouse = storehouseIdx,
                    Processblock = processBlockIdx,
                });
        }
        public void TryRefreshMarket()
        {
            CombatFrameManager.instance.SendCommand(
                GameConst.CommandType.RefreshMarket,
                MatchSystem.instance.selfId,
                new CmdRefreshMarket { });
        }
        public void TrySellMaterial(Int32 storeIdx)
        {
            var player = gameCenter.playerSet.GetSelfPlayer();
            var store = player.GetStorehouse(storeIdx);
            if (!store.IsEmpty())
            {
                CombatFrameManager.instance.SendCommand(
                    GameConst.CommandType.SellMaterial,
                    MatchSystem.instance.selfId,
                    new CmdSellMaterial
                    {
                        Storehouse = storeIdx,
                    });
            }
        }

        // --- callback --- //
        private void BuyMaterial(Int32 playerId, CmdBuyMaterial param)
        {
            Int32 matIdx = param.Index;
            bool isValid = CanPlayerBuyMaterial(playerId, matIdx);
            if (isValid)
            {
                var player = gameCenter.playerSet.GetPlayer(playerId);
                var mat = gameCenter.materialMarket.BuyMaterial(matIdx);
                var price = gameCenter.materialMarket.GetPrice(matIdx);
                player.AddMaterial(mat);
                player.SetMoney(player.money - price);
            }
            onBuyMaterial?.Invoke(playerId, param.Index);
        }
        private void HandOutQuest(Int32 playerId, CmdClaimQuest param)
        {
            Int32 questIdx = param.Index;
            bool isValid = CanPlayerHandOutQuest(playerId, questIdx);
            if (isValid)
            {
                var quest = gameCenter.questMarket.HandOutQuest(questIdx);
                var player = gameCenter.playerSet.GetPlayer(playerId);
                player.AddQuest(quest);
            }
        }
        private void PutInMaterial(Int32 playerId, CmdPutInMaterial param)
        {
            var player = gameCenter.playerSet.GetPlayer(playerId);
            var storehouse = player.GetStorehouse(param.Storehouse);
            if (storehouse.IsEmpty()) return;
            var leftMat = player.processFactory.AddMaterial(param.Processblock, storehouse);
            if (leftMat.count > 0)
            {
                player.SetStorehouse(param.Storehouse, leftMat);
            }
            else
            {
                player.RemoveStorehouse(param.Storehouse);
            }
            onPutInMaterial?.Invoke(playerId);
        }
        private void RefreshMarket(Int32 playerId, CmdRefreshMarket param)
        {
            var player = gameCenter.playerSet.GetPlayer(playerId);
            if(player.money < 10)
            {
                // 刷新商店需要花费10块
                return;
            }
            Int32 seed = MatchSystem.instance.random.Next();
            gameCenter.materialMarket.RefreshMarket(playerId, seed);
            player.SetMoney(player.money - 10);
            onRefreshMarket?.Invoke(playerId);
        }
        private void SellMaterial(Int32 playerId, CmdSellMaterial param)
        {
            var player = gameCenter.playerSet.GetPlayer(playerId);
            if (player == null) return;
            var store = player.GetStorehouse(param.Storehouse);
            if (store.IsEmpty()) return;
            player.SetMoney(player.money + store.count / 2);
            player.RemoveStorehouse(param.Storehouse);
            onSellMaterial?.Invoke(playerId);
        }


        // --- other --- //
        private bool CanPlayerHandOutQuest(Int32 playerId, Int32 questIdx)
        {
            var player = gameCenter.playerSet.GetPlayer(playerId);
            if (player != null && player.IsFullQuest())
                return false;

            var state = gameCenter.questMarket.GetQuestState(questIdx);
            if (state != CombatGameCenter.QuestMarket.QuestStateType.Normal)
                return false;

            return true;
        }
        private bool CanPlayerBuyMaterial(Int32 playerId, Int32 matId)
        {
            var player = gameCenter.playerSet.GetPlayer(playerId);
            var price = gameCenter.materialMarket.GetPrice(matId);
            var state = gameCenter.materialMarket.GetState(matId);
            return (
                player != null &&
                !player.IsFullStorehouse() &&
                state == CombatGameCenter.MaterialMarket.StateType.Sell &&
                player.money >= price);
        }
        private void UpdateOperationTime(Int32 seq)
        {
            float dt = CombatFrameManager.GetIntervalTime(m_StartSeq, seq);
            Int32 dtInt = (Int32)dt;
            if (m_IsFreezen)
            {
                if (dtInt < m_FreezenTime)
                {
                    m_CurrTime = dtInt;
                    if (onChangeFreezenTime != null)
                        onChangeFreezenTime.Invoke(m_FreezenTime - m_CurrTime);
                }
                else
                {
                    m_IsFreezen = false;
                    m_CurrTime = 0;
                    m_StartSeq = seq;
                    if (onChangeCountdownTime != null)
                        onChangeCountdownTime.Invoke(m_OpTime - m_CurrTime);
                }
            }
            else
            {
                if (dtInt != m_CurrTime)
                {
                    m_CurrTime = dtInt;
                    if (onChangeCountdownTime != null)
                        onChangeCountdownTime.Invoke(m_OpTime - m_CurrTime);
                }
            }
        }
        private void UpdateProcessTime(Int32 seq)
        {
            for (int i = 1; i <= MatchSystem.instance.playerCount; i++)
            {
                var player = gameCenter.playerSet.GetPlayer(i);
                List<Int32> rmvList = new List<Int32>();
                player.processFactory.ForEachProcessBlock((CombatGameCenter.ProcessFactory.ProcessBlock block) =>
                {
                    if (block.isRun)
                    {
                        float time = CombatFrameManager.GetIntervalTime(block.startSeq, seq);
                        if(time >= block.quest.processSecond)
                        {
                            rmvList.Add(block.index);
                            player.SetMoney(player.money + block.quest.reward);
                        }
                    }
                });
                for (int offset = 0; offset < rmvList.Count; offset++)
                {
                    player.processFactory.RemoveQuest(rmvList[offset] - offset);
                    player.RemoveQuest(rmvList[offset] - offset);
                }

                onChangeProcess?.Invoke(i);

                if (rmvList.Count > 0)
                {
                    onFinishProcess?.Invoke(i);
                }
            }
        }
        private void RefreshGameCenter()
        {
            var gameCenter = CombatManager.instance.gameCenter;
            Int32 seed = MatchSystem.instance.random.Next();
            System.Random random = new System.Random(seed);
            gameCenter.questMarket.RefreshQuest(random.Next());
            for (int playerId = 1; playerId <= MatchSystem.instance.playerCount; playerId++)
            {
                gameCenter.materialMarket.RefreshMarket(playerId, random.Next());
            }

            // 根据规则，每人每回合会获取投资金
            gameCenter.playerSet.ForEachPlayer((CombatGameCenter.Player player) =>
            {
                player.SetMoney(player.money + 100);
            });
        }
    }
}


