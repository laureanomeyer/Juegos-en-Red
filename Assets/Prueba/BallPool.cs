using UnityEngine;
using System.Collections.Generic;

public class BallPool : MonoBehaviour
{
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private int poolSize = 10;

    private Queue<GameObject> availableBalls;
    private HashSet<GameObject> activeBalls;

    private static BallPool instance;

    private void Awake()
    {
        instance = this;
        availableBalls = new Queue<GameObject>(poolSize);
        activeBalls = new HashSet<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject ball = Instantiate(ballPrefab, transform);
            ball.SetActive(false);
            availableBalls.Enqueue(ball);
        }
    }

    public static GameObject GetBall()
    {
        if (instance == null) return null;

        GameObject ball;

        if (instance.availableBalls.Count > 0)
        {
            ball = instance.availableBalls.Dequeue();
        }
        else
        {
            ball = Instantiate(instance.ballPrefab, instance.transform);
        }

        ball.SetActive(true);
        instance.activeBalls.Add(ball);
        return ball;
    }

    public static void ReturnBall(GameObject ball)
    {
        if (instance == null || ball == null) return;

        ball.SetActive(false);
        instance.activeBalls.Remove(ball);

        // Resetea f�sica
        Rigidbody rb = ball.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        ball.transform.position = Vector3.zero;
        instance.availableBalls.Enqueue(ball);

        Debug.Log("Pelota reciclada. Disponibles: " + instance.availableBalls.Count);
    }
}