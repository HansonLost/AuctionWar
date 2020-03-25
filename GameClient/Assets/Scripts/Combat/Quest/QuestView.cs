using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using AuctionWar;
using UnityEngine.UI;
using HamPig;

public class QuestView : MonoBehaviour
{
    private List<QuestButton> m_QuestButtons = new List<QuestButton>();
    private QuestInfoView m_ViewInformation;
    private List<Text> m_TxtPlayerQuests = new List<Text>();

    protected CombatGameCenter gameCenter { get { return CombatManager.instance.gameCenter; } }

    private void Awake()
    {
        BindReference();
        InitPlayerQuests();

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
        m_ViewInformation.gameObject.SetActive(false);
        InitQuest();
        BindQuestButton();
        BindPlayerQuestsEvent();
        //CmdLogListener.instance.AddListener((Int32 playerId, CmdLog cmdLog) => 
        //{
        //    Debug.Log(String.Format("玩家{0}说：{1}", playerId, cmdLog.Information));
        //});
    }
    private void OnDestroy()
    {
        RemovePlayerQuestsEvent();
        RemoveQuestMarketEvent();
    }

    private void BindReference()
    {
        Transform root = this.transform;
        for (int i = 0; i < GameConst.COUNT_QUEST; i++)
        {
            var button = transform.Find(String.Format("Quests/BtnQuest ({0})", i + 1)).GetComponent<ExButton>();
            var title = transform.Find(String.Format("Quests/BtnQuest ({0})/Text", i + 1)).GetComponent<Text>();
            m_QuestButtons.Add(new QuestButton
            {
                index = i,
                botton = button,
                title = title,
            });
        }
        m_ViewInformation = root.Find("QuestInformation").GetComponent<QuestInfoView>();
        for (int i = 1; i <= 2; i++)
        {
            Text txt = root.Find(String.Format("PlayerQuests/Quest ({0})/Text", i)).GetComponent<Text>();
            Debug.Log(txt.name);
            m_TxtPlayerQuests.Add(txt);
        }
        
    }
    private void BindQuestButton()
    {
        foreach (var item in m_QuestButtons)
        {
            // 任务详细面板
            item.botton.onEnter.AddListener(() =>
            {
                var quest = this.gameCenter.questMarket.GetQuest(item.index);
                ResetQuestInformationView(quest);
                m_ViewInformation.gameObject.SetActive(true);
            });
            // 接取任务
            item.botton.onClick.AddListener(() =>
            {
                CombatManager.instance.ClaimQuest(item.index);
            });
        }
        // 任务状态
        this.gameCenter.questMarket.onHandOutQuest += HandOutQuest;
    }
    private void BindPlayerQuestsEvent()
    {
        var gameCenter = CombatManager.instance.gameCenter;
        var selfPlayer = gameCenter.GetSelfPlayer();
        selfPlayer.onAddQuest += AddPlayerQuest;
    }
    private void RemovePlayerQuestsEvent()
    {
        var selfPlayer = this.gameCenter.GetSelfPlayer();
        selfPlayer.onAddQuest -= AddPlayerQuest;
    }
    private void RemoveQuestMarketEvent()
    {
        this.gameCenter.questMarket.onHandOutQuest -= HandOutQuest;
    }
    private void InitPlayerQuests()
    {
        var gameCenter = CombatManager.instance.gameCenter;
        var selfPlayer = gameCenter.GetSelfPlayer();
        Int32 startIdx = 0;
        selfPlayer.ForEachQuest((CombatGameCenter.Quest quest) =>
        {
            if(startIdx < m_TxtPlayerQuests.Count)
            {
                m_TxtPlayerQuests[startIdx].text = quest.name;
            }
            startIdx++;
        });
        for (int i = startIdx; i < m_TxtPlayerQuests.Count; i++)
        {
            m_TxtPlayerQuests[i].text = "";
        }
    }
    private void InitQuest()
    {
        for (int i = 0; i < GameConst.COUNT_QUEST; i++)
        {
            var quest = this.gameCenter.questMarket.GetQuest(i);
            var state = this.gameCenter.questMarket.GetQuestState(i);
            var btn = m_QuestButtons[i];
            btn.title.text = quest.name;
            btn.botton.interactable = (state == CombatGameCenter.QuestMarket.QuestStateType.Normal);
        }
    }
    private void ResetQuestInformationView(CombatGameCenter.Quest quest)
    {
        m_ViewInformation.SetName(quest.name);
        m_ViewInformation.SetMaterials(quest.materials);
        m_ViewInformation.SetProcessTime(quest.processSecond);
        m_ViewInformation.SetReward(quest.reward);
    }

    // --- callback --- //
    private void AddPlayerQuest(CombatGameCenter.Quest quest)
    {
        foreach (var txt in m_TxtPlayerQuests)
        {
            if (txt.text == "")
            {
                txt.text = quest.name;
                break;
            }
        }
    }
    private void HandOutQuest(Int32 questIdx, CombatGameCenter.Quest quest)
    {
        if(questIdx >= 0 && questIdx < m_QuestButtons.Count)
        {
            var btn = m_QuestButtons[questIdx];
            btn.botton.interactable = false;
        }
    }

    public class QuestButton
    {
        public Int32 index;
        public ExButton botton;
        public Text title;
    }
}
