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

        float globalAngleOffset = 0f;

        for (int stepIndex = 0; stepIndex < projData.waveSteps.Count; stepIndex++)
        {
            var wave = projData.waveSteps[stepIndex];
            int count = Mathf.Max(1, wave.projectileCount);
            float arc = wave.arcAngle;
            float stepAngle = arc / count;
            float startAngle = -arc * 0.5f + wave.angleOffset;

            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + (i * stepAngle);

                // Math Spécifique aux Shmups
                switch (projData.spreadShape)
                {
                    case ProjectileSpreadShape.DoubleSpiralOpposed:
                        // 2 flux inversés
                        float dirSign = (i % 2 == 0) ? 1f : -1f;
                        angle += dirSign * (stepIndex * projData.spinSpeed * 0.1f);
                        break;

                    case ProjectileSpreadShape.RosePetals:
                        // Modulation du rayon polaire façon rose de Rhodonea r = cos(k * theta)
                        float rad = (angle + globalAngleOffset) * Mathf.Deg2Rad;
                        float distMod = 1f + Mathf.Cos(projData.petalCount * rad) * projData.petalDistortion;
                        angle *= distMod;
                        break;

                    case ProjectileSpreadShape.WhipSweep:
                        angle = -arc * 0.5f + (stepIndex * 15f);
                        break;

                    case ProjectileSpreadShape.PredictiveLead:
                        if (bossCore.player != null)
                        {
                            Vector3 targetPos = bossCore.player.position;
                            var playerCc = bossCore.player.GetComponent<CharacterController>();
                            if (playerCc != null)
                            {
                                targetPos += playerCc.velocity * projData.leadTimeSeconds;
                            }
                            Vector3 toPredicted = (targetPos - spawnOrigin).normalized;
                            Quaternion predRot = Quaternion.LookRotation(toPredicted);
                            SpawnSingleBullet(spawnOrigin, predRot * Vector3.forward, projData);
                            continue;
                        }
                        break;
                }

                Quaternion rot = Quaternion.Euler(0, angle, 0) * transform.rotation;
                Vector3 shootDir = rot * Vector3.forward;

                SpawnSingleBullet(spawnOrigin, shootDir, projData);
            }
        }
    }

    private void SpawnSingleBullet(Vector3 origin, Vector3 direction, ProjectilePatternDefinition projData)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = $"ShmupBullet_{projData.spreadShape}";
        sphere.transform.position = origin;
        sphere.transform.localScale = Vector3.one * projData.bulletScale;
        sphere.GetComponent<Renderer>().material.color = projData.bulletColor;

        var col = sphere.GetComponent<SphereCollider>();
        if (col != null) col.isTrigger = true;

        var behavior = sphere.AddComponent<ShmupBulletBehavior>();
        behavior.Initialize(projData, direction, bossCore.player);
    }

    private void SetBossColor(Color c)
    {
        if (bossRenderer != null)
            bossRenderer.material.color = c;
    }
}
