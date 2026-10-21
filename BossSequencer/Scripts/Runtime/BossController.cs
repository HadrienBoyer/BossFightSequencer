using System;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 1000f;
    public float CurrentHealth { get; private set; }

    [Header("References")]
    public Transform player;
    public Animator animator;
    public Transform arenaCenter;

    [Header("Phase Configurations")]
    public BossPhaseConfig phase1Config;
    public BossPhaseConfig phase2Config;

    private IBossState currentState;
    public bool IsInvulnerable { get; set; } = false;

    private void Awake()
    {
        CurrentHealth = maxHealth;
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Start()
    {
        if (phase1Config != null)
            ChangeState(new BossFightPhaseState(this, phase1Config));
    }

    private void Update()
    {
        currentState?.Update();
    }

    public void ChangeState(IBossState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public void TakeDamage(float amount)
    {
        if (IsInvulnerable) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        float hpPercent = CurrentHealth / maxHealth;

        if (hpPercent <= 0.5f && !(currentState is BossWaveInterludeState) && phase2Config != null)
        {
            ChangeState(new BossWaveInterludeState(this, phase2Config));
        }
    }

    public float DistanceToPlayer()
    {
        if (player == null) return float.MaxValue;
        return Vector3.Distance(transform.position, player.position);
    }
}

[Serializable]
public class BossPhaseConfig
{
    public string phaseName = "Phase 1";
    public List<BossPatternData> availablePatterns = new List<BossPatternData>();
    public float baseMoveSpeed = 3.5f;
}
