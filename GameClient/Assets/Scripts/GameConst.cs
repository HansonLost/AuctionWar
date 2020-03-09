
using System;

public static class GameConst
{
    public enum SceneType : Int32
    {
        Root,
        Player,
        Combat,
    }

    public const float INTERVAL_HEART_BEAT = 5.0f;
    public const float INTERVAL_MAX_STOP_BEAT = 8.0f;
}
