using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestManager : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Button m_BtnMarket;
    [SerializeField] private Button m_BtnProcess;
#pragma warning restore 0649

    private void Awake()
    {
        m_BtnMarket.onClick.AddListener(delegate ()
        {
            CombatManager.instance.ShowPanel(CombatManager.PanelType.Market);
        });
    }

    private void LoadMarketPanel()
    {

    }
}
