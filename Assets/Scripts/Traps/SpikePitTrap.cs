using System.Collections;
using Photon.Pun;
using UnityEngine;

// Trampa de "pozo con pinchos": al activarse, el piso desaparece y expone
// los pinchos de abajo, que matan al toque. Después de un tiempo el piso vuelve,
// para que la trampa se pueda volver a usar.
//
// Va pegado en el objeto de los pinchos (con su Collider marcado como Trigger).
// El "floor" es una referencia al piso hermano que se apaga/prende.
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

        // Solo el dueño de ESE jugador reporta su propia muerte - evita que
        // varios clientes (que corren la física local del mismo trigger) manden
        // el mismo RPC de daño repetido.
        if (!vitals.photonView.IsMine) return;

        vitals.photonView.RPC(nameof(PlayerVitals.ApplyDamage), RpcTarget.All, float.MaxValue);
    }
}