using System.Collections.Generic;
using System.Linq;

namespace MELE_launcher.Models
{
    /// <summary>
    /// Maps user-friendly language names to BioWare's game-specific locale codes.
    /// BioWare uses different codes for ME1 vs ME2/ME3, and supports separate text and voice-over languages.
    /// </summary>
    public static class LocaleMapper
    {
        /// <summary>
        /// Available languages with display names.
        /// </summary>
        public static readonly List<LanguageOption> AvailableLanguages = new()
        {
            new LanguageOption { Code = "INT", DisplayName = "English", Description = "English" },
            new LanguageOption { Code = "FR", DisplayName = "French", Description = "French" },
            new LanguageOption { Code = "DE", DisplayName = "German", Description = "German" },
            new LanguageOption { Code = "ES", DisplayName = "Spanish", Description = "Spanish" },
            new LanguageOption { Code = "IT", DisplayName = "Italian", Description = "Italian" },
            new LanguageOption { Code = "RU", DisplayName = "Russian", Description = "Russian" },
            new LanguageOption { Code = "PL", DisplayName = "Polish", Description = "Polish" },
            new LanguageOption { Code = "JA", DisplayName = "Japanese", Description = "Japanese" }
        };

        /// <summary>
        /// Maps language codes to Mass Effect 1 locale codes.
        /// Format: [TextLanguage][VoiceLanguage] where E = English VO
        /// </summary>
        private static readonly Dictionary<(string text, string voice), string> ME1LocaleCodes = new()
        {
            // English text
            { ("INT", "INT"), "INT" },   // English text, English VO
            
            // French
            { ("FR", "INT"), "FE" },     // French text, English VO
            { ("FR", "FR"), "FR" },      // French text, French VO
            
            // German
            { ("DE", "INT"), "GE" },     // German text, English VO
            { ("DE", "DE"), "DE" },      // German text, German VO
            
            // Spanish
            { ("ES", "INT"), "ES" },     // Spanish text, English VO
            { ("ES", "ES"), "ES" },      // Spanish text (no separate VO)
            
            // Italian
            { ("IT", "INT"), "IE" },     // Italian text, English VO
            { ("IT", "IT"), "IT" },      // Italian text, Italian VO
            
            // Russian
            { ("RU", "INT"), "RU" },     // Russian text, English VO
            { ("RU", "RU"), "RA" },      // Russian text, Russian VO
            
            // Polish
            { ("PL", "INT"), "PL" },     // Polish text, English VO
            { ("PL", "PL"), "PLPC" },    // Polish text, Polish VO
            
            // Japanese
            { ("JA", "INT"), "JA" },     // Japanese text, English VO
            { ("JA", "JA"), "JA" }       // Japanese text (no separate VO)
        };

        /// <summary>
        /// Maps language codes to Mass Effect 2 locale codes.
        /// Format: [TextLanguage][VoiceLanguage] where E suffix = English VO
        /// </summary>
        private static readonly Dictionary<(string text, string voice), string> ME2LocaleCodes = new()
        {
            // English text
            { ("INT", "INT"), "INT" },   // English text, English VO
            
            // French
            { ("FR", "INT"), "FRE" },    // French text, English VO
            { ("FR", "FR"), "FRA" },     // French text, French VO
            
            // German
            { ("DE", "INT"), "DEE" },    // German text, English VO
            { ("DE", "DE"), "DEU" },     // German text, German VO
            
            // Spanish
            { ("ES", "INT"), "ESN" },    // Spanish text, English VO
            { ("ES", "ES"), "ESN" },     // Spanish text (no separate VO)
            
            // Italian
            { ("IT", "INT"), "ITE" },    // Italian text, English VO
            { ("IT", "IT"), "ITA" },     // Italian text, Italian VO
            
            // Russian
            { ("RU", "INT"), "RUS" },    // Russian text, English VO
            { ("RU", "RU"), "RUS" },     // Russian text, Russian VO
            
            // Polish
            { ("PL", "INT"), "POE" },    // Polish text, English VO
            { ("PL", "PL"), "POL" },     // Polish text, Polish VO
            
            // Japanese
            { ("JA", "INT"), "JPN" },    // Japanese text, English VO
            { ("JA", "JA"), "JPN" }      // Japanese text (no separate VO)
        };

        /// <summary>
        /// Maps language codes to Mass Effect 3 locale codes.
        /// Format: [TextLanguage][VoiceLanguage] where E suffix = English VO
        /// </summary>
        private static readonly Dictionary<(string text, string voice), string> ME3LocaleCodes = new()
        {
            // English text
            { ("INT", "INT"), "INT" },   // English text, English VO
            
            // French
            { ("FR", "INT"), "FRE" },    // French text, English VO
            { ("FR", "FR"), "FRA" },     // French text, French VO
            
            // German
            { ("DE", "INT"), "DEE" },    // German text, English VO
            { ("DE", "DE"), "DEU" },     // German text, German VO
            
            // Spanish
            { ("ES", "INT"), "ESN" },    // Spanish text, English VO
            { ("ES", "ES"), "ESN" },     // Spanish text (no separate VO)
            
            // Italian
            { ("IT", "INT"), "ITE" },    // Italian text, English VO
            { ("IT", "IT"), "ITA" },     // Italian text, Italian VO
            
            // Russian
            { ("RU", "INT"), "RUS" },    // Russian text, English VO
            { ("RU", "RU"), "RUS" },     // Russian text, Russian VO
            
            // Polish (Note: ME3 has English VO for Polish, ME2 had Polish VO)
            { ("PL", "INT"), "POL" },    // Polish text, English VO
            { ("PL", "PL"), "POL" },     // Polish text, English VO (no Polish VO in ME3)
            
            // Japanese
            { ("JA", "INT"), "JPN" },    // Japanese text, English VO
            { ("JA", "JA"), "JPN" }      // Japanese text (no separate VO)
        };

        /// <summary>
        /// Gets the game-specific locale code for given text and voice languages.
        /// </summary>
        /// <param name="textLanguage">The language code for text/subtitles (e.g., "INT", "FR", "DE").</param>
        /// <param name="voiceLanguage">The language code for voice-over (e.g., "INT", "FR", "DE").</param>
        /// <param name="gameType">The type of Mass Effect game.</param>
        /// <returns>The game-specific locale code to use in launch arguments.</returns>
        public static string GetGameLocaleCode(string textLanguage, string voiceLanguage, GameType gameType)
        {
            if (string.IsNullOrEmpty(textLanguage))
                textLanguage = "INT";
            if (string.IsNullOrEmpty(voiceLanguage))
                voiceLanguage = "INT";

            var textCode = textLanguage.ToUpperInvariant();
            var voiceCode = voiceLanguage.ToUpperInvariant();
            var key = (textCode, voiceCode);

            return gameType switch
            {
                GameType.ME1 => ME1LocaleCodes.TryGetValue(key, out var me1Code) ? me1Code : "INT",
                GameType.ME2 => ME2LocaleCodes.TryGetValue(key, out var me2Code) ? me2Code : "INT",
                GameType.ME3 => ME3LocaleCodes.TryGetValue(key, out var me3Code) ? me3Code : "INT",
                _ => "INT"
            };
        }

        /// <summary>
        /// Gets a language option by its universal code.
        /// </summary>
        /// <param name="code">The universal language code.</param>
        /// <returns>The language option, or English if not found.</returns>
        public static LanguageOption GetLanguageOption(string code)
        {
            if (string.IsNullOrEmpty(code))
                return AvailableLanguages[0]; // Default to English

            var upperCode = code.ToUpperInvariant();
            return AvailableLanguages.Find(l => l.Code == upperCode) ?? AvailableLanguages[0];
        }

        /// <summary>
        /// Checks if a language has native voice-over support in a specific game.
        /// </summary>
        /// <param name="languageCode">The language code to check.</param>
        /// <param name="gameType">The game type.</param>
        /// <returns>True if the language has native voice-over support.</returns>
        public static bool HasNativeVoiceOver(string languageCode, GameType gameType)
        {
            if (string.IsNullOrEmpty(languageCode))
                return false;

            var code = languageCode.ToUpperInvariant();

            // English always has native VO
            if (code == "INT")
                return true;

            // Languages with native voice-over support
            var nativeVOLanguages = new[] { "FR", "DE", "IT", "RU", "PL" };

            // Polish VO only in ME1 and ME2, not ME3
            if (code == "PL" && gameType == GameType.ME3)
                return false;

            return nativeVOLanguages.Contains(code);
        }
    }

    /// <summary>
    /// Represents a language option with display information.
    /// </summary>
    public class LanguageOption
    {
        /// <summary>
        /// Universal language code used internally (e.g., "INT", "FR", "DE").
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Display name for the language (e.g., "English", "French").
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Additional description (e.g., "French Voice-Over").
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets the formatted display string for selection menus.
        /// </summary>
        public string GetDisplayString()
        {
            return DisplayName;
        }
    }
}
