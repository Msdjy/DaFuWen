using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("引用部分")]
    [Tooltip("引用生成地图 Tile 的 MapManager")]
    public MapManager mapManager;
    [Tooltip("玩家头像预制体（例如小球），用于在地图上移动")]
    public GameObject playerPrefab;
    [Tooltip("显示玩家信息和提示的 UI 文本")]
    public Text infoText;
    [Tooltip("掷骰子按钮")]
    public Button rollDiceButton;
    [Tooltip("支持中文的 TMP 字体")]
    public TMP_FontAsset chineseFontAsset;

    [Header("购买面板相关")]
    public GameObject purchasePanel;          // 包含购买提示的面板（初始时隐藏）
    public TextMeshProUGUI purchasePanelText;   // 显示提示信息
    public Button buyButton;                    // “购买”按钮
    public Button skipButton;                   // “不购买”按钮

    [Header("UI部分")]
    [Tooltip("左上角显示玩家信息的 Text")]
    public Text leftPlayerInfoText;   // 普通 Text 组件
    [Tooltip("右上角显示玩家信息的 Text")]
    public Text rightPlayerInfoText;  // 普通 Text 组件

    private List<Player> players = new List<Player>();
    private int currentPlayerIndex = 0;
    /// <summary>
    /// 保存所有地图上生成的 Tile（按照 tileIndex 排序）
    /// </summary>
    private List<TileController> boardTiles = new List<TileController>();

    // 记录购买面板上的玩家决策
    private bool decisionMade;
    private bool buyDecision;

    IEnumerator Start()
    {
        // 等待一帧，确保 MapManager 已经生成了所有 Tile
        yield return null;

        // 查找所有生成的 TileController，并按 tileIndex 排序
        boardTiles = FindObjectsOfType<TileController>()
                        .OrderBy(t => t.tileIndex)
                        .ToList();

        if (boardTiles.Count == 0)
        {
            Debug.LogError("未在地图上找到任何 Tile，请检查 MapManager 是否正常生成！");
            yield break;
        }

        purchasePanel.SetActive(false);

        CreatePlayers();

        // 设置 UI 按钮事件
        rollDiceButton.onClick.AddListener(() => StartCoroutine(TakeTurn()));
        buyButton.onClick.AddListener(OnBuyButtonClicked);
        skipButton.onClick.AddListener(OnSkipButtonClicked);

        UpdateInfoText();
    }

    /// <summary>
    /// 创建玩家，初始化起始位置、资金，并在玩家头像上添加显示信息的文本
    /// </summary>
    void CreatePlayers()
    {
        // 例如创建 2 个玩家
        for (int i = 0; i < 2; i++)
        {
            Player newPlayer = new Player();
            newPlayer.name = "Player " + (i + 1);
            newPlayer.money = 1500; // 初始资金
            newPlayer.currentTileIndex = 0; // 从起始 Tile（索引 0）开始

            // 为玩家指定颜色
            if (i == 0)
            {
                newPlayer.playerColor = Color.red; // 玩家 1 为红色
            }
            else
            {
                newPlayer.playerColor = Color.green; // 玩家 2 为绿色
            }

            // 根据 MapManager 中保存的 Tile 位置生成玩家头像
            Vector3 startPos = GetTilePosition(newPlayer.currentTileIndex) + new Vector3(0, 0, 0);
            GameObject avatar = Instantiate(playerPrefab, startPos, Quaternion.identity);
            newPlayer.avatar = avatar;

            // 设置玩家头像颜色
            MeshRenderer avatarRenderer = avatar.GetComponentInChildren<MeshRenderer>();
            if (avatarRenderer != null)
            {
                avatarRenderer.material.color = newPlayer.playerColor;  // 设置颜色
            }

            CreatePlayerText(avatar, newPlayer);
            players.Add(newPlayer);
        }
    }

    /// <summary>
    /// 在玩家头像上添加用于显示玩家名称和资金信息的 TextMeshProUGUI 子物体
    /// </summary>
    void CreatePlayerText(GameObject avatar, Player player)
    {
        // 创建显示玩家信息的 TextMeshProUGUI
        GameObject textObj = new GameObject("PlayerInfo");
        textObj.transform.SetParent(avatar.transform);
        textObj.transform.localPosition = new Vector3(0, 2.5f, 0);
        textObj.transform.localRotation = Quaternion.Euler(90, 0, 0);
        textObj.transform.localScale = Vector3.one;

        // 使用 TextMeshProUGUI 来显示玩家信息
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = $"{player.name}\nMoney: ${player.money}";
        tmp.fontSize = 14;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = player.playerColor;

        player.playerText = tmp;


    }

    /// <summary>
    /// 根据地图上 Tile 索引返回对应的世界坐标
    /// </summary>
    Vector3 GetTilePosition(int tileIndex)
    {
        if (mapManager == null)
        {
            Debug.LogError("MapManager 引用为空！");
            return Vector3.zero;
        }
        if (mapManager.tilePositions == null || mapManager.tilePositions.Count == 0)
        {
            Debug.LogError("MapManager 中的 tilePositions 未初始化或为空！");
            return Vector3.zero;
        }
        if (tileIndex < 0 || tileIndex >= mapManager.tilePositions.Count)
        {
            Debug.LogError($"Tile index {tileIndex} 超出范围！");
            return mapManager.tilePositions[0];
        }
        return mapManager.tilePositions[tileIndex];
    }

    /// <summary>
    /// 玩家回合：掷骰子、移动、处理 Tile 事件
    /// </summary>
    IEnumerator TakeTurn()
    {
        rollDiceButton.interactable = false;
        Player currentPlayer = players[currentPlayerIndex];

        int diceRoll = RollDice();
        infoText.text = $"{currentPlayer.name} 掷出了 {diceRoll} 点";
        yield return new WaitForSeconds(1f);

        // 按骰子点数移动：逐步更新玩家的位置
        yield return StartCoroutine(MovePlayer(currentPlayer, diceRoll));

        // 玩家移动后，根据当前停留的格子触发事件
        yield return StartCoroutine(ProcessTileEvent(currentPlayer, currentPlayer.currentTileIndex));

        // 切换到下一位玩家
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        UpdateInfoText();
        rollDiceButton.interactable = true;
    }

    /// <summary>
    /// 模拟两个骰子（1～6），返回点数和
    /// </summary>
    int RollDice()
    {
        int die1 = Random.Range(1, 7);
        int die2 = Random.Range(1, 7);
        return die1 + die2;
    }

    /// <summary>
    /// 玩家逐格移动协程，从当前 Tile 移动到目标 Tile
    /// </summary>
    IEnumerator MovePlayer(Player player, int steps)
    {
        int totalTiles = mapManager.tilePositions.Count;

        for (int i = 0; i < steps; i++)
        {
            player.currentTileIndex = (player.currentTileIndex + 1) % totalTiles;
            Vector3 targetPos = GetTilePosition(player.currentTileIndex);

            while (Vector3.Distance(player.avatar.transform.position, targetPos) > 0.1f)
            {
                player.avatar.transform.position = Vector3.MoveTowards(
                    player.avatar.transform.position,
                    targetPos,
                    Time.deltaTime * 5f
                );
                yield return null;
            }

            yield return new WaitForSeconds(0.1f);
        }

        Debug.Log($"Player {player.name} is now at tile {player.currentTileIndex}");
    }

    /// <summary>
    /// 根据当前 Tile 类型处理对应事件，如购买、支付租金等
    /// </summary>
    IEnumerator ProcessTileEvent(Player player, int tileIndex)
    {
        int tileID = mapManager.tileIds[tileIndex];
        TileController tile = boardTiles.Find(t => t.tileIndex == tileID);
        if (tile == null)
            yield break;

        if (tile.tileData != null && tile.tileData.price > 0)
        {
            if (tile.owner < 0)
            {
                yield return StartCoroutine(ShowPurchasePanel(player, tile));
            }
            else if (tile.owner != currentPlayerIndex)
            {
                int rent = tile.tileData.rent;
                player.money -= rent;
                players[tile.owner].money += rent;
                infoText.text += $"\n{player.name} 向 {players[tile.owner].name} 支付了租金 ${rent}";
            }
        }
        else
        {
            infoText.text += $"\n{player.name} 落在了 {(tile.tileData != null ? tile.tileData.name : "空白")} 格子";
        }

        if (tile.owner == currentPlayerIndex)
        {
            MeshRenderer cubeRenderer = tile.GetComponentInChildren<MeshRenderer>();
            if (cubeRenderer != null)
            {
                cubeRenderer.material.color = player.playerColor;
            }
        }

        yield return null;
    }

    /// <summary>
    /// 显示购买面板，等待玩家作出“购买”或“放弃”的决策
    /// </summary>
    IEnumerator ShowPurchasePanel(Player player, TileController tile)
    {
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
                tile.owner = currentPlayerIndex;
                tile.UpdateTileText();
                infoText.text += $"\n{player.name} 购买了 {tile.tileData.name}";
                UpdatePlayerText(player);
                UpdateInfoText();
            }
            else
            {
                infoText.text += $"\n{player.name} 资金不足，无法购买 {tile.tileData.name}";
            }
        }
        else
        {
            infoText.text += $"\n{player.name} 放弃了购买 {tile.tileData.name}";
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

    /// <summary>
    /// 更新玩家头像上显示的资金信息
    /// </summary>
    void UpdatePlayerText(Player player)
    {
        if (player.playerText != null)
            player.playerText.text = $"{player.name}\nMoney: ${player.money}";
    }

    /// <summary>
    /// 更新全局 UI 信息（例如当前回合玩家及所有玩家资金）
    /// </summary>
    void UpdateInfoText()
    {
        string info = $"当前回合: {players[currentPlayerIndex].name}\n";
        foreach (var p in players)
        {
            info += $"{p.name} - 资金: ${p.money}\n";
        }
        infoText.text = info;

        // 更新左上角和右上角的玩家信息
        leftPlayerInfoText.text = $"{players[0].name}\nMoney: ${players[0].money}";
        rightPlayerInfoText.text = $"{players[1].name}\nMoney: ${players[1].money}";
        // 根据玩家颜色设置左上角和右上角文本颜色
        // TODO 不应该在这处理
        leftPlayerInfoText.color = players[0].playerColor;
        rightPlayerInfoText.color = players[1].playerColor;
    }
}
