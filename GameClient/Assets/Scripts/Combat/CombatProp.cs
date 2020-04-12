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
        new PropInfo
        {
            id = 3,
            name = "批发商·木头",
            upsetPrice = 10,
            script = typeof(PropWoodShop),
        },
        new PropInfo
        {
            id = 3,
            name = "批发商·石头",
            upsetPrice = 10,
            script = typeof(PropStoneShop),
        },
        new PropInfo
        {
            id = 3,
            name = "批发商·钢铁",
            upsetPrice = 10,
            script = typeof(PropIronShop),
        },
        new PropInfo
        {
            id = 3,
            name = "批发商·燃料",
            upsetPrice = 10,
            script = typeof(PropFuelShop),
        },
    };
}

public struct PropEventResult
{
    public bool isRemove;
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
        owner.AddProp(this);
    }
    public virtual void OnDiscard()
    {
        owner = null;
    }
    public virtual void OnBeginOperation(Int32 seed) { }
    public virtual PropEventResult OnEndOperation(Int32 seed)
    {
        return new PropEventResult { isRemove = false };
    }
}

public class PropWin : CombatProp
{
    public PropWin(PropInfo info) : base(info) { }

    public override void OnCollect(CombatGameCenter.Player player)
    {
        base.OnCollect(player);
        CombatManager.instance.TryWin();
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

public abstract class PropMaterialShop : CombatProp
{
    private Int32 m_ShopIndex = -1;
    public PropMaterialShop(PropInfo info) : base(info) { }

    protected abstract CombatGameCenter.Material.Type GetMaterialType();

    public override void OnBeginOperation(int seed)
    {
        base.OnBeginOperation(seed);
        if (owner.IsFullWholesale())
        {
            m_ShopIndex = -1;
            return;
        }
        Random random = new Random(seed);
        var matType = GetMaterialType();
        var mat = new CombatGameCenter.Material(matType, 10);
        owner.AddWholesale(mat, 7);
        m_ShopIndex = owner.WholesaleCount() - 1;
    }
    public override PropEventResult OnEndOperation(int seed)
    {
        base.OnEndOperation(seed);
        bool isInvalid = false;
        if (m_ShopIndex == -1)
        {
            isInvalid = true;
        }
        else
        {
            var shop = owner.GetWholesale(m_ShopIndex);
            if (!shop.isSellout)
            {
                // 本回合没有交易则道具变成失效
                isInvalid = true;
            }
        }
        return new PropEventResult
        {
            isRemove = isInvalid,
        };
    }
}

//public class PropWoodShop : CombatProp
//{
//    private Int32 m_ShopIndex = -1;

//    public PropWoodShop(PropInfo info) : base(info) { }

//    public override void OnBeginOperation(Int32 seed)
//    {
//        base.OnBeginOperation(seed);
//        if (owner.IsFullWholesale())
//        {
//            m_ShopIndex = -1;
//            return;
//        }
//        Random random = new Random(seed);
//        var mat = new CombatGameCenter.Material(CombatGameCenter.Material.Type.Wood, 10);
//        owner.AddWholesale(mat, 7);
//        m_ShopIndex = owner.WholesaleCount() - 1;
//    }
//    public override PropEventResult OnEndOperation(int seed)
//    {
//        base.OnEndOperation(seed);
//        bool isInvalid = false;
//        if (m_ShopIndex == -1)
//        {
//            isInvalid = true;
//        }
//        else
//        {
//            var shop = owner.GetWholesale(m_ShopIndex);
//            if (!shop.isSellout)
//            {
//                // 本回合没有交易则道具变成失效
//                isInvalid = true;
//            }
//        }
//        return new PropEventResult
//        {
//            isRemove = isInvalid,
//        };
//    }
//}

public class PropWoodShop : PropMaterialShop
{
    public PropWoodShop(PropInfo info) : base(info) { }
    protected override CombatGameCenter.Material.Type GetMaterialType() => CombatGameCenter.Material.Type.Wood;
}
public class PropIronShop : PropMaterialShop
{
    public PropIronShop(PropInfo info) : base(info) { }
    protected override CombatGameCenter.Material.Type GetMaterialType() => CombatGameCenter.Material.Type.Iron;
}
public class PropStoneShop : PropMaterialShop
{
    public PropStoneShop(PropInfo info) : base(info) { }
    protected override CombatGameCenter.Material.Type GetMaterialType() => CombatGameCenter.Material.Type.Stone;
}
public class PropFuelShop : PropMaterialShop
{
    public PropFuelShop(PropInfo info) : base(info) { }
    protected override CombatGameCenter.Material.Type GetMaterialType() => CombatGameCenter.Material.Type.Fuel;
}


public enum CombatPropType
{
    Win,
    Storehouse,
    WoodShop,
}



