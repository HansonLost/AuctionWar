using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class DragSelector : MonoBehaviour
    , IBeginDragHandler
    , IDragHandler
    , IEndDragHandler
    , IPointerExitHandler
    , IPointerClickHandler
{
    public UnityBeginSelectEvent onBeginSelect { get; set; } = new UnityBeginSelectEvent();
    public UnitySelectEvent onSelect { get; set; } = new UnitySelectEvent();
    public UnityCancelEvent onCancel { get; set; } = new UnityCancelEvent();

    private CanvasGroup m_Group;
    private bool m_IsDrag = false;
    private bool m_IsClick = false;

    private void Awake()
    {
        m_Group = GetComponent<CanvasGroup>();
    }

    private void Update()
    {
        if (m_IsClick)
        {
            this.transform.position = Input.mousePosition;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        m_IsDrag = true;
        m_Group.blocksRaycasts = false;
        onBeginSelect.Invoke();
    }

    public void OnDrag(PointerEventData eventData)
    {
        this.transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        onSelect.Invoke(eventData.pointerEnter);
        Destroy(this.gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (m_IsDrag || m_IsClick) return;
        onCancel.Invoke();
        Destroy(this.gameObject);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (m_IsDrag) return;
        if (m_IsClick)
        {
            // 选择
            m_Group.blocksRaycasts = false;
            var list = CanvasManager.instance.RaycastUI();
            GameObject topUI = (list.Count > 0 ? list[0].gameObject : null);
            onSelect.Invoke(topUI);
            Destroy(gameObject);
        }
        else
        {
            // 寻找
            m_IsClick = true;
            onBeginSelect.Invoke();
        }
    }

    public class UnityBeginSelectEvent : UnityEvent { }
    public class UnitySelectEvent : UnityEvent<GameObject> { }
    public class UnityCancelEvent : UnityEvent { }
}
