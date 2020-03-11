using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JustScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var target = DebugManager.instance;
            if (target == null)
                Debug.Log("target is null.");
            else
                Debug.Log("target is not null.");
        }
    }
}
