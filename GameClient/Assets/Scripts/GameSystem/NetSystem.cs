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
    public bool isConnnected { get; private set; }

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

        if (this.isConnnected)
        {
            this.UpdateHeartBeat();
        }
    }

    private void OnDestroy()
    {
        if (this.isConnnected)
        {
            this.isConnnected = false;
            NetManager.Close();
        }
    }

    private void ConnectServer()
    {
        NetManager.onConnect += delegate (bool isSucceed)
        {
            isConnnected = isSucceed;
            if (isSucceed)
            {
                Debug.Log("成功连接服务器");
                this.ResetHeartBeat();
            }
            else
            {
                Debug.Log("服务器拒绝连接");
            }
        };
        NetManager.onForceClose += delegate ()
        {
            this.isConnnected = false;
            Debug.Log("远程强制关闭连接");
        };

        NetManager.Connect("127.0.0.1", 8888);
    }

    private void ResetHeartBeat()   // 连接成功后调用
    {
        HeartbeatListener.instance.AddListener(this.HeartbeatRefresh);
        HamPig.Timer.CallInterval(GameConst.INTERVAL_HEART_BEAT, this.SendHeartBeat);

        m_SendBeat = Time.time;
        m_ReceiveBeat = Time.time;
    }

    private void UpdateHeartBeat()
    {
        if(Time.time - m_ReceiveBeat > GameConst.INTERVAL_MAX_STOP_BEAT)
        {
            Debug.Log("心跳中止过长，强制关闭 Socket.");
            HamPig.Timer.RemoveInterval(this.SendHeartBeat);
            isConnnected = false;
            NetManager.Close();
        }
    }

    private void SendHeartBeat()
    {
        Debug.Log("发送心跳包");
        NetManager.Send((Int16)ProtocType.Heartbeat, new Heartbeat());
        m_SendBeat = Time.time;
    }

    private void HeartbeatRefresh(Heartbeat heartbeat)
    {
        Debug.Log("新的心跳");
        m_ReceiveBeat = Time.time;
    }
}
