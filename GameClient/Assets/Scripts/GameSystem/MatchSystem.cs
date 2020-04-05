using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchSystem : GameBaseManager<MatchSystem>
{
    protected override MatchSystem GetInstance() => this;
    protected override bool IsDonDestroyOnLoad() => true;

    /// <summary>
    /// 游戏人数
    /// </summary>
    public Int32 playerCount = 1;
    /// <summary>
    /// 角色ID
    /// </summary>
    public Int32 selfId { get; set; } = 1;
    /// <summary>
    /// 随机数
    /// </summary>
    public System.Random random { get; set; } = new System.Random();
}
