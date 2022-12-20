

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public enum MoveType
{
    Move, Attack
}
public struct AIMove
{
    public Hex target;
    public Hex enemy;
    public MoveType type;
    public AIMove(MoveType type, Hex target, Hex enemy)
    {
        this.type = type;
        this.target = target;
        this.enemy = enemy;
    }
}

public class AIDecision
{
    private Dictionary<Hex, Content> obstacleMap;
    private List<CombatValue> m_list;
    private int m_maxdepth;
    private int m_index;
    private AIMove m_move; 
    private float alpha = float.MinValue;
    private float beta = float.MaxValue;

    public AIDecision(HexGridLayout hexgrid, List<CombatUnit> unitlist, int current_index, int depth = 2)
    {
        Debug.Log("New Decision");
        obstacleMap = new Dictionary<Hex, Content>(hexgrid.obstacleMap);
        m_maxdepth = depth;
        m_list = new();
        m_index = current_index;
        foreach(var unit in unitlist)
        {
            m_list.Add(new CombatValue(unit.Value));
        }

        
    }

    public AIMove GetAIMove()
    {
        Minimax(m_list, m_index);
        return m_move;
    }

    public static float Evaluate(List<CombatValue> list)
    {
        float valueA = 0;
        float valueB = 0;
        foreach (var unitvalue in list)
        {
            if (unitvalue.Team == Team.A)
                valueA += unitvalue.health_current * unitvalue.defense_melee / 10f + unitvalue.attack_melee;
            if (unitvalue.Team == Team.B)
                valueB += unitvalue.health_current * unitvalue.defense_melee / 10f + unitvalue.attack_melee;
        }
        return valueA - valueB;
    }

    private float Minimax(List<CombatValue> list, int index, int depth=0)
    {
        Debug.Log("MimaDepth " + depth);
        if (depth >= m_maxdepth)
        {
            return Evaluate(list);
        }
        // gameover check
        int sumA = 0;
        int sumB = 0;
        foreach (var unitvalue in list)
        {
            switch (unitvalue.Team)
            {
                case Team.A:
                    sumA += unitvalue.health_current;
                    break;
                case Team.B:
                    sumB += unitvalue.health_current;
                    break;
            }
        }
        if (sumA == 0 || sumB == 0)
            return Evaluate(list);

        if (index >= list.Count) index = 0;
        
        float best = 0;
        bool firstnodeindepth = true;
        switch (list[index].Team)
        {
            case Team.A:
                best = float.MinValue;
                break;
            case Team.B:
                best = float.MaxValue;
                break;
        }
        
        for (int op_idx = 0; op_idx < list.Count;op_idx++)
        {
            if (list[op_idx].Team != list[index].Team)
            {
                Debug.Log($"  branch: ({depth},{index},{op_idx}), now is {list[index].unitname}");
                // make the move
                CombatValue actor = list[index];
                CombatValue actorclone = new(list[index]);
                CombatValue enemy = list[op_idx];
                CombatValue enemyclone = new(enemy);

                // modify list, keep in mind to undo before next iterate
                list.Remove(actor);
                list.Insert(index, actorclone);
                list.Remove(enemy);
                list.Insert(op_idx, enemyclone);

                obstacleMap[enemyclone.Hex] = Content.Empty;
                obstacleMap[actorclone.Hex] = Content.Empty;
                var path = Hex.PathFinding(actorclone.Hex, enemyclone.Hex, obstacleMap);
                Debug.Log($"  AIpathfind: {actorclone.Hex} to {enemyclone.Hex}");
                string result = "  Path: ";
                foreach (var item in path)
                {
                    result += item.ToString() + ", ";
                }
                Debug.Log(result);
                AIMove new_move;
                if (path.Count > actorclone.movement + 2)
                {
                    // out of range
                    new_move = new(MoveType.Move, path[actorclone.movement], path[actorclone.movement]);
                    actorclone.Hex = path[actorclone.movement];
                }
                else
                {
                    new_move = new(MoveType.Attack, path[^2], enemyclone.Hex);
                    actorclone.Hex = path[^2];
                    int damage = CombatValue.GetMeleeDamage(actorclone, enemyclone);
                    enemyclone.sufferDamage(damage);
                    if (enemyclone.health_current == 0)
                    {
                        list.Remove(enemyclone);
                        Debug.Log("  In decision, "+enemyclone.unitname+" killed");
                    }
                    
                }
                obstacleMap[actorclone.Hex] = Content.Unit;
                obstacleMap[enemyclone.Hex] = Content.Unit;
                float old_best = best;

                

                // dive into recursive
                if (actorclone.Team == Team.A)
                {
                    best = Math.Max(best, Minimax(list, index + 1, depth + 1));
                }
                else if (actorclone.Team == Team.B)
                {
                    best = Math.Min(best, Minimax(list, index + 1, depth + 1));
                }
                
                if (depth == 0 && (firstnodeindepth || old_best != best))
                {
                    // update move
                    m_move = new_move;
                }

                firstnodeindepth = false;

                // undo
                if (list.Contains(enemyclone))
                {
                    Debug.Log($"  ({depth},{index},{op_idx}), refresh {enemy.unitname}");
                    list.Remove(enemyclone);
                    list.Insert(op_idx, enemy);
                }
                else
                {
                    list.Insert(op_idx, enemy);
                    Debug.Log($"  ({depth},{index},{op_idx}), undo killed " + enemy.unitname);
                }
                list.Remove(actorclone);
                list.Insert(index, actor);

                obstacleMap[actorclone.Hex] = Content.Empty;
                obstacleMap[enemyclone.Hex] = Content.Empty;
                obstacleMap[enemy.Hex] = Content.Unit;
                obstacleMap[actor.Hex] = Content.Unit;

                Debug.Log($"  branch: ({depth},{index},{op_idx}) closed");
                if (actorclone.Team == Team.A)
                {
                    if (best >= beta)
                    {
                        Debug.Log("B cout off");
                        break;
                    }
                    alpha = Math.Max(alpha, best);
                }
                else if (actorclone.Team == Team.B)
                {
                    if (best <= alpha)
                    {
                        Debug.Log("A cout off");
                        break;
                    }
                    beta = Math.Min(beta, best);
                }

            }
            
        }
        return best;
    }
}
