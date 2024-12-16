namespace Popcorn.Models.Localization
{
    /// <summary>
    /// English language
    /// </summary>
    public sealed class EnglishLanguage : User.Language
    {
        /// <summary>
        /// Initialize a new instance of EnglishLanguage
        /// </summary>
        public EnglishLanguage()
        {
            Name = "English";
            Culture = "en";
        }

        /// <summary>
        /// Check equality based on is localized name
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>True if equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            var item = obj as EnglishLanguage;

            return item != null && Name.Equals(item.Name);
        }

        /// <summary>
        /// Get hash code based on it localized name
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode() => Name.GetHashCode();
    }
}