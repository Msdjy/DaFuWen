using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using System;
using System.Linq.Expressions;
using UnityEngine.Animations;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    public List<Player> players = new List<Player>();

    // 四个颜色
    public Color[] playerColors = new Color[4]
    {
        // 依赖编译器中设置的颜色，这里设置的没用
        new Color(1f, 0.647f, 0.0f),  // 橙色 (RGB: 1, 0.647, 0)
        new Color(0.6f, 0.3f, 0.1f),  // 棕色 (RGB: 0.6, 0.3, 0.1)
        new Color(0.5f, 0.5f, 0.5f),  // 灰色 (RGB: 0.5, 0.5, 0.5)
        new Color(0.8f, 0.2f, 0.2f)   // 红色 (RGB: 0.8, 0.2, 0.2)
    };


    public int currentPlayerIndex = 0;

    public GameObject[] playerPrefabs = new GameObject[4];
    public TextMeshProUGUI[] PlayerGoldInfoTexts = new TextMeshProUGUI[4];
    public TextMeshProUGUI[] PlayerResourceInfoTexts = new TextMeshProUGUI[4];
    public TextMeshProUGUI[] PlayerHousePriceInfoTexts = new TextMeshProUGUI[4];




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
        int playerCount = 4;
        for (int i = 0; i < playerCount; i++)
        {
            players.Add(CreatePlayer(i));
        }
        ShowPlayerInfo();
    }

    private Player CreatePlayer(int index)
    {
        Player newPlayer = new Player($"Player {index + 1}", playerColors[index], playerPrefabs[index]);

        Vector3 startPos = TileManager.Instance.GetTileDataPositionByIndex(newPlayer.currentTileIndex);
        newPlayer.avatar = Instantiate(playerPrefabs[index], startPos, Quaternion.identity);
        newPlayer.SetAvatarColor(newPlayer.avatar, newPlayer);

        // 查找所有子对象的 Animator 组件
        Animator[] animators = newPlayer.avatar.GetComponentsInChildren<Animator>();

        if (animators.Length > 0)
        {
            newPlayer.animator = animators[0]; // 假设第一个 Animator 就是我们需要的
        }
        else
        {
            Debug.LogError("No Animator found in the avatar's children!");
        }


        return newPlayer;
    }


    #endregion

    #region Turn
    public void SwitchPlayer()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
    }
    #endregion

    #region Move 

    public IEnumerator MovePlayer(Player player, int steps)
    {
        player.animator.SetBool("isMoving", true); // 开始走路动画
        yield return StartCoroutine(player.MoveTo(steps)); // 调用Player类中的MoveTo方法
        player.animator.SetBool("isMoving", false); // 停止走路动画，切换回空闲状态
    }
    #endregion


    #region Get Player
    public Player GetCurrentPlayer() => players[currentPlayerIndex];
    // 根据index获取玩家
    public Player GetPlayerByIndex(int index) => players[index];
    #endregion

    #region Try buy or upgrade
    public bool trySpendMonryForCity(Player player, int cost)
    {
        if (CanPlayerAfford(player, cost))
        {
            SpendMoney(player, cost);
            AddHousePrice(player, cost / 2);
            return true;
        }
        return false;
    } 

    #endregion

    #region Money
    // 玩家1为玩家2支付租金
    public void PayRent(int playerIndex1, int playerIndex2, int rent)
    {
        Player player1 = GetPlayerByIndex(playerIndex1);
        Player player2 = GetPlayerByIndex(playerIndex2);

        SpendMoney(player1, rent);
        EarnMoney(player2, rent);

        ShowPlayerInfo();
    }
    // 玩家X花费了钱
    public void SpendMoney(Player player, int amount)
    {
        Debug.Log($"{player.name} 的金钱（支付前）：{player.money}");
        player.SpendMoney(amount);
        Debug.Log($"{player.name} 的金钱（支付后）：{player.money}");
        ShowPlayerInfo();
    }
    // 玩家x获得了钱
    public void EarnMoney(Player player, int amount)
    {
        Debug.Log($"{player.name} 的金钱（收取前）：{player.money}");
        player.EarnMoney(amount);
        Debug.Log($"{player.name} 的金钱（收取后）：{player.money}");
        ShowPlayerInfo();
    }
    // 判断玩家钱是否够
    public bool CanPlayerAfford(Player player, int amount)
    {
        return player.money >= amount;
    }

    #endregion

    #region House Price
    // 玩家X的房产价值
    public void AddHousePrice(Player player, float housePrice)
    {
        player.housePrice += housePrice;
        ShowPlayerInfo();
    }

    #endregion

    #region Resource
    // 玩家X获得资源
    public void AddResource(Player player, ResourceType type, int quantity)
    {
        player.AddResource(type, quantity);
        ShowPlayerInfo();
    }
    // 玩家X失去资源
    public void RemoveResource(Player player, ResourceType type, int quantity)
    {
        player.RemoveResource(type, quantity);
        ShowPlayerInfo();
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
        ShowPlayerInfo();
    }
    // 玩家X随机获得资源
    public void AddRandomResource(Player player)
    {
        // 不同type的资源获得概率相同
        ResourceType randomType = (ResourceType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(ResourceType)).Length);
        player.AddResource(randomType, 1);
        ShowPlayerInfo();
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
    // 生成玩家的详细信息文本
    public void ShowPlayerInfo()
    {   
        for (int i = 0; i < players.Count; i++)
        {
            PlayerGoldInfoTexts[i].text = $"{players[i].money}";
            PlayerResourceInfoTexts[i].text = $"{GetAllResourceCount(players[i])}";
            PlayerHousePriceInfoTexts[i].text = $"{players[i].housePrice}";
        }
    }
    #endregion
}
