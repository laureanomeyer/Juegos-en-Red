using System.Collections;
using Photon.Pun;
using UnityEngine;

public class SpikePitTrap : MonoBehaviour, ITrap
{
    [Header("Piso")]
    [SerializeField] private GameObject floor;
    [SerializeField] private float floorDownDuration = 3f;

    private Coroutine activeRoutine;

    public void Activate()
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
        }

        activeRoutine = StartCoroutine(OpenPitRoutine());
    }

    private IEnumerator OpenPitRoutine()
    {
        if (floor != null) floor.SetActive(false);

        yield return new WaitForSeconds(floorDownDuration);

        if (floor != null) floor.SetActive(true);
        activeRoutine = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerVitals vitals = other.GetComponentInParent<PlayerVitals>();
        if (vitals == null) return;

        if (!vitals.photonView.IsMine) return;

        vitals.photonView.RPC(nameof(PlayerVitals.LoseLife), RpcTarget.All);
    }
}