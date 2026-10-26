using UnityEngine;

public interface IPickable
{
    Rigidbody Body { get; }
    string Prompt { get; }
    float Weight { get; }                  // 0..1 aprox, afecta velocidad y "pesadez"
    bool CanPickUp(HandsHolder holder);
    void OnPickedUp(HandsHolder holder);
    void OnDropped(HandsHolder holder);
}