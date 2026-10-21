using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "NewBossPattern", menuName = "Boss/Pattern")]
public class BossPatternData : SerializedScriptableObject
{
    [BoxGroup("Identity")]
    public string patternName = "New Pattern";
    public string animationTrigger = "Attack";

    [BoxGroup("Conditions")]
    public float minDistance = 0f;
    public float maxDistance = 20f;
    public float cooldown = 6f;
    public int weight = 10;

    [BoxGroup("Timings (Ableton-like / DAW Step)")]
    public float telegraphDuration = 1.2f;
    public float attackDuration = 0.8f;
    public float recoveryDuration = 1.5f;

    [TabGroup("Components", "Projectiles")]
    [InlineEditor]
    public ProjectilePatternDefinition projectilePattern;

    [TabGroup("Components", "Feedbacks")]
    [Title("Telegraph Feedbacks")]
    [InlineEditor] public AudioFeedbackDefinition telegraphAudio;
    [InlineEditor] public VisualFeedbackDefinition telegraphVFX;

    [Title("Execution Feedbacks")]
    [InlineEditor] public AudioFeedbackDefinition executionAudio;
    [InlineEditor] public VisualFeedbackDefinition executionVFX;
}
