using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Interactor : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] float range = 3f;         // consolas / interactuables
    [SerializeField] float pickupRange = 6f;
    [SerializeField] LayerMask mask = ~0;
    [SerializeField] InputActionReference interactAction;
    [SerializeField] InputActionReference dropAction;
    [SerializeField] HandsHolder hands;


    public bool InputLocked { get; private set; }
    public string CurrentPrompt { get; private set; }   // lo lee tu UI de "[E] Agarrar"

    // Cualquier sistema (UI de prompts, audio, etc.) se puede suscribir
    public event Action<bool> InputLockChanged;

    IInteractable _interactable;
    IPickable _pickable;

    void OnEnable()
    {
        interactAction.action.Enable();
        dropAction.action.Enable();
        interactAction.action.performed += OnInteract;
        dropAction.action.performed += OnDrop;
    }

    void OnDisable()
    {
        interactAction.action.performed -= OnInteract;
        dropAction.action.performed -= OnDrop;
    }

    void Update()
    {
        _interactable = null; _pickable = null; CurrentPrompt = null;
        if (InputLocked) return;

        float maxRange = Mathf.Max(range, pickupRange);

        if (Physics.Raycast(cam.transform.position, cam.transform.forward,
                out var hit, maxRange, mask, QueryTriggerInteraction.Ignore))
        {
            if (hit.distance <= range)
            {
                var inter = hit.collider.GetComponentInParent<IInteractable>();
                if (inter != null && inter.CanInteract(this))
                {
                    _interactable = inter;
                    CurrentPrompt = inter.Prompt;
                    return;
                }
            }

            if (!hands.IsHolding && hit.distance <= pickupRange)
            {
                var pick = hit.collider.GetComponentInParent<IPickable>();
                if (pick != null && pick.CanPickUp(hands))
                {
                    _pickable = pick;
                    CurrentPrompt = pick.Prompt;
                }
            }
        }
    }

    void OnInteract(InputAction.CallbackContext _)
    {
        if (InputLocked) { InteractWhileLocked?.Invoke(); return; }
        if (_interactable != null) _interactable.Interact(this);
        else if (_pickable != null) hands.PickUp(_pickable);
    }

    void OnDrop(InputAction.CallbackContext _)
    {
        if (!InputLocked) hands.Drop();
    }

    // Lo usan las UIs (consolas) para bloquear movimiento/cámara y liberar el mouse.
    // PlayerMovement y PlayerLook leen InputLocked, así que no hace falta tocarlos acá.
    public void LockInput(bool locked)
    {
        InputLocked = locked;
        Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = locked;
        InputLockChanged?.Invoke(locked);
    }

    public event Action InteractWhileLocked;


}