using UnityEngine;
using Photon.Pun;

public class PlayerBrain : MonoBehaviourPun
{
    private TankMovement movement;
    private TankHp hp;

    private void Awake()
    {
        movement = GetComponent<TankMovement>();
        hp = GetComponent<TankHp>();
    }
}
