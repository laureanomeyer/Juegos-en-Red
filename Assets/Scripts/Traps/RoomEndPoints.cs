using UnityEngine;

public class RoomEndpoints : MonoBehaviour
{
    [SerializeField] private Transform head;
    [SerializeField] private Transform tail;

    public Vector3 HeadLocalPos => Vector3.Scale(head.localPosition, transform.localScale);
    public Vector3 TailLocalPos => Vector3.Scale(tail.localPosition, transform.localScale);
}