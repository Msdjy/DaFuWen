using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

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

    // 每个玩家的资源
    public Dictionary<ResourceType, List<ResourceCard>> playerResources;

    public void initializeResources()
    {
        playerResources = new Dictionary<ResourceType, List<ResourceCard>>();
        // 初始化每种资源
        // 初始化每种资源类型
        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
        {
            playerResources[type] = new List<ResourceCard>();

            // 例如，给玩家添加一些资源
            if (type == ResourceType.Silk)
                playerResources[type].Add(new ResourceCard(type, 5 ));
            else if (type == ResourceType.Gems)
                playerResources[type].Add(new ResourceCard(type, 5 ));
            else if (type == ResourceType.Tea)
                playerResources[type].Add(new ResourceCard(type, 5 ));
        }
    }
}