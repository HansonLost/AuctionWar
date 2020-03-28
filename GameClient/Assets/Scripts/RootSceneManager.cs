using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RootSceneManager : MonoBehaviour
{
    private bool m_IsSwitching = false;
    private void Update()
    {
        if (!m_IsSwitching)
        {
            m_IsSwitching = true;
            SceneManager.LoadScene((Int32)GameConst.SceneType.Player);
        }
    }
}
