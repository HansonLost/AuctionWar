using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using HamPig.Network;
using AuctionWar;

public class CombatManager : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Button m_BtnQuit;
#pragma warning disable 0649

    private void Awake()
    {
        CombatResultListener.instance.AddListener(this.QuitCombat);

        m_BtnQuit.onClick.AddListener(delegate()
        {
            NetManager.Send((Int16)ProtocType.QuitCombat, new QuitCombat { });
        });
    }

    private void OnDestroy()
    {
        CombatResultListener.instance.RemoveListener(this.QuitCombat);
    }

    private void QuitCombat(CombatResult combatResult)
    {
        SceneManager.LoadScene((Int32)GameConst.SceneType.Player);
    }
}
