namespace Popcorn.Models.Localization
{
    /// <summary>
    /// French language
    /// </summary>
    public sealed class SpanishLanguage : User.Language
    {
        /// <summary>
        /// Initialize a new instance of FrenchLanguage
        /// </summary>
        public SpanishLanguage()
        {
            Name = "Spanish";
            Culture = "es";
        }

        /// <summary>
        /// Check equality based on is localized name
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>True if equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            var item = obj as SpanishLanguage;

            return item != null && Name.Equals(item.Name);
        }

        /// <summary>
        /// Get hash code based on it localized name
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode() => Name.GetHashCode();
    }
}