using System.Reflection;
using WPFLocalizeExtension.Extensions;

namespace Popcorn.Helpers
{
    /// <summary>
    /// Used to localize resources on the fly using WPF Localize Extension
    /// </summary>
    public static class LocalizationProviderHelper
    {
        /// <summary>
        /// Retrieve the localized value of a resource based on its key
        /// </summary>
        /// <typeparam name="T">Type of value to localize</typeparam>
        /// <param name="key">Resource key</param>
        /// <returns>Localized value</returns>
        public static T GetLocalizedValue<T>(string key)
            => LocExtension.GetLocalizedValue<T>(Assembly.GetCallingAssembly().GetName().Name + ":Resources:" + key);
    }
}