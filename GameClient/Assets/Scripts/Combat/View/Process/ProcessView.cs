using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProcessView : MonoBehaviour
{
    protected CombatGameCenter gameCenter { get { return CombatManager.instance.gameCenter; } }
    protected CombatGameCenter.Player selfPlayer { get { return gameCenter.playerSet.GetSelfPlayer(); } }
    private List<QuestPanel> m_QuestPanels = new List<QuestPanel>();

    private void Awake()
    {
        LoadResource();
        BindReference();
    }
    private void Start()
    {
        BindEvent();
        UpdateQuestPanel();
    }
    private void OnDestroy()
    {
        RemoveEvent();
    }

    private void LoadResource()
    {
        QuestPanel.pfbQuestMat = Resources.Load("Combat/Process/BlkMaterial") as GameObject;
    }
    private void BindReference()
    {
        var root = this.transform;
        // 任务面板
        for (int i = 1; i <= GameConst.COUNT_CLAIM_QUEST; i++)
        {
            string path = String.Format("Quest/ImgQuest ({0})", i);
            var questPanel = new QuestPanel();
            questPanel.index = i - 1;
            questPanel.txtQuestName = root.Find(String.Format("{0}/TxtName", path)).GetComponent<Text>();
            questPanel.materialRoot = root.Find(String.Format("{0}/Materials", path)).GetComponent<RectTransform>();
            questPanel.txtRunTime = root.Find(String.Format("{0}/TxtRunTime", path)).GetComponent<Text>();
            questPanel.txtTotalTime = root.Find(String.Format("{0}/TxtTotalTime", path)).GetComponent<Text>();
            m_QuestPanels.Add(questPanel);
        }
    }
    private void BindEvent()
    {
        var player = this.selfPlayer;
        player.processFactory.onAddMaterial += OnUpdateQuestPanel;
        player.onAddQuest += OnAddUpdate;
    }
    private void RemoveEvent()
    {
        var player = this.selfPlayer;
        player.processFactory.onAddMaterial -= OnUpdateQuestPanel;
        player.onAddQuest -= OnAddUpdate;
    }
    private void UpdateQuestPanel()
    {
        for (int i = 0; i < m_QuestPanels.Count; i++)
        {
            var block = selfPlayer.processFactory.GetProcessBlock(i);
            var panel = m_QuestPanels[i];
            if(block != null)
            {
                Int32 runTime = 0;
                if (block.isRun)
                    runTime = (Int32)CombatFrameManager.GetIntervalTime(block.startSeq, CombatFrameManager.instance.seq);
                panel.Update(block.quest, runTime);
            }
            else
            {
                panel.Clear();
            }

        }
    }

    // --- callback --- //
    private void OnUpdateQuestPanel(Int32 idx, CombatGameCenter.ProcessFactory.ProcessBlock block, CombatGameCenter.Material mat)
    {
        UpdateQuestPanel();
    }
    private void OnAddUpdate(CombatGameCenter.Quest quest)
    {
        UpdateQuestPanel();
    }

    public class QuestPanel
    {
        public static GameObject pfbQuestMat;

        public Int32 index;
        public Text txtQuestName;
        public RectTransform materialRoot;
        public Text txtRunTime;
        public Text txtTotalTime;

        private List<GameObject> m_BlkMats = new List<GameObject>();

        public void Update(CombatGameCenter.Quest quest, Int32 runTime)
        {
            txtQuestName.text = quest.name;
            txtTotalTime.text = String.Format("{0}秒", quest.processSecond);
            txtRunTime.text = String.Format("{0}秒", runTime);

            Int32 matCount = quest.materials.Count;
            if(m_BlkMats.Count < matCount)
            {
                Int32 size = matCount - m_BlkMats.Count;
                for (int i = 0; i < size; i++)
                {
                    var go = Instantiate(pfbQuestMat, materialRoot);
                    m_BlkMats.Add(go);
                }
            }

            for (int i = 0; i < m_BlkMats.Count; i++)
            {
                m_BlkMats[i].SetActive(i < matCount);
            }

            for (int i = 0; i < matCount; i++)
            {
                var questMat = quest.materials[i];
                var root = m_BlkMats[i].transform;
                Text txtName = root.Find("TxtName").GetComponent<Text>();
                Text txtCount = root.Find("TxtCount").GetComponent<Text>();

                txtName.text = questMat.name;
                txtCount.text = String.Format("{0} / {1}", 0, questMat.count);
            }
        }
        public void Clear()
        {
            txtQuestName.text = "空闲";
            txtTotalTime.text = "";
            txtRunTime.text = "";
            for (int i = 0; i < m_BlkMats.Count; i++)
            {
                m_BlkMats[i].SetActive(false);
            }
        }
    }
}
