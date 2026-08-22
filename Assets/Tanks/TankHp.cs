using UnityEngine;
using Photon.Pun;
using Unity.VisualScripting;

public class TankHp : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private float hp;
    private float baseTimer = 2;
    private float timer = 2;
    private float networkHp;

    public float Hp => hp;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(hp);
        }
        else
        {
            networkHp = (float)stream.ReceiveNext();
            hp = networkHp;
            if (hp <= 0) Die();
            Debug.Log("hp to sync: " + networkHp);
        }
    }

    public void TakeDamage(float damage)
    {
        hp -= damage;
        Debug.Log("Hp take damage: " + hp);
    }

    public void Die()
    {
        Debug.Log("Taichu flopeo");
        PhotonNetwork.Destroy(gameObject);
    }
}
