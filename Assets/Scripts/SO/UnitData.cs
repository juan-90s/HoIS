using UnityEngine;
using UnityEngine.UI;

public enum MeleeType
    {
        Normal,
        Long,
        Wide,
        Spread
    }
[CreateAssetMenu(fileName ="NewUnit", menuName ="UnitData")]
public class UnitData : ScriptableObject
{
    
    [Header("Basic")]
    [SerializeField] private string _unitname;
    [SerializeField] private GameObject _prefab;
    [SerializeField] private Sprite _icon;
    [SerializeField] private int _health_max;
    [SerializeField] private int _damage_min;
    [SerializeField] private int _damage_max;
    [Header("Melee")]
    [SerializeField] private int _atk_melee;
    [SerializeField] private int _dfs_melee;
    [SerializeField] private MeleeType _melee_type;

    [Header("Range")]
    [SerializeField] private bool _canRange;
    [SerializeField] private int _atk_range;
    [SerializeField] private int _dfs_range;
    [SerializeField] private int _quiver;

    [Header("Movement")]
    [SerializeField] private int _movement;
    [SerializeField] private int _speed;

    public string unitname => _unitname;
    public GameObject prefab => _prefab;
    public Sprite icon => _icon;
    public int health_max => _health_max;
    public int damage_max => _damage_max;
    public int damage_min => _damage_min;

    public int attack_melee => _atk_melee;
    public int defense_melee => _dfs_melee;
    public MeleeType melee_type => _melee_type;
    public bool canRange => _canRange;
    public int attack_range => _atk_range;
    public int defense_range => _dfs_range;
    public int quiver => _quiver;
    public int movement => _movement;
    public int speed => _speed;
}
