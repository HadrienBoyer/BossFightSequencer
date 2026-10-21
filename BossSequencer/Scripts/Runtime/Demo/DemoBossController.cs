using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class DemoBossController : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private BossController bossCore;
    [SerializeField] private NavMeshAgent agent;

    [Header("Locomotion & NavMesh Tracking")]
    [SerializeField] private float stoppingDistance = 4.0f;
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private float repathRate = 0.25f;

    [Header("Visual Juice")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private MeshRenderer bossRenderer;

    private float repathTimer = 0f;
    private bool isExecutingAction = false;
    private Color originalColor = Color.red;

    private void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (bossCore == null) bossCore = GetComponent<BossController>();

        agent.stoppingDistance = stoppingDistance;
        if (bossRenderer != null) originalColor = bossRenderer.material.color;
    }

    private void Update()
    {
        if (bossCore == null || bossCore.CurrentHealth <= 0)
        {
            if (agent.isOnNavMesh && agent.isActiveAndEnabled) agent.isStopped = true;
            return;
        }

        UpdateNavMeshSpeed();
        HandleTrackingAndMovement();
    }

    private void UpdateNavMeshSpeed()
    {
        if (bossCore.characterProfile != null && bossCore.characterProfile.stats != null)
        {
            float baseSpeed = bossCore.characterProfile.stats.baseMoveSpeed;
            float multiplier = 1.0f;

            if (bossCore.CurrentPhaseIndex >= 0 && bossCore.CurrentPhaseIndex < bossCore.fightPhases.Count)
            {
                multiplier = bossCore.fightPhases[bossCore.CurrentPhaseIndex].speedMultiplier;
            }

            agent.speed = isExecutingAction ? 0f : baseSpeed * multiplier;
        }
    }

    private void HandleTrackingAndMovement()
    {
        if (isExecutingAction || bossCore.player == null) return;

        repathTimer += Time.deltaTime;
        if (repathTimer >= repathRate && agent.isOnNavMesh)
        {
            repathTimer = 0f;
            agent.SetDestination(bossCore.player.position);
        }

        // Rotation fluide vers le joueur
        Vector3 dir = bossCore.player.position - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }

    public void OnPatternExecute(BossPatternData pattern)
    {
        if (pattern == null) return;
        StartCoroutine(PerformPatternAction(pattern));
    }

    private IEnumerator PerformPatternAction(BossPatternData pattern)
    {
        isExecutingAction = true;
        if (agent.isOnNavMesh) agent.isStopped = true;

        // 1. Telegraph (Flash Jaune/Orange + charge visuelle)
        SetBossColor(Color.yellow);
        yield return new WaitForSeconds(pattern.telegraphDuration);

        // 2. Execution (Flash Rouge + Spawn projectiles ou impact)
        SetBossColor(new Color(1f, 0.2f, 0.2f));
        SpawnPatternProjectiles(pattern);
        yield return new WaitForSeconds(pattern.attackDuration);

        // 3. Recovery (Couleur grisée / repos)
        SetBossColor(Color.gray);
        yield return new WaitForSeconds(pattern.recoveryDuration);

        SetBossColor(originalColor);
        isExecutingAction = false;
        if (agent.isOnNavMesh) agent.isStopped = false;
    }

    private void SpawnPatternProjectiles(BossPatternData pattern)
    {
        if (pattern.projectilePattern == null) return;

        var projData = pattern.projectilePattern;
        Vector3 spawnOrigin = transform.position + Vector3.up * 1.2f + transform.forward * 1.5f;

        foreach (var wave in projData.waveSteps)
        {
            float stepAngle = wave.arcAngle / Mathf.Max(1, wave.projectileCount);
            float startAngle = -wave.arcAngle * 0.5f + wave.angleOffset;

            for (int i = 0; i < wave.projectileCount; i++)
            {
                float currentAngle = startAngle + (i * stepAngle);
                Quaternion rot = Quaternion.Euler(0, currentAngle, 0) * transform.rotation;
                Vector3 shootDir = rot * Vector3.forward;

                // Création d'une bille projectile fonctionnelle
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.name = "BossProjectile";
                sphere.transform.position = spawnOrigin;
                sphere.transform.localScale = Vector3.one * 0.6f;
                sphere.GetComponent<Renderer>().material.color = Color.magenta;

                var rb = sphere.AddComponent<Rigidbody>();
                rb.useGravity = false;
                rb.velocity = shootDir * projData.baseSpeed;

                Destroy(sphere, projData.projectileLifetime);
            }
        }
    }

    private void SetBossColor(Color c)
    {
        if (bossRenderer != null)
            bossRenderer.material.color = c;
    }
}
