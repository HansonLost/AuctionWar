using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using AuctionWar;
using UnityEngine.UI;

public class QuestView : MonoBehaviour
{
    private List<QuestButton> m_QuestButtons = new List<QuestButton>();

    private void Awake()
    {
        BindReference();


        //m_BtnQuest.onClick.AddListener(() =>
        //{
        //    CombatFrameManager.instance.SendCommand(GameConst.CommandType.Log, 1, new CmdLog
        //    {
        //        Information = "我嬲你妈妈别咧",
        //    });
        //});
    }
    private void Start()
    {
        RefreshQuest();
        CmdLogListener.instance.AddListener((Int32 playerId, CmdLog cmdLog) => 
        {
            Debug.Log(String.Format("玩家{0}说：{1}", playerId, cmdLog.Information));
        });

    }

    private void BindReference()
    {
        Transform root = this.transform;
        for (int i = 0; i < GameConst.COUNT_QUEST; i++)
        {
            var button = transform.Find(String.Format("Quests/BtnQuest{0}", i + 1)).GetComponent<Button>();
            var title = transform.Find(String.Format("Quests/BtnQuest{0}/Text", i + 1)).GetComponent<Text>();
            m_QuestButtons.Add(new QuestButton
            {
                botton = button,
                title = title,
            });
        }
    }
    private void RefreshQuest()
    {
        var gameCenter = CombatManager.instance.gameCenter;
        for (int i = 0; i < GameConst.COUNT_QUEST; i++)
        {
            var quest = gameCenter.GetQuest(i);
            m_QuestButtons[i].title.text = quest.name;
        }
    }

    public class QuestButton
    {
        public Button botton;
        public Text title;
    }
}
