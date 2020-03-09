using UnityEngine;
using HamPig;

public class GameSystem : MonoBehaviour
{
    public static GameSystem instance { get; private set; }
    private void Awake()
    {
        if (instance != null) return;
        instance = this;
        GameObject.DontDestroyOnLoad(this.gameObject);
    }
    private void Update()
    {
        Timer.Update();
    }
}
