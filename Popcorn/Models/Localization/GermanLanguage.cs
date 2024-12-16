using Popcorn.Models.User;

namespace Popcorn.Models.Localization
{
    public class GermanLanguage : Language
    {
        /// <summary>
        /// Initialize a new instance of GermanLanguage
        /// </summary>
        public GermanLanguage()
        {
            Name = "German";
            Culture = "de";
        }

        /// <summary>
        /// Check equality based on is localized name
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>True if equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            var item = obj as GermanLanguage;

            return item != null && Name.Equals(item.Name);
        }

        /// <summary>
        /// Get hash code based on it localized name
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode() => Name.GetHashCode();
    }
}
