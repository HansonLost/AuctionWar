using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using AuctionWar;
using UnityEngine.UI;

public class QuestManager : MonoBehaviour
{
    public Button m_BtnQuest;

    private void Awake()
    {
        m_BtnQuest.onClick.AddListener(() =>
        {
            CombatFrameManager.instance.SendCommand(GameConst.CommandType.Log, 1, new CmdLog
            {
                Information = "我嬲你妈妈别咧",
            });
        });
    }
    private void Start()
    {
        CmdLogListener.instance.AddListener((Int32 playerId, CmdLog cmdLog) => 
        {
            Debug.Log(String.Format("玩家{0}说：{1}", playerId, cmdLog.Information));
        });

    }
}
