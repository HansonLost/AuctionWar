using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using HamPig.Network;
using AuctionWar;

// 网络同步数据
public class CombatGameCenter : GameBaseManager<CombatGameCenter>
{
    protected override CombatGameCenter GetInstance() => this;
    protected override bool IsDonDestroyOnLoad() => false;

    private Int32 m_FrameSeq;
    public float logicTime { get { return m_FrameSeq * CommonConst.FRAME_INTERVAL; } }
    public CombatOperation combatOperation { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        FramePackageListener.instance.AddListener(this.UpdateFrame);
        m_FrameSeq = 0;
        combatOperation = new CombatOperation();
        combatOperation.Reset(logicTime);
    }

    private void UpdateFrame(FramePackage framePackage)
    {
        m_FrameSeq = framePackage.Seq;
        combatOperation.Update(logicTime);
    }

    public class CombatOperation
    {
        public Int32 remainTime { get; private set; }

        private float m_StartTime;
        public void Reset(float logicTime)
        {
            remainTime = GameConst.COMBAT_OPERATION_TIME;
            m_StartTime = logicTime;

        }
        public void Update(float logicTime)
        {
            remainTime = GameConst.COMBAT_OPERATION_TIME - (Int32)(logicTime - m_StartTime);
            if (remainTime < 0) remainTime = 0;
        }
    }
}


