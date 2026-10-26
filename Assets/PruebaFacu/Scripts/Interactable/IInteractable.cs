
public interface IInteractable
{
    string Prompt { get; }                 // "Usar consola [E]"
    bool CanInteract(Interactor who);
    void Interact(Interactor who);
}