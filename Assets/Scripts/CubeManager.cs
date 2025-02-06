using System.Collections.Generic;
using UnityEngine;
using System.Linq; // 用于 List 查找

// 注意：由于 JsonUtility 不直接支持嵌套数组，这里将二维数组转换成一维列表，并保存行数和列数信息
[System.Serializable]
public class MapConfig
{
    public List<int> map; // 将二维数组按行合并成一维数组
    public int rows;      // 行数
    public int columns;   // 列数
    public List<Tile> tiles;
}

[System.Serializable]
public class Tile
{
    public int id;
    public string name;
    public int price;
    public int rent;
    public int owner;
    public int level;
    public string desc;
}

public class CubeManager : MonoBehaviour
{
    // 将 JSON 配置文件以 TextAsset 方式拖拽到 Inspector 中
    public TextAsset jsonConfig;
    // 预制体引用（预制体内容为 Cube）
    public GameObject cubePrefab;
    // 每个 Cube 之间的间距
    public float spacing = 2.0f;
    // Cube 的 y 轴高度
    public float yOffset = 0f;

    private MapConfig config;

    void Start()
    {
        if (jsonConfig == null)
        {
            Debug.LogError("未指定 JSON 配置文件！");
            return;
        }

        if (cubePrefab == null)
        {
            Debug.LogError("未指定 Cube 预制体！");
            return;
        }

        // 解析 JSON 配置文件（请确保 JSON 文件中 map 为一维数组，同时包含 rows 和 columns 字段）
        config = JsonUtility.FromJson<MapConfig>(jsonConfig.text);
        if (config == null)
        {
            Debug.LogError("解析 JSON 配置文件失败！");
            return;
        }

        // 输出调试信息
        Debug.Log("Rows: " + config.rows + ", Columns: " + config.columns + ", Map count: " + config.map.Count);

        // 计算中心偏移量，使得地图中心在 (0, 0, 0)
        float offsetX = (config.columns - 1) / 2.0f;
        float offsetZ = (config.rows - 1) / 2.0f;

        // 根据一维数组的索引计算二维坐标
        for (int r = 0; r < config.rows; r++)
        {
            for (int c = 0; c < config.columns; c++)
            {
                int index = r * config.columns + c;
                int tileId = config.map[index];

                // 如果 tileId 为 0，则表示该位置没有对应的地图点（例如空白或不可用区域）
                if (tileId == 0)
                    continue;

                // 计算 Cube 的位置
                float posX = (c - offsetX) * spacing;
                float posZ = (r - offsetZ) * spacing;
                Vector3 position = new Vector3(posX, yOffset, posZ);

                // 使用预制体实例化 Cube
                GameObject cube = Instantiate(cubePrefab, position, Quaternion.identity);

                // 通过 tileId 查找对应的 tile 信息
                Tile tileData = config.tiles.FirstOrDefault(t => t.id == tileId);

                // 为了方便后续在游戏中查找格子，添加 TileController 组件
                TileController tc = cube.AddComponent<TileController>();
                tc.tileData = tileData;  // 来自 JSON 的数据
                tc.tileIndex = index;    // 此格子在数组中的索引（顺序决定玩家走的路径）
                
                if (tileData != null)
                {
                    // 设置 Cube 的名称为 tile 名称
                    cube.name = tileData.name;

                    // 根据 tile 价格设置颜色深浅（仅为示例）
                    float colorValue = Mathf.InverseLerp(400, 1000, tileData.price);
                    Renderer rend = cube.GetComponent<Renderer>();
                    if (rend != null)
                    {
                        rend.material.color = Color.Lerp(Color.green, Color.red, colorValue);
                    }
                    Debug.Log("生成 tile : " + tileData.name);
                    // 可以在此处为 Cube 添加其它逻辑或组件，例如点击事件等
                }
            }
        }
    }
}
