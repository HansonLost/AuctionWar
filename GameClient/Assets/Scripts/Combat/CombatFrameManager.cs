using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AuctionWar;
using System;
using Google.Protobuf;
using HamPig.Network;

// 处理帧包，并提供命令监听功能
public class CombatFrameManager : GameBaseManager<CombatFrameManager>
{
    protected override CombatFrameManager GetInstance() => this;
    protected override bool IsDonDestroyOnLoad() => false;

    public Int32 seq { get; private set; }
    public float logicTime { get { return seq * CommonConst.FRAME_INTERVAL; } }
    public Action<Int32> onLogicUpdate { get; set; }
    private Dictionary<GameConst.CommandType, ICmdListener> m_CmdListeners = new Dictionary<GameConst.CommandType, ICmdListener>();


    protected override void Awake()
    {
        base.Awake();
        FramePackageListener.instance.AddListener(this.UpdateFrame);
        this.seq = 0;
    }

    public static float GetIntervalTime(Int32 beforeSeq, Int32 afterSeq) { return (afterSeq - beforeSeq) * CommonConst.FRAME_INTERVAL; }

    public void RegisterCommand(GameConst.CommandType type, ICmdListener listener)
    {
        if (m_CmdListeners.ContainsKey(type)) return;
        m_CmdListeners.Add(type, listener);
    }
    public void SendCommand(GameConst.CommandType cmdType, Int32 playerId, IMessage parm)
    {
        var cmd = new CombatCommand
        {
            Id = (Int32)cmdType,
            Parm = parm.ToByteString(),
        };
        NetManager.Send((Int16)ProtocType.CombatCommand, cmd);
    }
    private void UpdateFrame(FramePackage framePackage)
    {
        if (onLogicUpdate != null)
            onLogicUpdate.Invoke(framePackage.Seq);

        var seq = framePackage.Seq;
        var cmds = framePackage.Data;
        foreach (var cmd in cmds)
        {
            if (m_CmdListeners.ContainsKey((GameConst.CommandType)cmd.CommandId))
            {
                var listener = m_CmdListeners[(GameConst.CommandType)cmd.CommandId];
                listener.Invoke(cmd.PlayerId, cmd.Parameter);
                
            }
        }
    }

}
