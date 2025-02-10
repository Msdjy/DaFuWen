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
                yield return StartCoroutine(ShowPurchasePanel(player, tile));
            }
            else if (tile.owner != playerManager.currentPlayerIndex)
            {
                int rent = tile.tileData.rent;
                player.money -= rent;
                playerManager.players[tile.owner].money += rent;
                gameManager.infoText.text += $"\n{player.name} 向 {playerManager.players[tile.owner].name} 支付了租金 ${rent}";
            }
        }
        else
        {
            gameManager.infoText.text += $"\n{player.name} 落在了 {(tile.tileData != null ? tile.tileData.name : "空白")} 格子";
        }

        if (tile.owner == playerManager.currentPlayerIndex)
        {
            MeshRenderer cubeRenderer = tile.GetComponentInChildren<MeshRenderer>();
            if (cubeRenderer != null)
            {
                cubeRenderer.material.color = player.playerColor;
            }
        }

        yield return null;
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

        purchasePanel.SetActive(false);

        if (buyDecision)
        {
            if (player.money >= tile.tileData.price)
            {
                player.money -= tile.tileData.price;
                tile.owner = playerManager.currentPlayerIndex;
                tile.UpdateTileText();
                gameManager.infoText.text += $"\n{player.name} 购买了 {tile.tileData.name}";
                playerManager.UpdateInfoText();
            }
            else
            {
                gameManager.infoText.text += $"\n{player.name} 资金不足，无法购买 {tile.tileData.name}";
            }
        }
        else
        {
            gameManager.infoText.text += $"\n{player.name} 放弃了购买 {tile.tileData.name}";
        }
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
