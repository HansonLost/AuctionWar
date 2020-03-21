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
        if (nextType != m_StateType) m_IsChangeState = true;
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
        private Int32 m_StartSeq;
        private Int32 m_FreezenTime;
        private bool m_IsFreezen;
        private Int32 m_OpTime;
        private Int32 m_CurrTime;
        public Action<Int32> onChangeTime;
        public void Reset(Int32 seq)
        {
            m_StartSeq = seq;
            m_FreezenTime = 3;
            m_IsFreezen = true;
            m_OpTime = 60;
            m_CurrTime = 0;
        }
        public void LoadResource()
        {

        }
        public void ReleaseResource()
        {

        }
        public StateType LogicUpdate(Int32 seq)
        {
            float dt = CombatFrameManager.GetIntervalTime(m_StartSeq, seq);
            if (m_IsFreezen)
            {
                if(dt >= m_FreezenTime)
                {
                    m_IsFreezen = false;
                    m_StartSeq = seq;
                    onChangeTime.Invoke(m_OpTime);
                }
            }
            else
            {
                Int32 dtInt = (Int32)dt;
                if(dtInt != m_CurrTime)
                {
                    m_CurrTime = dtInt;
                    onChangeTime.Invoke(m_OpTime - m_CurrTime);
                }
            }
            return (m_CurrTime >= m_OpTime ? StateType.Auction : StateType.Operation);
        }
    }

    public class AuctionState : IState
    {
        public void LoadResource()
        {
            throw new NotImplementedException();
        }

        public StateType LogicUpdate(Int32 seq)
        {
            throw new NotImplementedException();
        }

        public void ReleaseResource()
        {
            throw new NotImplementedException();
        }

        public void Reset(Int32 seq)
        {
            return;
        }
    }
}
