using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class CombatManager : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Button m_BtnQuit;
#pragma warning disable 0649

    private void Awake()
    {
        m_BtnQuit.onClick.AddListener(this.QuitCombat);
    }

    private void QuitCombat()
    {
        SceneManager.LoadScene((Int32)GameConst.SceneType.Player);
    }
}
