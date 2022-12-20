using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "NewTroop", menuName = "TroopData")]
public class TroopData : ScriptableObject
{
    public List<UnitData> unitList;
}
