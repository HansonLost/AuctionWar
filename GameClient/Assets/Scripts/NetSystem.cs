using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HamPig.Network;
using System;

public class NetSystem : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        NetManager.Connect("127.0.0.1", 8888);
        var combatMatch = new AuctionWar.CombatMatch { };
        NetManager.Send((Int16)ProtocType.CombatMatch, combatMatch);
    }

    // Update is called once per frame
    void Update()
    {
        NetManager.Update();
    }
}
