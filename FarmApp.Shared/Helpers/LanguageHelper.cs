namespace FarmApp.Shared.Helpers
{
    public static class LanguageHelper
    {
        /// <summary>
        /// Normilizes language code to full language name. For example, "en" becomes "English", "uk" becomes "Українська", etc. 
        /// If the language code is not recognized, it defaults to "English".
        /// </summary>
        public static string NormalizeLanguage(string language)
        {
            return language switch
            {
                "en" => "English",
                "uk" => "Українська",
                "it" => "Italiano",
                "pl" => "Polski",
                "de" => "Deutsch",
                _ => "English"
            };
        }
        /// <summary>
        /// Gets language code from full language name. For example, "English" becomes "en", "Українська" becomes "uk", etc.
        /// </summary>
        public static string GetLanguageCode(string language)
        {
            return language switch
            {
                "English" => "en",
                "Українська" => "uk",
                "Italiano" => "it",
                "Polski" => "pl",
                "Deutsch" => "de",
                _ => "en"
            };
        }
    }
}
