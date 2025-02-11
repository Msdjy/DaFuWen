using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public List<Player> players = new List<Player>();
    public int currentPlayerIndex = 0;
    public GameObject playerPrefab;
    public MapManager mapManager;

    public Text leftPlayerInfoText;
    public Text rightPlayerInfoText;

    public void CreatePlayers()
    {
        for (int i = 0; i < 2; i++)
        {
            players.Add(CreatePlayer(i));
        }
    }

    private Player CreatePlayer(int index)
    {
        Player newPlayer = new Player
        {
            name = $"Player {index + 1}",
            money = 15000,
            currentTileIndex = 0,
            playerColor = (index == 0) ? Color.red : Color.green
        };

        Vector3 startPos = GetTilePosition(newPlayer.currentTileIndex);
        GameObject avatar = Instantiate(playerPrefab, startPos, Quaternion.identity);
        newPlayer.avatar = avatar;

        SetAvatarColor(avatar, newPlayer);
        CreatePlayerText(avatar, newPlayer);

        return newPlayer;
    }

    Vector3 GetTilePosition(int tileIndex)
    {
        if (mapManager == null || mapManager.tilePositions == null || mapManager.tilePositions.Count == 0)
        {
            Debug.LogError("MapManager 引用或 tilePositions 未初始化！");
            return Vector3.zero;
        }

        if (tileIndex < 0 || tileIndex >= mapManager.tilePositions.Count)
        {
            Debug.LogError($"Tile index {tileIndex} 超出范围！");
            return mapManager.tilePositions[0];
        }

        return mapManager.tilePositions[tileIndex];
    }

    void SetAvatarColor(GameObject avatar, Player player)
    {
        MeshRenderer avatarRenderer = avatar.GetComponentInChildren<MeshRenderer>();
        if (avatarRenderer != null)
        {
            avatarRenderer.material.color = player.playerColor;
        }
    }

    private void CreatePlayerText(GameObject avatar, Player player)
    {
        GameObject textObj = new GameObject("PlayerInfo");
        textObj.transform.SetParent(avatar.transform);
        textObj.transform.localPosition = new Vector3(0, 2.5f, 0);
        textObj.transform.localRotation = Quaternion.Euler(90, 0, 0);
        textObj.transform.localScale = Vector3.one;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = $"{player.name}\nMoney: ${player.money}";
        tmp.fontSize = 14;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = player.playerColor;

        player.playerText = tmp;
    }

    public void UpdatePlayerInfoText()
    {
        leftPlayerInfoText.text = $"{players[0].name}\nMoney: ${players[0].money}";
        rightPlayerInfoText.text = $"{players[1].name}\nMoney: ${players[1].money}";

        leftPlayerInfoText.color = players[0].playerColor;
        rightPlayerInfoText.color = players[1].playerColor;
    }

    public void SwitchPlayer()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
    }
}
