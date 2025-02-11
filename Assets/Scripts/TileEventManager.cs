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
