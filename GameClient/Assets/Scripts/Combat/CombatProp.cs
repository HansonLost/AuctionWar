using System;
using System.Collections;
using System.Collections.Generic;

public struct PropInfo
{
    public Int32 id;
    public string name;
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
            var type = tblProp[index].script;
            return Activator.CreateInstance(type) as CombatProp;
        }
        return null;
    }

    public static readonly List<PropInfo> tblProp = new List<PropInfo>
    {
        new PropInfo
        {
            id = 1,
            name = "古老皇冠",
            script = typeof(PropWin),
        },
        new PropInfo
        {
            id = 2,
            name = "仓库",
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
    public string GetName()
    {
        return info.name;
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



