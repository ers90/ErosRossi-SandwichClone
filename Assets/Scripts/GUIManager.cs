using DG.Tweening;
using UnityEngine;
using TMPro;


public class GUIManager : MonoBehaviour
{
    public static GUIManager instance;

    [Header("UI References")]
    public TextMeshProUGUI levelNoText;
    public GameObject winPanel; 
    public GameObject gameplayPanel;
    public GameObject finalVictoryPanel;
    public TextMeshProUGUI warningText;

    public float fadeDuration = 0.5f;
    public float displayDuration = 2f;

    private void MakeInstance()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Awake()
    {
        MakeInstance();
    }

    private void Start()
    {
        if (warningText != null)
        {
            warningText.alpha = 0f;
        }
    }

    public void SetLevelText(int levelNo)
    {
        levelNoText.text = "LEVEL " + levelNo;
    }

    public void ShowWinPanel()
    {
        HideAllPanels();
        if (winPanel != null) winPanel.SetActive(true);
    }

    
    private void HideAllPanels()
    {
        if (winPanel != null) winPanel.SetActive(false);
        if (gameplayPanel != null) gameplayPanel.SetActive(false);
    }

    public void ShowWarningMessage(string message)
    {
        if (warningText == null)
        {
            Debug.LogWarning("warningText non è assegnato nel GUIManager!");
            return;
        }
        warningText.text = message;
        warningText.alpha = 0f;
        warningText.DOFade(1f, fadeDuration).OnComplete(() =>
        {
            warningText.DOFade(0f, fadeDuration).SetDelay(displayDuration);
        });
    }

    public void ShowFinalVictoryPanel()
    {
        HideAllPanels();
        if (finalVictoryPanel != null)
            finalVictoryPanel.SetActive(true);
    }
}
