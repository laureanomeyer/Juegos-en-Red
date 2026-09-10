using UnityEngine;
using Photon.Pun;

// Vive en la GameScene. Lee la secuencia de zonas que el Master generó
// y guardó como Custom Property de la sala al crearla (PhotonManager.CreateRoom),
// e instancia los prefabs LOCALMENTE en cada cliente. No hace falta que sea
// networked: como todos leen la misma property, todos arman el mismo nivel.
public class ZoneLevelBuilder : MonoBehaviour
{
    [Tooltip("Mismo orden y misma cantidad de elementos en TODOS los clientes.")]
    [SerializeField] private GameObject[] zonePrefabs;
    [SerializeField] private float zonaLength = 50f;
    [SerializeField] private Vector3 origen = Vector3.zero;
    [SerializeField] private Vector3 direccion = Vector3.forward;

    private void Start()
    {
        if (!PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(PhotonManager.ZONE_SEQ_KEY, out object raw))
        {
            Debug.LogError("No se encontró la secuencia de zonas en las Custom Properties de la sala.");
            return;
        }

        ConstruirNivel((int[])raw);
    }

    private void ConstruirNivel(int[] secuencia)
    {
        Vector3 dirNorm = direccion.normalized;

        for (int i = 0; i < secuencia.Length; i++)
        {
            int index = secuencia[i];
            if (index < 0 || index >= zonePrefabs.Length)
            {
                Debug.LogError($"Índice de zona inválido: {index}");
                continue;
            }

            Vector3 pos = origen + dirNorm * (zonaLength * i);
            Instantiate(zonePrefabs[index], pos, Quaternion.identity, transform);
        }
    }
}
