using System;
using System.Collections;
using System.Collections.Generic;

public struct PropInfo
{
    public Int32 id;
    public string name;
    public Int32 upsetPrice;
    public Type script;
}

public class CombatPropHelper
{
    public static CombatProp RandomProp(Int32 seed)
    {
        if(tblProp.Count > 0)
        {
            Random random = new Random(seed);
            Int32 index = random.Next() % tblProp.Count;
            var info = tblProp[index];
            return Activator.CreateInstance(info.script, info) as CombatProp;
        }
        return null;
    }
    public static CombatProp CreateProp(Int32 id)
    {
        Int32 idx = id - 1;
        if(Utility.IsInRange(idx, 0, tblProp.Count - 1))
        {
            var info = tblProp[idx];
            return Activator.CreateInstance(info.script, info) as CombatProp;
        }
        return null;
    }

    public static readonly List<PropInfo> tblProp = new List<PropInfo>
    {
        new PropInfo
        {
            id = 1,
            name = "古老皇冠",
            upsetPrice = 3000,
            script = typeof(PropWin),
        },
        new PropInfo
        {
            id = 2,
            name = "仓库",
            upsetPrice = 30,
            script = typeof(PropStorehouse),
        },
    };
}

public abstract class CombatProp
{
    protected PropInfo info;
    protected CombatGameCenter.Player owner;

    public CombatProp(PropInfo info)
    {
        this.info = info;
    }
    public Int32 GetId()
    {
        return info.id;
    }
    public string GetName()
    {
        return info.name;
    }
    public Int32 GetUpsetPrice()
    {
        return info.upsetPrice;
    }

    public virtual void OnCollect(CombatGameCenter.Player player)
    {
        owner = player;
    }
    public virtual void OnDiscard()
    {
        owner = null;
    }
}

public class PropWin : CombatProp
{
    public PropWin(PropInfo info) : base(info) { }

    public override void OnCollect(CombatGameCenter.Player player)
    {
        base.OnCollect(player);
        // TODO : 胜利
        CombatManager.instance.QuitCombat();
    }
}

public class PropStorehouse : CombatProp
{
    public PropStorehouse(PropInfo info) : base(info) { }

    public override void OnCollect(CombatGameCenter.Player player)
    {
        base.OnCollect(player);
        owner.SetStorehouseCapacity(owner.storehouseCapacity + 1);
    }
    public override void OnDiscard()
    {
        owner.SetStorehouseCapacity(owner.storehouseCapacity - 1);
        base.OnDiscard();
    }
}

public enum CombatPropType
{
    Win,
    Storehouse,
    WoodShop,
}



