using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using HamPig;
using UnityEngine.Events;


public class ExButton : Button
{
    /// <summary>
    /// 指针离开UI元素
    /// </summary>
    public ButtonExitEvent onExit { get; set; }
    /// <summary>
    /// 指针进入UI元素上
    /// </summary>
    public ButtonEnterEvent onEnter { get; set; }
    /// <summary>
    /// 指针悬停在UI元素上
    /// </summary>
    public ButtonFocusSpanEvent onFocusSpan { get; set; }
    private readonly float m_SpanTime = 0.3f;
    private Timer.Handle m_FocusHandle;

    protected ExButton() : base()
    {
        onExit = new ButtonExitEvent();
        onEnter = new ButtonEnterEvent();
        onFocusSpan = new ButtonFocusSpanEvent();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        onEnter.Invoke();
        m_FocusHandle = Timer.CallOnce(m_SpanTime, () =>
        {
            onFocusSpan.Invoke();
            m_FocusHandle = null;
        });
    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        onExit.Invoke();
        if (m_FocusHandle != null)
        {
            Timer.Remove(m_FocusHandle);
            m_FocusHandle = null;
        }
    }

    public class ButtonExitEvent : UnityEvent { }
    public class ButtonEnterEvent : UnityEvent { }
    public class ButtonFocusSpanEvent : UnityEvent { }
}




