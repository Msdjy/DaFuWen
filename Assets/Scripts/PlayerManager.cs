using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using System;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    public List<Player> players = new List<Player>();
    public int currentPlayerIndex = 0;
    public GameObject playerPrefab;
    public MapManager mapManager;

    
    public Text leftPlayerInfoText;
    public Text rightPlayerInfoText;


    #region Create 

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void InitializePlayers()
    {
        for (int i = 0; i < 2; i++)
        {
            players.Add(CreatePlayer(i));
        }
        UpdatePlayerInfoText();
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
        newPlayer.InitializeResources();

        Vector3 startPos = TileManager.Instance.GetTileDataPositionByIndex(newPlayer.currentTileIndex);
        GameObject avatar = Instantiate(playerPrefab, startPos, Quaternion.identity);
        newPlayer.avatar = avatar;

        SetAvatarColor(avatar, newPlayer);

        return newPlayer;
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

        // 生成玩家信息文本
        tmp.text = GeneratePlayerInfoText(player);
        player.playerText = tmp;
    }

    private void SetAvatarColor(GameObject avatar, Player player)
    {
        MeshRenderer avatarRenderer = avatar.GetComponentInChildren<MeshRenderer>();
        if (avatarRenderer != null)
        {
            avatarRenderer.material.color = player.playerColor;
        }
    }
    #endregion

    #region Turn
    public void SwitchPlayer()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
    }
    #endregion

    #region Move 
    public Player GetCurrentPlayer() => players[currentPlayerIndex];


    public IEnumerator MovePlayer(Player player, int steps)
    {
        yield return StartCoroutine(player.MoveTo(mapManager, steps)); // 调用Player类中的MoveTo方法
    }
    #endregion

    #region UI
    // 生成玩家的详细信息文本
    private string GeneratePlayerInfoText(Player player)
    {
        string playerInfo = $"{player.name}\nMoney: ${player.money}\n";
        foreach (var resource in player.resources)
        {
            playerInfo += $"{resource.Key}: {resource.Value.Count}\n";
        }
        return playerInfo;
    }

    public void UpdatePlayerInfoText()
    {
        // 更新左右玩家的资源信息
        string playerInfo1  = GeneratePlayerInfoText(players[0]);
        string playerInfo2 = GeneratePlayerInfoText(players[1]);

        ShowPlayerInfo(playerInfo1, playerInfo2, players[0].playerColor, players[1].playerColor);
    }
    #endregion


    #region Update
    // 根据index获取玩家
    public Player GetPlayerByIndex(int index) => players[index];
    // 玩家1为玩家2支付租金
    public void PayRent(int playerIndex1, int playerIndex2, int rent)
    {
        Player player1 = GetPlayerByIndex(playerIndex1);
        Player player2 = GetPlayerByIndex(playerIndex2);

        player1.SpendMoney(rent);
        player2.EarnMoney(rent);
        UpdatePlayerInfoText();
    }

    #endregion

    #region Money
    // 玩家X花费了钱
    public void SpendMoney(Player player, int amount)
    {
        player.SpendMoney(amount);
        UpdatePlayerInfoText();
    }
    // 玩家x获得了钱
    public void EarnMoney(Player player, int amount)
    {
        player.EarnMoney(amount);
        UpdatePlayerInfoText();
    }
    // 判断玩家钱是否够
    public bool CanPlayerAfford(Player player, int amount)
    {
        return player.money >= amount;
    }

    #endregion

    #region Resource
    // 玩家X获得资源
    public void AddResource(Player player, ResourceType type, int quantity)
    {
        player.AddResource(type, quantity);
    }
    // 玩家X失去资源
    public void RemoveResource(Player player, ResourceType type, int quantity)
    {
        player.RemoveResource(type, quantity);
    }
    // 玩家X随机失去资源
    public void RemoveRandomResource(Player player)
    {
        // 根据资源总数来计算概率
        if (player.resources.Sum(r => r.Value.Count) == 0) 
        {
            // show text
            // ShowEventInfo($"\n{player.name} 没有资源卡");
            return;
        }
        // 失去的资源type
        ResourceType randomType = player.resources.Keys.ElementAt(UnityEngine.Random.Range(0, player.resources.Count));
        // 此类资源失去一个
        player.RemoveResource(randomType, 1);
    }
    // 玩家X随机获得资源
    public void AddRandomResource(Player player)
    {
        // 不同type的资源获得概率相同
        ResourceType randomType = (ResourceType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(ResourceType)).Length);
        player.AddResource(randomType, 1);
    }
    // 获取玩家X某资源的数量
    public int GetResourceCount(Player player, ResourceType type)
    {
        return player.resources[type].Count;
    }
    // 获取玩家X所有资源的数量
    public int GetAllResourceCount(Player player)
    {
        return player.resources.Sum(r => r.Value.Count);
    }
    #endregion


    #region UI
    
    public void ShowPlayerInfo(string playerInfo1, string playerInfo2, Color playerColor1, Color playerColor2)
    {
        leftPlayerInfoText.text = playerInfo1;
        rightPlayerInfoText.text = playerInfo2;

        leftPlayerInfoText.color = playerColor1;
        rightPlayerInfoText.color = playerColor2;
    }
    #endregion
}
