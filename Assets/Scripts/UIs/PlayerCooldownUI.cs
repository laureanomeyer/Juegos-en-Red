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
        if (ownerView == null) ownerView = GetComponent<PhotonView>();
    }

    private void Update()
    {
        if (!ownerView.IsMine) return;

        pushCooldownFill.fillAmount = combat.PushCooldownRatio;
        grabCooldownFill.fillAmount = combat.GrabCooldownRatio;
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
            this.pushCooldownFill.fillAmount = (float)stream.ReceiveNext();
            this.grabCooldownFill.fillAmount = (float)stream.ReceiveNext();
        }
    }
}
