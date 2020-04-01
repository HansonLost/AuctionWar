using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class JustScript : MonoBehaviour
    , IPointerEnterHandler
{
    public GameObject selector;

    private bool m_IsSelect = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (m_IsSelect) return;
        m_IsSelect = true;
        var instance = CanvasManager.instance.CreatePanel(CanvasManager.PanelLevelType.UI, selector);
        instance.GetComponent<RectTransform>().anchoredPosition3D = GetComponent<RectTransform>().anchoredPosition3D;
        var drager = instance.GetComponent<DragSelector>();
        drager.onSelect.AddListener((GameObject go) => 
        {
            m_IsSelect = false;
            LogGameObject(go);
        });
        drager.onCancel.AddListener(() =>
        {
            m_IsSelect = false;
            Debug.Log("Cancel select.");
        });

    }

    private void LogGameObject(GameObject go)
    {
        string name = (go != null ? go.name : "(None)");
        Debug.Log(name);
    }
}
