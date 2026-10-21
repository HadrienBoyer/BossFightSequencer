using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public class BossAnimationEntry
{
    public string stateKey = "Attack";
    public string animatorTriggerName;
    public AnimationClip clip;
    public float normalizedTransitionTime = 0.15f;
    public bool lockMovement = true;
}

[CreateAssetMenu(fileName = "BossAnimations_New", menuName = "Boss/Data/Boss Animation Set")]
public class BossAnimationData : SerializedScriptableObject
{
    [Title("Locomotion Triggers")]
    public string idleTrigger = "Idle";
    public string moveForwardTrigger = "Move";
    public string enrageTrigger = "Enrage";
    public string deathTrigger = "Die";
    public string staggerTrigger = "Stagger";

    [Title("Attack & Pattern Animations")]
    [TableList]
    public List<BossAnimationEntry> attackAnimations = new List<BossAnimationEntry>();

    public BossAnimationEntry FindAnimation(string key)
    {
        return attackAnimations.Find(a => a.stateKey == key);
    }
}
