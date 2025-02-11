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

    public IEnumerator ProcessTileEvent(Player player, int tileIndex)
    {
        int tileID = gameManager.mapManager.tileIds[tileIndex];
        TileController tile = gameManager.boardTiles.Find(t => t.tileIndex == tileID);
        if (tile == null)
            yield break;

        if (tile.tileData != null && tile.tileData.price > 0)
        {
            if (tile.owner < 0)
            {
                yield return StartCoroutine(ShowPurchasePanel(player, tile)); // 如果该城市无人拥有，提供购买选择
            }
            else if (tile.owner != playerManager.currentPlayerIndex)
            {
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
                    eventInfoText.text = $"{player.name}，是否要升级 {tile.tileData.name}?";
                    // 触发升级逻辑
                    yield return StartCoroutine(ShowUpgradePanel(player, tile));
                }
                else
                {
                    eventInfoText.text = $"{tile.tileData.name} 已经达到最大等级。";
                }
            }
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
