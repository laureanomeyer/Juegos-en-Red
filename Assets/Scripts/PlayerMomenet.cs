using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;
using UnityEditor.Experimental.GraphView;

public class PLayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 8f;

    private Rigidbody rb;
    private Vector2 moveinput;
    private PhotonView View;

    private void Awake()
    {
        View = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void FixedUpdate()
    {
        if (!View.IsMine) return;

            Vector3 direction = (transform.forward * moveinput.y + transform.right * moveinput.x).normalized;
            Vector3 move = direction * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + move);
    }
    
    public void OnMove(InputValue action)
    {
        moveinput = action.Get<Vector2>();
    }
}