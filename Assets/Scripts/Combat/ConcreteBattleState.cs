using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeginState : BattleState
{
    public BeginState(BattleManager system) : base(system)
    {
    }

    public override IEnumerator Start()
    {
        manager.InitUnitLayout();
        manager.SortUnitList();
        yield return new WaitForSeconds(1f);
        manager.SetState(new PreActionState(manager));
    }
}

public class PreActionState : BattleState
{
    public PreActionState(BattleManager system) : base(system)
    {
    }
    public override IEnumerator Start()
    {
        if (manager.isAction)
            yield break;
        manager.isAction = true;
        HexGridLayout hexGrid = manager.HexGrid;
        manager.NextUnit();
        manager.SortUnitList();
        manager.MovableHexes = hexGrid.GetReachable(manager.ActiveUnit.Hex, manager.ActiveUnit.movement);
        if ((manager.ActiveUnit.Team == Team.A && manager.AisAI) || (manager.ActiveUnit.Team == Team.B && manager.BisAI))
        {
            // AI
            Debug.Log($"!!!{manager.ActiveUnit.unitname} AI MOVE");
            manager.AIMove();

        } else
        {
            // human control
            foreach (var hex in manager.MovableHexes)
            {
                foreach (var nbr in hex.AxialNeighbors())
                {
                    if (hexGrid.obstacleMap.ContainsKey(nbr) && hexGrid.obstacleMap[nbr] == Content.Unit)
                    {
                        foreach (var unit in manager.aliveList)
                        {
                            if (unit.Hex == nbr && unit.Team != manager.ActiveUnit.Team)
                                manager.GenAttackIndicator(hex, nbr);
                        }
                    }
                }
            }
        }
        manager.MovableHexes.Remove(manager.ActiveUnit.Hex);
        manager.ChangeMovableColor(new Color(0.22f, 0.82f, 0.63f));

        yield return new WaitForSeconds(0.2f);
        manager.SetState(new ActionState(manager));
    }
}

public class ActionState : BattleState
{
    public ActionState(BattleManager system) : base(system)
    {
    }
    public override IEnumerator ActionEnd()
    {
        manager.ClearIndicator();
        manager.ChangeMovableColor(new Color(0.6f, 0.6f, 0.6f));
        manager.MovableHexes.Clear();
        onCoroutine = false;
        yield return new WaitForSeconds(1f);
        manager.isAction = false;
        manager.SetState(new CheckState(manager));
    }

    public override IEnumerator Attack(Hex target, Hex enemy)
    {
        if (onCoroutine)
            yield break;
        onCoroutine = true;
        manager.ClearIndicator();
        manager.ChangeMovableColor(new Color(0.6f, 0.6f, 0.6f));
        manager.MovableHexes.Clear();

        CombatUnit unit = manager.ActiveUnit;
        HexGridLayout hexGrid = manager.HexGrid;

        manager.StartCoroutine(manager.HightHex(enemy));
        List<Hex> hexpath = hexGrid.PathFinding(unit.Hex, target);
        List<Vector3> path = new();
        foreach (var hex in hexpath)
        {
            path.Add(hexGrid.GetPositionFromHex(hex));
        }
        manager.ActiveUnit.OnSeek(path, hexGrid.GetPositionFromHex(enemy));
        // move complete
        hexGrid.obstacleMap[unit.Hex] = Content.Empty;
        unit.Hex = hexpath[hexpath.Count - 1];
        hexGrid.obstacleMap[unit.Hex] = Content.Unit;
        yield return null;
    }

    public override IEnumerator Move(Hex target)
    {
        if (onCoroutine)
            yield break;
        onCoroutine = true;
        manager.ClearIndicator();
        manager.ChangeMovableColor(new Color(0.6f, 0.6f, 0.6f));
        manager.MovableHexes.Clear();

        CombatUnit unit = manager.ActiveUnit;
        HexGridLayout hexGrid = manager.HexGrid;

        manager.StartCoroutine(manager.HightHex(target));
        List<Hex> hexpath = hexGrid.PathFinding(unit.Hex, target);
        List<Vector3> path = new();
        foreach (var hex in hexpath)
        {
            path.Add(hexGrid.GetPositionFromHex(hex));
        }
        manager.ActiveUnit.OnMove(path);
        // move complete
        hexGrid.obstacleMap[unit.Hex] = Content.Empty;
        unit.Hex = hexpath[^1];
        hexGrid.obstacleMap[unit.Hex] = Content.Unit;
        yield return null;
    }
}

public class CheckState : BattleState
{
    public CheckState(BattleManager system) : base(system)
    {
        bool hasAliveA = false;
        bool hasAliveB = false;
        List<CombatUnit> tmp_grave;
        tmp_grave = new List<CombatUnit>();
        foreach (var unit in manager.TeamA)
        {
            if (unit.health_current > 0)
                hasAliveA = true;
            else
                tmp_grave.Add(unit);
        }
        foreach (var corp in tmp_grave)
        {
            manager.TeamA.Remove(corp);
            manager.aliveList.Remove(corp);
        }
        tmp_grave.Clear();
        foreach (var unit in manager.TeamB)
        {
            if (unit.health_current > 0)
                hasAliveB = true;
            else
                tmp_grave.Add(unit);
        }
        foreach (var corp in tmp_grave)
        {
            manager.TeamB.Remove(corp);
            manager.aliveList.Remove(corp);
        }
        tmp_grave.Clear();
        if (hasAliveA && hasAliveB)
        {
            // next turn
            manager.SetState(new PreActionState(manager));
        } 
        else if(hasAliveA && !hasAliveB)
        {
            // A win
            Debug.Log("A win");
        }
        else if (!hasAliveA && hasAliveB)
        {
            // B win
            Debug.Log("B win");
        }
        else if(!hasAliveA && !hasAliveB)
        {
            // both lose
            Debug.Log("both lose");
        }
    }
    public override IEnumerator Start()
    {
        return base.Start();
    }

}

public class EndState : BattleState
{
    public EndState(BattleManager system) : base(system)
    {
    }
}

