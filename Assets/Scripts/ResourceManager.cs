using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    private Dictionary<ResourceType, List<ResourceCard>> playerResources;

    void Start()
    {
        playerResources = new Dictionary<ResourceType, List<ResourceCard>>();

        // 初始化每种资源的列表
        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
        {
            playerResources[type] = new List<ResourceCard>();
        }
    }

    // 添加资源卡
    public void AddResource(Player player, ResourceType type, int quantity)
    {
        for (int i = 0; i < quantity; i++)
        {
            player.playerResources[type].Add(new ResourceCard(type));
        }
        Debug.Log($"{player.name} 获得了 {quantity} 张 {type} 资源卡");
    }

    // 删除资源卡
    public bool RemoveResource(Player player, ResourceType type, int quantity)
    {
        var resources = player.playerResources[type];
        if (resources.Count >= quantity)
        {
            for (int i = 0; i < quantity; i++)
            {
                resources.RemoveAt(resources.Count - 1);
            }
            Debug.Log($"{player.name} 失去了 {quantity} 张 {type} 资源卡");
            return true;
        }
        return false;
    }

    // 获取资源卡数量
    public int GetResourceCount(Player player, ResourceType type)
    {
        return player.playerResources[type].Count;
    }

    // 显示玩家拥有的所有资源卡
    public void DisplayResources(Player player)
    {
        string resourceInfo = $"{player.name} 的资源卡：\n";
        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
        {
            int count = GetResourceCount(player, type);
            if (count > 0)
            {
                resourceInfo += $"{type}: {count} 张\n";
            }
        }
        Debug.Log(resourceInfo);
    }
}
