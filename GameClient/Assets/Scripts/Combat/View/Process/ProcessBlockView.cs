using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProcessBlockView : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Text m_TxtQuestName;
    [SerializeField] private RectTransform m_PnlMatRoot;
    [SerializeField] private Text m_TxtTotalTime;
    [SerializeField] private Text m_TxtRunTime;
    [SerializeField] private RectTransform m_PnlReceiver;
    [SerializeField] private GameObject m_PrefabProcessMaterialBlock;
#pragma warning restore 0649

    public Int32 index { get; private set; }

    private List<GameObject> m_BlkMats = new List<GameObject>();
    private Int32 m_MaterialCount = 0;

    private void Awake()
    {
        m_PnlReceiver.gameObject.SetActive(false);
    }

    public void Init(Int32 idx)
    {
        index = idx;
    }

    public void SetReceiverPanelActive(bool isActive)
    {
        m_PnlReceiver.gameObject.SetActive(isActive);
    }
    public void ResetQuest(CombatGameCenter.Quest quest)
    {
        if (quest.IsEmpty())
        {
            m_TxtQuestName.text = "空闲";
            m_TxtTotalTime.text = "";
            m_TxtRunTime.text = "";
            foreach (var panel in m_BlkMats)
            {
                panel.SetActive(false);
            }
            m_MaterialCount = 0;
        }
        else
        {
            m_TxtQuestName.text = quest.name;
            m_TxtTotalTime.text = String.Format("{0} 秒", quest.processSecond);
            m_TxtRunTime.text = "0 秒";

            Int32 matCount = quest.materials.Count;
            // 补充缺少的材料块
            if (m_BlkMats.Count < matCount)
            {
                Int32 size = matCount - m_BlkMats.Count;
                for (int i = 0; i < size; i++)
                {
                    var go = Instantiate(m_PrefabProcessMaterialBlock, m_PnlMatRoot);
                    m_BlkMats.Add(go);
                }
            }
            // 隐藏多余的材料块
            for (int i = 0; i < m_BlkMats.Count; i++)
            {
                m_BlkMats[i].SetActive(i < matCount);
            }
            // 设置材料块信息
            for (int i = 0; i < matCount; i++)
            {
                var questMat = quest.materials[i];
                var root = m_BlkMats[i].transform;
                Text txtName = root.Find("TxtName").GetComponent<Text>();
                Text txtCount = root.Find("TxtCount").GetComponent<Text>();
                Text txtMaxCount = root.Find("TxtMaxCount").GetComponent<Text>();

                txtName.text = questMat.name;
                txtMaxCount.text = String.Format("/ {0}", questMat.count);
                txtCount.text = "0";
            }
            m_MaterialCount = matCount;
        }
    }
    public void UpdateQuest(List<Int32> matBuffers, Int32 runTime)
    {
        m_TxtRunTime.text = String.Format("{0} 秒", runTime);
        Int32 count = Mathf.Min(matBuffers.Count, m_MaterialCount);
        for (int i = 0; i < count; i++)
        {
            var root = m_BlkMats[i].transform;
            Text txtCount = root.Find("TxtCount").GetComponent<Text>();
            txtCount.text = matBuffers[i].ToString();
        }
    }
}
