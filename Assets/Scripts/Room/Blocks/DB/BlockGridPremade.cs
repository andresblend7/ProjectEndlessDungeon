using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BlockGridPremade 
{
    static List<RowBlockPremade> rows;
    static List<RowBlockPremade> rows0;
    public static void StartBlockGridPremade()
    {
        rows = new List<RowBlockPremade>();
        rows.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.Iron, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.Gold, EnumBlockType.None, } });
        rows.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, } });
        rows.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, } });
        rows.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, } });
        rows.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, } });
        rows.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.None, EnumBlockType.None, EnumBlockType.Stone, EnumBlockType.Stone, } });
        rows.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.None, EnumBlockType.None, EnumBlockType.Stone, EnumBlockType.Stone, } });
        rows.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.Iron, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.None, EnumBlockType.None, EnumBlockType.Stone, EnumBlockType.Stone, } });
        rows.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, } });
        rows.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.Stone, } });
        rows.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.Stone, EnumBlockType.Iron, EnumBlockType.Stone, EnumBlockType.Stone, } });
        rows.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.Gold, EnumBlockType.None, EnumBlockType.None, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.Stone, } });
        rows.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, } });
        rows.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.Gold, EnumBlockType.None, EnumBlockType.None, } });
        rows.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, } });
        rows.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, } });
        rows.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.Stone, } });
        rows.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.Stone, } });
        rows.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.Stone, } });
        rows.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.Iron, EnumBlockType.Iron, } });
        rows.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.Stone, } });
        rows.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.None, EnumBlockType.None, } });
        rows.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, } });
        rows.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, } });
        rows.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.None, EnumBlockType.None, } });
        rows.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.None, EnumBlockType.None, } });
        rows.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.Stone, EnumBlockType.Iron, EnumBlockType.Stone, EnumBlockType.None, EnumBlockType.None, } });
        rows.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.Stone, EnumBlockType.Iron, EnumBlockType.Stone, EnumBlockType.None, EnumBlockType.None, } });
        rows.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.None, EnumBlockType.None, } });
        rows.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.Gold, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, } });
        rows.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, } });

    }

    public static void StartBlockGridPremade_0()
    {
        rows0 = new List<RowBlockPremade>();
        rows0.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, } });
        rows0.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, } });
        rows0.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, } });
        rows0.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, } });
        rows0.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, } });
        rows0.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, } });
        rows0.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, } });
        rows0.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, } });
        rows0.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, } });
        rows0.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, } });
        rows0.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, } });
        rows0.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, } });
        rows0.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, } });
        rows0.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, } });
        rows0.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, } });
        rows0.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, } });
        rows0.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, } });
        rows0.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.Iron, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, } });
        rows0.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, } });
        rows0.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.None, } });
        rows0.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.Stone, EnumBlockType.Iron, EnumBlockType.None, } });
        rows0.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, } });
        rows0.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, } });
        rows0.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, } });
        rows0.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.None, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.None, EnumBlockType.None, } });
        rows0.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.Stone, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, } });
        rows0.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.Gold, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, } });
        rows0.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.Iron, EnumBlockType.None, } });
        rows0.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.Stone, EnumBlockType.None, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.None, EnumBlockType.None, } });
        rows0.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.None, EnumBlockType.Stone, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.Stone, EnumBlockType.None, } });
        rows0.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.Gold, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, EnumBlockType.None, } });
        rows0.Add(new RowBlockPremade() { rowData = new List<EnumBlockType> { EnumBlockType.Iron, EnumBlockType.Iron, EnumBlockType.Iron, EnumBlockType.Stone, EnumBlockType.Stone, EnumBlockType.None, EnumBlockType.None, } });

    }



    public static List<RowBlockPremade> GetRows()
    {
        return rows;
    }
    public static List<RowBlockPremade> GetRows_0()
    {
        StartBlockGridPremade_0();
        return rows0;
    }
}

public class RowBlockPremade
{
    public List<EnumBlockType> rowData;

    public RowBlockPremade()
    {
        rowData = new List<EnumBlockType>();
    }
}

