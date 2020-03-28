using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JustScript : MonoBehaviour
{
    public GameObject target;

    private bool m_IsBuilt;

    //private void Start()
    //{
    //    Instantiate(target, this.transform);
    //    Debug.Log("Finish instantiate.");
    //}


    private void Update()
    {
        
        if (!m_IsBuilt)
        {
            Instantiate(target, this.transform);
            Debug.Log("Finish instantiate.");
            m_IsBuilt = true;
        }
    }
}
