using System.Collections;
using UnityEngine;

public abstract class BattleState
{
    protected BattleManager manager;
    protected bool onCoroutine = false;

    public BattleState(BattleManager system)
    {
        manager = system;
    }

    public virtual IEnumerator Start() { yield break; }
    public virtual IEnumerator Perform() { yield break; }
    public virtual IEnumerator Move(Hex target) { yield break; }
    public virtual IEnumerator Attack(Hex origin, Hex target) { yield break; }
    public virtual IEnumerator ActionEnd() { yield break; }
    public virtual IEnumerator Exit() { yield break; }
}

