using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ScoreSystemManager : MonoBehaviour
{
    // Get instance
    private static ScoreSystemManager _instance;
    public static ScoreSystemManager GetInstance()
    {
        if (_instance == null) _instance = FindObjectOfType<ScoreSystemManager>();
        return _instance;
    }

    // Init Score system
    [Header("AI FITNESS")]
    [SerializeField]
    private ScoreSystem Init;
    // Current Score System
    private static ScoreSystem _Current = null;
    public static ScoreSystem GetCurrent()
    {
        if (_Current == null)
            _Current = ScoreSystemManager.GetInstance().Init;

        return _Current;
    }

}
