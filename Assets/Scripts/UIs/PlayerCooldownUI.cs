using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCooldownUI : MonoBehaviour, IPunObservable
{
    [SerializeField] private PhotonView ownerView;
    [SerializeField] private PlayerCombat combat;
    [SerializeField] private RectTransform container;
    [SerializeField] private Image pushCooldownFill;
    [SerializeField] private Image grabCooldownFill;

    private void Awake()
    {
        if (ownerView == null) ownerView = GetComponentInParent<PhotonView>();

        if (!ownerView.IsMine) return;

        if (!PhotonManager.Instance.IsLocalPlayerTrapMaster())
        {
            container.transform.localRotation = Quaternion.Euler(-90f, -90, 0);
        }
    }

    private void Update()
    {
        if (ownerView.IsMine)
        {
            pushCooldownFill.fillAmount = combat.PushCooldownRatio;
            grabCooldownFill.fillAmount = combat.GrabCooldownRatio;
        }
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