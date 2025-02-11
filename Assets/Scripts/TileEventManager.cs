using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TileEventManager : MonoBehaviour
{
    public GameManager gameManager;
    public PlayerManager playerManager;
    public GameObject purchasePanel;
    public TextMeshProUGUI purchasePanelText;
    public ResourceManager resourceManager;  // 引用 ResourceManager
    public Text eventInfoText;
    public Button buyButton;
    public Button skipButton;
    [Tooltip("TextMeshPro 字体文件")]
    public TMP_FontAsset customFont;

    public GameObject cityLevel1Prefab;
    public GameObject cityLevel2Prefab;
    public GameObject cityLevel3Prefab;

    private bool decisionMade;
    private bool buyDecision;

    void Start()
    {
        purchasePanel.SetActive(false);
        buyButton.onClick.AddListener(OnBuyButtonClicked);
        skipButton.onClick.AddListener(OnSkipButtonClicked);
        // 清空 eventInfoText
        eventInfoText.text = "";
    }

    // 自动购买并升级指定城市
    public IEnumerator AutoBuyAndUpgradeCity(int playerIndex, int tileIndex)
    {
        Player player = playerManager.players[playerIndex];
        int tileID = gameManager.mapManager.tileIds[tileIndex];
        TileController tile = gameManager.boardTiles.Find(t => t.tileIndex == tileID);

        if (tile == null)
            yield break;

        // 自动购买城市
        if (tile.owner < 0 && player.money >= tile.tileData.price)
        {
            tile.owner = playerIndex;
            player.money -= tile.tileData.price;
            tile.UpdateTileText();
            eventInfoText.text = $"{player.name} 自动购买了 {tile.tileData.name}";

            MeshRenderer cubeRenderer = tile.GetComponentInChildren<MeshRenderer>();
            if (cubeRenderer != null)
            {
                cubeRenderer.material.color = player.playerColor;
            }

            // 自动升级城市
            for (int i = 0; i < 3; i++)
            {
                if (player.money >= tile.tileData.upgradeCosts[tile.tileData.level])
                {
                    tile.UpgradeCity(player); // 升级城市
                    yield return new WaitForSeconds(1); // 延迟1秒后继续升级
                }
                else
                {
                    break; // 如果资金不足，停止升级
                }
            }
        }
        else
        {
            eventInfoText.text = $"{player.name} 无法购买 {tile.tileData.name}，资金不足或该城市已经被购买";
        }
    }

    // 测试自动添加资源卡
    public IEnumerator AutoAddResourceCard(int playerIndex, ResourceType resourceType, int quantity)
    {
        Player player = playerManager.players[playerIndex];
        resourceManager.AddResource(player, resourceType, quantity);
        eventInfoText.text = $"{player.name} 自动添加了 {quantity} 张 {resourceType} 资源卡";
        yield return null;
    }


    public IEnumerator ProcessTileEvent(Player player, int tileIndex)
    {
        int tileID = gameManager.mapManager.tileIds[tileIndex];
        TileController tile = gameManager.boardTiles.Find(t => t.tileIndex == tileID);
        if (tile == null)
            yield break;
        
        // 城市格子
        if (tile.tileData != null && tile.tileData.type == "city")
        {
            if (tile.owner < 0)
            {
                // 购买
                yield return StartCoroutine(ShowPurchasePanel(player, tile)); // 如果该城市无人拥有，提供购买选择
            }
            else if (tile.owner != playerManager.currentPlayerIndex)
            {
                // 租金
                int rent = tile.tileData.rent;
                player.money -= rent;
                playerManager.players[tile.owner].money += rent;
                eventInfoText.text = $"\n{player.name} 向 {playerManager.players[tile.owner].name} 支付了租金 ${rent}";
            }
            else
            {
                // 如果玩家已经拥有该城市，提供升级选项
                if (tile.tileData.level < 3)
                {
                    // 触发升级逻辑
                    yield return StartCoroutine(ShowUpgradePanel(player, tile));
                }
                else
                {
                    eventInfoText.text = $"{tile.tileData.name} 已经达到最大等级。";
                }
            }
        }
        // 事件格子
        else if (tile.tileData != null && tile.tileData.type == "event")
        {
            eventInfoText.text = $"\n{player.name} 落在了 {tile.tileData.name} 格子";
            yield return StartCoroutine(TriggerEventCard(player));
        }
        // 开始格子
        else if (tile.tileData != null && tile.tileData.type == "start")
        {   
            // 玩家+800
            player.money += 800;
            eventInfoText.text = $"\n{player.name} 落在了 {tile.tileData.name} 格子，获得 $800";
        }
        // 市场格子
        else if (tile.tileData != null && tile.tileData.type == "market")
        {
            eventInfoText.text = $"\n{player.name} 落在了 {tile.tileData.name} 格子，获得 $500";
        }
        // 海盗格子
        else if (tile.tileData != null && tile.tileData.type == "pirate")
        {
            eventInfoText.text = $"\n{player.name} 落在了 {tile.tileData.name} 格子，失去 $500";
        }
        // 村民格子
        else if (tile.tileData != null && tile.tileData.type == "villager")
        {
            player.money += 300;
            eventInfoText.text = $"\n{player.name} 落在了 {tile.tileData.name} 格子，获得 $300";
        }
        // 资源格子
        else if (tile.tileData != null && tile.tileData.type == "resource")
        {
            eventInfoText.text = $"\n{player.name} 落在了 {tile.tileData.name} 格子，获得 $200";
        }
        else 
        {
            eventInfoText.text = $"\n{player.name} 落在了 {tile.tileData?.name ?? "空白"} 格子";
        }
        yield return null;
    }

    public IEnumerator ShowUpgradePanel(Player player, TileController tile)
    {
        purchasePanelText.text = $"{player.name}，是否花费 ${tile.tileData.upgradeCosts[tile.tileData.level]} 升级 {tile.tileData.name}?";
        decisionMade = false;
        buyDecision = false;
        purchasePanel.SetActive(true);

        while (!decisionMade)
        {
            yield return null;
        }

        if (buyDecision)
        {
            tile.UpgradeCity(player); // 触发城市升级
        }
        else
        {
            eventInfoText.text = $"\n{player.name} 放弃了升级 {tile.tileData.name}。";
        }
        purchasePanel.SetActive(false);
    }


    public IEnumerator ShowPurchasePanel(Player player, TileController tile)
    {
        // 设置自定义字体（如果已分配）
        if (customFont != null)
        {
            purchasePanelText.font = customFont;
        }
        purchasePanelText.text = $"{player.name}, 是否购买 {tile.tileData.name}\n价格: ${tile.tileData.price}?";
        decisionMade = false;
        buyDecision = false;
        purchasePanel.SetActive(true);

        while (!decisionMade)
        {
            yield return null;
        }

        if (buyDecision)
        {
            if (player.money >= tile.tileData.price)
            {
                player.money -= tile.tileData.price;
                tile.owner = playerManager.currentPlayerIndex;
                tile.UpdateTileText();
                eventInfoText.text = $"\n{player.name} 购买了 {tile.tileData.name}";
                
                MeshRenderer cubeRenderer = tile.GetComponentInChildren<MeshRenderer>();
                if (cubeRenderer != null)
                {
                    cubeRenderer.material.color = player.playerColor;
                }
            }
            else
            {
                eventInfoText.text = $"\n{player.name} 资金不足，无法购买 {tile.tileData.name}";
            }
        }
        else
        {
            eventInfoText.text = $"\n{player.name} 放弃了购买 {tile.tileData.name}";
        }
        purchasePanel.SetActive(false);
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
                eventInfoText.text = $"\n{player.name} 抽取了 1 张 {resourceType} 资源卡";
                break;
            case 1: // 失去一张资源卡
                if (player.playerResources[ResourceType.Silk].Count > 0)  // 检查是否有某种资源
                {
                    resourceManager.RemoveResource(player, ResourceType.Silk, 1);
                    eventInfoText.text = $"\n{player.name} 失去了 1 张 丝绸 资源卡";
                }
                else
                {
                    eventInfoText.text = $"\n{player.name} 没有足够的资源来失去 丝绸 资源卡";
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
                eventInfoText.text = "\n发现新商路：恭喜你发现一条通往神秘地区的新商路，可抽取一张资源卡。";
                // 抽取资源卡逻辑...
                yield return StartCoroutine(HandleResourceEvent(player, 0)); // 抽取资源卡
                break;
            case 1:
                eventInfoText.text = "\n文化交流：你成功与当地商人进行了文化交流，他们对你的慷慨和见识表示赞赏，赠送你500金。";
                player.money += 500;
                break;
            case 2:
                eventInfoText.text = "\n获得资助：一位富有的贵族看中了你的商业潜力，决定资助你，获得 800 金币。";
                player.money += 800;
                break;
            case 3:
                eventInfoText.text = "\n发现遗迹：在探索途中，你发现了一处古老遗迹，从中找到珍贵文物，换回一个地皮（自选不大于500金的地皮）。";
                // 处理购买地皮逻辑...
                break;
            case 4:
                eventInfoText.text = "\n幸运之神眷顾：幸运之神降临，接下来三个回合，你掷骰子的点数 +1。";
                // 设置幸运加成的逻辑...
                break;
            case 5:
                eventInfoText.text = "\n丰收之年：途经的地区迎来丰收，你收购到大量低价优质资源，抽取两张资源卡。";
                // 抽取两张资源卡...
                break;
            case 6:
                eventInfoText.text = "\n学会新技术：在当地学习到一种独特的制作工艺，收益700金。";
                player.money += 700;
                break;
            case 7:
                eventInfoText.text = "\n获得推荐信：得到当地一位重要人物的推荐信，可以免费在自己的地皮上加盖商铺。";
                // 增加商铺逻辑...
                break;
        }
        yield return null;
    }

    // 处理负面事件
    public IEnumerator HandleNegativeEvent(Player player, int eventIndex)
    {
        switch (eventIndex)
        {
            case 0:
                eventInfoText.text = "\n遭遇风沙：不幸遭遇强烈风沙，暂停下一回合行动，且损失 200 金币用于清理货物和修复商队装备。";
                player.money -= 200;
                // 暂停回合的逻辑...
                break;
            case 1:
                eventInfoText.text = "\n强盗袭击：一群强盗抢走了你部分财物，损失 300 金币和一张资源卡。";
                player.money -= 300;
                // 扣除资源卡逻辑...
                break;
            case 2:
                eventInfoText.text = "\n迷路：在沙漠中迷失方向，后退三格，浪费一回合时间寻找方向。";
                // 退后三格的逻辑...
                break;
            case 3:
                eventInfoText.text = "\n疾病流行：所在地区疾病流行，你的商队人员受到影响，下一回合无法建造建筑，且需花费 200 金币治疗。";
                player.money -= 200;
                // 设置不能建造建筑的逻辑...
                break;
            case 4:
                eventInfoText.text = "\n税收增加：当地政府临时增加税收，缴纳当前所拥有金币的 5% 作为税款。";
                player.money -= Mathf.FloorToInt(player.money * 0.05f);
                break;
            case 5:
                eventInfoText.text = "\n竞争对手破坏：竞争对手暗中破坏你的生意，拆除你在当前城市一座等级最低的建筑。";
                // 拆除建筑逻辑...
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
