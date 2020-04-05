using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MarketStorehouseView : MonoBehaviour
    , IPointerClickHandler
{
#pragma warning disable 0649
    [SerializeField] private Text m_TxtName;
    [SerializeField] private Text m_TxtCount;
#pragma warning restore 0649

    private Int32 m_StoreIdx;
    private bool m_IsLock = false;

    public void Init(Int32 storeIdx)
    {
        m_StoreIdx = storeIdx;
    }

    public void RefreshView(CombatGameCenter.Material mat)
    {
        if (m_IsLock)
        {
            return;
        }

        if (mat.IsEmpty())
        {
            m_TxtName.text = "空闲";
            m_TxtCount.text = "0";
        }
        else
        {
            m_TxtName.text = mat.name;
            m_TxtCount.text = mat.count.ToString();
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        var state = CombatManager.instance.GetState<CombatManager.OperationState>();
        state.TrySellMaterial(m_StoreIdx);
    }
    public void SetLock(bool value)
    {
        m_IsLock = value;
        if(value)
        {
            m_TxtCount.text = "?";
            m_TxtName.text = "锁";
        }
    }
}
