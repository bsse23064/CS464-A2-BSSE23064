using UnityEngine;
using System.Collections;
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    // Movement
    [SerializeField] float speed = 5f;
    [SerializeField] float jumpHeight = 1.8f;
    [SerializeField] float fallMultiplier = 2.2f;

    // Coyote implementation
    [SerializeField] float coyoteTime = 0.1f;
    [SerializeField] float jumpBuffer = 0.12f;


    // Groun check
    [SerializeField] Transform feet;
    [SerializeField] float checkRadius = 0.2f;
    [SerializeField] LayerMask groundMask;

    // Juice: landing squash
    [SerializeField] Transform body;             
    [SerializeField] float squashY = 0.7f;
    [SerializeField] float squashDuration = 0.25f;

    Rigidbody rb;
    Vector3 move;
    bool isGrounded, wasGrounded, jumpQueued;
    float lastGroundedTime = -1f, lastPressedTime = -1f;
    float airTime;
    Coroutine squashRoutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Update()
    {
        isGrounded = Physics.CheckSphere(feet.position, checkRadius, groundMask,
                                         QueryTriggerInteraction.Ignore);

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        move = Vector3.ClampMagnitude(new Vector3(h, 0f, v), 1f);

        if (isGrounded && !jumpQueued && rb.linearVelocity.y <= 0f)
            lastGroundedTime = Time.time;
        if (Input.GetKeyDown(KeyCode.Space))
            lastPressedTime = Time.time;

        bool canJump  = Time.time - lastGroundedTime <= coyoteTime;
        bool wantJump = Time.time - lastPressedTime  <= jumpBuffer;
        if (canJump && wantJump)
        {
            jumpQueued = true;
            lastGroundedTime = lastPressedTime = -1f;   // used up
        }

        if (!isGrounded)
            airTime += Time.deltaTime;
        else
        {
            if (!wasGrounded && airTime > 0.1f)
            {
                if (squashRoutine != null) StopCoroutine(squashRoutine);
                squashRoutine = StartCoroutine(Squash());
            }
            airTime = 0f;
        }
        wasGrounded = isGrounded;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector3(move.x * speed,
                                        rb.linearVelocity.y,
                                        move.z * speed);

        if (jumpQueued)
        {
            jumpQueued = false;
            Vector3 vel = rb.linearVelocity;
            rb.linearVelocity = new Vector3(vel.x, 0f, vel.z);
            float g  = -Physics.gravity.y;
            float v0 = Mathf.Sqrt(2f * g * jumpHeight);
            rb.AddForce(Vector3.up * rb.mass * v0, ForceMode.Impulse);
        }

        if (rb.linearVelocity.y < 0f)
            rb.AddForce(Physics.gravity * (fallMultiplier - 1f),
                        ForceMode.Acceleration);
    }

    IEnumerator Squash()
    {
        for (float elapsed = 0f; elapsed < squashDuration; elapsed += Time.deltaTime)
        {
            float p = Ease.OutBack(elapsed / squashDuration);
            float y = Mathf.LerpUnclamped(squashY, 1f, p);  
            float xz = 1f / Mathf.Sqrt(y);                  
            body.localScale = new Vector3(xz, y, xz);
            body.localPosition = new Vector3(0f, -(1f - y), 0f);
            yield return null;
        }
        body.localScale = Vector3.one;       
        body.localPosition = Vector3.zero;
        squashRoutine = null;
    }

    void OnDrawGizmosSelected()
    {
        if (feet == null) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(feet.position, checkRadius);
    }
}