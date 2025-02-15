using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    // 单例
    public static GameManager Instance;
    public MapManager mapManager;
    public TileEventManager tileEventManager;
    public Button rollDiceButton;


    IEnumerator Start()
    {
        yield return null;
        SetupGame();
    }

    private void SetupGame()
    {
        rollDiceButton.onClick.AddListener(() => StartCoroutine(TakeTurn()));
        PlayerManager.Instance.InitializePlayers();

        // 测试
        StartCoroutine(TileManager.Instance.AutoBuyAndUpgradeCity(PlayerManager.Instance.GetPlayerByIndex(0), 1));
    }

    private IEnumerator TakeTurn()
    {
        rollDiceButton.interactable = false;
        Player currentPlayer = PlayerManager.Instance.GetCurrentPlayer();
        int diceRoll = RollDice();
        UIManager.Instance.ShowTurnInfo($"{currentPlayer.name} 掷出了 {diceRoll} 点");
        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(PlayerManager.Instance.MovePlayer(currentPlayer, diceRoll));
        yield return StartCoroutine(tileEventManager.ProcessTileEvent(currentPlayer, currentPlayer.currentTileIndex));

        PlayerManager.Instance.SwitchPlayer();
        UIManager.Instance.ShowTurnInfo($"当前回合: {PlayerManager.Instance.GetCurrentPlayer().name}\n");
        PlayerManager.Instance.UpdatePlayerInfoText();
        rollDiceButton.interactable = true;
    }

    private int RollDice() => Random.Range(1, 7) + Random.Range(1, 7);
}
