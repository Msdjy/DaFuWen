using UnityEngine;
using TMPro;

public class TileController : MonoBehaviour
{
    public Tile tileData;
    public int tileIndex;
    /// <summary>
    /// owner 为 -1 表示无人拥有，否则存储拥有该 Tile 的玩家索引（0、1、…）
    /// </summary>
    public int owner = -1;

    public TextMeshPro tileText;

    /// <summary>
    /// 更新 Tile 显示的文字信息（例如名称及拥有者）
    /// </summary>
    public void UpdateTileText()
    {
        string ownerStr = owner < 0 ? "Unowned" : $"Player {owner + 1}";
        tileText.text = tileData != null ? $"{tileData.name}\n{ownerStr}\n{tileData.level}\n{tileIndex}" : $"Index: {tileIndex}\n{ownerStr}";
    }

    // 升级城市的方法
    public void UpgradeCity(Player player)
    {
        if (tileData.level < 3 && player.money >= tileData.upgradeCosts[tileData.level])
        {
            player.money -= tileData.upgradeCosts[tileData.level]; // 扣除当前等级的升级费用
            tileData.level++;  // 提升城市的等级
            tileData.rent += tileData.upgradeRents[tileData.level - 1]; // 增加租金
            UpdateTileText(); // 更新城市显示（显示等级和租金）
            Debug.Log($"{player.name} 升级了 {tileData.name} 到等级 {tileData.level}，当前租金：${tileData.rent}");
        }
        else
        {
            Debug.Log($"{player.name} 无法升级 {tileData.name}。资金不足或已达到最大等级");
        }
    }

}
