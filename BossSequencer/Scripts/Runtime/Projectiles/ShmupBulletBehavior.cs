using UnityEngine;

public class ShmupBulletBehavior : MonoBehaviour
{
    private ProjectilePatternDefinition pattern;
    private Vector3 initialDir;
    private float currentSpeed;
    private float lifeTimer = 0f;
    private Transform targetPlayer;

    private bool isFrozen = false;
    private bool hasSplit = false;
    private Vector3 perpendicularAxis;

    public void Initialize(ProjectilePatternDefinition def, Vector3 direction, Transform playerTarget = null)
    {
        pattern = def;
        initialDir = direction.normalized;
        currentSpeed = def != null ? def.baseSpeed : 10f;
        targetPlayer = playerTarget;
        perpendicularAxis = Vector3.Cross(initialDir, Vector3.up).normalized;
    }

    private void Update()
    {
        if (pattern == null) return;

        lifeTimer += Time.deltaTime;

        switch (pattern.spreadShape)
        {
            case ProjectileSpreadShape.DelayedBurst:
                HandleDelayedBurst();
                break;

            case ProjectileSpreadShape.WaveSine:
                HandleSineWave();
                break;

            case ProjectileSpreadShape.ClusterSplit:
                HandleClusterSplit();
                break;

            default:
                HandleStandardMovement();
                break;
        }

        if (lifeTimer >= pattern.projectileLifetime)
        {
            Destroy(gameObject);
        }
    }

    private void HandleStandardMovement()
    {
        if (pattern.hasAcceleration)
        {
            currentSpeed += pattern.acceleration * Time.deltaTime;
        }

        Vector3 moveDir = initialDir;

        // Homing behavior
        if (pattern.isHoming && targetPlayer != null)
        {
            Vector3 toTarget = (targetPlayer.position - transform.position).normalized;
            moveDir = Vector3.RotateTowards(moveDir, toTarget, pattern.homingTurnSpeed * Mathf.Deg2Rad * Time.deltaTime, 0f);
            initialDir = moveDir;
        }

        transform.position += moveDir * currentSpeed * Time.deltaTime;
    }

    private void HandleDelayedBurst()
    {
        // Ralentit jusqu'à 0, reste immobile, puis explose à pleine vitesse
        if (lifeTimer < 0.6f)
        {
            float t = lifeTimer / 0.6f;
            currentSpeed = Mathf.Lerp(pattern.baseSpeed, 0f, t);
        }
        else if (lifeTimer < 0.6f + pattern.freezeDuration)
        {
            currentSpeed = 0f;
            isFrozen = true;
        }
        else
        {
            currentSpeed = pattern.baseSpeed * pattern.postFreezeSpeedMultiplier;
            isFrozen = false;
        }

        if (currentSpeed > 0f)
        {
            transform.position += initialDir * currentSpeed * Time.deltaTime;
        }
    }

    private void HandleSineWave()
    {
        // Avancement principal
        Vector3 forwardStep = initialDir * pattern.baseSpeed * Time.deltaTime;
        
        // Déplacement ondulatoire latéral
        float sineOffset = Mathf.Sin(lifeTimer * pattern.sineFrequency) * pattern.sineAmplitude * Time.deltaTime * 5f;
        Vector3 lateralStep = perpendicularAxis * sineOffset;

        transform.position += forwardStep + lateralStep;
    }

    private void HandleClusterSplit()
    {
        if (!hasSplit && lifeTimer >= pattern.splitDelay)
        {
            hasSplit = true;
            // Explosion en sous-projectiles circulaires
            int count = pattern.splitChildCount;
            float step = 360f / Mathf.Max(1, count);

            for (int i = 0; i < count; i++)
            {
                float angle = i * step;
                Quaternion rot = Quaternion.Euler(0, angle, 0);
                Vector3 childDir = rot * Vector3.forward;

                GameObject child = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                child.name = "ClusterSubBullet";
                child.transform.position = transform.position;
                child.transform.localScale = transform.localScale * 0.6f;
                child.GetComponent<Renderer>().material.color = Color.cyan;

                var b = child.AddComponent<ShmupBulletBehavior>();
                b.Initialize(pattern, childDir, targetPlayer);
            }

            Destroy(gameObject);
            return;
        }

        transform.position += initialDir * currentSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[ShmupBullet] Impact Joueur ! Dégâts : {pattern.damage}");
            Destroy(gameObject);
        }
    }
}
