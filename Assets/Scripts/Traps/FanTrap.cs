using System.Collections;
using Photon.Pun;
using UnityEngine;

public class FanTrap : MonoBehaviour, ITrap
{
    [Header("Debug")]
    [SerializeField] private bool debugActivar;
    [Header("Ventilador")]
    [SerializeField] private GameObject fanVisual; 
    [SerializeField] private float activeDuration = 3f; 
    [Header("Empuje")]
    [SerializeField] private float pushForce = 6f;
    [SerializeField] private float windFadeSpeed = 20f;
    [SerializeField] private Transform pushDirectionSource;

    private bool isBlowing;
    private Coroutine activeRoutine;

    public void Activate()
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
        }

        activeRoutine = StartCoroutine(BlowRoutine());
    }

    private IEnumerator BlowRoutine()
    {
        isBlowing = true;
        if (fanVisual != null) fanVisual.SetActive(true);

        yield return new WaitForSeconds(activeDuration);

        isBlowing = false;
        if (fanVisual != null) fanVisual.SetActive(false);
        activeRoutine = null;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isBlowing) return;

        PlayerMovement movement = other.GetComponentInParent<PlayerMovement>();
        if (movement == null) return;

        PhotonView view = movement.GetComponent<PhotonView>();
        if (view == null || !view.IsMine) return; 
        Vector3 direction = (pushDirectionSource != null ? pushDirectionSource.forward : transform.forward).normalized;
        movement.ApplyWind(direction, pushForce, windFadeSpeed);
    }

    private void OnValidate()
    {
        if (debugActivar)
        {
            debugActivar = false; 
            if (Application.isPlaying) Activate();
        }
    }
}