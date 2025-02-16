using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TileEventManager : MonoBehaviour
{
    // 单例
    public static TileEventManager Instance;

    public GameObject purchasePanel;
    public TextMeshProUGUI purchasePanelText;
    public TextMeshProUGUI eventInfoText;
    public Button buyButton;
    public Button skipButton;

    public GameObject cityLevel1Prefab;
    public GameObject cityLevel2Prefab;
    public GameObject cityLevel3Prefab;

    private bool decisionMade;
    private bool buyDecision;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        buyButton.onClick.AddListener(OnBuyButtonClicked);
        skipButton.onClick.AddListener(OnSkipButtonClicked);
        // 清空 eventInfoText
        ShowEventInfo("");
        SetPurchasePanelActive(false);
    }

    #region Process Tile Event
    // 处理玩家的回合逻辑
    public IEnumerator ProcessTileEvent(Player player, int tileIndex)
    {
        TileData tileData = TileManager.Instance.GetTileDataByIndex(tileIndex);
        Tile tile = tileData.tile;

        // 如果是城市格子
        if (tile != null && tile.type == "city")
        {
            // 如果该城市没有被购买
            if (tile.owner < 0)
            {   
                // 显示购买面板
                yield return StartCoroutine(ShowPurchasePanel(player, tileData));
            }
            // 如果该城市已经被其他玩家购买
            else if (tile.owner != PlayerManager.Instance.currentPlayerIndex)
            {
                // 当前玩家向城市所有者支付租金
                PlayerManager.Instance.PayRent(PlayerManager.Instance.currentPlayerIndex, tile.owner, tile.rent);
                ShowEventInfo($"\n{player.name} 向 {PlayerManager.Instance.GetPlayerByIndex(tile.owner).name} 支付了租金 ${tile.rent}");

            }
            // 如果该城市已经被当前玩家购买
            else
            {
                if (tile.level < 3)
                {
                    // 显示升级面板
                    yield return StartCoroutine(ShowUpgradePanel(player, tile));
                }
                else
                {
                    ShowEventInfo($"{tile.name} 已经达到最大等级。");
                }
            }
        }
        else if (tile != null && tile.type == "event")
        {
            ShowEventInfo($"\n{player.name} 落在了 {tile.name} 格子");
            yield return StartCoroutine(TriggerEventCard(player));
        }
        else
        {
            ShowEventInfo($"\n{player.name} 落在了 {tile.name} 格子");
            // 对于其他类型的格子，处理资源、市场等
            yield return StartCoroutine( HandleOtherTileTypes(player, tile));
        }
        yield return null;
    }

    // 对于其他类型的格子，处理资源、市场等
    private IEnumerator HandleOtherTileTypes(Player player, Tile tile)
    {
        if (tile != null)
        {
            switch (tile.type)
            {
                case "start":
                    // TODO
                    ShowEventInfo($"\n{player.name} 落在了 {tile.name} 格子，获得 $800");
                    break;
                case "market":
                    ShowEventInfo($"\n{player.name} 落在了 {tile.name}");
                    // TODO 资源购买Panle
                    // 现在就购买随机卡
                    yield return StartCoroutine(ShowPurchaseResourcePanel(player));
                    break;
                case "pirate":
                    ShowEventInfo($"\n{player.name} 落在了 {tile.name}");
                    // TODO
                    break;
                case "villager":
                    PlayerManager.Instance.EarnMoney(player, 300);
                    ShowEventInfo($"\n{player.name} 落在了 {tile.name}");
                    break;
                case "resource":
                    // TODO 是否改成协程模式
                    PlayerManager.Instance.AddRandomResource(player);
                    ShowEventInfo($"\n{player.name} 落在了 {tile.name}");
                    break;
                default:
                    ShowEventInfo($"\n{player.name} 落在了 {tile?.name ?? "空白"} 格子");
                    break;
            }
        }
    }
    #endregion


    #region Panel button

    public IEnumerator ShowPurchaseResourcePanel(Player player)
    {
        // 显示购买资源的提示信息
        ShowPurchasePanelText($"{player.name}，是否购买一张资源卡？价格: $300?");
        
        decisionMade = false;
        buyDecision = false;
        SetPurchasePanelActive(true);

        // 等待玩家做出决定
        while (!decisionMade)
        {
            yield return null;
        }

        if (buyDecision)
        {
            int cost = 300;  // 假设资源卡的固定价格为300
            // 判断玩家是否有足够资金
            if (PlayerManager.Instance.CanPlayerAfford(player, cost))
            {
                PlayerManager.Instance.SpendMoney(player, cost);
                // 购买资源卡
                PlayerManager.Instance.AddRandomResource(player);
                ShowEventInfo($"\n{player.name} 购买了一张资源卡，花费了 ${cost}");
            }
            else
            {
                ShowEventInfo($"\n{player.name} 资金不足，无法购买资源卡");
            }
        }
        else
        {
            ShowEventInfo($"\n{player.name} 放弃了购买资源卡");
        }
        
        SetPurchasePanelActive(false);
    }

    public IEnumerator ShowUpgradePanel(Player player, Tile tile)
    {
        ShowPurchasePanelText($"{player.name}，是否花费 ${tile.upgradeCosts[tile.level]} 升级 {tile.name}?");
        decisionMade = false;
        buyDecision = false;
        SetPurchasePanelActive(true);

        while (!decisionMade)
        {
            yield return null;
        }

        if (buyDecision)
        {   
            int cost = tile.upgradeCosts[tile.level];
            // 玩家钱够
            if (PlayerManager.Instance.trySpendMonryForCity(player, cost))
            {
                TileManager.Instance.UpgradeTile(tile);
                ShowEventInfo($"\n{player.name} 升级了 {tile.name}。花费了 ${cost}");
            }
            else
            {
                ShowEventInfo($"\n{player.name} 资金不足，无法升级 {tile.name}。");
            }
        }
        else
        {
            ShowEventInfo($"\n{player.name} 放弃了升级 {tile.name}。");
        }
        SetPurchasePanelActive(false);
    }

    public IEnumerator ShowPurchasePanel(Player player, TileData tileData)
    {
        Tile tile = tileData.tile;
        ShowPurchasePanelText($"{player.name}, 是否购买 {tile.name}\n价格: ${tile.price}?");
        decisionMade = false;
        buyDecision = false;
        SetPurchasePanelActive(true);

        while (!decisionMade)
        {
            yield return null;
        }

        if (buyDecision)
        {
            int cost = tile.price;
            if (PlayerManager.Instance.trySpendMonryForCity(player, cost))
            {
                TileManager.Instance.ownerTile(tile, player.playerIndex, player.playerColor);
                ShowEventInfo($"\n{player.name} 购买了 {tile.name}， 花费了 ${cost}");
            }
            else
            {
                ShowEventInfo($"\n{player.name} 资金不足，无法购买 {tile.name}");
            }
        }
        else
        {
            ShowEventInfo($"\n{player.name} 放弃了购买 {tile.name}");
        }
        SetPurchasePanelActive(false);
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
    #endregion

    #region event 
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

    // 处理正面事件
    public IEnumerator HandlePositiveEvent(Player player, int eventIndex)
    {
        switch (eventIndex)
        {
            case 0:
                ShowEventInfo("\n发现新商路：恭喜你发现一条通往神秘地区的新商路，可抽取一张资源卡。");
                yield return StartCoroutine(HandleResourceEvent(player, 0));
                break;
            case 1:
                ShowEventInfo("\n文化交流：你成功与当地商人进行了文化交流，他们对你的慷慨和见识表示赞赏，赠送你500金。");
                PlayerManager.Instance.EarnMoney(player, 500);
                break;
            case 2:
                ShowEventInfo("\n获得资助：一位富有的贵族看中了你的商业潜力，决定资助你，获得 800 金币。");
                PlayerManager.Instance.EarnMoney(player, 800);
                break;
            case 3:
                ShowEventInfo("\n发现遗迹：在探索途中，你发现了一处古老遗迹，从中找到珍贵文物，换回一个地皮（自选不大于500金的地皮）。");
                // TODO
                break;
            case 4:
                ShowEventInfo("\n幸运之神眷顾：幸运之神降临，接下来三个回合，你掷骰子的点数 +1。");
                // TODO
                break;
            case 5:
                ShowEventInfo("\n丰收之年：途经的地区迎来丰收，你收购到大量低价优质资源，抽取两张资源卡。");
                for (int i = 0; i < 2; i++)
                {
                    yield return StartCoroutine(HandleResourceEvent(player, 0));
                }
                break;
            case 6:
                ShowEventInfo("\n学会新技术：在当地学习到一种独特的制作工艺，收益700金。");
                PlayerManager.Instance.EarnMoney(player, 700);
                break;
            case 7:
                ShowEventInfo("\n获得推荐信：得到当地一位重要人物的推荐信，可以免费在自己的地皮上加盖商铺。");
                // TODO
                break;
            case 8:
                ShowEventInfo("\n新贸易伙伴：结识了一位远方的贸易伙伴，给你带来了一张资源卡和300金。");
                yield return StartCoroutine(HandleResourceEvent(player, 0));
                PlayerManager.Instance.EarnMoney(player, 300);
                break;
        }
        yield return null;
    }

    public IEnumerator HandleNegativeEvent(Player player, int eventIndex)
    {
        switch (eventIndex)
        {
            case 0:
                ShowEventInfo("\n遭遇风沙：不幸遭遇强烈风沙，暂停下一回合行动，且损失 200 金币用于清理货物和修复商队装备。");
                PlayerManager.Instance.SpendMoney(player, 200);
                break;
            case 1:
                ShowEventInfo("\n强盗袭击：一群强盗抢走了你部分财物，损失 300 金币和一张资源卡。");
                yield return StartCoroutine(HandleResourceEvent(player, 1));
                PlayerManager.Instance.SpendMoney(player, 300);
                break;
            case 2:
                ShowEventInfo("\n迷路：在沙漠中迷失方向，后退三格，浪费一回合时间寻找方向。");
                // TODO
                break;
            case 3:
                ShowEventInfo("\n疾病流行：所在地区疾病流行，你的商队人员受到影响，下一回合无法建造建筑，且需花费 200 金币治疗。");
                PlayerManager.Instance.SpendMoney(player, 200);
                // TODO
                break;
            case 4:
                ShowEventInfo("\n税收增加：当地政府临时增加税收，缴纳当前所拥有金币的 5% 作为税款。");
                PlayerManager.Instance.SpendMoney(player, (int)(player.money * 0.05f));
                break;
            case 5:
                ShowEventInfo("\n竞争对手破坏：竞争对手暗中破坏你的生意，拆除你在当前城市一座等级最低的建筑。");
                // TODO
                break;
        }
        yield return null;
    }
    #endregion  

    #region Resource

    // 触发资源相关事件（例如抽取资源卡）
    public IEnumerator HandleResourceEvent(Player player, int eventIndex)
    {
        switch (eventIndex)
        {
            case 0: // 抽取一张资源卡
                // AddRandomResource
                PlayerManager.Instance.AddRandomResource(player);
                break;
            case 1: // 失去一张资源卡
                // RemoveRandomResource
                PlayerManager.Instance.RemoveRandomResource(player);
                break;
            // 更多的资源相关事件逻辑可以继续添加
        }
        yield return null;
    }
    #endregion

    
    #region UI

    public void ShowPurchasePanelText(string message)
    {
        purchasePanelText.text = message;
    }

    public void SetPurchasePanelActive(bool active)
    {
        purchasePanel.SetActive(active);
    }
    
    public void ShowEventInfo(string message)
    {
        eventInfoText.text = message;
    }
    #endregion
    

    #region test 
    public IEnumerator AutoBuyAndUpgradeCityAndResource(Player player, int tileIndex){
        // 自动购买和升级城市
        TileData tileData = TileManager.Instance.GetTileDataByIndex(tileIndex);
        if (tileData.tile.CanPurchase())
        {   
            PlayerManager.Instance.SpendMoney(player, tileData.tile.price);
            PlayerManager.Instance.AddHousePrice(player, tileData.tile.price / 2);
            TileManager.Instance.ownerTile(tileData.tile, player.playerIndex, player.playerColor);
        }
        
        // 升级城市三次
        for (int i = 0; i < 3; i++)
        {
            if (tileData.tile.CanUpgrade())
            {
                PlayerManager.Instance.SpendMoney(player, tileData.tile.upgradeCosts[tileData.tile.level]);
                PlayerManager.Instance.AddHousePrice(player, tileData.tile.upgradeCosts[tileData.tile.level] / 2);
                TileManager.Instance.UpgradeTile(tileData.tile);
                yield return new WaitForSeconds(1);
            }
        }
        // 获得资源卡
        for (int i = 0; i < 3; i++)
        {
            PlayerManager.Instance.AddRandomResource(player);
            yield return new WaitForSeconds(1);
        }

        yield break; 

    }
    
    #endregion
}
