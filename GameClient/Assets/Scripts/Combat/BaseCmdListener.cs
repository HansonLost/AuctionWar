using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Google.Protobuf;
using HamPig;
using AuctionWar;

public interface ICmdListener
{
    void Invoke(Int32 playerId, ByteString parm);
}

public abstract class BaseCmdListener<TSelf, TCmd> : Singleton<TSelf>, ICmdListener
    where TSelf : class, new()
    where TCmd : IMessage<TCmd>
{
    public abstract GameConst.CommandType GetCommandType();
    public abstract MessageParser<TCmd> GetParser();

    private Action<Int32, TCmd> m_Action;
    public BaseCmdListener()
    {
        CombatFrameManager.instance.RegisterCommand(this.GetCommandType(), this);
    }
    public void AddListener(Action<Int32, TCmd> action)
    {
        m_Action += action;
    }
    public void RemoveListener(Action<Int32, TCmd> action)
    {
        m_Action -= action;
    }
    public void Invoke(Int32 playerId, ByteString parm)
    {
        var parser = this.GetParser();
        var cmd = parser.ParseFrom(parm);
        m_Action.Invoke(playerId, cmd);
    }
}

public class CmdLogListener : BaseCmdListener<CmdLogListener, CmdLog>
{
    public override GameConst.CommandType GetCommandType() => GameConst.CommandType.Log;
    public override MessageParser<CmdLog> GetParser() => CmdLog.Parser;
}

public class CmdClaimQuestListener : BaseCmdListener<CmdClaimQuestListener, CmdClaimQuest>
{
    public override GameConst.CommandType GetCommandType() => GameConst.CommandType.ClaimQuest;
    public override MessageParser<CmdClaimQuest> GetParser() => CmdClaimQuest.Parser;
}

public class CmdBuyMaterialListener : BaseCmdListener<CmdBuyMaterialListener, CmdBuyMaterial>
{
    public override GameConst.CommandType GetCommandType() => GameConst.CommandType.BuyMaterial;
    public override MessageParser<CmdBuyMaterial> GetParser() => CmdBuyMaterial.Parser;
}

public class CmdAuctionRisePriceListener : BaseCmdListener<CmdAuctionRisePriceListener, CmdAuctionRisePrice>
{
    public override GameConst.CommandType GetCommandType() => GameConst.CommandType.AuctionRisePrice;
    public override MessageParser<CmdAuctionRisePrice> GetParser() => CmdAuctionRisePrice.Parser;
}

public class CmdPutInMaterialListener : BaseCmdListener<CmdPutInMaterialListener, CmdPutInMaterial>
{
    public override GameConst.CommandType GetCommandType() => GameConst.CommandType.PutInMaterial;
    public override MessageParser<CmdPutInMaterial> GetParser() => CmdPutInMaterial.Parser;
}

public class CmdRefreshMarketListener : BaseCmdListener<CmdRefreshMarketListener, CmdRefreshMarket>
{
    public override GameConst.CommandType GetCommandType() => GameConst.CommandType.RefreshMarket;
    public override MessageParser<CmdRefreshMarket> GetParser() => CmdRefreshMarket.Parser;
}

public class CmdSellMaterialListener : BaseCmdListener<CmdSellMaterialListener, CmdSellMaterial>
{
    public override GameConst.CommandType GetCommandType() => GameConst.CommandType.SellMaterial;
    public override MessageParser<CmdSellMaterial> GetParser() => CmdSellMaterial.Parser;
}