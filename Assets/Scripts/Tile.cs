using UnityEngine;

[System.Serializable]
public class Tile
{
    public int id;
    public string name;
    public int price;
    public int rent;
    public int owner = -1;
    public int level;
    public string desc;
    public string type;
    public int[] upgradeCosts;
    public int[] upgradeRents;

    // 提供一些对外可访问的方法
    public bool CanUpgrade() => level < upgradeCosts.Length;
    public bool CanPurchase() => owner == -1;

    // 城市升级
    public void Upgrade()
    {
        level++;
    }
    // 城市被购买
    public void updateOwner(int playerIndex)
    {   
        owner = playerIndex;
    }
}
