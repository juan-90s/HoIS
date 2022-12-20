using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;



public class AttackIndicator : MonoBehaviour
{
    public Hex origin, target;
    public void SetIndicator(Hex origin, Hex target)
    {
        this.origin = origin; 
        this.target = target;
    }
}
