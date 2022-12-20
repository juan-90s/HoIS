using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[Serializable] 
public class ActionEndEvent : UnityEvent { }

public class CombatValue
{
    private UnitData data;
    private Team m_team;
    private int m_health;
    private int m_quiver;

    public Hex Hex;
    public HashSet<EffectData> buffs;

    public Team Team => m_team;
    public string unitname => data.unitname;
    public bool canRange => data.canRange;
    public int quiver => m_quiver;
    public MeleeType meleeType => data.melee_type;
    public int health_max => data.health_max;
    public int health_current => m_health;
    public int damage
    {
        get
        {
            return UnityEngine.Random.Range(data.damage_min, data.damage_max + 1);
        }
    }
    public int attack_melee
    {
        get
        {
            float mul = 1;
            foreach (var buff in buffs) { mul *= buff.atk_melee_multiplier; }
            return (int)(data.attack_melee * mul);
        }
    }
    public int attack_range
    {
        get
        {
            float mul = 1;
            foreach (var buff in buffs) { mul *= buff.atk_range_multiplier; }
            return (int)(data.attack_range * mul);
        }
    }
    public int defense_melee
    {
        get
        {
            float mul = 1;
            foreach (var buff in buffs) { mul *= buff.dfs_melee_multiplier; }
            return (int)(data.defense_range * mul);
        }
    }
    public int defense_range
    {
        get
        {
            float mul = 1;
            foreach (var buff in buffs) { mul *= buff.dfs_range_multiplier; }
            return (int)(data.defense_range * mul);
        }
    }

    public int movement
    {
        get
        {
            int addend = 0;
            foreach (var buff in buffs) { addend += buff.movementAddend; }
            return data.movement + addend;
        }
    }
    public int speed
    {
        get
        {
            int addend = 0;
            foreach (var buff in buffs) { addend += buff.speedAddend; }
            return data.speed + addend;
        }
    }

    public CombatValue(UnitData data, Team team)
    {
        this.data = data;
        this.m_team = team;
        m_health = data.health_max;
        m_quiver = data.quiver;
        buffs = new();
    }
    public CombatValue(CombatValue copy)
    {
        data = copy.data;
        m_team = copy.m_team;
        m_health = copy.m_health;
        m_quiver = copy.m_quiver;
        buffs=copy.buffs;
        Hex=copy.Hex;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetMeleeDamage(CombatValue attacker, CombatValue receiver)
    {
        return (int)Mathf.Round(attacker.damage * attacker.attack_melee / receiver.defense_melee);
    }
    public void sufferDamage(int damage)
    {
        m_health = m_health > damage ? m_health - damage : 0;
    }
}

public class CombatUnit : MonoBehaviour
{
    [SerializeField] private Collider attack_trigger;
    private NavMeshAgent agent;
    private Animator animator;
    private CapsuleCollider capsule;
    private List<Vector3> path;
    private bool will_attack = false;
    private Vector3 target_attack;
    public ActionEndEvent actionEnd;
    public CombatValue Value;

    public Hex Hex
    {
        set
        {
            Value.Hex = value;
        }
        get
        {
            return Value.Hex;
        }
    }
    public string unitname => Value.unitname;
    public int movement => Value.movement;
    public int speed => Value.speed;
    public Team Team => Value.Team;
    public int health_current => Value.health_current;

    public void Activate(UnitData uData, Team team)
    {
        Value = new CombatValue(uData, team);
        agent.enabled = true;
    }

    public void OnMove(List<Vector3> path)
    {
        capsule.enabled = false;
        this.path = path;
        agent.destination = path[0];
        agent.speed = movement * 1f;
    }

    public void OnSeek(List<Vector3> path, Vector3 target)
    {
        capsule.enabled = false;
        target_attack = target;
        will_attack = true;
        this.path = path;
        agent.destination = path[0];
        agent.speed = movement * 1f;
    }

    public IEnumerator OnAttack(Vector3 target)
    {
        yield return new WaitForSeconds(0.5f);
        transform.LookAt(target);
        animator.SetTrigger("Attack");
        will_attack = false;
        yield return new WaitForSeconds(1f);
        actionEnd.Invoke();
    }
    public void EnableTrigger()
    {
        attack_trigger.enabled = true;
    }

    public void DisableTrigger()
    {
        attack_trigger.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "AttackTrigger") return;
        CombatUnit attacker = other.GetComponentInParent<CombatUnit>();
        
        if (attacker != null && attacker != this && attacker.Team != Team)
        {
            int suffer = CombatValue.GetMeleeDamage(attacker.Value, this.Value);
            Value.sufferDamage(suffer);
            if (health_current > 0)
            {
                animator.SetTrigger("Stagger");
            }
            else
            {
                animator.SetBool("Dead", true);
            }
        }
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        capsule = GetComponent<CapsuleCollider>();
        path = new List<Vector3>();
        attack_trigger.enabled = false;
    }

    private void Update()
    {
        // Moving
        if (path.Count > 0 && Vector3.Distance(transform.position, path[0]) < 0.5f)
        {
            path.RemoveAt(0);
            if(path.Count > 0)
            {
                agent.destination = path[0];
            }
            else
            {
                // arrive destination
                if (will_attack)
                {
                    StartCoroutine(OnAttack(target_attack));
                }

                else
                {
                    actionEnd.Invoke();
                }
                    
                capsule.enabled = true;
            }

        }
    }

    private void LateUpdate()
    {
        animator.SetBool("Dead", (health_current <= 0));
        animator.SetFloat("Speed", agent.velocity.magnitude / agent.speed);
    }

    private class SortSpeedDescendingHelper : IComparer<CombatUnit>
    {
        public int Compare(CombatUnit a, CombatUnit b)
        {

            return -a.speed.CompareTo(b.speed);
        }
    }

    public static IComparer<CombatUnit> SortSpeedDescending()
    {
        return new SortSpeedDescendingHelper();
    }
}
