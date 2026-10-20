using UnityEngine;
using System.Collections;

public class LanzaPelotas : MonoBehaviour
{
    [SerializeField] private float lanzamientoIntervalo = 5f;
    [SerializeField] private float fuerzaLanzamiento = 20f;
    [SerializeField] private float radioSpawn = 2f;
    [SerializeField] private float delayInicial = 1f;

    private void Start()
    {
        // Espera un frame para que BallPool esté inicializado
        StartCoroutine(InitializarPelotas());
        // Comienza a lanzar pelotas
        StartCoroutine(LanzarPelotasIntervalo());
    }

    private IEnumerator InitializarPelotas()
    {
        yield return new WaitForSeconds(delayInicial);

        // Instancia 5 pelotas en posiciones random al inicio
        for (int i = 0; i < 5; i++)
        {
            SpawnPelota();
            yield return new WaitForSeconds(0.5f); // Pequeña pausa entre spawns
        }
    }

    private void SpawnPelota()
    {
        GameObject pelota = BallPool.GetBall();

        if (pelota == null)
        {
            Debug.LogError("No se pudo obtener pelota del pool");
            return;
        }

        // Posición random dentro de un radio
        Vector3 posicionRandom = new Vector3(
            Random.Range(-radioSpawn, radioSpawn),
            2f,
            Random.Range(-radioSpawn, radioSpawn)
        );

        pelota.transform.position = transform.position + posicionRandom;

        // Aplica fuerza en dirección random
        Rigidbody rb = pelota.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 direccion = Random.onUnitSphere;
            rb.linearVelocity = direccion * fuerzaLanzamiento;
        }

        Debug.Log("Pelota lanzada en posición: " + pelota.transform.position);
    }

    private IEnumerator LanzarPelotasIntervalo()
    {
        yield return new WaitForSeconds(lanzamientoIntervalo);

        while (true)
        {
            yield return new WaitForSeconds(lanzamientoIntervalo);
            SpawnPelota();
        }
    }
}