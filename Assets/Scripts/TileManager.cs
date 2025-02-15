using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class TileManager : MonoBehaviour
{
    public static TileManager Instance;

    [Tooltip("用于生成 Tile 的预制体（内容为 Cube）")]
    public GameObject tilePrefab;

    [Tooltip("用于生成 Tile之上城市等级 的预制体（内容为 House Model）")]
    public GameObject cityLevel1Prefab;
    public GameObject cityLevel2Prefab;
    public GameObject cityLevel3Prefab;


    private List<TileData> tileDatas = new List<TileData>();

    #region Unity Methods
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
    #endregion


    #region Tiles create
    public void RegisterTile(Tile tile, Vector3 innerPosition, Vector3 outerPosition)
    {
        GameObject tileObj = Instantiate(tilePrefab, outerPosition, Quaternion.identity);
        tileObj.transform.localScale *= 1.44f;

        TileData newTileData = new TileData
        {
            tileObject = tileObj,
            tile = tile,
            innerPosition = innerPosition,
            index = tile.id - 1
        };

        tileDatas.Add(newTileData);
        Debug.Log("生成 Tile: " + (tile != null ? tile.name : tile.id.ToString()));
        Debug.Log($"tile: id={tile.id}, name={tile.name}, type={tile.type}, price={tile.price}");


        // 设置 Tile 的颜色
        SetTileColor(tileObj, tile);
    }

    private void SetTileColor(GameObject tileObj, Tile tile)
    {
        MeshRenderer renderer = tileObj.GetComponentInChildren<MeshRenderer>();
        if (renderer != null)
        {
            Color tileColor = Color.white;
            switch (tile.type)
            {
                case "event":
                    tileColor = Color.blue;
                    break;
                case "start":
                    tileColor = Color.yellow;
                    break;
                case "market":
                    tileColor = Color.green;
                    break;
                case "pirate":
                    tileColor = Color.black;
                    break;
                case "villager":
                    tileColor = Color.cyan;
                    break;
                case "resource":
                    tileColor = Color.magenta;
                    break;
            }
            renderer.material.color = tileColor;
        }
    }
    #endregion

    #region Tile get
    // 获取指定 Tile
    public TileData GetTileDataById(int tileId)
    {
        return tileDatas.Find(t => t.tile.id == tileId);
    }

    public TileData GetTileDataByIndex(int index)
    {
        return tileDatas.Find(t => t.index == index);
    }
    public Vector3 GetTileDataPositionByIndex(int index)
    {
        return tileDatas.Find(t => t.index == index).innerPosition;
    }
    public int GetTilesCount()
    {
        return tileDatas.Count;
    }
    #endregion


    #region Tile process
    // tile升级
    public void UpgradeTile(Tile tile)
    {
        if (tile.CanUpgrade())
        {
            tile.Upgrade();
            // TODO tile model 升级
            UpgradeCityModel(tile);
            Debug.Log($"Tile {name} 升级到了 {tile.level} 级");
        }
        else
        {
            Debug.Log($"Tile {name} 已经达到最高级别，无法再升级");
        }
    }
    // tile 玩家拥有
    public void ownerTile(TileData tileData, int playerIndex, Color playerColor)
    {
        tileData.tile.updateOwner(playerIndex);

        // 染色
        MeshRenderer cubeRenderer = tileData.tileObject.GetComponentInChildren<MeshRenderer>();
        if (cubeRenderer != null)
        {
            cubeRenderer.material.color = playerColor;
        }

        Debug.Log($"Tile {name} 被玩家 {playerIndex} 购买");
    }

    // 根据城市的等级选择相应的城市模型，并将其放置在 Tile 上
    private void UpgradeCityModel(Tile tile)
    {
        // 查找该 Tile 上已存在的城市模型并销毁它
        TileData tileData = GetTileDataById(tile.id);
        if (tileData != null)
        {
            Transform existingCity = tileData.tileObject.transform.Find("CityModel");
            if (existingCity != null)
            {
                Destroy(existingCity.gameObject); // 销毁旧的城市模型
            }
        }

        // 根据城市等级选择对应的预制体
        GameObject cityPrefab = null;
        switch (tile.level)
        {
            case 1:
                cityPrefab = cityLevel1Prefab;
                break;
            case 2:
                cityPrefab = cityLevel2Prefab;
                break;
            case 3:
                cityPrefab = cityLevel3Prefab;
                break;
            default:
                Debug.LogWarning("未知的城市等级: " + tile.level);
                return;
        }

        // 在 Tile 上生成新的城市模型
        if (cityPrefab != null)
        {
            GameObject cityObj = Instantiate(cityPrefab, tileData.tileObject.transform.position, Quaternion.identity);
            cityObj.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f); // 设置缩放比例
            // 获取模型的 MeshRenderer 组件并设置颜色
            MeshRenderer renderer = cityObj.GetComponentInChildren<MeshRenderer>();
            if (renderer != null)
            {
                MeshRenderer cubeRenderer = tileData.tileObject.GetComponentInChildren<MeshRenderer>();
                // 这里设置模型的颜色，可以替换为任何颜色
                renderer.material.color = cubeRenderer.material.color;
            }
            cityObj.name = "CityModel"; // 设置城市模型的名称，便于查找和销毁
            cityObj.transform.parent = tileData.tileObject.transform; // 将城市模型作为 Tile 的子物体
        }
    }

    #endregion

    #region test 
    public IEnumerator AutoBuyAndUpgradeCity(Player player, int tileIndex){
        // 自动购买和升级城市
        TileData tileData = GetTileDataByIndex(tileIndex);
        if (tileData.tile.CanPurchase())
        {
            ownerTile(tileData, player.playerIndex, player.playerColor);
        }
        
        // 升级城市三次
        for (int i = 0; i < 3; i++)
        {
            if (tileData.tile.CanUpgrade())
            {
                UpgradeTile(tileData.tile);
                yield return new WaitForSeconds(1);
            }
        }
        yield break; 

    }
    
    #endregion
}


// Tile 数据结构
public class TileData
{
    public GameObject tileObject;
    public Tile tile;
    public Vector3 innerPosition;
    public int index;

}
