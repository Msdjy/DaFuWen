using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using System;
using System.Linq.Expressions;
using UnityEngine.Animations;

public class Player
{
    public string name;
    public int money;
    /// <summary>
    /// 当前玩家所在的 Tile 索引（）
    /// </summary>
    public int currentTileIndex;
    public GameObject avatar;
    public Animator animator;
    /// <summary>
    /// 用于显示玩家名称和资金信息的 TextMeshPro 组件
    // /// </summary>
    // public TextMeshProUGUI playerText;

    public int playerIndex;
    public Dictionary<ResourceType, List<ResourceCard>> resources;
    public float housePrice;

    // 新增的字段：玩家的颜色
    public Color playerColor;

    #region Create
    // 默认构造
    public Player(string name, Color playColor, GameObject avatar, int money = 5000, int currentTileIndex = 0)
    {
        this.name = name;
        this.money = money;
        this.currentTileIndex = currentTileIndex;
        this.playerColor = playColor;

        InitializeResources();
    }
    #endregion

    public void SetAvatarColor(GameObject avatar, Player player)
    {
        MeshRenderer avatarRenderer = avatar.GetComponentInChildren<MeshRenderer>();
        if (avatarRenderer != null)
        {
            avatarRenderer.material.color = player.playerColor;
        }
    }

    #region resource

    // 初始化资源
    public void InitializeResources()
    {
        resources = new Dictionary<ResourceType, List<ResourceCard>>();
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            resources[type] = new List<ResourceCard>();
        }
    }

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

    #endregion

    #region move
    // 移动玩家到目标位置
    public IEnumerator MoveTo(int steps)
    {
        for (int i = 0; i < steps; i++)
        {
            
            currentTileIndex = (currentTileIndex + 1) % TileManager.Instance.GetTilesCount();
            // Vector3 targetPos = mapManager.tileInnerPosition[currentTileIndex];
            // 通过index查询tile pos
            Vector3 targetPos = TileManager.Instance.GetTileDataPositionByIndex(currentTileIndex);

            while (Vector3.Distance(avatar.transform.position, targetPos) > 0.1f)
            {
                avatar.transform.position = Vector3.MoveTowards(avatar.transform.position, targetPos, Time.deltaTime * 5f);
                yield return null;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
    #endregion


    #region money
    // 玩家花费了钱
    public void SpendMoney(int amount)
    {
        money -= amount;
    }
    // 玩家获得了钱
    public void EarnMoney(int amount)
    {
        money += amount;
    }
    #endregion

    #region house
    // 购买房屋
    #endregion

}
