using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchSystem : GameBaseManager<MatchSystem>
{
    protected override MatchSystem GetInstance() => this;
    protected override bool IsDonDestroyOnLoad() => true;

    public Int32 randomSeed { get; set; }
    /// <summary>
    /// 角色ID
    /// </summary>
    public Int32 selfId { get; set; }
}
