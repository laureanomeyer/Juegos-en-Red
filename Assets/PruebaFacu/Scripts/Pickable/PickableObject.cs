// PickableObject.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PickableObject : MonoBehaviour, IPickable
{
    [SerializeField] string prompt = "Agarrar [E]";
    [SerializeField, Range(0f, 1f)] float weight = 0.3f;

    public Rigidbody Body { get; private set; }
    public string Prompt => prompt;
    public float Weight => weight;

    void Awake() => Body = GetComponent<Rigidbody>();

    public bool CanPickUp(HandsHolder holder) => true;
    public void OnPickedUp(HandsHolder holder) { }
    public void OnDropped(HandsHolder holder) { }
}