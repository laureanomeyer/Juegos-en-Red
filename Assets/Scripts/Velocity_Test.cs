using UnityEngine;
using Unity.Services.RemoteConfig;

public class Velocity_Test : MonoBehaviour
{
    public Transform puntoA;
    public Transform puntoB;

    private float velocidad = 0;
    private Transform destino;

    void Awake()
    {
        RemoteConfigService.Instance.FetchCompleted += AplicarVelocidad;
    }

    void Start()
    {
        transform.position = puntoA.position;
        destino = puntoB;
    }

    void AplicarVelocidad(ConfigResponse response)
    {
        velocidad = RemoteConfigService.Instance.appConfig.GetFloat("General_Speed", 3f);
        Debug.Log("Velocidad cargada: " + velocidad);
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            destino.position,
            velocidad * Time.deltaTime
        );

        if (transform.position == destino.position)
        {
            destino = (destino == puntoB) ? puntoA : puntoB;
        }
    }

    void OnDestroy()
    {
        RemoteConfigService.Instance.FetchCompleted -= AplicarVelocidad;
    }
}