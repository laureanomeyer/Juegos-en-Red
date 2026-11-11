using UnityEngine;

[DefaultExecutionOrder(-50)]
public class PlayerInputReader : MonoBehaviour
{
    public Vector2 Move { get; private set; }
    public Vector2 Look { get; private set; }

    private void Update()
    {
        if (UIState.IsAnyOpen)
        {
            Move = Vector2.zero;
            Look = Vector2.zero;
            return;
        }

        ReadInput();
    }

    private void ReadInput()
    {
#if ENABLE_INPUT_SYSTEM
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        var mouse = UnityEngine.InputSystem.Mouse.current;

        Vector2 move = Vector2.zero;
        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed) move.y += 1f;
            if (keyboard.sKey.isPressed) move.y -= 1f;
            if (keyboard.dKey.isPressed) move.x += 1f;
            if (keyboard.aKey.isPressed) move.x -= 1f;
        }

        Move = move;
        Look = mouse != null ? mouse.delta.ReadValue() : Vector2.zero;
#else
        Move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Look = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * 10f;
#endif
    }
}