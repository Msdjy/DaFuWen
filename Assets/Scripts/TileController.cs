using UnityEngine;
using TMPro;

public class TileController : MonoBehaviour
{
    public Tile tileData;
    public int tileIndex;
    // owner：-1 表示无人拥有，否则记录玩家的索引（0、1、…）
    public int owner = -1;

    // 存储显示该格子信息（名称、拥有者）的 TextMeshPro 组件
    public TextMeshPro tileText;

    /// <summary>
    /// 更新格子上的显示文字。显示房产名称以及拥有者信息。
    /// </summary>
    public void UpdateTileText()
    {
        string ownerStr = (owner < 0) ? "Unowned" : $"Player {owner + 1}";
        if (tileData != null)
            tileText.text = $"{tileData.name}\n{ownerStr}";
        else
            tileText.text = $"Index: {tileIndex}\n{ownerStr}";
    }
}
