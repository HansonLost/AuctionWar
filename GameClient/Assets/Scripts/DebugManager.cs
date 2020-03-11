using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugManager : GameBaseManager<DebugManager>
{
    protected override DebugManager GetInstance() => this;
    protected override bool IsDonDestroyOnLoad() => false;
}
