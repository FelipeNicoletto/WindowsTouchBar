using WindowsInput.Native;

namespace WindowsTouchBar.Models;

internal partial class Settings
{
    public string? BackgroundColor { get; set; }

    public IEnumerable<View>? Views { get; set; }

    internal class View
    {
        public ViewType Type { get; set; }

        public bool Default { get; set; }

        public string? BackgroundColor { get; set; }

        public string? Application { get; set; }

        public IEnumerable<Component>? Components { get; set; }
    }

    internal class Component
    {
        public ComponentType Type { get; set; }

        public string? Title { get; set; }

        public string? Icon { get; set; }

        public string? Image { get; set; }

        public VirtualKeyCode[]? KeyCodes { get; set; }

        public VirtualKeyCode[]? ModifierKeyCodes { get; set; }

        public int? Width { get; set; }

        public string? Color { get; set; }

        public string? TextColor { get; set; }
    }

    internal enum ComponentType
    {
        Button,
        Spacer
    }
}
