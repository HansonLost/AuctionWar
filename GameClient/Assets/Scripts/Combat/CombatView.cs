using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using HamPig.Network;
using AuctionWar;

public class CombatView : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private PanelPrefab m_PanelPrefabs;
#pragma warning restore 0649
    private Button m_BtnQuitCombat;
    private Dictionary<OperationType, Button> m_BtnOperation = new Dictionary<OperationType, Button>();
    private Text m_TxtTisTime;
    private Dictionary<OperationType, RectTransform> m_PnlOperation = new Dictionary<OperationType, RectTransform>();

    private void Awake()
    {
        LoadPanel();
        SwitchOperationPanel(OperationType.Quest);
    }
    private void Start()
    {
        BindButton();
        CombatManager.instance.GetState<CombatManager.OperationState>().onChangeTime += (Int32 time) =>
        {
            m_TxtTisTime.text = time.ToString();
        };
    }

    private void LoadPanel()
    {
        // 加载运营面板
        var opPrefabs = new Dictionary<OperationType, GameObject>()
        {
            { OperationType.Quest,      m_PanelPrefabs.pnlQuest },
            { OperationType.Market,     m_PanelPrefabs.pnlMarket },
            { OperationType.Process,    m_PanelPrefabs.pnlProcess },
        };
        foreach (var pair in opPrefabs)
        {
            var go = Instantiate(pair.Value, this.transform);
            m_PnlOperation.Add(pair.Key, go.GetComponent<RectTransform>());
        }

        // 加载战斗系统面板
        var pnlCombat = Instantiate(m_PanelPrefabs.pnlCombat, this.transform).transform;
        m_BtnQuitCombat = pnlCombat.Find("BtnQuit").GetComponent<Button>();
        m_BtnOperation.Add(OperationType.Quest, pnlCombat.Find("PnlOperation/BtnQuest").GetComponent<Button>());
        m_BtnOperation.Add(OperationType.Market, pnlCombat.Find("PnlOperation/BtnMarket").GetComponent<Button>());
        m_BtnOperation.Add(OperationType.Process, pnlCombat.Find("PnlOperation/BtnProcess").GetComponent<Button>());
        m_TxtTisTime = pnlCombat.Find("PnlOperation/TisTime/Text").GetComponent<Text>();
    }
    private void BindButton()
    {
        foreach (var pair in m_BtnOperation)
        {
            var button = pair.Value;
            button.onClick.AddListener(delegate ()
            {
                SwitchOperationPanel(pair.Key);
            });
        }

        m_BtnQuitCombat.onClick.AddListener(delegate ()
        {
            // TODO 传入 CombatManager 处理，不擅自发送协议
            NetManager.Send((Int16)ProtocType.QuitCombat, new QuitCombat { });
        });
    }
    private void SwitchOperationPanel(OperationType type)
    {
        foreach (var pair in m_PnlOperation)
        {
            var panel = pair.Value;
            panel.gameObject.SetActive(false);
        }
        if (m_PnlOperation.ContainsKey(type))
        {
            m_PnlOperation[type].gameObject.SetActive(true);
        }
    }

    [Serializable]
    public class PanelPrefab
    {
        public GameObject pnlCombat;
        public GameObject pnlQuest;
        public GameObject pnlMarket;
        public GameObject pnlProcess;
        public GameObject pnlAuction;
    }

    public enum OperationType
    {
        Quest,
        Market,
        Process,
    }
}
