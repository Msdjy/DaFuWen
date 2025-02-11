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
        string tileInfo = tileData != null ? $"{tileData.name}\n{ownerStr}\n{tileData.level}\n{tileIndex}" : $"Index: {tileIndex}\n{ownerStr}";
        tileText.text = tileInfo;
    }

    /// <summary>
    /// 升级城市
    /// </summary>
    public void UpgradeCity(Player player)
    {
        int upgradeCost = tileData.upgradeCosts[tileData.level];
        if (player.money >= upgradeCost)
        {
            player.money -= upgradeCost;
            tileData.level++;
            tileData.rent += tileData.upgradeRents[tileData.level - 1];
            UpdateTileText();
            Debug.Log($"{player.name} 升级了 {tileData.name} 到 {tileData.level} 级，当前租金：${tileData.rent}");
        }
    }
}
