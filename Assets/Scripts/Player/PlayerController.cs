using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviourPun
{
    private PlayerMovement movement;
    private PlayerVitals vitals;
    private PlayerInput input;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Camera masterPlayerCamera;

    private PhotonView view;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        vitals = GetComponent<PlayerVitals>();
        input = GetComponent<PlayerInput>();
        view = GetComponent<PhotonView>();

        if (input != null && !view.IsMine)
        {
            input.enabled = false;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            masterPlayerCamera.enabled = true;
            playerCamera.enabled = false;
        } 
        else
        {
            playerCamera.enabled = true;
            masterPlayerCamera.enabled= false;
        }

        if (playerCamera != null && !view.IsMine)
        {
            playerCamera.enabled = false;
        }
        if (masterPlayerCamera != null && !view.IsMine)
        {
            masterPlayerCamera.enabled = false;
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
