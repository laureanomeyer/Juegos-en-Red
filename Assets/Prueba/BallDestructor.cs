using UnityEngine;

public class BallDestructor : MonoBehaviour
{
    [SerializeField] private float tiempoDestruir = 15f;
    private float tiempoTranscurrido = 0f;

    private void OnEnable()
    {
        tiempoTranscurrido = 0f;
    }

    private void Update()
    {
        tiempoTranscurrido += Time.deltaTime;

        if (tiempoTranscurrido >= tiempoDestruir)
        {
            BallPool.ReturnBall(gameObject);
            return;
        }

        // También retorna si se cae muy bajo
        if (transform.position.y < -10f)
        {
            BallPool.ReturnBall(gameObject);
        }
    }
}