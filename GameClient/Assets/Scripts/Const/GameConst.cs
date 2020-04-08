
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
        AuctionRisePrice,
        PutInMaterial,
        RefreshMarket,
        SellMaterial,
        AuctionPass,
        BuyWholesale,
    }

    public const float INTERVAL_HEART_BEAT = 5.0f;
    public const float INTERVAL_MAX_STOP_BEAT = 8.0f;

    public const Int32 COMBAT_OPERATION_TIME = 10;   // 对战的运营阶段总时间
    public const Int32 COUNT_QUEST = 9;             // 需求总数量
    public const Int32 COUNT_MARKET = 3;            // 原料市场可出售总数量
    public const Int32 COUNT_WHOLESALE = 2;         // 批发商最高数量
    public const Int32 COUNT_STOREHOUSE_BEGIN = 4;        // 玩家初始时仓库数量
    public const Int32 COUNT_STOREHOUSE_MAX = 6;        // 玩家仓库最大数量
    public const Int32 COMBAT_VAST = 100;           // 投资金额
    public const Int32 COUNT_CLAIM_QUEST = 2;       // 任务最大领取数量
}
