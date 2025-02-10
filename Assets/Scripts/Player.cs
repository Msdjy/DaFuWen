using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Player
{
    public string name;
    public int money;
    /// <summary>
    /// 当前玩家所在的 Tile 索引（对应 MapManager.tilePositions 中的下标）
    /// </summary>
    public int currentTileIndex;
    public GameObject avatar;
    /// <summary>
    /// 用于显示玩家名称和资金信息的 TextMeshPro 组件
    /// </summary>
    public TextMeshProUGUI playerText;

    // 新增的字段：玩家的颜色
    public Color playerColor;
}