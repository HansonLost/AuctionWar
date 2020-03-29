using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OperationView : MonoBehaviour
{
    private Dictionary<OperationType, RectTransform> m_PnlOp;
    private Dictionary<OperationType, Button> m_BtnOp;
    private Text m_TxtCountdown;

    private void Awake()
    {
        BindReference();
    }
    private void Start()
    {
        BindButton();
        BindEvent();

        SwitchOperationPanel(OperationType.Quest);
    }
    private void OnDestroy()
    {
        RemoveEvent();
    }

    private void BindReference()
    {
        var root = this.transform;
        m_BtnOp = new Dictionary<OperationType, Button>
        {
            { OperationType.Quest, root.Find("PnlCtrl/BtnQuest").GetComponent<Button>() },
            { OperationType.Market, root.Find("PnlCtrl/BtnMarket").GetComponent<Button>() },
            { OperationType.Process, root.Find("PnlCtrl/BtnProcess").GetComponent<Button>() },
        };
        m_PnlOp = new Dictionary<OperationType, RectTransform>
        {
            { OperationType.Quest, root.Find("PnlQuest").GetComponent<RectTransform>() },
            { OperationType.Market, root.Find("PnlMarket").GetComponent<RectTransform>() },
            { OperationType.Process, root.Find("PnlProcess").GetComponent<RectTransform>() },
        };
        m_TxtCountdown = root.Find("PnlCtrl/Countdown/Text").GetComponent<Text>();
    }
    private void BindButton()
    {
        foreach (var pair in m_BtnOp)
        {
            var button = pair.Value;
            button.onClick.AddListener(delegate ()
            {
                SwitchOperationPanel(pair.Key);
            });
        }
    }
    private void BindEvent()
    {
        CombatManager.instance.GetState<CombatManager.OperationState>().onChangeFreezenTime += RefreshTime;
        CombatManager.instance.GetState<CombatManager.OperationState>().onChangeCountdownTime += RefreshTime;
    }
    private void RemoveEvent()
    {
        CombatManager.instance.GetState<CombatManager.OperationState>().onChangeFreezenTime -= RefreshTime;
        CombatManager.instance.GetState<CombatManager.OperationState>().onChangeCountdownTime -= RefreshTime;
    }
    private void SwitchOperationPanel(OperationType type)
    {
        if (!m_PnlOp.ContainsKey(type)) return;

        foreach (var pair in m_PnlOp)
        {
            var panel = pair.Value;
            panel.gameObject.SetActive(false);
        }
        m_PnlOp[type].gameObject.SetActive(true);

    }
    private void RefreshTime(Int32 time)
    {
        m_TxtCountdown.text = time.ToString();
    }
    public enum OperationType
    {
        Quest,
        Market,
        Process,
    }
}
