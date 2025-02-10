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
    [Tooltip("用于生成 Tile 的预制体（内容为 Cube）")]
    public GameObject tilePrefab;
    [Tooltip("Tile 之间的间距（此参数不再直接用于位置计算）")]
    public float spacing = 2.0f;
    [Tooltip("Tile 在 Y 轴上的偏移")]
    public float yOffset = 0f;
    #endregion

    #region Runtime Data
    [HideInInspector]
    public MapConfig config;
    [HideInInspector]
    public List<int> tileIds = new List<int>();
    /// <summary>
    /// 内圈（逻辑）位置（延法向量 1 个单位后的位置）
    /// </summary>
    [HideInInspector]
    public List<Vector3> tilePositions = new List<Vector3>();
    #endregion

    #region Unity Methods
    void Start()
    {
        if (jsonConfig == null)
        {
            Debug.LogError("未指定 JSON 配置文件！");
            return;
        }
        if (tilePrefab == null)
        {
            Debug.LogError("未指定 Tile 预制体！");
            return;
        }

        // 解析 JSON 配置文件
        config = JsonUtility.FromJson<MapConfig>(jsonConfig.text);
        if (config == null)
        {
            Debug.LogError("解析 JSON 配置文件失败！");
            return;
        }

        Debug.Log($"Rows: {config.rows}, Columns: {config.columns}, Map Count: {config.map.Count}");

        tileIds.Clear();
        tilePositions.Clear();

        GenerateMap();

        Debug.Log("Tile IDs: " + string.Join(", ", tileIds));
        Debug.Log("Tile Inner Positions: " + string.Join(", ", tilePositions.Select(p => p.ToString()).ToArray()));
    }
    #endregion

    #region Map Generation
    /// <summary>
    /// 生成地图：只生成棋盘边缘的 Tile，
    /// 顺序：从右下角开始，沿边界顺时针遍历（角点只生成一次）。
    /// </summary>
    void GenerateMap()
    {
        // 1. 底边：从右下角到左下角
        for (int c = config.columns - 1; c >= 0; c--)
        {
            ProcessTile(config.rows - 1, c);
        }

        // 2. 左侧：从左下角到左上角
        for (int r = config.rows - 2; r >= 1; r--)
        {
            ProcessTile(r, 0);
        }

        // 3. 顶边：从左上角到右上角
        for (int c = 0; c < config.columns; c++)
        {
            ProcessTile(0, c);
        }

        // 4. 右侧：从右上角到右下角
        for (int r = 1; r < config.rows - 1; r++)
        {
            ProcessTile(r, config.columns - 1);
        }
    }


    #endregion

    #region Tile Processing

    /// <summary>
    /// 根据行和列生成单个 Tile：
    /// 1. 计算基础内圈坐标和法向量；
    /// 2. 计算内圈（逻辑）位置 = BaseInner + normal * innerOffset；
    /// 3. 计算外圈（Cube）位置 = BaseInner + normal * outerOffset；
    /// 若为事件格，则只记录内圈信息，不生成 Cube。
    /// </summary>
    /// <param name="r">行号</param>
    /// <param name="c">列号</param>
    void ProcessTile(int r, int c)
    {
        int index = r * config.columns + c;
        int tileId = config.map[index];
        if (tileId == 0)
            return;

        // 分别计算内圈和外圈位置
        Vector3 innerPosition = GetInnerPosition(r, c);
        // 记录内圈位置
        tileIds.Add(tileId);
        tilePositions.Add(innerPosition);

        // 查找对应的 Tile 数据
        Tile tileData = config.tiles.FirstOrDefault(t => t.id == tileId);
        if (tileData != null && (tileData.type == "event" || tileData.type == "Start"))
        {
            Debug.Log("跳过事件格子: " + tileData.name);
            return;
        }

        Vector3 outerPosition = GetOuterPosition(r, c);
        Debug.Log("外圈位置 Y 坐标: " + outerPosition.y);

        // 实例化 Cube，放置在外圈位置并缩放
        GameObject tileObj = Instantiate(tilePrefab, outerPosition, Quaternion.identity);
        tileObj.transform.localScale *= 1.44f;

        TileController tc = tileObj.AddComponent<TileController>();
        tc.tileData = tileData;
        tc.tileIndex = tileId;

        if (tileData != null)
        {
            tileObj.name = tileData.name;
            float colorValue = Mathf.InverseLerp(400, 1000, tileData.price);
            Renderer rend = tileObj.GetComponent<Renderer>();
            if (rend != null)
                rend.material.color = Color.Lerp(Color.green, Color.red, colorValue);
        }
        
        Debug.Log("生成 Tile: " + (tileData != null ? tileData.name : tileId.ToString()));
        Debug.Log($"tileData: id={tileData.id}, name={tileData.name}, type={tileData.type}, price={tileData.price}");

        CreateTileText(tileObj, tileData, tileId);
    }

    /// <summary>
    /// 根据行和列计算基础内圈坐标和法向量。
    /// 边界采用 -8.5 到 8.5 为固定值（非角则使用插值）。
    /// </summary>
    void CalculateBaseInnerAndNormal(int r, int c, out Vector3 baseInner, out Vector3 normal)
    {
        baseInner = Vector3.zero;
        normal = Vector3.zero;

        bool isTop = (r == 0);
        bool isBottom = (r == config.rows - 1);
        bool isLeft = (c == 0);
        bool isRight = (c == config.columns - 1);

        if (isBottom && isLeft)
        {
            baseInner = new Vector3(-8.5f, yOffset, -8.5f);
            normal = new Vector3(-1, 0, -1);
        }
        else if (isBottom && isRight)
        {
            baseInner = new Vector3(8.5f, yOffset, -8.5f);
            normal = new Vector3(1, 0, -1);
        }
        else if (isTop && isLeft)
        {
            baseInner = new Vector3(-8.5f, yOffset, 8.5f);
            normal = new Vector3(-1, 0, 1);
        }
        else if (isTop && isRight)
        {
            baseInner = new Vector3(8.5f, yOffset, 8.5f);
            normal = new Vector3(1, 0, 1);
        }
        else if (isBottom)
        {
            baseInner.x = Mathf.Lerp(-8.5f, 8.5f, (float)c / (config.columns - 1));
            baseInner.z = -8.5f;
            baseInner.y = yOffset;
            normal = new Vector3(0, 0, -1);
        }
        else if (isTop)
        {
            baseInner.x = Mathf.Lerp(-8.5f, 8.5f, (float)c / (config.columns - 1));
            baseInner.z = 8.5f;
            baseInner.y = yOffset;
            normal = new Vector3(0, 0, 1);
        }
        else if (isLeft)
        {
            baseInner.x = -8.5f;
            baseInner.z = Mathf.Lerp(8.5f, -8.5f, (float)r / (config.rows - 1));
            baseInner.y = yOffset;
            normal = new Vector3(-1, 0, 0);
        }
        else if (isRight)
        {
            baseInner.x = 8.5f;
            baseInner.z = Mathf.Lerp(8.5f, -8.5f, (float)r / (config.rows - 1));
            baseInner.y = yOffset;
            normal = new Vector3(1, 0, 0);
        }
    }

    /// <summary>
    /// 计算内圈（逻辑）位置：基础内圈坐标 + normal * innerOffset
    /// </summary>
    Vector3 GetInnerPosition(int r, int c)
    {
        Vector3 baseInner, normal;
        CalculateBaseInnerAndNormal(r, c, out baseInner, out normal);
        float innerOffset = 0.5f;
        return baseInner + normal * innerOffset;
    }

    /// <summary>
    /// 计算外圈（Cube）位置：基础内圈坐标 + normal * outerOffset
    /// </summary>
    Vector3 GetOuterPosition(int r, int c)
    {
        Vector3 baseInner, normal;
        CalculateBaseInnerAndNormal(r, c, out baseInner, out normal);
        float outerOffset = 2.24f;
        return baseInner + normal * outerOffset;
    }
    #endregion

    #region UI Methods
    /// <summary>
    /// 在 Cube 上添加 TextMeshPro 子物体，用于显示 Tile 的名称和状态（例如 Unowned）
    /// </summary>
    void CreateTileText(GameObject tileObj, Tile tileData, int index)
    {
        GameObject textObj = new GameObject("TileText");
        textObj.transform.SetParent(tileObj.transform);
        textObj.transform.localPosition = new Vector3(0, 1.5f, 0);
        textObj.transform.localRotation = Quaternion.Euler(90, 0, 0);
        textObj.transform.localScale = Vector3.one;

        TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
        tmp.text = tileData != null ? $"{tileData.name}\nUnowned" : $"Index: {index}\nUnowned";
        tmp.fontSize = 4;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.black;

        TileController tc = tileObj.GetComponent<TileController>();
        if (tc != null)
        {
            tc.tileText = tmp;
            tc.UpdateTileText();
        }
    }
    #endregion
}
