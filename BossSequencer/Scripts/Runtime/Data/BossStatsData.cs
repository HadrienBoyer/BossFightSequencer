using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "BossStats_New", menuName = "Boss/Data/Boss Stats")]
public class BossStatsData : ScriptableObject
{
    [BoxGroup("Vitality")]
    [MinValue(100f)] public float maxHealth = 3000f;
    [MinValue(0f)] public float armor = 20f;
    [Range(0f, 1f)] public float damageReduction = 0.1f;

    [BoxGroup("Movement")]
    public float baseMoveSpeed = 4f;
    public float sprintMoveSpeed = 7.5f;
    public float rotationSpeed = 360f;

    [BoxGroup("Combat Dynamics")]
    [Tooltip("Facteur multiplicateur de vitesse/cooldowns quand le boss est enragé")]
    [Range(1f, 2.5f)] public float enrageSpeedMultiplier = 1.35f;

    [Tooltip("Résistance au stagger / interruption")]
    public float poiseMeter = 100f;
    public float poiseRecoveryRate = 15f;
}
