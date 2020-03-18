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

    protected override void Awake()
    {
        base.Awake();
        FramePackageListener.instance.AddListener(this.UpdateFrame);
    }
    
    private void UpdateFrame(FramePackage framePackage)
    {
        var seq = framePackage.Seq;
        Debug.Log(string.Format("Fresh frame - {0}", seq));
    }
}


