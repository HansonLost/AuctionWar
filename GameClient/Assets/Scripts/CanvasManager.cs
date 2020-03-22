using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : GameBaseManager<CanvasManager>
{
    protected override CanvasManager GetInstance() => this;
    protected override bool IsDonDestroyOnLoad() => false;

#pragma warning disable 0649
    [SerializeField] private RectTransform m_Background;
    [SerializeField] private RectTransform m_Normal;
    [SerializeField] private RectTransform m_UI;
    [SerializeField] private RectTransform m_Top;
#pragma warning restore 0649

    private Dictionary<PanelLevelType, RectTransform> m_PnlNodes;

    protected override void Awake()
    {
        base.Awake();
        m_PnlNodes = new Dictionary<PanelLevelType, RectTransform>
        {
            { PanelLevelType.Background, m_Background },
            { PanelLevelType.Normal, m_Normal },
            { PanelLevelType.UI, m_UI },
            { PanelLevelType.Top, m_Top },
        };
    }

    public GameObject CreatePanel(PanelLevelType level, String path)
    {
        if (!m_PnlNodes.ContainsKey(level)) return null;
        var node = m_PnlNodes[level];
        var pnl = Resources.Load(path);
        if (pnl == null) return null;
        var go = (GameObject)GameObject.Instantiate(pnl, node);
        return go;
    }

    public enum PanelLevelType
    {
        Background,
        Normal,
        UI,
        Top,
    }
}
