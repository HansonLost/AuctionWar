using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AuctionView : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Text m_TxtItemName;
    [SerializeField] private Text m_TxtPrice;
    [SerializeField] private Text m_Time;
    [SerializeField] private Button m_BtnPass;
    [SerializeField] private Button[] m_RisePrice;
#pragma warning restore 0649

    public CombatGameCenter gameCenter { get { return CombatManager.instance.gameCenter; } }

    private void Awake()
    {
        BindButton();
    }
    private void Start()
    {
        BindEvent();
        UpdateItem();
    }
    private void OnDestroy()
    {
        RemoveEvent();
    }

    private void BindButton()
    {
        m_BtnPass.onClick.AddListener(() =>
        {
            var state = CombatManager.instance.GetState<AuctionState>();
            state.TryPass();
        });
        for (int i = 0; i < m_RisePrice.Length; i++)
        {
            var btn = m_RisePrice[i];
            Int32 gap = m_TblCallLevel[i];
            btn.onClick.AddListener(() =>
            {
                var state = CombatManager.instance.GetState<AuctionState>();
                state.TryRisePrice(gap);
            });
        }
    }
    private void BindEvent()
    {
        var state = CombatManager.instance.GetState<AuctionState>();
        state.onNextAuctionItem += this.UpdateItem;
        state.onRisePrice += this.RisePrice;
        state.onUpdate += this.UpdateTime;
    }
    private void RemoveEvent()
    {
        var state = CombatManager.instance.GetState<AuctionState>();
        state.onNextAuctionItem -= UpdateItem;
        state.onRisePrice -= this.RisePrice;
        state.onUpdate -= this.UpdateTime;
    }

    private void UpdateItem()
    {
        var prop = gameCenter.auction.GetCurrentProp();
        if(prop != null)
        {
            m_TxtItemName.text = prop.GetName();
            m_TxtPrice.text = gameCenter.auction.GetCurrentPrice().ToString();
        }
        foreach (var btn in m_RisePrice)
        {
            btn.interactable = true;
        }
        m_BtnPass.interactable = true;
    }
    private void RisePrice(Int32 playerId)
    {
        m_TxtPrice.text = gameCenter.auction.GetCurrentPrice().ToString();

        if(MatchSystem.instance.selfId != playerId)
        {
            foreach (var btn in m_RisePrice)
            {
                btn.interactable = true;
            }
        }
        else
        {
            m_BtnPass.interactable = false;
            foreach (var b in m_RisePrice)
            {
                b.interactable = false;
            }
        }
    }
    private void UpdateTime(float leftTime)
    {
        m_Time.text = ((Int32)leftTime).ToString();
    }

    private static readonly List<Int32> m_TblCallLevel = new List<Int32>
    {
        10,
        50,
        100,
    };
}
