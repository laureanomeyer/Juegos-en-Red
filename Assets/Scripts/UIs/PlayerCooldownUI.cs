using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCooldownUI : MonoBehaviour, IPunObservable
{
    [SerializeField] private PhotonView ownerView;
    [SerializeField] private PlayerCombat combat;
    [SerializeField] private Image pushCooldownFill;
    [SerializeField] private Image grabCooldownFill;

    private void Awake()
    {
        if (ownerView == null) ownerView = GetComponentInParent<PhotonView>();
    }

    private void Update()
    {
        if (ownerView.IsMine)
        {
            pushCooldownFill.fillAmount = combat.PushCooldownRatio;
            grabCooldownFill.fillAmount = combat.GrabCooldownRatio;
        }
    }

    private void LateUpdate()
    {
        if (Camera.main == null) return;

        transform.forward = Camera.main.transform.forward;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(pushCooldownFill.fillAmount);
            stream.SendNext(grabCooldownFill.fillAmount);
        }
        else
        {
            pushCooldownFill.fillAmount = (float)stream.ReceiveNext();
            grabCooldownFill.fillAmount = (float)stream.ReceiveNext();
        }
    }
}