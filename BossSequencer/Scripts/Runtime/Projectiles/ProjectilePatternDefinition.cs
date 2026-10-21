using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public enum ProjectileSpreadShape
{
    Single,
    RadialCircle,
    ConeArc,
    Spiral,
    HomingBurst
}

[System.Serializable]
public class ProjectileWaveStep
{
    [HorizontalGroup("Wave", Width = 0.5f)]
    public int projectileCount = 8;

    [HorizontalGroup("Wave", Width = 0.5f)]
    public float delayBeforeNextWave = 0.15f;

    [Range(0f, 360f)]
    public float angleOffset = 0f;

    [Range(0f, 360f)]
    public float arcAngle = 90f;
}

[CreateAssetMenu(fileName = "Pattern_NewProjectiles", menuName = "Boss/Combat/Projectile Pattern Definition")]
public class ProjectilePatternDefinition : SerializedScriptableObject
{
    [BoxGroup("General")]
    public string patternIdentifier = "Barrage_01";

    [BoxGroup("Pooling & Prefab")]
    [Tooltip("Clé pour le Pooling System Runtime")]
    public string projectilePoolKey = "EnergyOrb_Pool";
    public GameObject projectilePrefab;

    [BoxGroup("Shape & Physics")]
    public ProjectileSpreadShape spreadShape = ProjectileSpreadShape.RadialCircle;

    [MinValue(0.5f)] public float baseSpeed = 12f;
    [MinValue(0.1f)] public float projectileLifetime = 5f;
    public float damage = 25f;

    [BoxGroup("Trajectory Modifiers")]
    public bool isHoming = false;
    [ShowIf("isHoming")] public float homingTurnSpeed = 180f;

    public bool hasAcceleration = false;
    [ShowIf("hasAcceleration")] public float acceleration = 5f;

    [Title("Wave Sequences")]
    [TableList]
    public List<ProjectileWaveStep> waveSteps = new List<ProjectileWaveStep>();

    [Title("Feedbacks")]
    [InlineEditor]
    public AudioFeedbackDefinition launchAudioFeedback;
    [InlineEditor]
    public VisualFeedbackDefinition launchVisualFeedback;
    [InlineEditor]
    public VisualFeedbackDefinition impactVisualFeedback;
}
