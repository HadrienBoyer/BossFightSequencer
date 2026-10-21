using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "BossProfile_New", menuName = "Boss/Data/Boss Character Profile")]
public class BossCharacterProfile : SerializedScriptableObject
{
    [HorizontalGroup("Top", Width = 110)]
    [PreviewField(100, Alignment = ObjectFieldAlignment.Left), HideLabel]
    public Sprite portrait;

    [VerticalGroup("Top/Info")]
    [Title("Boss Identity")]
    public string bossName = "Malakor, The Void Sovereign";

    [VerticalGroup("Top/Info")]
    public string title = "Ancient Arch-Demon";

    [VerticalGroup("Top/Info")]
    [TextArea(2, 4)]
    public string loreDescription = "A terrifying entity awakened deep inside the arena catacombs.";

    [TabGroup("Tabs", "Stats & Combat")]
    [InlineEditor(Expanded = true)]
    public BossStatsData stats;

    [TabGroup("Tabs", "Animations")]
    [InlineEditor(Expanded = true)]
    public BossAnimationData animations;

    [TabGroup("Tabs", "Global Feedbacks")]
    [BoxGroup("Tabs/Global Feedbacks/Hit & Hurt")]
    [InlineEditor] public AudioFeedbackDefinition onHurtAudio;
    [BoxGroup("Tabs/Global Feedbacks/Hit & Hurt")]
    [InlineEditor] public VisualFeedbackDefinition onHurtVFX;

    [BoxGroup("Tabs/Global Feedbacks/Phase Transitions")]
    [InlineEditor] public AudioFeedbackDefinition phaseTransitionAudio;
    [BoxGroup("Tabs/Global Feedbacks/Phase Transitions")]
    [InlineEditor] public VisualFeedbackDefinition phaseTransitionVFX;

    [BoxGroup("Tabs/Global Feedbacks/Death")]
    [InlineEditor] public AudioFeedbackDefinition deathAudio;
    [BoxGroup("Tabs/Global Feedbacks/Death")]
    [InlineEditor] public VisualFeedbackDefinition deathVFX;

    [TabGroup("Tabs", "Available Patterns")]
    [ListDrawerSettings(ShowIndexLabels = true)]
    public List<BossPatternData> allPatterns = new List<BossPatternData>();
}
