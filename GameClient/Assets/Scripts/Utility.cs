﻿using System;
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
}
