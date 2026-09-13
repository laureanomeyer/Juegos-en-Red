using UnityEngine;
using Photon.Pun;

public class PlayerVitals : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private int maxLives = 4;
    private int currentLives;

    public int CurrentLives => currentLives;
    public int MaxLives => maxLives;

    public System.Action<int> OnLivesChanged;
    public System.Action OnLifeLost;
    public System.Action OnDepleted;

    private void Awake()
    {
        currentLives = maxLives;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(currentLives);
        }
        else
        {
            currentLives = (int)stream.ReceiveNext();
            OnLivesChanged?.Invoke(currentLives);
        }
    }

    [PunRPC]
    public void LoseLife()
    {
        currentLives = Mathf.Max(0, currentLives - 1);
        OnLivesChanged?.Invoke(currentLives);

        if (!photonView.IsMine) return;

        if (currentLives <= 0)
        {
            OnDepleted?.Invoke();
        }
        else
        {
            OnLifeLost?.Invoke();
        }
    }
}