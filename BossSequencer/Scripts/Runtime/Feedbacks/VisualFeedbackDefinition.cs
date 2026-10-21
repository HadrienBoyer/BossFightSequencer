using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public enum TweenType
{
    None,
    PunchScale,
    ShakePosition,
    FadeAlpha,
    FlashColor
}

[System.Serializable]
public class TweenDefinition
{
    public TweenType type = TweenType.None;

    [ShowIf("@type != TweenType.None")]
    public float duration = 0.35f;

    [ShowIf("@type == TweenType.PunchScale")]
    public Vector3 punchScale = new Vector3(0.3f, 0.3f, 0.3f);

    [ShowIf("@type == TweenType.ShakePosition")]
    public float shakeStrength = 0.5f;

    [ShowIf("@type == TweenType.ShakePosition")]
    public int shakeVibrato = 10;

    [ShowIf("@type == TweenType.FlashColor")]
    public Color flashColor = Color.white;

    [ShowIf("@type != TweenType.None")]
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
}

[System.Serializable]
public class VisualFeedbackEntry
{
    [Tooltip("Identifiant pour les Object Pools runtime")]
    public string poolKey = "Default_VFX";

    public GameObject vfxPrefab;

    public bool attachToTransform = false;
    public Vector3 offset = Vector3.zero;
    public Vector3 rotationOffset = Vector3.zero;
    public float scaleMultiplier = 1f;

    [MinValue(0.1f)]
    public float autoRecycleDelay = 2.5f;

    [BoxGroup("Tweening / Juice")]
    public TweenDefinition localTween;
}

[CreateAssetMenu(fileName = "VisualFeedback_New", menuName = "Boss/Feedbacks/Visual Feedback Definition")]
public class VisualFeedbackDefinition : SerializedScriptableObject
{
    [Title("Visual Settings")]
    public bool triggerCameraShake = false;

    [ShowIf("triggerCameraShake")]
    public float cameraShakeIntensity = 0.4f;

    [ShowIf("triggerCameraShake")]
    public float cameraShakeDuration = 0.25f;

    [Title("Poolable VFX Entries")]
    [TableList]
    public List<VisualFeedbackEntry> vfxEntries = new List<VisualFeedbackEntry>();
}
