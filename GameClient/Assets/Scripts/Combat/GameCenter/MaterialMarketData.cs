using System;
using System.Collections;
using System.Collections.Generic;

// 原料商店数据
public class MaterialMarketData
{ 
    public const Int32 maxCountMarket = GameConst.COUNT_MARKET;
    private List<MarketBlockData> m_Blocks = new List<MarketBlockData>();

    /// <summary>
    /// 刷新商店可售材料。
    /// </summary>
    public void Refresh(Int32 seed)
    {
        Random random = new Random(seed);
        m_Blocks.Clear();
        for (int i = 0; i < maxCountMarket; i++)
        {
            Int32 matTypeCount = (Int32)CombatGameCenter.Material.Type.LastMaterial;
            var matType = (CombatGameCenter.Material.Type)(random.Next() % matTypeCount);
            var matCount = random.Next() % 6 + 5;
            var block = new MarketBlockData
            {
                material = new CombatGameCenter.Material(matType, matCount),
                isSaleout = false,
            };
            m_Blocks.Add(block);
        }
    }
    /// <summary>
    /// 取走可售材料
    /// </summary>
    public CombatGameCenter.Material TakeAway(Int32 idx)
    {
        var block = GetBlock(idx);
        if(block != null)
        {
            block.isSaleout = true;
            return block.material;
        }
        return CombatGameCenter.Material.empty;
    }
    public MarketBlockData GetBlock(Int32 idx)
    {
        if (Utility.CheckIndex(idx, m_Blocks))
        {
            return m_Blocks[idx];
        }
        return null;
    }
    public Int32 GetBlockPrice(Int32 idx)
    {
        var block = GetBlock(idx);
        if(block != null)
        {
            return GetPrice(block.material);
        }
        return 0;
    }

    private Int32 GetPrice(CombatGameCenter.Material mat)
    {
        // 所有材料统一单价 1 金币
        return mat.count * 1;
    }
}

public class MarketBlockData
{
    public CombatGameCenter.Material material;
    public bool isSaleout;
}
