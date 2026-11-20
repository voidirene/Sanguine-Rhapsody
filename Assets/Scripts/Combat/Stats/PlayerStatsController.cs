using Gamekit3D;
using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
public class PlayerStatsController : MonoBehaviour
{
    private PlayerStats _playerStats;
    private MeleeWeapon _meleeWeapon;

    void Awake()
    {
        _playerStats = GetComponent<PlayerStats>();
        _meleeWeapon = transform.parent.GetComponentInChildren<MeleeWeapon>();
    }

    private void OnEnable()
    {
        if (_playerStats != null)
            _playerStats.OnStatChanged += StatChanged;
    }

    private void OnDisable()
    {
        if (_playerStats != null)
            _playerStats.OnStatChanged -= StatChanged;
    }

    private void StatChanged(StatType statType)
    {
        switch (statType)
        {
            case StatType.Damage:
                _meleeWeapon.multiplierDamage = _playerStats.GetFinal(StatType.Damage);
                break;
            case StatType.EcstasySlowerDrain:
                // WIP
                break;
            case StatType.AttackSpeed:
                // WIP
                break;
            case StatType.AttackSize:
                // WIP
                break;
            case StatType.MoveSpeed:
                // WIP
                break;
        }
    }
}