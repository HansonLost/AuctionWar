using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HamPig.Network;
using System;
using AuctionWar;

public class NetSystem : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        NetManager.Connect("127.0.0.1", 8888);
        CombatMatchResListener.instance.AddListener(delegate (CombatMatchRes combatMatchRes)
        {
            Debug.Log(String.Format("进入房间{0}.", combatMatchRes.RoomId));
        });

        var combatMatch = new CombatMatch { };
        var id = (Int16)ProtocType.CombatMatch;
        NetManager.Send(id, combatMatch);
    }

    // Update is called once per frame
    void Update()
    {
        NetManager.Update();
    }
}
