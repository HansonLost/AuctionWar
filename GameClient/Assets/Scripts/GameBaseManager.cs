using System;
using UnityEngine;

/// <summary>
/// MonoBehaviour 单例
/// </summary>
public abstract class GameBaseManager<TSelf> : MonoBehaviour
    where TSelf : GameBaseManager<TSelf>
{
    public static TSelf instance { get; private set; }

    protected virtual void Awake()
    {
        if(instance != null)
        {
            string name = GetInstance().GetType().ToString();
            GameObject.Destroy(this.gameObject);
            return;
        }

        instance = this.GetInstance();
        if (this.IsDonDestroyOnLoad()) GameObject.DontDestroyOnLoad(this.gameObject);
    }

    protected abstract TSelf GetInstance();
    protected abstract bool IsDonDestroyOnLoad();
}
