using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockGenerator : MonoBehaviour
{
    public BlockPoolController pool;

    private void Start()
    {
       BlockGridPremade.StartBlockGridPremade();
        GenerateLevel_0();
    }

    public void GenerateLevel_0()
    {
        var blocks_0 = BlockGridPremade.GetRows_0();
        for (int z = 0; z < blocks_0.Count; z++)
        {
            var row = blocks_0[z];

            for (int idx = 0; idx < row.rowData.Count; idx++)
            {
                if (row.rowData[idx] == EnumBlockType.None) continue;

                var type = row.rowData[idx];
                var block = pool.Get(type);

                var xPositionGrid = idx - 3;
                var zPositionGrid = z -30;

                block.gameObject.transform.position = new Vector3(xPositionGrid, 0, zPositionGrid);
                block.gameObject.SetActive(true);
            }
        }
    }

    public void Generate(int floorZPosition)
    {
        var BlockGrid = BlockGridPremade.GetRows();
        for (int z = 0; z < BlockGrid.Count; z++)
        {
            var row = BlockGrid[z];

            for (int idx = 0; idx < row.rowData.Count; idx++)
            {
                if (row.rowData[idx] == EnumBlockType.None) continue;

                var type = row.rowData[idx];
                var block = pool.Get(type);

                var xPositionGrid = idx - 3;
                var zPositionGrid = z + floorZPosition;

                block.gameObject.transform.position = new Vector3(xPositionGrid, 0, zPositionGrid);
                block.gameObject.SetActive(true);
            }
        }
    }
}
