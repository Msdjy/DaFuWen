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
    public int quantity;

    public ResourceCard(ResourceType type, int quantity = 1)
    {
        resourceType = type;
        this.quantity = quantity;
    }
}
