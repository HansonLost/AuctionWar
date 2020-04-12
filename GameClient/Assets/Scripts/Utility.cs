using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility
{
    public static bool IsInRange(Int32 value, Int32 min, Int32 max)
    {
        return (value >= min && value <= max);
    }
    public static bool CheckIndex(Int32 idx, ICollection collection)
    {
        return IsInRange(idx, 0, collection.Count - 1);
    }
    public static void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
