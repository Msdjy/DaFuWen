using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("引用部分")]
    public MapManager mapManager;
    public PlayerManager playerManager;
    public TileEventManager tileEventManager;
    public Text infoText;
    public Button rollDiceButton;

    public List<TileController> boardTiles = new List<TileController>();

    IEnumerator Start()
    {
        yield return null;
        SetupGame(); // 初始化游戏
    }

    private void SetupGame()
    {
        boardTiles = GetSortedTileControllers();
        rollDiceButton.onClick.AddListener(() => StartCoroutine(TakeTurn()));
        playerManager.CreatePlayers();
        tileEventManager.gameManager = this;
        tileEventManager.playerManager = playerManager;

        // 将 TileEventManager 引用传递给所有 TileController
        foreach (var tile in boardTiles)
        {
            tile.tileEventManager = tileEventManager;  // 设置引用
        }

        UpdateInfoText();


        // 新增：自动测试，模拟玩家1购买并升级指定城市
        StartCoroutine(tileEventManager.AutoBuyAndUpgradeCity(0, 1)); // 假设 0 是玩家1，1 是城市的 tileIndex

    }

    private List<TileController> GetSortedTileControllers()
    {
        return FindObjectsOfType<TileController>().OrderBy(t => t.tileIndex).ToList();
    }

    private IEnumerator TakeTurn()
    {
        rollDiceButton.interactable = false;
        Player currentPlayer = playerManager.players[playerManager.currentPlayerIndex];
        int diceRoll = RollDice();
        infoText.text = $"{currentPlayer.name} 掷出了 {diceRoll} 点";
        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(playerManager.MovePlayer(currentPlayer, diceRoll));
        yield return StartCoroutine(tileEventManager.ProcessTileEvent(currentPlayer, currentPlayer.currentTileIndex));

        playerManager.SwitchPlayer(); // 切换玩家
        UpdateInfoText();
        rollDiceButton.interactable = true;
    }

    private int RollDice() => Random.Range(1, 7) + Random.Range(1, 7);


    private void UpdateInfoText()
    {
        string info = $"当前回合: {playerManager.players[playerManager.currentPlayerIndex].name}\n";
        infoText.text = info;
        playerManager.UpdatePlayerInfoText();
    }
}
