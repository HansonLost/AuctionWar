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
        { StateType.Operation, new OperationState() },
        { StateType.Auction, new AuctionState() },
    };
    private Dictionary<Type, IState> m_TypeToState = new Dictionary<Type, IState>();

    protected override void Awake()
    {
        base.Awake();
        gameCenter = new CombatGameCenter(MatchSystem.instance.randomSeed);
        gameCenter.ResetQuest();
        foreach (var pair in m_States)
        {
            var state = pair.Value;
            m_TypeToState.Add(state.GetType(), state);
        }
        m_StateType = StateType.Operation;
        m_IsChangeState = true;
    }
    private void Start()
    {
        m_States[m_StateType].LoadResource();

        CombatFrameManager.instance.onLogicUpdate += this.UpdateLogicFrame;
        CombatResultListener.instance.AddListener(this.QuitCombat);
        NetManager.Send((Int16)ProtocType.CombatReady, new CombatReady());
    }
    private void OnDestroy()
    {
        CombatResultListener.instance.RemoveListener(this.QuitCombat);
    }

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
    private void QuitCombat(CombatResult combatResult)
    {
        SceneManager.LoadScene((Int32)GameConst.SceneType.Player);
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

    public enum StateType
    {
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

    public class OperationState : IState
    {
        private GameObject m_View;

        private Int32 m_StartSeq;
        private readonly Int32 m_FreezenTime = 3;
        private bool m_IsFreezen;
        private readonly Int32 m_OpTime = 5;
        private Int32 m_CurrTime;

        public Action<Int32> onChangeFreezenTime;
        public Action<Int32> onChangeCountdownTime;
        
        public void Reset(Int32 seq)
        {
            m_StartSeq = seq;
            m_IsFreezen = true;
            m_CurrTime = 0;
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
