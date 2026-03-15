// ============================================================
//  InputHandler.cs  —  mouse + touch via new Input System API
//  Drag to move, release to drop
// ============================================================
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    private FruitSpawner spawner;
    private Camera       cam;
    private bool         wasPressed;
    private bool         isDragging;

    public void Initialize(FruitSpawner sp)
    {
        spawner = sp;
        cam     = Camera.main;
    }

    private void Update()
    {
        if (GameManager.Instance?.State != GameState.Playing) return;

        // ── Pointer position ────────────────────────────────
        var pointer = Pointer.current;
        if (pointer == null) return;

        Vector2 screenPos = pointer.position.ReadValue();
        Vector3 world     = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));

        bool pressed = pointer.press.isPressed;

        // ── Press → start dragging ──────────────────────────
        if (pressed && !wasPressed)
        {
            isDragging = true;
        }

        // ── Drag → move held fruit horizontally ─────────────
        if (pressed && isDragging)
        {
            spawner.SetPointerX(world.x);
        }

        // ── Release → drop ──────────────────────────────────
        if (!pressed && wasPressed && isDragging)
        {
            spawner.Drop();
            isDragging = false;
        }

        wasPressed = pressed;
    }
}