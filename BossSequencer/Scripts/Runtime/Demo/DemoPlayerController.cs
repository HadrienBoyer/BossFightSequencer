using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class DemoPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 7.5f;
    [SerializeField] private float sprintMultiplier = 1.4f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float rotationSpeed = 15f;

    [Header("Combat & Attacks")]
    [SerializeField] private float attackRange = 4.5f;
    [SerializeField] private float attackDamage = 120f;
    [SerializeField] private float attackCooldown = 0.4f;
    [SerializeField] private LayerMask bossLayer;

    private CharacterController controller;
    private Camera mainCam;
    private Vector3 velocity;
    private float nextAttackTime = 0f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        mainCam = Camera.main;
        gameObject.tag = "Player";
    }

    private void Update()
    {
        HandleMovement();
        HandleLook();
        HandleAttack();
    }

    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(h, 0f, v).normalized;

        float currentSpeed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? sprintMultiplier : 1f);

        if (direction.magnitude >= 0.1f)
        {
            Vector3 moveDir = direction;
            if (mainCam != null)
            {
                Vector3 camFwd = Vector3.ProjectOnPlane(mainCam.transform.forward, Vector3.up).normalized;
                Vector3 camRight = Vector3.ProjectOnPlane(mainCam.transform.right, Vector3.up).normalized;
                moveDir = (camFwd * v + camRight * h).normalized;
            }

            controller.Move(moveDir * currentSpeed * Time.deltaTime);

            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleLook()
    {
        // Visée avec la souris (Raycast vers le sol)
        if (mainCam == null) return;

        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 lookDir = hitPoint - transform.position;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), rotationSpeed * Time.deltaTime);
            }
        }
    }

    private void HandleAttack()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            PerformSlash();
        }
    }

    private void PerformSlash()
    {
        Debug.Log("[DemoPlayer] Coup d'épée !");

        // Détection de sphère en face du joueur
        Vector3 center = transform.position + transform.forward * (attackRange * 0.5f);
        Collider[] hits = Physics.OverlapSphere(center, attackRange * 0.5f);

        foreach (var col in hits)
        {
            var boss = col.GetComponentInParent<BossController>();
            if (boss != null)
            {
                boss.TakeDamage(attackDamage);
                break;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + transform.forward * (attackRange * 0.5f), attackRange * 0.5f);
    }
}
