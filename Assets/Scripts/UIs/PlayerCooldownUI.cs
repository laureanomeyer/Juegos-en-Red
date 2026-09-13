using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCooldownUI : MonoBehaviour
{
    [SerializeField] private PhotonView ownerView;
    [SerializeField] private PlayerCombat combat;
    [SerializeField] private Image pushCooldownFill;
    [SerializeField] private Image grabCooldownFill;

    private void Awake()
    {
        if (!ownerView.IsMine) { gameObject.SetActive(false); return; } // el HUD es solo del jugador local
    }

    private void Update()
    {
        pushCooldownFill.fillAmount = combat.PushCooldownRatio;
        grabCooldownFill.fillAmount = combat.GrabCooldownRatio;
    }
}
