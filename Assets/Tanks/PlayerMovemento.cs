using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class TankMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float rotationSpeed = 100f;

    private Rigidbody rb;
    private Vector2 input;
    private PhotonView myView;

    private void Awake()
    {
        myView = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void FixedUpdate()
    {
        if (!myView.IsMine) return;

        Vector3 move = transform.forward * input.y * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);

        float rotation = input.x * rotationSpeed * Time.fixedDeltaTime;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, rotation, 0f));
    }

    public void OnMove(InputValue action)
    {
        input = action.Get<Vector2>();
    }
}