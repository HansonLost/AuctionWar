using System;
using UnityEngine;

/// <summary>
/// MonoBehaviour 单例
/// </summary>
public abstract class GameBaseManager<T> : MonoBehaviour
    where T : GameBaseManager<T>
{
    public static T instance { get; private set; }

    protected virtual void Awake()
    {
        if(instance != null)
        {
            string name = GetInstance().GetType().ToString();
            Debug.LogWarning(String.Format("{0} : GameBaseManager 在场景中多于1个.", name));
            GameObject.Destroy(this.gameObject);
            return;
        }

        instance = this.GetInstance();
        if (this.IsDonDestroyOnLoad()) GameObject.DontDestroyOnLoad(this.gameObject);
    }

    protected abstract T GetInstance();
    protected abstract bool IsDonDestroyOnLoad();
}
