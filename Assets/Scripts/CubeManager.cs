using System.Collections.Generic;
using UnityEngine;
using System.Linq; // 用于 List 查找
using TMPro;

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
                tc.tileIndex = tileId;    // 此格子在数组中的索引（顺序决定玩家走的路径）

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
                }

                Debug.Log("生成 tile : " + tileData.name);

                // 添加子物体来显示文字
                CreateTextForCube(cube, tileData, tileId);
            }
        }
    }

    /// <summary>
    /// 在 Cube 上添加一个 TextMesh 用于显示 tile 的 name、索引和拥有者信息
    /// </summary>
    /// <param name="cube">Cube 实例</param>
    /// <param name="tileData">对应的 tile 数据</param>
    /// <param name="index">该 Cube 的索引</param>
    void CreateTextForCube(GameObject cube, Tile tileData, int index)
    {
        // 创建一个空的子物体
        GameObject textObj = new GameObject("TileText");
        // 将其设为 cube 的子物体
        textObj.transform.SetParent(cube.transform);
        // 使文本位于 Cube 正上方（根据需要调整偏移量）
        textObj.transform.localPosition = new Vector3(0, 1.5f, 0);
        
        // 旋转使得文字朝上（文字的 forward 指向全局 Y 轴正方向）
        textObj.transform.localRotation = Quaternion.Euler(90, 0, 0);
        // 设置局部缩放为均匀（防止文字被拉伸）
        textObj.transform.localScale = Vector3.one;

        // 添加 TextMeshPro 组件
        TextMeshPro textMeshPro = textObj.AddComponent<TextMeshPro>();
        // 初始时显示 tile 名称和拥有者信息（无人拥有时显示 Unowned）
        if (tileData != null)
        {
            textMeshPro.text = $"{tileData.name}\nUnowned";
        }
        else
        {
            textMeshPro.text = $"Index: {index}\nUnowned";
        }
        
        // 设置其他文本属性，根据需要调整
        textMeshPro.fontSize = 4;
        textMeshPro.alignment = TextAlignmentOptions.Center;
        textMeshPro.color = Color.black;

        // 将创建的文字组件赋值给 TileController 的 tileText 字段
        TileController tc = cube.GetComponent<TileController>();
        if (tc != null)
        {
            tc.tileText = textMeshPro;
            tc.UpdateTileText(); // 更新文字内容（Owner信息会根据 tc.owner 显示）
        }
    }


}
