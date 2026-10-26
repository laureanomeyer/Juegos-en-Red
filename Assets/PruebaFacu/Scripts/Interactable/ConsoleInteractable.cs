using UnityEngine;
using UnityEngine.InputSystem;

public class ConsoleInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] GameObject uiPanel;
    [SerializeField] string prompt = "Usar consola [E]";

    Interactor _user;

    public string Prompt => prompt;
    public bool CanInteract(Interactor who) => _user == null;

    public void Interact(Interactor who)
    {
        _user = who;
        uiPanel.SetActive(true);
        who.LockInput(true);
        who.InteractWhileLocked += Close;   // E dentro de la consola = cerrar
    }

    public void Close()   // también lo conectás al botón "Exit"
    {
        if (_user == null) return;
        uiPanel.SetActive(false);
        _user.InteractWhileLocked -= Close;
        _user.LockInput(false);
        _user = null;
    }

    void Update()
    {
        if (_user != null && Keyboard.current.escapeKey.wasPressedThisFrame) Close();
    }
}