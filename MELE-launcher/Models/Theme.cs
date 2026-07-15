using Spectre.Console;

namespace MELE_launcher.Models
{
    /// <summary>
    /// Shared visual theme used by the interactive menu and the command terminal.
    /// Provides both <see cref="Color"/> values (for Spectre.Console styles) and their
    /// markup name equivalents (for inline markup strings).
    /// </summary>
    public class Theme
    {
        public Color Primary, Secondary, Accent, Muted, Highlight, Border, Alert;
        public string SecondaryName, AccentName, HighlightName;

        public Theme() => SetStandardMode();

        public void SetStandardMode()
        {
            Primary = Color.White;
            Secondary = Color.SlateBlue1;
            Accent = Color.Cyan1;
            Muted = Color.Grey39;
            Highlight = Color.Cyan1;
            Border = Color.SlateBlue3;
            Alert = Color.Orange1;
            SecondaryName = "slateBlue1";
            AccentName = "cyan1";
            HighlightName = "cyan1";
        }

        public void SetAdminMode()
        {
            Primary = Color.White;
            Secondary = Color.Orange1;
            Accent = Color.Red1;
            Muted = Color.Grey39;
            Highlight = Color.Orange1;
            Border = Color.Red3;
            Alert = Color.Red;
            SecondaryName = "orange1";
            AccentName = "red1";
            HighlightName = "orange1";
        }
    }
}
