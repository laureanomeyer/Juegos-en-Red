using Unity.VisualScripting;
using UnityEngine;
using Photon.Pun;

public class BulletScript : MonoBehaviour
{
    private Vector3 direction;
    private float deathTimer = 4;

    private GameObject owner;
    private PhotonView myView;

    private void Awake()
    {
        myView = GetComponent<PhotonView>();
    }
    private void Update()
    {
        deathTimer -= Time.deltaTime;
        if (deathTimer < 0) Destroy(gameObject);
    }
    private void FixedUpdate()
    {
        transform.position += direction;
    }

    public void SetMove(Vector3 dir)
    {
        direction = dir;
    }
    public void SetOwner(GameObject own)
    {
        owner = own;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!myView.IsMine) return;

        Debug.Log("Trigger");
        if (other.CompareTag("Player") && other != owner)
        {
            other.GetComponent<TankHp>().TakeDamage(20);
            PhotonNetwork.Destroy(gameObject);
        }
    }
}