using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class TankShooter : MonoBehaviour
{
    [SerializeField] private InputActionReference fireAction;
    [SerializeField] private GameObject bullet;

    private PhotonView myView;

    private void Awake()
    {
        myView = GetComponent<PhotonView>();
    }
    public void OnAttack()
    {
        if (!myView.IsMine) return;

        var bull = PhotonNetwork.Instantiate(bullet.name, transform.position, Quaternion.identity);
        var bulScript = bull.GetComponent<BulletScript>();
        if (bulScript != null)
        {
            bulScript.SetMove(transform.forward.normalized);
            bulScript.SetOwner(gameObject);
        }
        bull.SetActive(true);
    }
}
