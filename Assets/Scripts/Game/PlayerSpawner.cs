using System.Collections;
using Photon.Pun;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Tooltip("El prefab Player. Tiene que estar dentro de una carpeta Resources.")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float waitForRoomTimeout = 10f;
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

    private IEnumerator Start()
    {
        // El segundo jugador puede llegar a esta escena un instante antes de que Photon
        // lo marque como "dentro de la sala": esperamos en lugar de rendirnos.
        float remaining = waitForRoomTimeout;
        while (!PhotonNetwork.InRoom && remaining > 0f)
        {
            remaining -= Time.unscaledDeltaTime;
            yield return null;
        }

        if (!PhotonNetwork.InRoom)
        {
            Debug.LogWarning("[Spawner] No se entró a una sala a tiempo. Arrancá desde la escena Menu.");
            yield break;
        }

        if (playerPrefab == null || Resources.Load<GameObject>(playerPrefab.name) == null)
        {
            Debug.LogError("[Spawner] El prefab Player no está asignado o no está dentro de una carpeta Resources.");
            yield break;
        }

        if (PlayerController.Local == null)
            SpawnLocalPlayer();
    }

    // Después de reconectar esperamos un poco: Photon puede devolverte tu jugador solo.
    // Si pasado ese tiempo no hay ninguno tuyo, lo creamos de nuevo.
    private void HandleReconnected() => StartCoroutine(RespawnIfMissing());

    private IEnumerator RespawnIfMissing()
    {
        yield return new WaitForSeconds(reconnectCheckDelay);

        if (PlayerController.Local == null && playerPrefab != null)
            SpawnLocalPlayer();
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

        Debug.Log($"[Spawner] Instanciando '{playerPrefab.name}' (Actor {PhotonNetwork.LocalPlayer.ActorNumber}) en {position}");
        PhotonNetwork.Instantiate(playerPrefab.name, position, rotation);
    }
}