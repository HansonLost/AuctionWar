using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HamPig.Network;
using System;

// 管理对战过程中通用的 View 元素
public class CombatView : MonoBehaviour
{
    private Button m_BtnQuit;
    private Text m_TxtMoney;
    private List<Int32> m_List = new List<int>();
    private CombatGameCenter.Player m_SelfPlayer;

    private void Awake()
    {
        BindReference();
    }
    private void Start()
    {
        BindEvent();
        BindButton();
        InitView();
    }
    private void OnDestroy()
    {
        RemoveEvent();
    }

    private void BindReference()
    {
        var root = this.transform;
        m_BtnQuit = root.Find("BtnQuit").GetComponent<Button>();
        m_TxtMoney = root.Find("Money/TxtValue").GetComponent<Text>();
    }
    private void BindButton()
    {
        m_BtnQuit.onClick.AddListener(() =>
        {
            CombatManager.instance.QuitCombat();
        });
    }
    private void BindEvent()
    {
        var player = GetSelfPlayer();
        player.onChangeMoney += this.SetMoneyView;
    }
    private void RemoveEvent()
    {
        var player = GetSelfPlayer();
        player.onChangeMoney -= this.SetMoneyView;
    }
    private void InitView()
    {
        var gameCenter = CombatManager.instance.gameCenter;
        var player = gameCenter.playerSet.GetSelfPlayer();
        m_TxtMoney.text = player.money.ToString();
    }
    private CombatGameCenter.Player GetSelfPlayer()
    {
        if (m_SelfPlayer == null)
        {
            var gameCenter = CombatManager.instance.gameCenter;
            m_SelfPlayer = gameCenter.playerSet.GetSelfPlayer();
        }
        return m_SelfPlayer;
    }
    private void SetMoneyView(Int32 value)
    {
        m_TxtMoney.text = value.ToString();
    }
}
