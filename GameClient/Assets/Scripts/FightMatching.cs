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
    [SerializeField] private Button m_BtnQuit;
    [SerializeField] private Text m_TxtLog;
#pragma warning restore 0649

    private bool m_IsMatching = false;
    private float m_StartMatchTime = 0;

    public void Awake()
    {
        CombatMatchResListener.instance.AddListener(this.LogMatchMsg);

        m_BtnQuit.onClick.AddListener(() =>
        {
            Utility.QuitGame();
        });
        m_BtnMatch.onClick.AddListener(this.CombatMatching);
        m_BtnCancel.onClick.AddListener(this.CancelCambatMatching);
    }

    Action action;
    // Start is called before the first frame update
    void Start()
    {
        m_TxtLog.text = "";
        m_BtnMatch.interactable = true;
        m_TxtLog.gameObject.SetActive(false);
        m_BtnCancel.gameObject.SetActive(false);
    }
    private void Update()
    {
        if(m_IsMatching)
        {
            float second = Time.time - m_StartMatchTime;
            m_TxtLog.text = TimeFormat(second);
        }
    }
    private void OnDestroy()
    {
        CombatMatchResListener.instance.RemoveListener(this.LogMatchMsg);
    }

    // 匹配对手
    public void CombatMatching()
    {
        m_IsMatching = true;
        m_StartMatchTime = Time.time;
        m_BtnMatch.interactable = false;
        m_BtnCancel.gameObject.SetActive(true);
        m_TxtLog.gameObject.SetActive(true);
        m_TxtLog.text = TimeFormat(0);
        var combatMatch = new CombatMatch { };
        var id = (Int16)ProtocType.CombatMatch;
        NetManager.Send(id, combatMatch);
    }

    public void CancelCambatMatching()
    {
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
            MatchSystem.instance.random = new System.Random(combatMatchRes.Seed);
            MatchSystem.instance.playerCount = combatMatchRes.PlayerCount;
            MatchSystem.instance.selfId = combatMatchRes.SelfId;
            SceneManager.LoadScene((Int32)GameConst.SceneType.Combat);
        }

        m_IsMatching = false;
        m_BtnMatch.interactable = true;
        m_TxtLog.gameObject.SetActive(false);
        m_BtnCancel.gameObject.SetActive(false);
    }

    private string TimeFormat(float second)
    {
        Int32 sec = (Int32)second % 60;
        Int32 min = (Int32)second / 60;
        return String.Format("{0:00}:{1:00}", min, sec);
    }
}
