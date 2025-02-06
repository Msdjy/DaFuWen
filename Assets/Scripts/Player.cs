using UnityEngine;
using TMPro;

public class Player
{
    public string name;
    public int money;
    public int currentTile;
    public GameObject avatar;
    // 新增一个引用，用于保存玩家头像上显示信息的 TextMeshPro 组件
    public TextMeshPro playerText;
}
