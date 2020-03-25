using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestInfoView : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private GameObject m_MaterialPrefab;
#pragma warning restore 0649

    private Text m_TxtName;
    private RectTransform m_MaterialRoot;
    private Text m_TxtProcessTime;
    private Text m_TxtReward;

    private List<GameObject> m_Materials = new List<GameObject>();
    private Dictionary<CombatGameCenter.Material.Type, String> m_TypeToName = new Dictionary<CombatGameCenter.Material.Type, string>
    {
        { CombatGameCenter.Material.Type.Wood,      "木材" },
        { CombatGameCenter.Material.Type.Iron,      "钢铁" },
        { CombatGameCenter.Material.Type.Stone,     "石头" },
        { CombatGameCenter.Material.Type.Fuel,      "燃料" },
        { CombatGameCenter.Material.Type.Clay,      "粘土" },
        { CombatGameCenter.Material.Type.Cotton,    "棉花" },
        { CombatGameCenter.Material.Type.Plastic,   "塑料" },
    };

    private void Awake()
    {
        BindReference();
    }

    private void BindReference()
    {
        var root = this.transform;
        m_TxtName = root.Find("Name/TxtName").GetComponent<Text>();
        m_MaterialRoot = root.Find("Materials").GetComponent<RectTransform>();
        m_TxtProcessTime = root.Find("ProcessTime").GetComponent<Text>();
        m_TxtReward = root.Find("TxtReward").GetComponent<Text>();
    }

    public void SetName(String name)
    {
        m_TxtName.text = name;
    }
    public void SetProcessTime(Int32 value)
    {
        m_TxtProcessTime.text = String.Format("{0} 秒", value.ToString());
    }
    public void SetReward(Int32 value)
    {
        m_TxtReward.text = String.Format("{0} 金币", value.ToString());
    }
    public void SetMaterials(List<CombatGameCenter.Material> materials)
    {
        foreach (var go in m_Materials)
        {
            Destroy(go);
        }
        m_Materials.Clear();

        foreach (var m in materials)
        {
            String name = m_TypeToName[m.type];
            var go = Instantiate(m_MaterialPrefab, m_MaterialRoot) as GameObject;
            var txtName = go.transform.Find("TxtName").GetComponent<Text>();
            var txtCount = go.transform.Find("TxtCount").GetComponent<Text>();
            txtName.text = name;
            txtCount.text = String.Format("× {0}", m.count);
            m_Materials.Add(go);
        }
    }
}
