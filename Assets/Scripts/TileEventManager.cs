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
                    UpdateCityModel(tile); // 更新预制体模型
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

    // 根据等级更新城市模型
    public void UpdateCityModel(TileController tile)
    {
        // 销毁现有的模型（如果存在） - 获取第三个子节点并销毁
        if (tile.transform.childCount > 2) // 确保存在至少三个子节点
        {
            // 销毁第三个子节点，即城市模型
            Destroy(tile.transform.GetChild(2).gameObject);
        }

        GameObject cityModel = null;

        // 根据等级选择相应的预制体
        switch (tile.tileData.level)
        {
            case 1:
                cityModel = Instantiate(cityLevel1Prefab, tile.transform.position, Quaternion.identity, tile.transform);
                break;
            case 2:
                cityModel = Instantiate(cityLevel2Prefab, tile.transform.position, Quaternion.identity, tile.transform);
                break;
            case 3:
                cityModel = Instantiate(cityLevel3Prefab, tile.transform.position, Quaternion.identity, tile.transform);
                break;
        }

        // 如果生成了模型，设置其缩放比例
        if (cityModel != null)
        {
            cityModel.transform.localScale = new Vector3(0.06f, 0.30f, 0.06f); // 设置缩放比例

            // 获取模型的 MeshRenderer 组件并设置颜色
            MeshRenderer renderer = cityModel.GetComponentInChildren<MeshRenderer>();
            if (renderer != null)
            {
                // 这里设置模型的颜色，可以替换为任何颜色
                renderer.material.color = tile.owner == -1 ? Color.gray : playerManager.players[tile.owner].playerColor;
            }
        }

        // 显示等级信息
        Debug.Log($"{tile.tileData.name} 等级：{tile.tileData.level}");
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
