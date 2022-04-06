using WindowsInput;
using WindowsInput.Native;

namespace WindowsTouchBar.Commands;

internal class KeyPressCommand : ITouchCommand
{
    private readonly InputSimulator _inputSimulator;

    public VirtualKeyCode[]? KeyCodes { get; set; }

    public VirtualKeyCode[]? ModifierKeyCodes { get; set; }

    public KeyPressCommand(VirtualKeyCode? keyCode = null)
    {
        _inputSimulator = new InputSimulator();

        if (keyCode.HasValue)
        {
            KeyCodes = new VirtualKeyCode[] { keyCode.Value };
        }
    }

    public void TouchDown()
    {
        if (KeyCodes?.Length > 0)
        {
            if (ModifierKeyCodes?.Length > 0)
            {
                _inputSimulator.Keyboard.ModifiedKeyStroke(ModifierKeyCodes, KeyCodes);
            }
            else if (KeyCodes?.Length == 1)
            {
                _inputSimulator.Keyboard.KeyDown(KeyCodes[0]);
            }
            else
            {
                _inputSimulator.Keyboard.KeyPress(KeyCodes);
            }
        }
    }

    public void TouchUp()
    {
        if (KeyCodes?.Length > 0)
        {
            if (ModifierKeyCodes?.Length is null or 0 &&
                KeyCodes?.Length == 1)
            {
                _inputSimulator.Keyboard.KeyUp(KeyCodes[0]);
            }
        }
    }
}
