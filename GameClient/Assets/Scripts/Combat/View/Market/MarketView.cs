using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class MarketView : MonoBehaviour
{
    public CombatGameCenter gameCenter { get{ return CombatManager.instance.gameCenter; } }
    private Button m_BtnRefresh;
    private List<SellBlock> m_PnlSell = new List<SellBlock>();
    private List<SellBlock> m_PnlWholesale = new List<SellBlock>();
    private List<MarketStorehouseView> m_ViewStorehouses = new List<MarketStorehouseView>();

    private void Awake()
    {
        BindReference();
    }
    private void Start()
    {
        BindEvent();
        InitMarket();
        InitStorehousePanel();
        UpdateWholesaler(MatchSystem.instance.selfId);
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
        // 批发面板
        for (int i = 0; i < GameConst.COUNT_WHOLESALE; i++)
        {
            string blockPath = String.Format("Wholesaler/Sell ({0})", i + 1);
            var block = new SellBlock();
            block.txtMatName = root.Find(String.Format("{0}/Icon/Text", blockPath)).GetComponent<Text>();
            block.txtPrice = root.Find(String.Format("{0}/Cost/Text", blockPath)).GetComponent<Text>();
            block.btnBuy = root.Find(String.Format("{0}/BtnBuy", blockPath)).GetComponent<Button>();
            m_PnlWholesale.Add(block);
        }
        // 仓库面板
        for (int i = 1; i <= GameConst.COUNT_STOREHOUSE_MAX; i++)
        {
            var go = root.Find(String.Format("BlkStorehouse/Storehouse ({0})", i));
            var view = go.GetComponent<MarketStorehouseView>();
            m_ViewStorehouses.Add(view);
        }
    }
    private void BindEvent()
    {
        Int32 selfId = MatchSystem.instance.selfId;
        var player = gameCenter.playerSet.GetPlayer(selfId);
        player.onAddMaterial += this.AddMaterial;

        var state = CombatManager.instance.GetState<CombatManager.OperationState>();
        state.onBuyWholesale += this.UpdateWholesaler;
        state.onBuyMaterial += this.BuyMaterial;
        state.onRefreshMarket += this.RefreshMarket;
        state.onPutInMaterial += this.UpdateStorehouse;
        state.onSellMaterial += this.UpdateStorehouse;
    }
    private void RemoveEvent()
    {
        Int32 selfId = MatchSystem.instance.selfId;
        var player = gameCenter.playerSet.GetPlayer(selfId);
        player.onAddMaterial -= this.AddMaterial;

        var state = CombatManager.instance.GetState<CombatManager.OperationState>();
        state.onBuyMaterial -= this.BuyMaterial;
        state.onBuyWholesale -= this.UpdateWholesaler;
        state.onRefreshMarket -= this.RefreshMarket;
        state.onPutInMaterial -= this.UpdateStorehouse;
        state.onSellMaterial -= this.UpdateStorehouse;
    }
    private void InitMarket()
    {
        var player = gameCenter.playerSet.GetSelfPlayer();

        // 商店
        for (int i = 0; i < m_PnlSell.Count; i++)
        {
            var dataBlock = player.marketData.GetBlock(i);
            var block = m_PnlSell[i];
            block.txtMatName.text = dataBlock.material.name;
            block.txtPrice.text = player.marketData.GetBlockPrice(i).ToString();
            //block.txtMatName.text = gameCenter.materialMarket.GetMaterial(i).name;
            //block.txtPrice.text = gameCenter.materialMarket.GetPrice(i).ToString();
            Int32 idx = i;
            block.btnBuy.onClick.AddListener(() =>
            {
                var state = CombatManager.instance.GetState<CombatManager.OperationState>();
                state.TryBuyMaterial(idx);
            });
        }

        // 批发商
        for (int i = 0; i < GameConst.COUNT_WHOLESALE; i++)
        {
            var block = m_PnlWholesale[i];
            block.txtMatName.text = player.GetWholesale(i).material.name;
            block.txtPrice.text = player.GetWholesale(i).price.ToString();
            Int32 idx = i;
            block.btnBuy.onClick.AddListener(() =>
            {
                var state = CombatManager.instance.GetState<CombatManager.OperationState>();
                state.TryBuyWholesale(idx);
            });
        }
    }
    private void InitStorehousePanel()
    {
        var selfPlayer = gameCenter.playerSet.GetSelfPlayer();
        UpdateStorehouse(selfPlayer.id);
    }

    // --- callback --- //
    private void AddMaterial(CombatGameCenter.Material mat)
    {
        InitStorehousePanel();
    }
    private void BuyMaterial(Int32 playerId, Int32 marketIdx)
    {
        if (playerId != MatchSystem.instance.selfId) return;
        if(Utility.IsInRange(marketIdx, 0, m_PnlSell.Count - 1))
        {
            m_PnlSell[marketIdx].btnBuy.interactable = false;
        }
    }
    private void RefreshMarket(Int32 playerId)
    {
        if (playerId != MatchSystem.instance.selfId) return;
        var player = gameCenter.playerSet.GetSelfPlayer();
        for (int i = 0; i < m_PnlSell.Count; i++)
        {
            var dataBlock = player.marketData.GetBlock(i);
            var block = m_PnlSell[i];
            block.btnBuy.interactable = true;
            block.txtMatName.text = dataBlock.material.name;
            block.txtPrice.text = player.marketData.GetBlockPrice(i).ToString();
            //block.txtMatName.text = gameCenter.materialMarket.GetMaterial(i).name;
            //block.txtPrice.text = gameCenter.materialMarket.GetPrice(i).ToString();
        }
    }
    private void UpdateStorehouse(Int32 playerId)
    {
        if (playerId != MatchSystem.instance.selfId) return;
        var player = gameCenter.playerSet.GetSelfPlayer();
        Int32 startIdx = 0;
        player.ForEachMaterial((CombatGameCenter.Material mat) =>
        {
            if (startIdx < m_ViewStorehouses.Count)
            {
                var view = m_ViewStorehouses[startIdx];
                view.SetLock(false);
                view.RefreshView(mat);
            }
            startIdx++;
        });
        for (int i = startIdx; i < player.storehouseCapacity; i++)
        {
            var view = m_ViewStorehouses[i];
            view.SetLock(false);
            view.RefreshView(CombatGameCenter.Material.empty);
            startIdx++;
        }
        for (int i = startIdx; i <m_ViewStorehouses.Count; i++)
        {
            var view = m_ViewStorehouses[i];
            view.SetLock(true);
        }
    }
    private void UpdateWholesaler(Int32 playerId)
    {
        var player = gameCenter.playerSet.GetPlayer(MatchSystem.instance.selfId);
        Int32 idx = 0;
        player.ForEachWholesaler((CombatGameCenter.Material mat, Int32 price, bool isSellout) =>
        {
            if(idx < m_PnlWholesale.Count)
            {
                m_PnlWholesale[idx].txtMatName.text = mat.name;
                m_PnlWholesale[idx].txtPrice.text = price.ToString();
                m_PnlWholesale[idx].btnBuy.interactable = !isSellout;
            }
            idx++;

        });
        while(idx < m_PnlWholesale.Count)
        {
            m_PnlWholesale[idx].txtMatName.text = "暂无";
            m_PnlWholesale[idx].txtPrice.text = "???";
            m_PnlWholesale[idx].btnBuy.interactable = false;
            idx++;
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
