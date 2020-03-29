
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
        ClaimQuest,
        BuyMaterial,
    }

    public const float INTERVAL_HEART_BEAT = 5.0f;
    public const float INTERVAL_MAX_STOP_BEAT = 8.0f;

    public const Int32 COMBAT_PLAYER_COUNT = 2;     // 对战人数
    public const Int32 COMBAT_OPERATION_TIME = 10;   // 对战的运营阶段总时间
    public const Int32 COUNT_QUEST = 9;             // 需求总数量
    public const Int32 COUNT_MARKET = 3;            // 原料市场可出售总数量
    public const Int32 COUNT_STOREHOUSE = 4;        // 普通状态下，玩家仓库数量
    public const Int32 COMBAT_VAST = 100;           // 投资金额
    public const Int32 COUNT_CLAIM_QUEST = 2;       // 任务最大领取数量
}
