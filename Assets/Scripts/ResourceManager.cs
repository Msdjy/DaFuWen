using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    // private Dictionary<ResourceType, List<ResourceCard>> playerResources;

    void Start()
    {

    }

    // 资源添加
    public void AddResource(Player player, ResourceType type, int quantity)
    {
        for (int i = 0; i < quantity; i++)
        {
            player.AddResource(type, 1);
        }
        Debug.Log($"{player.name} gained {quantity} {type} resource.");
    }

    // 资源移除
    public bool RemoveResource(Player player, ResourceType type, int quantity)
    {
        return player.RemoveResource(type, quantity);
    }

    // 获取某资源的数量
    public int GetResourceCount(Player player, ResourceType type)
    {
        return player.resources[type].Count;
    }


 // 显示玩家所有资源
    public void DisplayResources(Player player)
    {
        string resourceInfo = $"{player.name} Resources:\n";
        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
        {
            int count = GetResourceCount(player, type);
            if (count > 0)
            {
                resourceInfo += $"{type}: {count} cards\n";
            }
        }
        Debug.Log(resourceInfo);
    }
}
