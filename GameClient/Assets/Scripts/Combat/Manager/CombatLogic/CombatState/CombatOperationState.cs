using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

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
        }
        public void ReleaseResource()
        {
            if (m_View == null) return;
            GameObject.Destroy(m_View);
            m_View = null;
        }
        public StateType LogicUpdate(Int32 seq)
        {
            UpdateOperationTime(seq);
            UpdateProcessTime(seq);
            return (m_CurrTime >= m_OpTime ? StateType.Auction : StateType.Operation);
        }

        // --- command --- //
        public void TryPutInMaterial(Int32 storehouseIdx, Int32 processBlockIdx)
        {
            // TODO : 改为发送放入材料命令
            PutInMaterial(MatchSystem.instance.selfId, storehouseIdx, processBlockIdx);
        }
        public void TryRefreshMarket()
        {
            RefreshMarket(MatchSystem.instance.selfId);
        }

        // --- callback --- //
        public void PutInMaterial(Int32 playerId, Int32 storehouseIdx, Int32 processBlockIdx)
        {
            var player = gameCenter.playerSet.GetSelfPlayer();
            var storehouse = player.GetStorehouse(storehouseIdx);
            if (storehouse.IsEmpty()) return;
            var leftMat = player.processFactory.AddMaterial(processBlockIdx, storehouse);
            if (leftMat.count > 0)
            {
                player.SetStorehouse(storehouseIdx, leftMat);
            }
            else
            {
                player.ClearStorehouse(storehouseIdx);
            }
            onPutInMaterial?.Invoke(playerId);
        }
        public void RefreshMarket(Int32 playerId)
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
            onRefreshMarket.Invoke(playerId);
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
            for (int i = 1; i <= GameConst.COMBAT_PLAYER_COUNT; i++)
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
            for (int playerId = 1; playerId <= GameConst.COMBAT_PLAYER_COUNT; playerId++)
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


