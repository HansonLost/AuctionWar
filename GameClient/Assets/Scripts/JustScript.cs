using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JustScript : MonoBehaviour
{
    public GameObject target;
    private ExButton m_ExButton;

    private void Awake()
    {
        m_ExButton = GetComponent<ExButton>();
        target.SetActive(false);
        m_ExButton.onFocusSpan.AddListener(() =>
        {
            target.SetActive(true);
        });
        m_ExButton.onExit.AddListener(() =>
        {
            target.SetActive(false);
        });
    }
}
