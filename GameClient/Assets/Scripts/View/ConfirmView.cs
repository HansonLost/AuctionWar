using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ConfirmView : MonoBehaviour
{
    private Text m_TxtMsg;
    private Button m_BtnConfirm;

    public Button.ButtonClickedEvent onConfirm { get { return m_BtnConfirm.onClick; } }

    private void Awake()
    {
        BindReference();
    }

    private void BindReference()
    {
        var root = this.transform;
        m_TxtMsg = root.Find("ImgTextBlock/Text").GetComponent<Text>();
        m_BtnConfirm = root.Find("BtnConfirm").GetComponent<Button>();
    }
    public void SetLogMessage(String msg)
    {
        m_TxtMsg.text = msg;
    }
}
