using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Vector3 offset = new Vector3(0f, 6f, -8f);
    [SerializeField] float smoothTime = 0.3f;
    [SerializeField] float lookAhead = 0.35f;

    Rigidbody targetRb;
    Vector3 aimPoint, aimVelocity;

    void Awake()
    {
        if (target == null)
        {
            var player = FindAnyObjectByType<PlayerController>();
            if (player != null) target = player.transform;
        }
        if (target != null) targetRb = target.GetComponent<Rigidbody>();
    }

    void Start()
    {
        if (target == null) return;
        aimPoint = target.position;
        transform.position = aimPoint + offset;
        transform.LookAt(aimPoint);
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 flatVel = targetRb.linearVelocity; flatVel.y = 0f;
        Vector3 wanted = target.position + flatVel * lookAhead;

        aimPoint = Vector3.SmoothDamp(aimPoint, wanted, ref aimVelocity, smoothTime);

        transform.position = aimPoint + offset;
        transform.LookAt(aimPoint);
    }
}