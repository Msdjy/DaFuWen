using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

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

    public int playerIndex;
    public Dictionary<ResourceType, List<ResourceCard>> resources;

    // 新增的字段：玩家的颜色
    public Color playerColor;

    // 每个玩家的资源
    // public Dictionary<ResourceType, List<ResourceCard>> playerResources;


    // 初始化资源
    public void InitializeResources()
    {
        resources = new Dictionary<ResourceType, List<ResourceCard>>();
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            resources[type] = new List<ResourceCard>();
        }
    }

    // public void initializeResources()
    // {
    //     playerResources = new Dictionary<ResourceType, List<ResourceCard>>();
    //     // 初始化每种资源
    //     // 初始化每种资源类型
    //     foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
    //     {
    //         playerResources[type] = new List<ResourceCard>();

    //         // 例如，给玩家添加一些资源
    //         if (type == ResourceType.Silk)
    //             playerResources[type].Add(new ResourceCard(type, 5 ));
    //         else if (type == ResourceType.Gems)
    //             playerResources[type].Add(new ResourceCard(type, 5 ));
    //         else if (type == ResourceType.Tea)
    //             playerResources[type].Add(new ResourceCard(type, 5 ));
    //     }
    // }

    // 添加资源
    public void AddResource(ResourceType type, int quantity)
    {
        for (int i = 0; i < quantity; i++)
        {
            resources[type].Add(new ResourceCard(type));
        }
    }

    // 移除资源
    public bool RemoveResource(ResourceType type, int quantity)
    {
        if (resources[type].Count >= quantity)
        {
            resources[type].RemoveRange(resources[type].Count - quantity, quantity);
            return true;
        }
        return false;
    }
}