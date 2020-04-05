using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using HamPig.Network;
using AuctionWar;

// 对战整体流程框架
public partial class CombatManager : GameBaseManager<CombatManager>
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

        NetManager.Send((Int16)ProtocType.CombatReady, new CombatReady());
    }
    private void OnDestroy()
    {
        CombatFrameManager.instance.onLogicUpdate -= this.UpdateLogicFrame;
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
        string msg = "";
        if(combatResult.WinnerId == MatchSystem.instance.selfId)
        {
            msg = "我是冠军";
        }
        else
        {
            msg = "你输了";
        }
        var go = CanvasManager.instance.CreatePanel(CanvasManager.PanelLevelType.Top, "System/PnlComfirmView");
        var wnd = go.GetComponent<ConfirmView>();
        wnd.SetLogMessage(msg);
        wnd.onConfirm.AddListener(() =>
        {
            SceneManager.LoadScene((Int32)GameConst.SceneType.Player);
        });
        
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
    public void TryWin()
    {
        NetManager.Send((Int16)ProtocType.WinCombat, new WinCombat { });
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
}
