using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class FanTrap : MonoBehaviour, ITrap
{
    [Header("Debug")]
    [SerializeField] private bool debugActivar;

    [Header("Ventilador")]
    [SerializeField] private GameObject fanVisual; // mesh / partículas / animación a prender-apagar (opcional)
    [SerializeField] private float activeDuration = 3f; // cuánto queda encendido tras activarse

    [Header("Empuje")]
    [SerializeField] private float pushForce = 12f;
    [SerializeField] private float pushInterval = 0.5f; // cooldown por jugador mientras sigue parado en la zona
    [SerializeField] private Transform pushDirectionSource; // opcional; si es null, usa este transform

    private bool isBlowing;
    private Coroutine activeRoutine;

    private readonly Dictionary<PlayerMovement, float> lastPushTime = new Dictionary<PlayerMovement, float>();

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
        if (view == null || !view.IsMine) return; // solo el dueño de ESE jugador reporta su propio empuje

        if (lastPushTime.TryGetValue(movement, out float last) && Time.time - last < pushInterval)
            return;

        lastPushTime[movement] = Time.time;

        Vector3 direction = (pushDirectionSource != null ? pushDirectionSource.forward : transform.forward).normalized;
        view.RPC(nameof(PlayerMovement.RPC_ApplyPush), RpcTarget.All, direction, pushForce);
    }

    private void OnValidate()
    {
        if (debugActivar)
        {
            debugActivar = false; // se destilda solo, para poder volver a probar
            if (Application.isPlaying) Activate();
        }
    }
}