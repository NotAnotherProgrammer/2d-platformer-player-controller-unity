using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float jumpForce = 16f;
    public float maxFallSpeed = -25f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    [Header("Attack")]
    public WeaponHitbox weaponHitbox;
    public float attackDuration = 0.18f;
    public float attackCooldown = 0.25f;

    [Header("Facing")]
    public Transform graphicsRoot;

    private Rigidbody2D rb;
    private Animator animator;
    private float moveInput;
    private bool isGrounded;
    private bool isAttacking;
    private float attackTimer;
    private float cooldownTimer;
    private bool facingRight = true;

    public bool IsGrounded => isGrounded;
    public bool IsAttacking => isAttacking;
    public bool FacingRight => facingRight;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (weaponHitbox != null)
        {
            weaponHitbox.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        CheckGrounded();
        HandleJump();
        HandleAttack();
        HandleFlip();
        HandleAnimation();
    }

    private void FixedUpdate()
    {
        Move();
        ClampFallSpeed();
    }

    private void Move()
    {
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    private void HandleAttack()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.LeftShift) && cooldownTimer <= 0f)
        {
            StartAttack();
        }

        if (isAttacking)
        {
            attackTimer -= Time.deltaTime;

            if (attackTimer <= 0f)
            {
                EndAttack();
            }
        }
    }

    private void StartAttack()
    {
        isAttacking = true;
        attackTimer = attackDuration;
        cooldownTimer = attackCooldown;

        if (weaponHitbox != null)
        {
            weaponHitbox.SetOwner(this);
            weaponHitbox.gameObject.SetActive(true);
            weaponHitbox.UpdateDirection(facingRight);
        }
    }

    private void EndAttack()
    {
        isAttacking = false;

        if (weaponHitbox != null)
        {
            weaponHitbox.gameObject.SetActive(false);
        }
    }

    private void HandleFlip()
    {
        if (moveInput > 0 && !facingRight)
        {
            Flip(true);
        }
        else if (moveInput < 0 && facingRight)
        {
            Flip(false);
        }

        if (weaponHitbox != null)
        {
            weaponHitbox.UpdateDirection(facingRight);
        }
    }

    private void Flip(bool faceRight)
    {
        facingRight = faceRight;

        Vector3 scale = graphicsRoot != null ? graphicsRoot.localScale : transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (facingRight ? 1 : -1);

        if (graphicsRoot != null)
            graphicsRoot.localScale = scale;
        else
            transform.localScale = scale;
    }

    private void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void ClampFallSpeed()
    {
        if (rb.linearVelocity.y < maxFallSpeed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, maxFallSpeed);
        }
    }

    private void HandleAnimation()
    {
        if (animator == null) return;

        animator.SetFloat("Speed", Mathf.Abs(moveInput));
        animator.SetBool("Grounded", isGrounded);
        animator.SetFloat("VerticalVelocity", rb.linearVelocity.y);
        animator.SetBool("Attacking", isAttacking);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
