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

    // 玩家列表（示例中创建 2 个玩家）
    private List<Player> players = new List<Player>();
    private int currentPlayerIndex = 0;
    // 存储棋盘上所有格子（通过场景中所有 TileController 获取）
    private List<TileController> boardTiles = new List<TileController>();

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

        // 创建玩家（示例中创建 2 个玩家，起始金钱设为 1500）
        CreatePlayers();

        // 设置 UI 按钮点击事件
        rollDiceButton.onClick.AddListener(() => StartCoroutine(TakeTurn()));

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
            newPlayer.currentTile = 0; // 从起始格子（索引 0）开始
            // 在起始格子位置生成玩家头像（可适当上移一点以便显示在棋盘上方）
            Vector3 startPos = GetTilePosition(0) + new Vector3(0, 1, 0);
            GameObject avatar = Instantiate(playerPrefab, startPos, Quaternion.identity);
            newPlayer.avatar = avatar;
            players.Add(newPlayer);
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

        // 掷骰子（这里用两个骰子，相加的值）
        int diceRoll = RollDice();
        infoText.text = currentPlayer.name + " 掷出了 " + diceRoll + " 点";

        // 等待一秒钟模拟掷骰动画
        yield return new WaitForSeconds(1f);

        // 计算目标格子索引（如果超过棋盘末尾，则循环到开头）
        int targetIndex = (currentPlayer.currentTile + diceRoll) % boardTiles.Count;

        // 执行玩家移动动画（沿着棋盘逐步移动）
        yield return StartCoroutine(MovePlayer(currentPlayer, targetIndex));

        // 移动结束后，根据目标格子触发事件（例如购买或者支付租金）
        ProcessTileEvent(currentPlayer, targetIndex);

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
            int nextTile = (player.currentTile + 1) % boardTiles.Count;
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
    void ProcessTileEvent(Player player, int tileIndex)
    {
        TileController tile = boardTiles.Find(t => t.tileIndex == tileIndex);
        if (tile == null)
            return;

        // 示例：如果该格子代表房产（价格大于 0）
        if (tile.tileData != null && tile.tileData.price > 0)
        {
            // 如果无人拥有，则自动购买（可以扩展为弹出询问窗口让玩家选择）
            if (tile.owner < 0)
            {
                if (player.money >= tile.tileData.price)
                {
                    player.money -= tile.tileData.price;
                    tile.owner = currentPlayerIndex;  // 用当前玩家的索引作为所有者 ID
                    infoText.text += "\n" + player.name + " 购买了 " + tile.tileData.name;
                }
                else
                {
                    infoText.text += "\n" + player.name + " 资金不足，无法购买 " + tile.tileData.name;
                }
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
