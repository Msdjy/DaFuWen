using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("引用部分")]
    // 引用生成棋盘的 CubeManager（建议将 CubeManager 放在同一场景中的某个空物体上）
    public CubeManager cubeManager;
    // 玩家头像预制体（可以是一个小球或者其它形状，用于在棋盘上移动）
    public GameObject playerPrefab;
    // UI 文本，用于显示玩家信息和提示
    public Text infoText;
    // 掷骰子按钮
    public Button rollDiceButton;
    // 假设你已经在 Inspector 中引用了一个支持中文的 TMP_FontAsset 资源
    public TMP_FontAsset chineseFontAsset;


    [Header("购买面板相关")]
    public GameObject purchasePanel;      // 包含购买提示的面板（初始时应隐藏）
    public TextMeshProUGUI purchasePanelText; // 显示提示信息的文本组件
    public Button buyButton;              // “购买”按钮
    public Button skipButton;             // “不购买”按钮

    // 玩家列表（示例中创建 2 个玩家）
    private List<Player> players = new List<Player>();
    private int currentPlayerIndex = 0;
    // 存储棋盘上所有格子（通过场景中所有 TileController 获取）
    private List<TileController> boardTiles = new List<TileController>();

    // 用于记录玩家在购买面板上的选择
    private bool decisionMade;
    private bool buyDecision;

    IEnumerator Start()
    {
        // 等待一帧，确保 CubeManager 已经生成所有 Tile
        yield return null;

        // 查找所有生成的棋盘格子（TileController 组件必须已添加到各个 Cube 上）
        boardTiles = Object.FindObjectsByType<TileController>(FindObjectsSortMode.None)
                .OrderBy(t => t.tileIndex)
                .ToList();

        // 检查是否有格子
        if (boardTiles.Count == 0)
        {
            Debug.LogError("棋盘上没有找到任何格子，请检查 CubeManager 是否正常生成地图！");
            yield break;
        }

        // 初始化购买面板，确保隐藏
        purchasePanel.SetActive(false);

        // 创建玩家（示例中创建 2 个玩家，起始金钱设为 1500）
        CreatePlayers();

        // 设置 UI 按钮点击事件
        rollDiceButton.onClick.AddListener(() => StartCoroutine(TakeTurn()));

        // 购买面板按钮事件设置
        buyButton.onClick.AddListener(OnBuyButtonClicked);
        skipButton.onClick.AddListener(OnSkipButtonClicked);


        UpdateInfoText();
    }

    void CreatePlayers()
    {
        // 示例中创建两个玩家
        for (int i = 0; i < 2; i++)
        {
            Player newPlayer = new Player();
            newPlayer.name = "Player " + (i + 1);
            newPlayer.money = 1500; // 初始金钱
            newPlayer.currentTile = 1; // 从起始格子（索引 0）开始

            // 在起始格子位置生成玩家头像（可适当上移一点以便显示在棋盘上方）
            Vector3 startPos = GetTilePosition(1) + new Vector3(0, 1, 0);
            GameObject avatar = Instantiate(playerPrefab, startPos, Quaternion.identity);
            newPlayer.avatar = avatar;
            players.Add(newPlayer);

            // 为玩家头像添加上方显示玩家信息的文本
            CreatePlayerTextForAvatar(avatar, newPlayer);
        }
    }

    void CreatePlayerTextForAvatar(GameObject avatar, Player player)
    {
        // 创建一个空子物体
        GameObject textObj = new GameObject("PlayerInfo");
        // 将其设为玩家头像的子物体
        textObj.transform.SetParent(avatar.transform);
        // 调整局部位置，使文字出现在玩家头像上方
        textObj.transform.localPosition = new Vector3(0, 4f, 0);
        // 旋转文字使其朝上
        textObj.transform.localRotation = Quaternion.Euler(90, 0, 0);
        // 设置局部缩放为均匀
        textObj.transform.localScale = Vector3.one;

        // 添加 TextMeshPro 组件
        TextMeshPro textMeshPro = textObj.AddComponent<TextMeshPro>();
        // 设置文本内容：玩家名称和当前资金
        textMeshPro.text = $"{player.name}\nMoney: ${player.money}";
        textMeshPro.fontSize = 3;
        textMeshPro.alignment = TextAlignmentOptions.Center;
        textMeshPro.color = Color.white;

        // 保存该引用到 Player 对象中
        player.playerText = textMeshPro;
    }

    void UpdatePlayerAvatarText(Player player)
    {
        if (player.playerText != null)
        {
            player.playerText.text = $"{player.name}\nMoney: ${player.money}";
        }
    }




    // 根据棋盘格子索引获取该格子的世界坐标
    Vector3 GetTilePosition(int tileIndex)
    {
        TileController tile = boardTiles.Find(t => t.tileIndex == tileIndex);
        if (tile != null)
        {
            return tile.transform.position;
        }
        else
        {
            Debug.LogError("未能找到索引为 " + tileIndex + " 的格子！");
            return Vector3.zero;
        }
    }

    // 掷骰子并处理当前玩家的回合
    IEnumerator TakeTurn()
    {
        // 禁用按钮防止重复点击
        rollDiceButton.interactable = false;

        Player currentPlayer = players[currentPlayerIndex];

        // 掷骰子
        int diceRoll = RollDice();
        infoText.text = currentPlayer.name + " 掷出了 " + diceRoll + " 点";
        yield return new WaitForSeconds(1f);

        // 计算目标格子索引
        int targetIndex = (currentPlayer.currentTile - 1 + diceRoll) % boardTiles.Count + 1;

        // 执行玩家移动动画
        yield return StartCoroutine(MovePlayer(currentPlayer, targetIndex));

        // **使用 yield return 启动 ProcessTileEvent 协程**
        yield return StartCoroutine(ProcessTileEvent(currentPlayer, targetIndex));

        // 结束当前回合，切换到下一位玩家
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        UpdateInfoText();
        rollDiceButton.interactable = true;
    }


    // 模拟两个骰子随机数（1~6），返回点数和
    int RollDice()
    {
        int die1 = Random.Range(1, 7);
        int die2 = Random.Range(1, 7);
        return die1 + die2;
    }

    // 逐格移动玩家，从当前格子到目标格子（简单线性移动）
    IEnumerator MovePlayer(Player player, int destinationIndex)
    {
        while (player.currentTile != destinationIndex)
        {
            int nextTile = (player.currentTile % boardTiles.Count + 1);
            Vector3 targetPos = GetTilePosition(nextTile) + new Vector3(0, 1, 0);
            // 逐帧移动到下一个格子
            while (Vector3.Distance(player.avatar.transform.position, targetPos) > 0.1f)
            {
                player.avatar.transform.position = Vector3.MoveTowards(player.avatar.transform.position, targetPos, Time.deltaTime * 5f);
                yield return null;
            }
            player.currentTile = nextTile;
            yield return new WaitForSeconds(0.2f);
        }
    }

    // 处理玩家落在某个格子时触发的事件（例如购买房产或支付租金）
    IEnumerator ProcessTileEvent(Player player, int tileIndex)
    {
        TileController tile = boardTiles.Find(t => t.tileIndex == tileIndex);
        if (tile == null)
            yield break;
        
        // 示例：如果该格子代表房产（价格大于 0）
        if (tile.tileData != null && tile.tileData.price > 0)
        {
            // 如果无人拥有，则自动购买（可以扩展为弹出询问窗口让玩家选择）
            if (tile.owner < 0)
            {
                // 显示购买面板，等待玩家决策
                yield return StartCoroutine(ShowPurchasePanel(player, tile));
            }
            // 如果该房产已经被其他玩家拥有，则支付租金
            else if (tile.owner != currentPlayerIndex)
            {
                int rent = tile.tileData.rent;
                player.money -= rent;
                players[tile.owner].money += rent;
                infoText.text += "\n" + player.name + " 向 " + players[tile.owner].name + " 支付了租金 $" + rent;
            }
            // 如果房产归自己所有，则不作处理
        }
        // 此处可以扩展其它类型的格子事件（如命运、社区、税收等）
        else
        {
            // 其它格子事件，比如命运、税收、社区等（可扩展）
            infoText.text += "\n" + player.name + " 落在了 " + (tile.tileData != null ? tile.tileData.name : "空白") + " 格子";
        }
        yield return null;
    }

    // 显示购买面板，等待玩家作出选择
    IEnumerator ShowPurchasePanel(Player player, TileController tile)
    {
        // 更新面板文本显示购买提示信息
        purchasePanelText.text = $"{player.name}, 是否购买 {tile.tileData.name}\n价格: ${tile.tileData.price}?";
        // 重置决策标志
        decisionMade = false;
        buyDecision = false;
        // 显示购买面板
        purchasePanel.SetActive(true);

        // 等待玩家点击按钮作出决策
        while (!decisionMade)
        {
            yield return null;
        }

        // 隐藏购买面板
        purchasePanel.SetActive(false);

        if (buyDecision)
        {
            if (player.money >= tile.tileData.price)
            {
                player.money -= tile.tileData.price;
                tile.owner = currentPlayerIndex;  // 标记当前玩家为房产所有者
                tile.UpdateTileText(); // 更新 tile 上的文字显示
                infoText.text += "\n" + player.name + " 购买了 " + tile.tileData.name;
                // 更新玩家头像上的文本
                UpdatePlayerAvatarText(player);
                UpdateInfoText(); // 同时更新全局信息显示
            }
            else
            {
                infoText.text += "\n" + player.name + " 资金不足，无法购买 " + tile.tileData.name;
            }
        }
        else
        {
            infoText.text += "\n" + player.name + " 放弃了购买 " + tile.tileData.name;
        }
    }


    // 购买面板“购买”按钮点击事件
    void OnBuyButtonClicked()
    {
        buyDecision = true;
        decisionMade = true;
    }

    // 购买面板“不购买”按钮点击事件
    void OnSkipButtonClicked()
    {
        buyDecision = false;
        decisionMade = true;
    }

    // 更新 UI 显示当前回合玩家及所有玩家的资金信息
    void UpdateInfoText()
    {
        string info = "当前回合：" + players[currentPlayerIndex].name + "\n";
        foreach (Player p in players)
        {
            info += p.name + " - 资金：$" + p.money + "\n";
        }
        infoText.text = info;
    }
}
