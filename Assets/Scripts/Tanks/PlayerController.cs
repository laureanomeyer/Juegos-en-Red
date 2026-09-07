using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPun
{
    private PlayerMovement movement;
    private PlayerVitals vitals;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        vitals = GetComponent<PlayerVitals>();
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
