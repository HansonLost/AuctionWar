using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AuctionWar;
using System;
using HamPig.Network;
using UnityEngine.SceneManagement;

public class FightMatching : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private Button m_BtnMatch;
    [SerializeField] private Button m_BtnCancel;
    [SerializeField] private Text m_TxtLog;
#pragma warning restore 0649

    public void Awake()
    {
        CombatMatchResListener.instance.AddListener(this.LogMatchMsg);

        m_BtnMatch.onClick.AddListener(this.CombatMatching);
        m_BtnCancel.onClick.AddListener(this.CancelCambatMatching);
    }

    Action action;
    // Start is called before the first frame update
    void Start()
    {
        m_TxtLog.text = "请点击匹配";
    }

    private void OnDestroy()
    {
        CombatMatchResListener.instance.RemoveListener(this.LogMatchMsg);
    }

    // 匹配对手
    public void CombatMatching()
    {
        m_TxtLog.text = "匹配中";
        var combatMatch = new CombatMatch { };
        var id = (Int16)ProtocType.CombatMatch;
        NetManager.Send(id, combatMatch);
    }

    public void CancelCambatMatching()
    {
        m_TxtLog.text = "请点击匹配";
        NetManager.Send((Int16)ProtocType.CancelCombatMatch, new CancelCombatMatch { });
    }

    public void LogMatchMsg(CombatMatchRes combatMatchRes)
    {
        if(combatMatchRes.RoomId == CommonConst.ROOM_ERROR)
        {
            m_TxtLog.text = "匹配失败，请再次点击匹配";
        }
        else
        {
            MatchSystem.instance.randomSeed = combatMatchRes.Seed;
            SceneManager.LoadScene((Int32)GameConst.SceneType.Combat);
        }
    }
}
