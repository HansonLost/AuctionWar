using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using HamPig.Network;
using AuctionWar;

// 对战整体流程框架
public class CombatManager : GameBaseManager<CombatManager>
{
    protected override CombatManager GetInstance() => this;
    protected override bool IsDonDestroyOnLoad() => false;

#pragma warning disable 0649
    [SerializeField] private RectTransform m_Canvas;
#pragma warning restore 0649

    public CombatGameCenter gameCenter { get; private set; }

    private StateType m_StateType;  // 当前状态
    private bool m_IsChangeState;
    private Dictionary<StateType, IState> m_States = new Dictionary<StateType, IState>()
    {
        { StateType.Awake, new AwakeState() },
        { StateType.Operation, new OperationState() },
        { StateType.Auction, new AuctionState() },
    };
    private Dictionary<Type, IState> m_TypeToState = new Dictionary<Type, IState>();

    protected override void Awake()
    {
        base.Awake();
        gameCenter = new CombatGameCenter();
        gameCenter.playerSet.ResetPlayer();
        foreach (var pair in m_States)
        {
            var state = pair.Value;
            m_TypeToState.Add(state.GetType(), state);
        }
        m_StateType = StateType.Awake;
        m_IsChangeState = true;
    }
    private void Start()
    {
        m_States[m_StateType].LoadResource();

        // system
        CombatFrameManager.instance.onLogicUpdate += this.UpdateLogicFrame;
        CombatResultListener.instance.AddListener(this.CombatCheckout);

        // command
        CmdClaimQuestListener.instance.AddListener(this.HandOutQuest);
        CmdBuyMaterialListener.instance.AddListener(this.BuyMaterial);

        NetManager.Send((Int16)ProtocType.CombatReady, new CombatReady());
    }
    private void OnDestroy()
    {
        CombatResultListener.instance.RemoveListener(this.CombatCheckout);
        CmdClaimQuestListener.instance.RemoveListener(this.HandOutQuest);
    }

    public T GetState<T>() where T : class, IState
    {
        Type parmType = typeof(T);
        if (m_TypeToState.ContainsKey(parmType))
        {
            return (T)m_TypeToState[parmType];
        }
        return null;
    }

    // --- callback --- //
    private void UpdateLogicFrame(Int32 seq)
    {
        var state = m_States[m_StateType];
        if (m_IsChangeState)
        {
            m_IsChangeState = false;
            state.Reset(seq);
            state.LoadResource();
        }
        StateType nextType = state.LogicUpdate(seq);
        if (nextType != m_StateType)
        {
            m_IsChangeState = true;
            state.ReleaseResource();
        } 
        m_StateType = nextType;
    }
    private void CombatCheckout(CombatResult combatResult)
    {
        SceneManager.LoadScene((Int32)GameConst.SceneType.Player);
    }
    private void HandOutQuest(Int32 playerId, CmdClaimQuest cmdClaimQuest)
    {
        Int32 questIdx = cmdClaimQuest.Index;
        bool isValid = CanPlayerHandOutQuest(playerId, questIdx);
        if(isValid)
        {
            var quest = gameCenter.questMarket.HandOutQuest(questIdx);
            var player = gameCenter.playerSet.GetPlayer(playerId);
            player.AddQuest(quest);
        }
    }
    private void BuyMaterial(Int32 playerId, CmdBuyMaterial cmdBuyMaterial)
    {
        Int32 selfId = MatchSystem.instance.selfId;
        Int32 matIdx = cmdBuyMaterial.Index;
        if (selfId != playerId) return;
        bool isValid = CanPlayerBuyMaterial(playerId, matIdx);
        if (isValid)
        {
            var player = gameCenter.playerSet.GetPlayer(playerId);
            var mat = gameCenter.materialMarket.BuyMaterial(matIdx);
            var price = gameCenter.materialMarket.GetPrice(matIdx);
            player.AddMaterial(mat);
            player.SetMoney(player.money - price);
        }
    }
    
    // --- command --- //
    public void QuitCombat()
    {
        NetManager.Send((Int16)ProtocType.QuitCombat, new QuitCombat());
    }
    public void ClaimQuest(Int32 index)
    {
        Int32 selfId = MatchSystem.instance.selfId;
        bool isValid = CanPlayerHandOutQuest(selfId, index);
        if (isValid)
        {
            CombatFrameManager.instance.SendCommand(
               GameConst.CommandType.ClaimQuest,
               selfId,
               new CmdClaimQuest
               {
                   Index = index,
               });
        }
    }
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
            state == CombatGameCenter.MaterialMarket.StateType.Sell &&
            player.money >= price);
    }

    public enum StateType
    {
        Awake,      // 启动阶段
        Operation,  // 运营阶段
        Auction,    // 拍卖阶段
        End,        // 结束阶段
    }

    public interface IState
    {
        void Reset(Int32 seq);
        void LoadResource();
        void ReleaseResource();
        StateType LogicUpdate(Int32 seq);
    }

    public class AwakeState : IState
    {
        private bool m_IsLoad = false;
        public void Reset(int seq) { }
        public void LoadResource()
        {
            if (m_IsLoad) return;
            m_IsLoad = true;
            CanvasManager.instance.CreatePanel(CanvasManager.PanelLevelType.UI, "Combat/PnlCombatView");
        }
        public void ReleaseResource() { }
        public StateType LogicUpdate(int seq)
        {
            return StateType.Operation;
        }
    }

    public class OperationState : IState
    {
        private GameObject m_View;

        private Int32 m_StartSeq;
        private readonly Int32 m_FreezenTime = 3;
        private bool m_IsFreezen;
        private readonly Int32 m_OpTime = GameConst.COMBAT_OPERATION_TIME;
        private Int32 m_CurrTime;

        public Action<Int32> onChangeFreezenTime;
        public Action<Int32> onChangeCountdownTime;
        
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
            float dt = CombatFrameManager.GetIntervalTime(m_StartSeq, seq);
            Int32 dtInt = (Int32)dt;
            if (m_IsFreezen)
            {
                if(dtInt < m_FreezenTime)
                {
                    m_CurrTime = dtInt;
                    if(onChangeFreezenTime != null)
                        onChangeFreezenTime.Invoke(m_FreezenTime - m_CurrTime);
                }
                else
                {
                    m_IsFreezen = false;
                    m_CurrTime = 0;
                    m_StartSeq = seq;
                    if(onChangeCountdownTime != null)
                        onChangeCountdownTime.Invoke(m_OpTime - m_CurrTime);
                }
            }
            else
            {
                if(dtInt != m_CurrTime)
                {
                    m_CurrTime = dtInt;
                    if(onChangeCountdownTime != null)
                        onChangeCountdownTime.Invoke(m_OpTime - m_CurrTime);
                }
            }
            return (m_CurrTime >= m_OpTime ? StateType.Auction : StateType.Operation);
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

    public class AuctionState : IState
    {
        private GameObject m_View;
        private Int32 m_StartSeq;
        private readonly Int32 m_WaitTime = 3;

        public void Reset(Int32 seq)
        {
            m_StartSeq = seq;
            return;
        }
        public void LoadResource()
        {
            if (m_View != null) return;
            m_View = CanvasManager.instance.CreatePanel(CanvasManager.PanelLevelType.Normal, "Combat/PnlAuctionView");
        }
        public void ReleaseResource()
        {
            if (m_View == null) return;
            GameObject.Destroy(m_View);
            m_View = null;
        }
        public StateType LogicUpdate(Int32 seq)
        {
            Int32 dt = (Int32)CombatFrameManager.GetIntervalTime(m_StartSeq, seq);
            return (dt >= m_WaitTime ? StateType.Operation : StateType.Auction);
        }
    }
}
