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
        tileText.text = tileData != null ? $"{tileData.name}\n{ownerStr}\n{tileIndex}" : $"Index: {tileIndex}\n{ownerStr}";
    }
}
