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
    public GameObject playerPrefab;
    public Text infoText;
    public Button rollDiceButton;



    public List<TileController> boardTiles = new List<TileController>();

    IEnumerator Start()
    {
        yield return null;
        boardTiles = GetSortedTileControllers();
        if (boardTiles.Count == 0)
        {
            Debug.LogError("未在地图上找到任何 Tile，请检查 MapManager 是否正常生成！");
            yield break;
        }

        rollDiceButton.onClick.AddListener(() => StartCoroutine(TakeTurn()));

        playerManager.CreatePlayers();
        tileEventManager.gameManager = this;
        tileEventManager.playerManager = playerManager;

        playerManager.UpdateInfoText();
    }

    List<TileController> GetSortedTileControllers()
    {
        return FindObjectsOfType<TileController>().OrderBy(t => t.tileIndex).ToList();
    }

    IEnumerator TakeTurn()
    {
        rollDiceButton.interactable = false;
        Player currentPlayer = playerManager.players[playerManager.currentPlayerIndex];
        int diceRoll = RollDice();
        infoText.text = $"{currentPlayer.name} 掷出了 {diceRoll} 点";
        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(MovePlayer(currentPlayer, diceRoll));

        yield return StartCoroutine(tileEventManager.ProcessTileEvent(currentPlayer, currentPlayer.currentTileIndex));

        playerManager.SwitchPlayer();
        playerManager.UpdateInfoText();
        rollDiceButton.interactable = true;
    }

    int RollDice()
    {
        return Random.Range(1, 7) + Random.Range(1, 7);
    }
    

    IEnumerator MovePlayer(Player player, int steps)
    {
        for (int i = 0; i < steps; i++)
        {
            player.currentTileIndex = (player.currentTileIndex + 1) % mapManager.tilePositions.Count;
            Vector3 targetPos = mapManager.tilePositions[player.currentTileIndex];
            while (Vector3.Distance(player.avatar.transform.position, targetPos) > 0.1f)
            {
                player.avatar.transform.position = Vector3.MoveTowards(player.avatar.transform.position, targetPos, Time.deltaTime * 5f);
                yield return null;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
}
