﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;
    
    public int levelNo=1;

    public int sandwichElementCount;

    private List<GridData> levels;
    private Action<int> onLevelDataReady;

    void MakeInstance()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Awake()
    {        
        MakeInstance();
        levels = Levels.Instance.levels;
        levelNo = PlayerPrefs.GetInt("LevelNo");        
    }

    private void Start()
    {
        PlayerPrefs.DeleteKey("LevelNo");
        GUIManager.instance.SetLevelText(levelNo);
        onLevelDataReady += GameManager.instance.OnLevelDataReady;
    }

    public GridData GetCurrentLevelData()
    {
        CheckLevelNo();
        sandwichElementCount = 0;

        foreach (var tile in levels[levelNo-1].tiles)
        {
            if (tile.tileState != TileData.TileState.EMPTY)
            {
                sandwichElementCount++;
            }
        }
        Debug.Log($"Sandwich Element Count = ({sandwichElementCount}");
        onLevelDataReady?.Invoke(sandwichElementCount);
        return levels[levelNo-1];
    }

    private void CheckLevelNo()
    {
        if (levelNo <= 0 || levelNo > levels.Count )
            levelNo = 1;
        GUIManager.instance.SetLevelText(levelNo);
    }
    
    public void LoadNextLevel()
    {
        levelNo += 1;
        if (levelNo > levels.Count)
        {
            GUIManager.instance.ShowFinalVictoryPanel();
            return;
        }
        CheckLevelNo();
        PlayerPrefs.SetInt("LevelNo", levelNo);
        DOTween.KillAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    } 

    public void RestartGame()
    {
        levelNo = 1;
        PlayerPrefs.SetInt("LevelNo", levelNo);
        DOTween.KillAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
