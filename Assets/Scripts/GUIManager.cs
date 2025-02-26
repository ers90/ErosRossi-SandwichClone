using DG.Tweening;
using UnityEngine;
using TMPro;


public class GUIManager : MonoBehaviour
{
    public static GUIManager instance;

    [Header("UI References")]
    public TextMeshProUGUI levelNoText;
    public GameObject winPanel; 
    public GameObject finalVictoryPanel; 
    public GameObject gameplayPanel;

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

   
    public void OnTapToNextLevel()
    {
        LevelManager.instance.LoadNextLevel();
    }

   
    public void OnTapToRetryLevel()
    {
        LevelManager.instance.LoadCurrentLevel();
    }

    public void ShowFinalVictoryPanel()
    {
        HideAllPanels();
        if (finalVictoryPanel != null)
            finalVictoryPanel.SetActive(true);
    }

    public void OnTapToRestartGame()
    {
        LevelManager.instance.RestartGame();
    }
}
