using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using HamPig.Network;
using AuctionWar;

public class CombatManager : GameBaseManager<CombatManager>
{
    protected override CombatManager GetInstance() => this;
    protected override bool IsDonDestroyOnLoad() => false;

#pragma warning disable 0649
    [SerializeField] private RectTransform m_Canvas;
    [SerializeField] private Button m_BtnQuit;
    [SerializeField] private PanelPrefab[] m_Panels;
#pragma warning disable 0649

    private Dictionary<PanelType, GameObject> m_PanelMap = new Dictionary<PanelType, GameObject>();

    public void ShowPanel(PanelType type)
    {
        if (!m_PanelMap.ContainsKey(type)) return;
        foreach (var pair in m_PanelMap)
        {
            bool isActive = (pair.Key == type ? true : false);
            pair.Value.SetActive(isActive);
        }
    }

    protected override void Awake()
    {
        base.Awake();
        LoadPanel();
        CombatResultListener.instance.AddListener(this.QuitCombat);

        m_BtnQuit.onClick.AddListener(delegate()
        {
            NetManager.Send((Int16)ProtocType.QuitCombat, new QuitCombat { });
        });
    }
    private void Start()
    {
        this.ShowPanel(PanelType.Quest);
    }
    private void OnDestroy()
    {
        CombatResultListener.instance.RemoveListener(this.QuitCombat);
    }
    private void LoadPanel()
    {
        foreach (var info in m_Panels)
        {
            var panel = GameObject.Instantiate(info.prefab, m_Canvas);
            var rect = panel.GetComponent<RectTransform>();
            panel.SetActive(false);
            m_PanelMap.Add(info.type, panel);
        }
    }
    private void QuitCombat(CombatResult combatResult)
    {
        SceneManager.LoadScene((Int32)GameConst.SceneType.Player);
    }

    [Serializable]
    public class PanelPrefab
    {
        public PanelType type;
        public GameObject prefab;
    }
    public enum PanelType
    {
        Quest,
        Market,
        Process,
        Auction,
    }
}
