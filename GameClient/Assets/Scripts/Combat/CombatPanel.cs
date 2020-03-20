using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using HamPig.Network;
using AuctionWar;

public class CombatPanel : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Button m_BtnQuest;
    [SerializeField] private Button m_BtnMarket;
    [SerializeField] private Button m_BtnProcess;
    [SerializeField] private Button m_BtnQuit;
    [SerializeField] private Text m_TxtTisTime;
#pragma warning restore 0649

    private void Awake()
    {
        m_BtnQuest.onClick.AddListener(delegate ()
        {
            CombatManager.instance.ShowPanel(CombatManager.PanelType.Quest);
        });
        m_BtnMarket.onClick.AddListener(delegate ()
        {
            CombatManager.instance.ShowPanel(CombatManager.PanelType.Market);
        });
        m_BtnProcess.onClick.AddListener(delegate ()
        {
            CombatManager.instance.ShowPanel(CombatManager.PanelType.Process);
        });
        m_BtnQuit.onClick.AddListener(delegate ()
        {
            NetManager.Send((Int16)ProtocType.QuitCombat, new QuitCombat { });
        });
    }

    private void FixedUpdate()
    {
        var gameCenter = CombatGameCenter.instance;
        Int32 time = (Int32)gameCenter.combatOperation.remainTime;
        m_TxtTisTime.text = time.ToString();
    }
}
