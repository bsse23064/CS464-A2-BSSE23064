using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float speed = 5f;
    [SerializeField] float jumpHeight = 1.8f;

    [SerializeField] Transform feet;
    [SerializeField] float checkRadius = 0.2f;
    [SerializeField] LayerMask groundMask;

    Rigidbody rb;
    Vector3 move;
    bool isGrounded, jumpQueued;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Update()
    {
        isGrounded = Physics.CheckSphere(feet.position, checkRadius, groundMask,QueryTriggerInteraction.Ignore);

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        move = Vector3.ClampMagnitude(new Vector3(h, 0f, v), 1f);   // diagonals clamped
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
            jumpQueued = true;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector3(move.x * speed,rb.linearVelocity.y, move.z * speed);

        if (jumpQueued)
        {
            jumpQueued = false;                       // applied once
            float g  = -Physics.gravity.y;            // 9.81, read from the project
            float v0 = Mathf.Sqrt(2f * g * jumpHeight);
            rb.AddForce(Vector3.up * rb.mass * v0, ForceMode.Impulse);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (feet == null) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(feet.position, checkRadius);
    }
}