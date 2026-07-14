using MELE_launcher.Models;
using Xunit;

namespace MELE_launcher.Tests
{
    public class LocaleMapperTests
    {
        [Theory]
        // English is universal across all games.
        [InlineData("INT", "INT", GameType.ME1, "INT")]
        [InlineData("INT", "INT", GameType.ME2, "INT")]
        [InlineData("INT", "INT", GameType.ME3, "INT")]
        // ME1 specific codes (text + English VO vs native VO).
        [InlineData("FR", "INT", GameType.ME1, "FE")]
        [InlineData("FR", "FR", GameType.ME1, "FR")]
        [InlineData("DE", "INT", GameType.ME1, "GE")]
        [InlineData("DE", "DE", GameType.ME1, "DE")]
        [InlineData("IT", "INT", GameType.ME1, "IE")]
        [InlineData("IT", "IT", GameType.ME1, "IT")]
        [InlineData("RU", "INT", GameType.ME1, "RU")]
        [InlineData("RU", "RU", GameType.ME1, "RA")]
        [InlineData("PL", "INT", GameType.ME1, "PL")]
        [InlineData("PL", "PL", GameType.ME1, "PLPC")]
        [InlineData("JA", "INT", GameType.ME1, "JA")]
        // ME2 specific codes.
        [InlineData("FR", "INT", GameType.ME2, "FRE")]
        [InlineData("FR", "FR", GameType.ME2, "FRA")]
        [InlineData("DE", "DE", GameType.ME2, "DEU")]
        [InlineData("PL", "PL", GameType.ME2, "POL")]
        [InlineData("PL", "INT", GameType.ME2, "POE")]
        [InlineData("JA", "JA", GameType.ME2, "JPN")]
        // ME3 specific codes (Polish has no native VO in ME3).
        [InlineData("FR", "FR", GameType.ME3, "FRA")]
        [InlineData("DE", "INT", GameType.ME3, "DEE")]
        [InlineData("PL", "PL", GameType.ME3, "POL")]
        [InlineData("PL", "INT", GameType.ME3, "POL")]
        public void GetGameLocaleCode_ReturnsExpectedCode(string text, string voice, GameType gameType, string expected)
        {
            Assert.Equal(expected, LocaleMapper.GetGameLocaleCode(text, voice, gameType));
        }

        [Fact]
        public void GetGameLocaleCode_IsCaseInsensitive()
        {
            Assert.Equal("FR", LocaleMapper.GetGameLocaleCode("fr", "fr", GameType.ME1));
            Assert.Equal("FRA", LocaleMapper.GetGameLocaleCode("Fr", "fR", GameType.ME2));
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData(null, "INT")]
        [InlineData("INT", null)]
        public void GetGameLocaleCode_NullOrEmpty_DefaultsToEnglish(string text, string voice)
        {
            Assert.Equal("INT", LocaleMapper.GetGameLocaleCode(text, voice, GameType.ME2));
        }

        [Theory]
        [InlineData("XX", "XX")]
        [InlineData("FR", "DE")] // unmapped text/voice combination
        public void GetGameLocaleCode_UnknownCombination_FallsBackToEnglish(string text, string voice)
        {
            Assert.Equal("INT", LocaleMapper.GetGameLocaleCode(text, voice, GameType.ME1));
        }

        [Fact]
        public void GetGameLocaleCode_UnknownGameType_ReturnsEnglish()
        {
            Assert.Equal("INT", LocaleMapper.GetGameLocaleCode("FR", "FR", (GameType)999));
        }

        [Fact]
        public void GetLanguageOption_KnownCode_ReturnsMatch()
        {
            var option = LocaleMapper.GetLanguageOption("FR");
            Assert.Equal("FR", option.Code);
            Assert.Equal("French", option.DisplayName);
        }

        [Fact]
        public void GetLanguageOption_IsCaseInsensitive()
        {
            Assert.Equal("DE", LocaleMapper.GetLanguageOption("de").Code);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("does-not-exist")]
        public void GetLanguageOption_UnknownOrEmpty_DefaultsToEnglish(string code)
        {
            Assert.Equal("INT", LocaleMapper.GetLanguageOption(code).Code);
        }

        [Fact]
        public void GetDisplayString_ReturnsDisplayName()
        {
            var option = LocaleMapper.GetLanguageOption("IT");
            Assert.Equal(option.DisplayName, option.GetDisplayString());
        }

        [Fact]
        public void AvailableLanguages_ContainsExpectedCodes()
        {
            var codes = LocaleMapper.AvailableLanguages.ConvertAll(l => l.Code);
            Assert.Contains("INT", codes);
            Assert.Contains("FR", codes);
            Assert.Contains("JA", codes);
            Assert.Equal(8, LocaleMapper.AvailableLanguages.Count);
        }

        [Theory]
        [InlineData("INT", GameType.ME1, true)]
        [InlineData("INT", GameType.ME3, true)]
        [InlineData("FR", GameType.ME1, true)]
        [InlineData("DE", GameType.ME2, true)]
        [InlineData("IT", GameType.ME3, true)]
        [InlineData("RU", GameType.ME2, true)]
        [InlineData("PL", GameType.ME1, true)]
        [InlineData("PL", GameType.ME2, true)]
        [InlineData("PL", GameType.ME3, false)] // No Polish VO in ME3
        [InlineData("ES", GameType.ME1, false)] // Spanish has no native VO
        [InlineData("JA", GameType.ME2, false)]
        public void HasNativeVoiceOver_ReturnsExpected(string code, GameType gameType, bool expected)
        {
            Assert.Equal(expected, LocaleMapper.HasNativeVoiceOver(code, gameType));
        }

        [Fact]
        public void HasNativeVoiceOver_IsCaseInsensitive()
        {
            Assert.True(LocaleMapper.HasNativeVoiceOver("fr", GameType.ME1));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void HasNativeVoiceOver_NullOrEmpty_ReturnsFalse(string code)
        {
            Assert.False(LocaleMapper.HasNativeVoiceOver(code, GameType.ME1));
        }
    }
}
