using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TileEventManager : MonoBehaviour
{
    public GameManager gameManager;
    public PlayerManager playerManager;
    public ResourceManager resourceManager;
    public Button buyButton;
    public Button skipButton;

    public GameObject cityLevel1Prefab;
    public GameObject cityLevel2Prefab;
    public GameObject cityLevel3Prefab;

    private bool decisionMade;
    private bool buyDecision;

    void Start()
    {
        buyButton.onClick.AddListener(OnBuyButtonClicked);
        skipButton.onClick.AddListener(OnSkipButtonClicked);
        // 清空 eventInfoText
        UIManager.Instance.ShowEventInfo("");
    }


    // 处理玩家的回合逻辑
    public IEnumerator ProcessTileEvent(Player player, int tileIndex)
    {
        // int tileID = gameManager.mapManager.tileIndex2Id[tileIndex];
        // TileController tile = gameManager.boardTiles.Find(t => t.tileIndex == tileID);
        TileData tileData = TileManager.Instance.GetTileDataByIndex(tileIndex);
        Tile tile = tileData.tile;

        // 如果是城市格子
        if (tile != null && tile.type == "city")
        {
            // 如果该城市没有被购买
            if (tile.owner < 0)
            {
                yield return StartCoroutine(ShowPurchasePanel(player, tileData));
            }
            // 如果该城市已经被其他玩家购买
            else if (tile.owner != playerManager.currentPlayerIndex)
            {
                // 当前玩家向城市所有者支付租金
                PlayerManager.Instance.PayRent(player.playerIndex, tile.owner, tile.rent);
                PlayerManager.Instance.UpdatePlayerInfoText();
            }
            // 如果该城市已经被当前玩家购买
            else
            {
                if (tile.level < 3)
                {
                    yield return StartCoroutine(ShowUpgradePanel(player, tile));
                }
                else
                {
                    UIManager.Instance.ShowEventInfo($"{tile.name} 已经达到最大等级。");
                }
            }
        }
        else if (tile != null && tile.type == "event")
        {
            UIManager.Instance.ShowEventInfo($"\n{player.name} 落在了 {tile.name} 格子");
            yield return StartCoroutine(TriggerEventCard(player));
        }
        else
        {
            // 对于其他类型的格子，处理资源、市场等
            HandleOtherTileTypes(player, tile);
        }
        yield return null;
    }

    private void HandleOtherTileTypes(Player player, Tile tile)
    {
        if (tile != null)
        {
            switch (tile.type)
            {
                case "start":
                    player.money += 800;
                    UIManager.Instance.ShowEventInfo($"\n{player.name} 落在了 {tile.name} 格子，获得 $800");
                    break;
                case "market":
                    UIManager.Instance.ShowEventInfo($"\n{player.name} 落在了 {tile.name} 格子，获得 $500");
                    break;
                case "pirate":
                    UIManager.Instance.ShowEventInfo($"\n{player.name} 落在了 {tile.name} 格子，失去 $500");
                    break;
                case "villager":
                    player.money += 300;
                    UIManager.Instance.ShowEventInfo($"\n{player.name} 落在了 {tile.name} 格子，获得 $300");
                    break;
                case "resource":
                    UIManager.Instance.ShowEventInfo($"\n{player.name} 落在了 {tile.name} 格子，获得 $200");
                    break;
                default:
                    UIManager.Instance.ShowEventInfo($"\n{player.name} 落在了 {tile?.name ?? "空白"} 格子");
                    break;
            }
        }
    }

    public IEnumerator ShowUpgradePanel(Player player, Tile tile)
    {
        UIManager.Instance.ShowPurchasePanelText($"{player.name}，是否花费 ${tile.upgradeCosts[tile.level]} 升级 {tile.name}?");
        decisionMade = false;
        buyDecision = false;
        UIManager.Instance.SetPurchasePanelActive(true);

        while (!decisionMade)
        {
            yield return null;
        }

        if (buyDecision)
        {
            // 玩家钱够
            if (PlayerManager.Instance.CanPlayerAfford(player, tile.upgradeCosts[tile.level]))
            {
                PlayerManager.Instance.SpendMoney(player, tile.upgradeCosts[tile.level]);
                PlayerManager.Instance.UpdatePlayerInfoText();
                TileManager.Instance.UpgradeTile(tile);

                UIManager.Instance.ShowEventInfo($"\n{player.name} 升级了 {tile.name}。");
            }
            else
            {
                UIManager.Instance.ShowEventInfo($"\n{player.name} 资金不足，无法升级 {tile.name}。");
            }
        }
        else
        {
            UIManager.Instance.ShowEventInfo($"\n{player.name} 放弃了升级 {tile.name}。");
        }
        UIManager.Instance.SetPurchasePanelActive(false);
    }

    public IEnumerator ShowPurchasePanel(Player player, TileData tileData)
    {
        Tile tile = tileData.tile;
        UIManager.Instance.ShowPurchasePanelText($"{player.name}, 是否购买 {tile.name}\n价格: ${tile.price}?");
        decisionMade = false;
        buyDecision = false;
        UIManager.Instance.SetPurchasePanelActive(true);

        while (!decisionMade)
        {
            yield return null;
        }

        if (buyDecision)
        {
            if (PlayerManager.Instance.CanPlayerAfford(player, tile.price))
            {
                PlayerManager.Instance.SpendMoney(player, tile.price);
                TileManager.Instance.ownerTile(tileData, player.playerIndex, player.playerColor);
                
                UIManager.Instance.ShowEventInfo($"\n{player.name} 购买了 {tile.name}");

                // MeshRenderer cubeRenderer = tile.GetComponentInChildren<MeshRenderer>();
                // if (cubeRenderer != null)
                // {
                //     cubeRenderer.material.color = player.playerColor;
                // }
            }
            else
            {
                UIManager.Instance.ShowEventInfo($"\n{player.name} 资金不足，无法购买 {tile.name}");
            }
        }
        else
        {
            UIManager.Instance.ShowEventInfo($"\n{player.name} 放弃了购买 {tile.name}");
        }
        UIManager.Instance.SetPurchasePanelActive(false);
    }

    // 触发事件卡（正面或负面）
    private IEnumerator TriggerEventCard(Player player)
    {
        // 随机选择一个事件卡
        bool isPositiveEvent = Random.value > 0.5f;  // 随机选择正面或负面事件
        if (isPositiveEvent)
        {
            int eventIndex = Random.Range(0, 8);  // 正面事件 0-7
            yield return StartCoroutine(HandlePositiveEvent(player, eventIndex));
        }
        else
        {
            int eventIndex = Random.Range(0, 6);  // 负面事件 0-5
            yield return StartCoroutine(HandleNegativeEvent(player, eventIndex));
        }
    }

    // 触发资源相关事件（例如抽取资源卡）
    public IEnumerator HandleResourceEvent(Player player, int eventIndex)
    {
        switch (eventIndex)
        {
            case 0: // 抽取一张资源卡
                ResourceType resourceType = (ResourceType)Random.Range(0, 7);  // 随机选择资源
                resourceManager.AddResource(player, resourceType, 1);
                UIManager.Instance.ShowEventInfo($"\n{player.name} 抽取了 1 张 {resourceType} 资源卡");

                break;
            case 1: // 失去一张资源卡
                if (player.resources[ResourceType.Silk].Count > 0)  // 检查是否有某种资源
                {
                    resourceManager.RemoveResource(player, ResourceType.Silk, 1);
                    UIManager.Instance.ShowEventInfo($"\n{player.name} 失去了 1 张 丝绸 资源卡");
                }
                else
                {
                    UIManager.Instance.ShowEventInfo($"\n{player.name} 没有足够的资源来失去 丝绸 资源卡");
                }
                break;
            // 更多的资源相关事件逻辑可以继续添加
        }
        yield return null;
    }

    // 处理正面事件
    public IEnumerator HandlePositiveEvent(Player player, int eventIndex)
    {
        switch (eventIndex)
        {
            case 0:
                UIManager.Instance.ShowEventInfo("\n发现新商路：恭喜你发现一条通往神秘地区的新商路，可抽取一张资源卡。");
                yield return StartCoroutine(HandleResourceEvent(player, 0));
                break;
            case 1:
                UIManager.Instance.ShowEventInfo("\n文化交流：你成功与当地商人进行了文化交流，他们对你的慷慨和见识表示赞赏，赠送你500金。");
                player.money += 500;
                break;
            case 2:
                UIManager.Instance.ShowEventInfo("\n获得资助：一位富有的贵族看中了你的商业潜力，决定资助你，获得 800 金币。");
                player.money += 800;
                break;
            case 3:
                UIManager.Instance.ShowEventInfo("\n发现遗迹：在探索途中，你发现了一处古老遗迹，从中找到珍贵文物，换回一个地皮（自选不大于500金的地皮）。");
                break;
            case 4:
                UIManager.Instance.ShowEventInfo("\n幸运之神眷顾：幸运之神降临，接下来三个回合，你掷骰子的点数 +1。");
                break;
            case 5:
                UIManager.Instance.ShowEventInfo("\n丰收之年：途经的地区迎来丰收，你收购到大量低价优质资源，抽取两张资源卡。");
                break;
            case 6:
                UIManager.Instance.ShowEventInfo("\n学会新技术：在当地学习到一种独特的制作工艺，收益700金。");
                player.money += 700;
                break;
            case 7:
                UIManager.Instance.ShowEventInfo("\n获得推荐信：得到当地一位重要人物的推荐信，可以免费在自己的地皮上加盖商铺。");
                break;
        }
        yield return null;
    }

    public IEnumerator HandleNegativeEvent(Player player, int eventIndex)
    {
        switch (eventIndex)
        {
            case 0:
                UIManager.Instance.ShowEventInfo("\n遭遇风沙：不幸遭遇强烈风沙，暂停下一回合行动，且损失 200 金币用于清理货物和修复商队装备。");
                player.money -= 200;
                break;
            case 1:
                UIManager.Instance.ShowEventInfo("\n强盗袭击：一群强盗抢走了你部分财物，损失 300 金币和一张资源卡。");
                player.money -= 300;
                break;
            case 2:
                UIManager.Instance.ShowEventInfo("\n迷路：在沙漠中迷失方向，后退三格，浪费一回合时间寻找方向。");
                break;
            case 3:
                UIManager.Instance.ShowEventInfo("\n疾病流行：所在地区疾病流行，你的商队人员受到影响，下一回合无法建造建筑，且需花费 200 金币治疗。");
                player.money -= 200;
                break;
            case 4:
                UIManager.Instance.ShowEventInfo("\n税收增加：当地政府临时增加税收，缴纳当前所拥有金币的 5% 作为税款。");
                player.money -= Mathf.FloorToInt(player.money * 0.05f);
                break;
            case 5:
                UIManager.Instance.ShowEventInfo("\n竞争对手破坏：竞争对手暗中破坏你的生意，拆除你在当前城市一座等级最低的建筑。");
                break;
        }
        yield return null;
    }

    void OnBuyButtonClicked()
    {
        buyDecision = true;
        decisionMade = true;
    }

    void OnSkipButtonClicked()
    {
        buyDecision = false;
        decisionMade = true;
    }
}
