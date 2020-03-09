using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HamPig.Network;
using UnityEngine.SceneManagement;
using System;
using AuctionWar;

public class NetSystem : MonoBehaviour
{
    public static NetSystem instance { get; private set; }
    public bool isConnnect { get; private set; }

    private float m_SendBeat;   // 上一次发送 beat 的时间
    private float m_ReceiveBeat;   // 上一次接收 beat 的时间

    private void Awake()
    {
        if (instance != null) return;
        instance = this;

        GameObject.DontDestroyOnLoad(this.gameObject);

        this.ConnectServer();
        
    }

    private void Start()
    {
        SceneManager.LoadScene((Int32)GameConst.SceneType.Player);
    }

    private void Update()
    {
        NetManager.Update();

        if (this.isConnnect)
        {
            this.UpdateHeartBeat();
        }
    }

    private void OnDestroy()
    {
           
    }

    private void ConnectServer()
    {
        NetManager.onConnect += delegate (bool isSucceed)
        {
            isConnnect = isSucceed;
            Debug.Log(String.Format("连接{0}", isSucceed));
            if (isSucceed)
            {
                this.ResetHeartBeat();
            }
        };

        NetManager.Connect("127.0.0.1", 8888);
    }

    private void ResetHeartBeat()   // 连接成功后调用
    {
        HeartbeatListener.instance.AddListener(this.HeartbeatRefresh);

        m_SendBeat = Time.time;
    }

    private void UpdateHeartBeat()  // 连接状态才进行倒计时
    {

    }

    private void HeartbeatRefresh(Heartbeat heartbeat)  // 协议驱动刷新心跳
    {

    }
}
