using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class MapManager : MonoBehaviour
{
    #region Public Fields
    [Header("地图配置")]
    [Tooltip("将 JSON 地图配置文件拖拽到此处")]
    public TextAsset jsonConfig;

    
    [Tooltip("Tile 在 Y 轴上的偏移")]
    public float yOffset = 0f; // 没有用到
    #endregion


    // 最内圈坐标
    private float baseCoordinate = 8.5f;
    // 内圈坐标，基于最内圈往外延申位置，用于玩家模型的移动位置
    private float innerOffset = 0.5f;
    // 外圈坐标，基于最内圈往外延申，用于tile模型位置摆放
    private float outerOffset = 2.24f;



    #region Runtime Data
    [HideInInspector]
    public MapConfig config;

    #endregion

    #region Unity Methods
    void Start()
    {
        if (jsonConfig == null)
        {
            Debug.LogError("未指定配置文件！");
            return;
        }

        config = JsonUtility.FromJson<MapConfig>(jsonConfig.text);
        if (config == null)
        {
            Debug.LogError("解析配置文件失败！");
            return;
        }

        Debug.Log($"Rows: {config.rows}, Columns: {config.columns}, Map Count: {config.map.Count}");

        GenerateMap();

    }
    #endregion

    #region Map Generation
    /// <summary>
    /// 生成地图：只生成棋盘边缘的 Tile，
    /// 顺序：从右下角开始，沿边界顺时针遍历（角点只生成一次）。
    /// </summary>
    private void GenerateMap()
    {
        // 1. 底边：从右下角到左下角
        for (int c = config.columns - 1; c >= 0; c--)
        {
            CreateTile(config.rows - 1, c);
        }

        // 2. 左侧：从左下角到左上角
        for (int r = config.rows - 2; r >= 1; r--)
        {
            CreateTile(r, 0);
        }

        // 3. 顶边：从左上角到右上角
        for (int c = 0; c < config.columns; c++)
        {
            CreateTile(0, c);
        }

        // 4. 右侧：从右上角到右下角
        for (int r = 1; r < config.rows - 1; r++)
        {
            CreateTile(r, config.columns - 1);
        }
    }
    #endregion

    #region Tile Generation

    /// <summary>
    /// 根据行和列生成单个 Tile：
    /// 1. 计算基础内圈坐标和法向量；
    /// 2. 计算内圈（逻辑）位置 = BaseInner + normal * innerOffset；
    /// 3. 计算外圈（Cube）位置 = BaseInner + normal * outerOffset；
    /// 若为事件格，则只记录内圈信息，不生成 Cube。
    /// </summary>
    /// <param name="r">行号</param>
    /// <param name="c">列号</param>
    private void CreateTile(int r, int c)
    {
        int index = r * config.columns + c;
        int tileId = config.map[index];
        if (tileId == 0) return;

        // 确保 tiles 列表不为空
        if (config.tiles == null || config.tiles.Count == 0)
        {
            Debug.LogError("地图配置文件中的 tiles 数据为空！");
            return;
        }
        
        // 查找对应的 Tile 数据
        Tile tile = config.tiles.FirstOrDefault(t => t.id == tileId);
        if (tile == null)
        {
            Debug.LogError($"未找到与 Tile ID {tileId} 对应的 Tile 数据！");
            return;
        }

        // 分别计算内圈和外圈位置
        Vector3 innerPosition = GetInnerPosition(r, c);
        Vector3 outerPosition = GetOuterPosition(r, c);

        TileManager.Instance.RegisterTile(tile, innerPosition, outerPosition);  // 将Tile注册到TileManager

    }

    #endregion


    #region Position Calculation

    private Vector3 GetInnerPosition(int r, int c)
    {
        Vector3 baseInner, normal;
        CalculateBaseInnerAndNormal(r, c, out baseInner, out normal);

        return baseInner + normal * innerOffset;
    }

    private Vector3 GetOuterPosition(int r, int c)
    {
        Vector3 baseInner, normal;
        CalculateBaseInnerAndNormal(r, c, out baseInner, out normal);
        return baseInner + normal * outerOffset;
    }

    private void CalculateBaseInnerAndNormal(int r, int c, out Vector3 baseInner, out Vector3 normal)
    {
        baseInner = Vector3.zero;
        normal = Vector3.zero;

        bool isTop = (r == 0);
        bool isBottom = (r == config.rows - 1);
        bool isLeft = (c == 0);
        bool isRight = (c == config.columns - 1);

        if (isBottom && isLeft)
        {
            baseInner = new Vector3(-baseCoordinate, yOffset, -baseCoordinate);
            normal = new Vector3(-1, 0, -1);
        }
        else if (isBottom && isRight)
        {
            baseInner = new Vector3(baseCoordinate, yOffset, -baseCoordinate);
            normal = new Vector3(1, 0, -1);
        }
        else if (isTop && isLeft)
        {
            baseInner = new Vector3(-baseCoordinate, yOffset, baseCoordinate);
            normal = new Vector3(-1, 0, 1);
        }
        else if (isTop && isRight)
        {
            baseInner = new Vector3(baseCoordinate, yOffset, baseCoordinate);
            normal = new Vector3(1, 0, 1);
        }
        else if (isBottom)
        {
            baseInner.x = Mathf.Lerp(-baseCoordinate, baseCoordinate, (float)c / (config.columns - 1));
            baseInner.z = -baseCoordinate;
            baseInner.y = yOffset;
            normal = new Vector3(0, 0, -1);
        }
        else if (isTop)
        {
            baseInner.x = Mathf.Lerp(-baseCoordinate, baseCoordinate, (float)c / (config.columns - 1));
            baseInner.z = baseCoordinate;
            baseInner.y = yOffset;
            normal = new Vector3(0, 0, 1);
        }
        else if (isLeft)
        {
            baseInner.x = -baseCoordinate;
            baseInner.z = Mathf.Lerp(baseCoordinate, -baseCoordinate, (float)r / (config.rows - 1));
            baseInner.y = yOffset;
            normal = new Vector3(-1, 0, 0);
        }
        else if (isRight)
        {
            baseInner.x = baseCoordinate;
            baseInner.z = Mathf.Lerp(baseCoordinate, -baseCoordinate, (float)r / (config.rows - 1));
            baseInner.y = yOffset;
            normal = new Vector3(1, 0, 0);
        }
    }
    #endregion
}
