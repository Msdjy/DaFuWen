using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    public TextMeshProUGUI turnInfoText;
    public GameObject purchasePanel;
    public TextMeshProUGUI purchasePanelText;
    public TextMeshProUGUI eventInfoText;

    public Text leftPlayerInfoText;
    public Text rightPlayerInfoText;

    private void Awake()
    {
        Instance = this;
        purchasePanel.SetActive(false);
    }

    public void ShowTurnInfo(string message)
    {
        turnInfoText.text = message;
    }

    public void ShowEventInfo(string message)
    {
        eventInfoText.text = message;
    }

    public void ShowPlayerInfo(string playerInfo1, string playerInfo2, Color playerColor1, Color playerColor2)
    {
        leftPlayerInfoText.text = playerInfo1;
        rightPlayerInfoText.text = playerInfo2;

        leftPlayerInfoText.color = playerColor1;
        rightPlayerInfoText.color = playerColor2;
    }

    public void ShowPurchasePanelText(string message)
    {
        purchasePanelText.text = message;
    }

    public void SetPurchasePanelActive(bool active)
    {
        purchasePanel.SetActive(active);
    }
}
