using UnityEngine;
using TMPro;

public class TileController : MonoBehaviour
{
    public Tile tileData;
    public int tileIndex;
    public int owner = -1;
    public TextMeshPro tileText;

    [HideInInspector]
    public TileEventManager tileEventManager;  // 引用 TileEventManager

    // 更新 Tile 显示的文字信息（例如名称及拥有者）
    public void UpdateTileText()
    {
        string ownerStr = owner < 0 ? "Unowned" : $"Player {owner + 1}";
        string tileInfo = tileData != null ? $"{tileData.name}\n{ownerStr}\n{tileData.level}\n{tileIndex}" : $"Index: {tileIndex}\n{ownerStr}";
        tileText.text = tileInfo;
    }

    // 升级城市
    public void UpgradeCity(Player player)
    {
        int upgradeCost = tileData.upgradeCosts[tileData.level];
        if (player.money >= upgradeCost)
        {
            player.money -= upgradeCost;
            tileData.level++;
            tileData.rent += tileData.upgradeRents[tileData.level - 1];
            UpdateTileText();
            Debug.Log($"{player.name} 升级了 {tileData.name} 到 {tileData.level} 级，当前租金：${tileData.rent}");

            UpdateCityModel(); // 更新城市模型
        }
    }

    // 更新城市模型，根据等级显示不同的城市预制体
    public void UpdateCityModel()
    {
        // 销毁现有的模型（如果存在） - 获取第三个子节点并销毁
        if (transform.childCount > 2) // 确保存在至少三个子节点
        {
            // 销毁第三个子节点，即城市模型
            Destroy(transform.GetChild(2).gameObject);
        }

        GameObject cityModel = null;

        // 根据等级选择相应的预制体
        switch (tileData.level)
        {
            case 1:
                cityModel = Instantiate(tileEventManager.cityLevel1Prefab, transform.position, Quaternion.identity, transform);
                break;
            case 2:
                cityModel = Instantiate(tileEventManager.cityLevel2Prefab, transform.position, Quaternion.identity, transform);
                break;
            case 3:
                cityModel = Instantiate(tileEventManager.cityLevel3Prefab, transform.position, Quaternion.identity, transform);
                break;
        }

        // 如果生成了模型，设置其缩放比例
        if (cityModel != null)
        {
            cityModel.transform.localScale = new Vector3(0.06f, 0.30f, 0.06f); // 设置缩放比例

            // 获取模型的 MeshRenderer 组件并设置颜色
            MeshRenderer renderer = cityModel.GetComponentInChildren<MeshRenderer>();
            if (renderer != null)
            {
                // 这里设置模型的颜色，可以替换为任何颜色
                renderer.material.color = owner == -1 ? Color.gray : tileEventManager.playerManager.players[owner].playerColor;
            }
        }

        // 显示等级信息
        Debug.Log($"{tileData.name} 等级：{tileData.level}");
    }
}
