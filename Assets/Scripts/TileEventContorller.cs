using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EventController : MonoBehaviour
{
    public Text eventInfoText;  // 用于显示事件信息

    // 处理正面事件
    public IEnumerator HandlePositiveEvent(Player player, int eventIndex)
    {
        switch (eventIndex)
        {
            case 0:
                eventInfoText.text = "\n发现新商路：恭喜你发现一条通往神秘地区的新商路，可抽取一张资源卡。";
                // 抽取资源卡逻辑...
                break;
            case 1:
                eventInfoText.text = "\n文化交流：你成功与当地商人进行了文化交流，他们对你的慷慨和见识表示赞赏，赠送你500金。";
                player.money += 500;
                break;
            case 2:
                eventInfoText.text = "\n获得资助：一位富有的贵族看中了你的商业潜力，决定资助你，获得 800 金币。";
                player.money += 800;
                break;
            case 3:
                eventInfoText.text = "\n发现遗迹：在探索途中，你发现了一处古老遗迹，从中找到珍贵文物，换回一个地皮（自选不大于500金的地皮）。";
                // 处理购买地皮逻辑...
                break;
            case 4:
                eventInfoText.text = "\n幸运之神眷顾：幸运之神降临，接下来三个回合，你掷骰子的点数 +1。";
                // 设置幸运加成的逻辑...
                break;
            case 5:
                eventInfoText.text = "\n丰收之年：途经的地区迎来丰收，你收购到大量低价优质资源，抽取两张资源卡。";
                // 抽取两张资源卡...
                break;
            case 6:
                eventInfoText.text = "\n学会新技术：在当地学习到一种独特的制作工艺，收益700金。";
                player.money += 700;
                break;
            case 7:
                eventInfoText.text = "\n获得推荐信：得到当地一位重要人物的推荐信，可以免费在自己的地皮上加盖商铺。";
                // 增加商铺逻辑...
                break;
        }
        yield return null;
    }

    // 处理负面事件
    public IEnumerator HandleNegativeEvent(Player player, int eventIndex)
    {
        switch (eventIndex)
        {
            case 0:
                eventInfoText.text = "\n遭遇风沙：不幸遭遇强烈风沙，暂停下一回合行动，且损失 200 金币用于清理货物和修复商队装备。";
                player.money -= 200;
                // 暂停回合的逻辑...
                break;
            case 1:
                eventInfoText.text = "\n强盗袭击：一群强盗抢走了你部分财物，损失 300 金币和一张资源卡。";
                player.money -= 300;
                // 扣除资源卡逻辑...
                break;
            case 2:
                eventInfoText.text = "\n迷路：在沙漠中迷失方向，后退三格，浪费一回合时间寻找方向。";
                // 退后三格的逻辑...
                break;
            case 3:
                eventInfoText.text = "\n疾病流行：所在地区疾病流行，你的商队人员受到影响，下一回合无法建造建筑，且需花费 200 金币治疗。";
                player.money -= 200;
                // 设置不能建造建筑的逻辑...
                break;
            case 4:
                eventInfoText.text = "\n税收增加：当地政府临时增加税收，缴纳当前所拥有金币的 5% 作为税款。";
                player.money -= Mathf.FloorToInt(player.money * 0.05f);
                break;
            case 5:
                eventInfoText.text = "\n竞争对手破坏：竞争对手暗中破坏你的生意，拆除你在当前城市一座等级最低的建筑。";
                // 拆除建筑逻辑...
                break;
        }
        yield return null;
    }
}
