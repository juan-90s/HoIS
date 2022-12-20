using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectData : ScriptableObject
{
    public string effectName;
    public int duration;
    public float atk_melee_multiplier = 1;
    public float dfs_melee_multiplier = 1;
    public float atk_range_multiplier = 1;
    public float dfs_range_multiplier = 1;
    public int movementAddend = 0;
    public int speedAddend = 0;
}
