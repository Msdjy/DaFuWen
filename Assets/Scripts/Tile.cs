using UnityEngine;

[System.Serializable]
public class Tile
{
    public int id;
    public string name;
    public int price;
    public int rent;
    public int owner;
    public int level;
    public string desc;
    /// <summary>
    /// 可取值："city"、"event" 或 "start"
    /// </summary>
    public string type;

    public int[] upgradeCosts;  // 记录三次升级的费用
    public int[] upgradeRents;  // 记录三次升级后的租金收益
}
