using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ProcessStorehouseView : MonoBehaviour
    , IPointerEnterHandler
{
#pragma warning disable 0649
    [SerializeField] private GameObject m_PrefabSelector;
    [SerializeField] private Text m_TxtMatName;
    [SerializeField] private Text m_TxtMatCount;
#pragma warning restore 0649

    private ProcessView m_ProcessView;
    private bool m_IsLock = false;
    private Int32 m_HouseIdx;
    private bool m_IsSelect = false;

    public void Init(ProcessView view, Int32 idx)
    {
        m_ProcessView = view;
        m_HouseIdx = idx;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (m_IsLock || m_IsSelect) return;
        m_IsSelect = true;
        var panel = CanvasManager.instance.CreatePanel(CanvasManager.PanelLevelType.UI, m_PrefabSelector);
        panel.transform.position = this.transform.position;
        var drager = panel.GetComponent<DragSelector>();
        drager.onBeginSelect.AddListener(() =>
        {
            m_ProcessView.SetQuestReceiverActive(true);
        });
        drager.onSelect.AddListener((GameObject go) =>
        {
            m_IsSelect = false;
            m_ProcessView.SetQuestReceiverActive(false);
            var block = FindBlockView(go);
            if (block != null)
            {
                var state = CombatManager.instance.GetState<CombatManager.OperationState>();
                state.TryPutInMaterial(m_HouseIdx, block.index);
            }
        });
        drager.onCancel.AddListener(() =>
        {
            m_IsSelect = false;
        });
    }
    
    public void SetLock(bool value)
    {
        m_IsLock = value;
        if (value)
        {
            m_TxtMatCount.text = "?";
            m_TxtMatName.text = "锁";
        }
    }
    public void RefreshView(CombatGameCenter.Material mat)
    {
        if (m_IsLock) return;
        if (mat.IsEmpty())
        {
            m_TxtMatName.text = "空闲";
            m_TxtMatCount.text = "0";
        }
        else
        {
            m_TxtMatName.text = mat.name;
            m_TxtMatCount.text = mat.count.ToString();
        }
    }

    private ProcessBlockView FindBlockView(GameObject go)
    {
        var pointer = go.GetComponent<RootPointer>();
        if (!pointer) return null;
        var root = pointer.root.gameObject;
        var view = root.GetComponent<ProcessBlockView>();
        return view;
    }

    public class UnitySelectEvent : UnityEvent<GameObject> { }
}
