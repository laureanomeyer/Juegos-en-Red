using UnityEngine;

public class TrapMasterStatusUI : MonoBehaviour
{
    [SerializeField] private GameObject banner; 

    private void OnEnable() => PhotonManager.Instance.OnTrapMasterDisconnected += ShowBanner;
    private void OnDisable() => PhotonManager.Instance.OnTrapMasterDisconnected -= ShowBanner;

    private void ShowBanner() => banner.SetActive(true);
}
