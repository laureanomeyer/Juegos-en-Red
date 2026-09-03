using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPun
{
    private PLayerMovement movement;
    private PlayerVItals vitals;

    private void Awake()
    {
        movement = GetComponent<PLayerMovement>();
        vitals = GetComponent<PlayerVItals>();
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
