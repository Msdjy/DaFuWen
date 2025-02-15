using UnityEngine;

public enum ResourceType
{
    Silk,
    Porcelain,
    Tea,
    Spices,
    Gems,
    Fur,
    Herbs
}

public class ResourceCard
{
    public ResourceType resourceType;
    
    // name 细分资源子种类
    public string name;

    public ResourceCard(ResourceType type)
    {
        resourceType = type;
    }
}


