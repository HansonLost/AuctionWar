using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MarketView : MonoBehaviour
{
    public CombatGameCenter gameCenter { get{ return CombatManager.instance.gameCenter; } }
    private Button m_BtnRefresh;
    private List<SellBlock> m_PnlSell = new List<SellBlock>();
    private List<Storehouse> m_PnlStorehouses = new List<Storehouse>();

    private void Awake()
    {
        BindReference();
    }
    private void Start()
    {
        BindEvent();
        InitSellPanel();
        InitStorehousePanel();
    }
    private void OnDestroy()
    {
        RemoveEvent();
    }

    private void BindReference()
    {
        var root = this.transform;
        m_BtnRefresh = root.Find("BtnRefresh").GetComponent<Button>();
        m_BtnRefresh.onClick.AddListener(() =>
        {
            var state = CombatManager.instance.GetState<CombatManager.OperationState>();
            state.TryRefreshMarket();
        });
        // 商店面板
        for (int i = 1; i <= GameConst.COUNT_MARKET; i++)
        {
            string blockPath = String.Format("BlkSell/Sell ({0})", i);
            var block = new SellBlock();
            block.txtMatName = root.Find(String.Format("{0}/Icon/Text", blockPath)).GetComponent<Text>();
            block.txtPrice = root.Find(String.Format("{0}/Cost/Text", blockPath)).GetComponent<Text>();
            block.btnBuy = root.Find(String.Format("{0}/BtnBuy", blockPath)).GetComponent<Button>();
            m_PnlSell.Add(block);
        }
        // 仓库面板
        for (int i = 1; i <= GameConst.COUNT_STOREHOUSE; i++)
        {

            string blockPath = String.Format("BlkStorehouse/Storehouse ({0})", i);
            var block = new Storehouse();
            block.txtMatName = root.Find(String.Format("{0}/TxtName", blockPath)).GetComponent<Text>();
            block.txtMatCount = root.Find(String.Format("{0}/Count/Text", blockPath)).GetComponent<Text>();
            m_PnlStorehouses.Add(block);
        }
    }
    private void BindEvent()
    {
        Int32 selfId = MatchSystem.instance.selfId;
        var player = gameCenter.playerSet.GetPlayer(selfId);
        player.onAddMaterial += this.AddMaterial;
        gameCenter.materialMarket.onBuyMaterial += this.BuyMaterial;

        var state = CombatManager.instance.GetState<CombatManager.OperationState>();
        state.onRefreshMarket += this.RefreshMarket;
        state.onPutInMaterial += this.UpdateStorehouse;
    }
    private void RemoveEvent()
    {
        Int32 selfId = MatchSystem.instance.selfId;
        var player = gameCenter.playerSet.GetPlayer(selfId);
        player.onAddMaterial -= this.AddMaterial;
        gameCenter.materialMarket.onBuyMaterial -= this.BuyMaterial;

        var state = CombatManager.instance.GetState<CombatManager.OperationState>();
        state.onRefreshMarket -= this.RefreshMarket;
        state.onPutInMaterial -= this.UpdateStorehouse;
    }
    private void InitSellPanel()
    {
        for (int i = 0; i < GameConst.COUNT_MARKET; i++)
        {
            var block = m_PnlSell[i];
            block.txtMatName.text = gameCenter.materialMarket.GetMaterial(i).name;
            block.txtPrice.text = gameCenter.materialMarket.GetPrice(i).ToString();
            Int32 idx = i;
            block.btnBuy.onClick.AddListener(() =>
            {
                CombatManager.instance.TryBuyMaterial(idx);
            });
        }
    }
    private void InitStorehousePanel()
    {
        var gameCenter = CombatManager.instance.gameCenter;
        var selfPlayer = gameCenter.playerSet.GetSelfPlayer();
        Int32 startIdx = 0;
        selfPlayer.ForEachMaterial((CombatGameCenter.Material mat) =>
        {
            if (startIdx < m_PnlStorehouses.Count)
            {
                var sh = m_PnlStorehouses[startIdx];
                sh.txtMatName.text = mat.name;
                sh.txtMatCount.text = mat.count.ToString();
            }
            startIdx++;
        });
        for (int i = startIdx; i < m_PnlStorehouses.Count; i++)
        {
            var sh = m_PnlStorehouses[i];
            sh.txtMatName.text = "空闲";
            sh.txtMatCount.text = "0";
        }
    }
    // --- callback --- //
    private void AddMaterial(CombatGameCenter.Material mat)
    {
        InitStorehousePanel();
    }
    private void BuyMaterial(Int32 idx, Int32 price, CombatGameCenter.Material mat)
    {
        m_PnlSell[idx].btnBuy.interactable = false;
    }
    private void RefreshMarket(Int32 playerId)
    {
        if (playerId != MatchSystem.instance.selfId) return;
        for (int i = 0; i < m_PnlSell.Count; i++)
        {
            var block = m_PnlSell[i];
            block.btnBuy.interactable = true;
            block.txtMatName.text = gameCenter.materialMarket.GetMaterial(i).name;
            block.txtPrice.text = gameCenter.materialMarket.GetPrice(i).ToString();
            Int32 idx = i;  // 使用 i 的话，所有值都编程一样的。
            block.btnBuy.onClick.AddListener(() =>
            {
                CombatManager.instance.TryBuyMaterial(idx);
            });
        }
    }
    private void UpdateStorehouse(Int32 playerId)
    {
        if (playerId != MatchSystem.instance.selfId) return;
        var player = gameCenter.playerSet.GetSelfPlayer();
        Int32 startIdx = 0;
        player.ForEachMaterial((CombatGameCenter.Material mat) =>
        {
            if (startIdx < m_PnlStorehouses.Count)
            {
                var sh = m_PnlStorehouses[startIdx];
                sh.txtMatName.text = mat.name;
                sh.txtMatCount.text = mat.count.ToString();
            }
            startIdx++;
        });
        for (int i = startIdx; i < m_PnlStorehouses.Count; i++)
        {
            var sh = m_PnlStorehouses[i];
            sh.txtMatName.text = "空闲";
            sh.txtMatCount.text = "0";
        }
    }


    public class SellBlock
    {
        public Text txtMatName;
        public Text txtPrice;
        public Button btnBuy;
    }
    
    public class Storehouse
    {
        public Text txtMatName;
        public Text txtMatCount;
    }
}
