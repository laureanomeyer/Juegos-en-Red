using UnityEngine;
using Photon.Pun;
using Unity.VisualScripting;

public class PlayerVitals : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private float maxValue = 100f; 
    private float currentValue;

    public float CurrentValue => currentValue;
    public System.Action<float> OnValueChanged;
    public System.Action OnDepleted;

    private void Awake()
    {
        currentValue = maxValue;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(currentValue);
        }
        else
        {
            currentValue = (float)stream.ReceiveNext();
            OnValueChanged?.Invoke(currentValue);
        }
    }

    [PunRPC]
    public void ApplyDamage(float amount)
    {
        currentValue -= amount;
        OnValueChanged?.Invoke(currentValue);

        if (currentValue <= 0)
        {
            HandleDepleted();
        }
    }

    private void HandleDepleted() 
    {
        if (!photonView.IsMine) return; 
        OnDepleted?.Invoke();
    }
}
