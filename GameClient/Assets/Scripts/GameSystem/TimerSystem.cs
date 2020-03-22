using UnityEngine;
using HamPig;

public class TimerSystem : GameBaseManager<TimerSystem>
{
    protected override TimerSystem GetInstance() => this;
    protected override bool IsDonDestroyOnLoad() => true;

    private void Update()
    {
        Timer.Update();
    }
}
