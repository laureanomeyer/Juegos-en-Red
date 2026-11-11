using System.Collections;
using Photon.Pun;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private string playerPrefabName = "Player";
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float reconnectCheckDelay = 1.5f;

    private void OnEnable()
    {
        if (PhotonManager.Instance != null)
            PhotonManager.Instance.OnReconnected += HandleReconnected;
    }

    private void OnDisable()
    {
        if (PhotonManager.Instance != null)
            PhotonManager.Instance.OnReconnected -= HandleReconnected;
    }

    private void Start()
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogWarning("[PlayerSpawner] No estás en una sala. Arrancá desde la escena Menu.");
            return;
        }

        SpawnLocalPlayer();
    }

    // Después de reconectar esperamos un poco: Photon puede devolverte tu jugador solo.
    // Si pasado ese tiempo no hay ninguno tuyo en la escena, lo creamos de nuevo.
    private void HandleReconnected() => StartCoroutine(RespawnIfMissing());

    private IEnumerator RespawnIfMissing()
    {
        yield return new WaitForSeconds(reconnectCheckDelay);

        if (!HasLocalPlayer())
            SpawnLocalPlayer();
    }

    private bool HasLocalPlayer()
    {
        foreach (var view in FindObjectsByType<PhotonView>(FindObjectsSortMode.None))
        {
            if (view.IsMine && view.gameObject.name.StartsWith(playerPrefabName))
                return true;
        }
        return false;
    }

    private void SpawnLocalPlayer()
    {
        Vector3 position = Vector3.zero;
        Quaternion rotation = Quaternion.identity;

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            // Cada jugador usa el punto según su orden en la sala (el primero en entrar, el punto 0).
            int index = System.Array.IndexOf(PhotonNetwork.PlayerList, PhotonNetwork.LocalPlayer);
            Transform point = spawnPoints[Mathf.Max(index, 0) % spawnPoints.Length];
            position = point.position;
            rotation = point.rotation;
        }

        PhotonNetwork.Instantiate(playerPrefabName, position, rotation);
    }
}