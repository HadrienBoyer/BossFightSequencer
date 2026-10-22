using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public enum ProjectileSpreadShape
{
    Single,
    RadialCircle,           // Anneau Nova 360°
    ConeArc,                // Éventail / Fan
    Spiral,                 // Spirale simple
    DoubleSpiralOpposed,    // Double spirale contre-rotative (croisements Danmaku)
    RosePetals,             // Courbes polaires type Rhodonea (fleurs / lobes)
    DelayedBurst,           // Stop & Go / Freeze & Release
    ClusterSplit,           // Orbe gigogne qui éclate en sous-projectiles
    WaveSine,               // Serpentin / oscillation sinusoïdale
    WhipSweep,              // Balayage rotatif continu (phare / aiguille)
    CrossGrid,              // Quadrillage / grille laser
    PredictiveLead          // Tir avec prédiction de trajectoire de la cible
}

[System.Serializable]
public class ProjectileWaveStep
{
    [HorizontalGroup("Wave", Width = 0.5f)]
    public int projectileCount = 12;

    [HorizontalGroup("Wave", Width = 0.5f)]
    public float delayBeforeNextWave = 0.15f;

    [Range(0f, 360f)]
    public float angleOffset = 0f;

    [Range(0f, 360f)]
    public float arcAngle = 360f;
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
    public Color bulletColor = new Color(0.9f, 0.2f, 0.8f, 1f);
    public float bulletScale = 0.6f;

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

    [BoxGroup("Shmup Math Parameters")]
    [ShowIf("@spreadShape == ProjectileSpreadShape.Spiral || spreadShape == ProjectileSpreadShape.DoubleSpiralOpposed || spreadShape == ProjectileSpreadShape.WhipSweep")]
    [Tooltip("Vitesse de rotation angulaire (degrés/seconde)")]
    public float spinSpeed = 90f;

    [ShowIf("@spreadShape == ProjectileSpreadShape.RosePetals")]
    [Tooltip("Nombre de pétales (k harmonique)")]
    [Range(2, 12)]
    public int petalCount = 5;

    [ShowIf("@spreadShape == ProjectileSpreadShape.RosePetals")]
    [Tooltip("Facteur d'amplitude des lobes")]
    public float petalDistortion = 0.5f;

    [ShowIf("@spreadShape == ProjectileSpreadShape.DelayedBurst")]
    [Tooltip("Temps d'arrêt / freeze avant ré-accélération subite")]
    public float freezeDuration = 1.2f;

    [ShowIf("@spreadShape == ProjectileSpreadShape.DelayedBurst")]
    public float postFreezeSpeedMultiplier = 2.0f;

    [ShowIf("@spreadShape == ProjectileSpreadShape.WaveSine")]
    [Tooltip("Fréquence d'oscillation latérale")]
    public float sineFrequency = 5f;

    [ShowIf("@spreadShape == ProjectileSpreadShape.WaveSine")]
    [Tooltip("Amplitude de l'oscillation latérale")]
    public float sineAmplitude = 3f;

    [ShowIf("@spreadShape == ProjectileSpreadShape.ClusterSplit")]
    [Tooltip("Délai avant scission du projectile")]
    public float splitDelay = 1.5f;

    [ShowIf("@spreadShape == ProjectileSpreadShape.ClusterSplit")]
    [Tooltip("Nombre de sous-projectiles générés à l'éclatement")]
    public int splitChildCount = 8;

    [ShowIf("@spreadShape == ProjectileSpreadShape.PredictiveLead")]
    [Tooltip("Délai futur anticipé (secondes)")]
    public float leadTimeSeconds = 0.6f;

    [Title("Wave Sequences")]
    [TableList]
    public List<ProjectileWaveStep> waveSteps = new List<ProjectileWaveStep>
    {
        new ProjectileWaveStep { projectileCount = 16, arcAngle = 360f, delayBeforeNextWave = 0.2f }
    };

    [Title("Feedbacks")]
    [InlineEditor]
    public AudioFeedbackDefinition launchAudioFeedback;
    [InlineEditor]
    public VisualFeedbackDefinition launchVisualFeedback;
    [InlineEditor]
    public VisualFeedbackDefinition impactVisualFeedback;

    [ButtonGroup("PresetButtons")]
    [Button("🌀 Preset : Double Spirale", ButtonSizes.Small)]
    public void ApplyPresetDoubleSpiral()
    {
        spreadShape = ProjectileSpreadShape.DoubleSpiralOpposed;
        spinSpeed = 120f;
        baseSpeed = 10f;
        waveSteps = new List<ProjectileWaveStep>
        {
            new ProjectileWaveStep { projectileCount = 4, arcAngle = 360f, delayBeforeNextWave = 0.12f, angleOffset = 0f },
            new ProjectileWaveStep { projectileCount = 4, arcAngle = 360f, delayBeforeNextWave = 0.12f, angleOffset = 18f },
            new ProjectileWaveStep { projectileCount = 4, arcAngle = 360f, delayBeforeNextWave = 0.12f, angleOffset = 36f },
            new ProjectileWaveStep { projectileCount = 4, arcAngle = 360f, delayBeforeNextWave = 0.12f, angleOffset = 54f }
        };
    }

    [ButtonGroup("PresetButtons")]
    [Button("🌸 Preset : Pétales Rose (Danmaku)", ButtonSizes.Small)]
    public void ApplyPresetRosePetals()
    {
        spreadShape = ProjectileSpreadShape.RosePetals;
        petalCount = 5;
        petalDistortion = 0.6f;
        baseSpeed = 9f;
        waveSteps = new List<ProjectileWaveStep>
        {
            new ProjectileWaveStep { projectileCount = 30, arcAngle = 360f, delayBeforeNextWave = 0.25f, angleOffset = 0f },
            new ProjectileWaveStep { projectileCount = 30, arcAngle = 360f, delayBeforeNextWave = 0.25f, angleOffset = 6f }
        };
    }

    [ButtonGroup("PresetButtons")]
    [Button("⏱️ Preset : Stop & Go (Freeze & Burst)", ButtonSizes.Small)]
    public void ApplyPresetFreezeRelease()
    {
        spreadShape = ProjectileSpreadShape.DelayedBurst;
        freezeDuration = 1.0f;
        postFreezeSpeedMultiplier = 2.2f;
        baseSpeed = 5f;
        waveSteps = new List<ProjectileWaveStep>
        {
            new ProjectileWaveStep { projectileCount = 16, arcAngle = 360f, delayBeforeNextWave = 0.2f },
            new ProjectileWaveStep { projectileCount = 20, arcAngle = 360f, delayBeforeNextWave = 0.2f, angleOffset = 9f }
        };
    }

    [ButtonGroup("PresetButtons")]
    [Button("💥 Preset : Cluster Orbs", ButtonSizes.Small)]
    public void ApplyPresetCluster()
    {
        spreadShape = ProjectileSpreadShape.ClusterSplit;
        splitDelay = 1.2f;
        splitChildCount = 8;
        baseSpeed = 8f;
        waveSteps = new List<ProjectileWaveStep>
        {
            new ProjectileWaveStep { projectileCount = 6, arcAngle = 360f, delayBeforeNextWave = 0.3f }
        };
    }
}
