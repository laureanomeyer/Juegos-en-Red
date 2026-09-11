using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviourPun
{
    private PlayerMovement movement;
    private PlayerVitals vitals;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        vitals = GetComponent<PlayerVitals>();

        var input = GetComponent<PlayerInput>();

        if (input != null && !GetComponent<PhotonView>().IsMine)
        {
            input.enabled = false;
        }
    }

    private void OnEnable()
    {
        vitals.OnDepleted += HandleDepleted; 
    }

    private void OnDisable()
    {
        vitals.OnDepleted -= HandleDepleted;
    }

    private void HandleDepleted()
    {

    }
}
