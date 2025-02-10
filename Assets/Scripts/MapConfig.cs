using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapConfig
{
    /// <summary>
    /// 二维地图数据被合并成一维数组（按行排列）
    /// </summary>
    public List<int> map;
    public int rows;
    public int columns;
    public List<Tile> tiles;
}
