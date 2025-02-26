﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu]
public class Levels : ScriptableObject
{
    
    public List<GridData> levels;
    
    public static Levels instance;
    
    public static Levels Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load("Levels") as Levels;
            }
            
            return instance;
        }
    }

}
