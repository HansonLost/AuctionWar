using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogState : MonoBehaviour
{
    private bool m_IsCalled = false;

    private void Awake()
    {
        Debug.Log(string.Format("{0} - Awake", gameObject.name));
    }
    private void Start()
    {
        Debug.Log(string.Format("{0} - Start", gameObject.name));
    }
    private void Update()
    {
        if(!m_IsCalled)
        {
            Debug.Log(string.Format("{0} - Update", gameObject.name));
            m_IsCalled = true;
        }
    }
}
