using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public MapManager mapManager;
    public PlayerManager playerManager;
    public TileEventManager tileEventManager;
    public Button rollDiceButton;
    public List<TileController> boardTiles = new List<TileController>();

    IEnumerator Start()
    {
        yield return null;
        SetupGame();
    }

    private void SetupGame()
    {
        boardTiles = GetSortedTileControllers();
        rollDiceButton.onClick.AddListener(() => StartCoroutine(TakeTurn()));
        playerManager.InitializePlayers();
        tileEventManager.gameManager = this;
        tileEventManager.playerManager = playerManager;

        foreach (var tile in boardTiles)
        {
            tile.tileEventManager = tileEventManager;
        }

        // UpdateInfoText();
        StartCoroutine(tileEventManager.AutoBuyAndUpgradeCity(0, 1));
    }

    private List<TileController> GetSortedTileControllers()
    {
        return FindObjectsOfType<TileController>().OrderBy(t => t.tileIndex).ToList();
    }

    private IEnumerator TakeTurn()
    {
        rollDiceButton.interactable = false;
        Player currentPlayer = playerManager.GetCurrentPlayer();
        int diceRoll = RollDice();
        UIManager.Instance.ShowTurnInfo($"{currentPlayer.name} 掷出了 {diceRoll} 点");
        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(playerManager.MovePlayer(currentPlayer, diceRoll));
        yield return StartCoroutine(tileEventManager.ProcessTileEvent(currentPlayer, currentPlayer.currentTileIndex));

        playerManager.SwitchPlayer();
        UIManager.Instance.ShowTurnInfo($"当前回合: {playerManager.GetCurrentPlayer().name}\n");
        playerManager.UpdatePlayerInfoText();
        rollDiceButton.interactable = true;
    }

    private int RollDice() => Random.Range(1, 7) + Random.Range(1, 7);
}
