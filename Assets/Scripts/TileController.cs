using UnityEngine;

public class TileController : MonoBehaviour
{
    // 保存从 JSON 解析的格子数据
    public Tile tileData;
    // 此格子在棋盘中的顺序索引（决定玩家行走的路径）
    public int tileIndex;
    // 当前格子的所有者编号（-1 表示无人拥有）
    public int owner = -1;
}
