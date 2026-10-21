using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[Serializable]
public class BossFightPhaseDefinition
{
    [HorizontalGroup("PhaseHeader", Width = 0.5f)]
    public string phaseName = "Phase 1 - Introduction";

    [HorizontalGroup("PhaseHeader", Width = 0.5f)]
    public Color phaseThemeColor = new Color(0.2f, 0.7f, 1f);

    [Range(0f, 1f)]
    [Tooltip("Franchissement de ce pourcentage de PV pour déclencher la phase")]
    public float healthThreshold = 1.0f;

    [Tooltip("La timeline DAW jouée pendant cette phase")]
    [InlineEditor]
    public BossTimelineSequence timelineSequence;

    [Header("Stat Multipliers (Dynamic Evolution)")]
    public float speedMultiplier = 1.0f;
    public float damageMultiplier = 1.0f;
    public float defenseMultiplier = 1.0f;

    [Header("Transition Feedbacks")]
    public string bannerAlertText = "PHASE 1 INITIÉE";
    [InlineEditor] public AudioFeedbackDefinition transitionSFX;
    [InlineEditor] public VisualFeedbackDefinition transitionVFX;
}

public class BossController : SerializedMonoBehaviour
{
    [Title("Profile & Setup")]
    [InlineEditor]
    public BossCharacterProfile characterProfile;

    [Title("Dynamic Multi-Timeline Phases")]
    [ListDrawerSettings(ShowIndexLabels = true)]
    public List<BossFightPhaseDefinition> fightPhases = new List<BossFightPhaseDefinition>();

    [Title("Runtime Components")]
    public Transform player;
    public Animator animator;
    public Transform arenaCenter;
    public BossSequencerPlayer sequencerPlayer;

    public float CurrentHealth { get; private set; }
    public float CurrentPoise { get; private set; }
    public float MaxHealth => characterProfile != null && characterProfile.stats != null ? characterProfile.stats.maxHealth : 1000f;
    public float MaxPoise => characterProfile != null && characterProfile.stats != null ? characterProfile.stats.poiseMeter : 100f;

    public int CurrentPhaseIndex { get; private set; } = -1;
    public bool IsInvulnerable { get; set; } = false;

    private IBossState currentState;

    private void Awake()
    {
        if (player == null)
        {
            var pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj != null) player = pObj.transform;
        }

        if (sequencerPlayer == null)
            sequencerPlayer = GetComponent<BossSequencerPlayer>();

        ResetStats();
    }

    private void Start()
    {
        if (BossFightHUD.Instance != null && characterProfile != null)
        {
            BossFightHUD.Instance.Initialize(
                characterProfile.bossName,
                characterProfile.title,
                MaxHealth,
                MaxPoise
            );
        }

        // Démarre la première phase
        if (fightPhases.Count > 0)
        {
            TransitionToPhase(0);
        }
    }

    private void Update()
    {
        currentState?.Update();

        // Récupération naturelle de Poise
        if (characterProfile != null && characterProfile.stats != null && CurrentPoise < MaxPoise)
        {
            CurrentPoise = Mathf.Min(MaxPoise, CurrentPoise + characterProfile.stats.poiseRecoveryRate * Time.deltaTime);
            BossFightHUD.Instance?.UpdatePoise(CurrentPoise, MaxPoise);
        }

        // Progression de la Timeline dans le HUD
        if (sequencerPlayer != null && CurrentPhaseIndex >= 0 && CurrentPhaseIndex < fightPhases.Count)
        {
            var pDef = fightPhases[CurrentPhaseIndex];
            float total = pDef.timelineSequence != null ? pDef.timelineSequence.totalDuration : 1f;
            BossFightHUD.Instance?.UpdateTimelineProgress(pDef.phaseName, sequencerPlayer.CurrentPlayhead, total);
        }
    }

    public void ResetStats()
    {
        CurrentHealth = MaxHealth;
        CurrentPoise = MaxPoise;
    }

    public void TransitionToPhase(int newIndex)
    {
        if (newIndex < 0 || newIndex >= fightPhases.Count) return;

        CurrentPhaseIndex = newIndex;
        var phaseDef = fightPhases[newIndex];

        Debug.Log($"[BossController] Transition vers {phaseDef.phaseName}");

        // HUD Feedback
        if (BossFightHUD.Instance != null)
        {
            BossFightHUD.Instance.ShowPhaseTransitionBanner(phaseDef.bannerAlertText, phaseDef.phaseThemeColor);
            BossFightHUD.Instance.TriggerDamageFlash(new Color(phaseDef.phaseThemeColor.r, phaseDef.phaseThemeColor.g, phaseDef.phaseThemeColor.b, 0.4f), 0.4f);
            BossFightHUD.Instance.PushCombatLog($"Transition : {phaseDef.phaseName} (Dmg x{phaseDef.damageMultiplier}, Vit x{phaseDef.speedMultiplier})");
        }

        // Lancement de la timeline associée à cette phase
        if (sequencerPlayer != null && phaseDef.timelineSequence != null)
        {
            sequencerPlayer.PlayWave(phaseDef.timelineSequence);
        }
    }

    public void TakeDamage(float amount)
    {
        if (IsInvulnerable || CurrentHealth <= 0) return;

        float defense = 1.0f;
        if (CurrentPhaseIndex >= 0 && CurrentPhaseIndex < fightPhases.Count)
            defense = fightPhases[CurrentPhaseIndex].defenseMultiplier;

        float effectiveDamage = amount / Mathf.Max(0.1f, defense);
        CurrentHealth = Mathf.Max(0, CurrentHealth - effectiveDamage);

        // Feedbacks HUD
        BossFightHUD.Instance?.UpdateHealth(CurrentHealth, MaxHealth);
        BossFightHUD.Instance?.TriggerDamageFlash(new Color(1f, 0f, 0f, 0.35f), 0.2f);
        BossFightHUD.Instance?.PushCombatLog($"Boss subit {effectiveDamage:F0} dégâts (PV restants: {CurrentHealth:F0})");

        // Poise damage
        CurrentPoise = Mathf.Max(0, CurrentPoise - (amount * 0.4f));
        BossFightHUD.Instance?.UpdatePoise(CurrentPoise, MaxPoise);

        if (CurrentHealth <= 0)
        {
            OnBossDeath();
            return;
        }

        CheckPhaseProgression();
    }

    private void CheckPhaseProgression()
    {
        float hpPercent = CurrentHealth / MaxHealth;

        for (int i = fightPhases.Count - 1; i > CurrentPhaseIndex; i--)
        {
            if (hpPercent <= fightPhases[i].healthThreshold)
            {
                TransitionToPhase(i);
                break;
            }
        }
    }

    private void OnBossDeath()
    {
        Debug.Log("[BossController] Boss vaincu !");
        BossFightHUD.Instance?.ShowPhaseTransitionBanner("BOSS VAINCU !", Color.yellow);
        BossFightHUD.Instance?.PushCombatLog("Le Boss a été terrassé !");
        sequencerPlayer?.StopWave();
        animator?.SetTrigger("Die");
    }

    public float DistanceToPlayer()
    {
        if (player == null) return float.MaxValue;
        return Vector3.Distance(transform.position, player.position);
    }
}
