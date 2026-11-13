using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.LowLevel;
using UnityEngine.Rendering;

public class PlayerController : MonoBehaviourPun
{
    /// <summary>El jugador de ESTA instancia (null si todavía no apareció).</summary>
    public static PlayerController Local { get; private set; }

    [SerializeField] private Camera playerCamera;

    private PlayerInput input;
    private PlayerLook look;
    private AudioListener listener;
    private PhotonView view;

    private void Awake()
    {
        input = GetComponent<PlayerInput>();
        look = GetComponent<PlayerLook>();
        view = GetComponent<PhotonView>();

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>(true);

        Debug.Log($"[Player] Awake | dueño={view.OwnerActorNr} yo={PhotonNetwork.LocalPlayer.ActorNumber} " +
                  $"IsMine={view.IsMine} | cámara={(playerCamera != null)}");

        // Remoto: sin input (como en el proyecto anterior).
        if (input != null && !view.IsMine)
            input.enabled = false;

        if (look != null)
            look.enabled = view.IsMine;

        if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(true); // por si el prefab la dejó apagada; manda Camera.enabled
            playerCamera.enabled = view.IsMine;

            listener = playerCamera.GetComponent<AudioListener>();
            if (listener != null) listener.enabled = view.IsMine;
        }
        else
        {
            Debug.LogError("[Player] El prefab Player necesita una Camera como hijo.", this);
        }

        if (!view.IsMine) return;

        // Si ya había otro jugador local (por ejemplo, restaurado al reconectar), le sacamos la cámara.
        if (Local != null && Local != this)
            Local.DisableCamera();

        Local = this;

        // Nuestro propio cuerpo no se ve, pero sigue proyectando sombra.
        foreach (var rend in GetComponentsInChildren<Renderer>(true))
            rend.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
    }

    private void DisableCamera()
    {
        if (playerCamera != null) playerCamera.enabled = false;
        if (listener != null) listener.enabled = false;
    }

    private void OnDestroy()
    {
        if (Local == this) Local = null;
    }

    // Por si el editor tiene desactivado el reinicio de dominio.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => Local = null;
}