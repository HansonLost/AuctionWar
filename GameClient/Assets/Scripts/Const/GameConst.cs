
using System;

public static class GameConst
{
    public enum SceneType : Int32
    {
        Root,
        Player,
        Combat,
    }

    public enum CommandType : Int32
    {
        Log,
    }

    public const float INTERVAL_HEART_BEAT = 5.0f;
    public const float INTERVAL_MAX_STOP_BEAT = 8.0f;

    public const Int32 COMBAT_OPERATION_TIME = 60;   // 对战的运营阶段总时间
    public const Int32 COUNT_QUEST = 9;             // 需求总数量
}
