using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HamPig.Network;
using UnityEngine.SceneManagement;
using System;

public class NetSystem : MonoBehaviour
{
    private void Awake()
    {
        GameObject.DontDestroyOnLoad(this.gameObject);
    }
    // Start is called before the first frame update
    void Start()
    {
        NetManager.Connect("127.0.0.1", 8888);
        SceneManager.LoadScene((Int32)GameConst.SceneType.Player);
    }

    // Update is called once per frame
    void Update()
    {
        NetManager.Update();
    }
}
