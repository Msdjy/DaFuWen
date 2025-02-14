using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    public TextMeshProUGUI infoText;
    public GameObject purchasePanel;
    public TextMeshProUGUI purchasePanelText;
    public TextMeshProUGUI eventInfoText;
    public Button buyButton;
    public Button skipButton;

    private System.Action<bool> purchaseCallback;

    private void Awake()
    {
        Instance = this;
        purchasePanel.SetActive(false);
    }

    public void UpdatePlayerInfo(Player player)
    {
        string info = $"{player.name}\nMoney: ${player.money}\n";
        foreach (var resource in player.resources)
        {
            info += $"{resource.Key}: {resource.Value.Count}\n";
        }
        infoText.text = info;
    }

    public void ShowPurchasePanel(string message, System.Action<bool> callback)
    {
        purchasePanelText.text = message;
        purchasePanel.SetActive(true);
        purchaseCallback = callback;
    }

    public void OnPurchaseDecision(bool decision)
    {
        purchasePanel.SetActive(false);
        purchaseCallback?.Invoke(decision);
    }

    public void ShowEventInfo(string message)
    {
        eventInfoText.text = message;
    }
}
