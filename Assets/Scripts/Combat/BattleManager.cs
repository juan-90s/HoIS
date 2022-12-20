using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;

public enum Team
{
    A,B
}

public class BattleManager: MonoBehaviour
{
    private BattleState currentState;
    private HexGridLayout m_grid;
    private int uIndex = -1;
    private int turnSum = 0;
    private GameObject activeIndicator;
    public bool isAction = false;

    public bool AisAI;
    public bool BisAI;
    public int AIForcastDepth = 2;
    public List<UnitData> troopA;
    public List<UnitData> troopB;
    public List<CombatUnit> aliveList;
    public List<CombatUnit> deadList;
    public List<CombatUnit> TeamA;
    public List<CombatUnit> TeamB;
    public GameObject attackIndicatorPrefab;
    public GameObject activeIndicatorPrefab;
    public List<GameObject> AttackIndicatorList;
    public HashSet<Hex> MovableHexes;


    // Property
    public int TurnSum => turnSum;
    public HexGridLayout HexGrid => m_grid;
    public int IndexAlive => uIndex;
    public CombatUnit ActiveUnit
    {
        get
        {
            if (uIndex >= 0 && uIndex < aliveList.Count) { return aliveList[uIndex]; }
            else { return null; }
        }
    }
    

    private void Awake()
    {
        m_grid = GetComponent<HexGridLayout>();
        aliveList = new();
        deadList = new();
        TeamA = new();
        TeamB = new();
        AttackIndicatorList = new();
        activeIndicator = GameObject.Instantiate(activeIndicatorPrefab);
        activeIndicator.transform.position = new Vector3(0,-10,0);
    }
    private void Start()
    {
        
        SetState(new BeginState(this));
        
    }

    public void SetState(BattleState state)
    {
        currentState?.Exit();
        currentState = state;
        StartCoroutine(currentState.Start());
    }

    public void InitUnitLayout()
    {
        for (int i = 0; i < HexGrid.spawnpoint.Count; i++)
        {
            if (i < troopA.Count)
                InitUnit(troopA[i], Team.A, -HexGrid.spawnpoint[i]);
            if (i < troopB.Count)
                InitUnit(troopB[i], Team.B, HexGrid.spawnpoint[i]);
        }
    }

    public void InitUnit(UnitData unitData, Team team, Hex point)
    {
        GameObject prefabToSpawn = unitData.prefab;
        GameObject go = Instantiate(prefabToSpawn, HexGrid.GetPositionFromHex(point), new Quaternion());
        CombatUnit newUnit = go.GetComponent<CombatUnit>();
        newUnit.Activate(unitData, team);
        newUnit.Hex = point;
        HexGrid.obstacleMap[point] = Content.Unit;
        if(team == Team.A)
        {
            TeamA.Add(newUnit);
            newUnit.transform.LookAt(newUnit.transform.position + Vector3.forward);
        } else if(team == Team.B)
        {
            TeamB.Add(newUnit);
            newUnit.transform.LookAt(newUnit.transform.position + Vector3.back);
        }
    }
    public void NextUnit()
    {
        turnSum++;
        ActiveUnit?.actionEnd.RemoveAllListeners();
        uIndex++;
        if (uIndex >= aliveList.Count || uIndex < 0) uIndex = 0;

        activeIndicator.transform.parent = ActiveUnit.transform;
        activeIndicator.transform.position = ActiveUnit.transform.position + new Vector3(0,3,0);
        ActiveUnit.actionEnd.AddListener(OnActionEnd);
    }
    public void SortUnitList()
    {
        CombatUnit current_unit = ActiveUnit;
        aliveList.Clear();
        TeamA.Sort(CombatUnit.SortSpeedDescending());
        TeamB.Sort(CombatUnit.SortSpeedDescending());
        int indexA = 0;
        int indexB = 0;
        bool isA = true;
        while (indexA != TeamA.Count && indexB != TeamB.Count)
        {
            if (TeamA[indexA].speed > TeamB[indexB].speed)
            {
                if(TeamA[indexA].health_current > 0)
                {
                    aliveList.Add(TeamA[indexA]);
                    indexA++;
                    isA = true;
                }
                
            } 
            else if (TeamA[indexA].speed < TeamB[indexB].speed)
            {
                if (TeamB[indexB].health_current > 0)
                {
                    aliveList.Add(TeamB[indexB]);
                    indexB++;
                    isA = false;
                }
            } 
            else if (TeamA[indexA].speed == TeamB[indexB].speed)
            {
                if (aliveList.Count == 0)
                {
                    if (TeamA[indexA].health_current > 0)
                    {
                        aliveList.Add(TeamA[indexA]);
                        indexA++;
                        isA = true;
                    }
                } 
                else
                {
                    if (isA)
                    {
                        if (TeamB[indexB].health_current > 0)
                        {
                            aliveList.Add(TeamB[indexB]);
                            indexB++;
                            isA = false;
                        }
                    } else
                    {
                        if (TeamA[indexA].health_current > 0)
                        {
                            aliveList.Add(TeamA[indexA]);
                            indexA++;
                            isA = true;
                        }
                    }

                }
            }

            if(indexA == TeamA.Count)
            {
                for(; indexB < TeamB.Count; indexB++) aliveList.Add(TeamB[indexB]);
                break;
            } else if(indexB == TeamB.Count)
            {
                for (; indexA < TeamA.Count; indexA++) aliveList.Add(TeamA[indexA]);
                break;
            }
        }
        // ReSort complete
        if(current_unit != null)
            uIndex = aliveList.IndexOf(current_unit);
    }
    public void ChangeMovableColor(Color color)
    {
        color.a = 0.3f;
        foreach (Hex hex in MovableHexes)
        {
            HexGrid.SetColor(hex, color);
        }
    }

    public IEnumerator HightHex(Hex hex)
    {
        HexGrid.SetColor(hex, new Color(1, 1, 1, 0.3f));
        yield return new WaitForSeconds(2f);
        HexGrid.SetColor(hex, new Color(0.5f, 0.5f, 0.5f, 0.3f));
    }

    public void GenAttackIndicator(Hex origin, Hex target)
    {
        GameObject indicator = GameObject.Instantiate(attackIndicatorPrefab, HexGrid.GetPositionFromHex(origin), Quaternion.identity);
        indicator.transform.LookAt(HexGrid.GetPositionFromHex(target));
        indicator.GetComponent<AttackIndicator>().SetIndicator(origin, target);
        AttackIndicatorList.Add(indicator);
    }


    public void ClearIndicator()
    {
        foreach(var go in AttackIndicatorList)
        {
            Destroy(go);
        }
        AttackIndicatorList.Clear();
    }

    public void AIMove()
    {
        AIDecision decision = new AIDecision(HexGrid, aliveList, IndexAlive, AIForcastDepth);
        AIMove iMove = decision.GetAIMove();
        decision = null;
        switch (iMove.type)
        {
            case MoveType.Move:
                SetState(new ActionState(this));
                StartCoroutine(currentState.Move(iMove.target));
                break;
            case MoveType.Attack:
                SetState(new ActionState(this));
                StartCoroutine(currentState.Attack(iMove.target, iMove.enemy));
                break;
            default:
                SetState(new ActionState(this));
                StartCoroutine(currentState.ActionEnd());
                break;
        }
    }

    public void OnScreenClick(Vector3 position)
    {
        Hex targetHex = HexGrid.GetHexFromPosition(position);
        if(MovableHexes.Contains(targetHex))
            StartCoroutine(currentState.Move(targetHex));
    }

    public void OnAttackIndicate(GameObject go)
    {
        Debug.Log("OnAttackIndicate");
        AttackIndicator indicator = go.GetComponent<AttackIndicator>();
        if(indicator != null)
        {
            StartCoroutine(currentState.Attack(indicator.origin, indicator.target));
        }
    }

    public void OnActionEnd()
    {
        StartCoroutine(currentState.ActionEnd());
    }

}
