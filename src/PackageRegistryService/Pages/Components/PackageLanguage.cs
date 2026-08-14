namespace PackageRegistryService.Pages.Components
{
    /// <summary>
    /// Renders normalized labels for supported package implementation languages.
    /// </summary>
    public class PackageLanguage
    {
        /// <summary>Renders a labeled language badge.</summary>
        public static string Render(string language)
        {
            var style = language.ToLower() switch
            {
                "fsharp" => "background-color:purple; color: white",
                "python" => "background-color:blue; color: white",
                _ => "background-color:red; color: white",
            };

            var name = language.ToLower() switch
            {
                "fsharp" => "F#",
                "python" => "Python",
                _ => "Unknown",
            };

            return $@"Language: <code style='{style}'>{name}</code>";

        }
        /// <summary>Renders only the compact language badge.</summary>
        public static string RenderTagOnly(string language)
        {
            var style = language.ToLower() switch
            {
                "fsharp" => "background-color:purple; color: white",
                "python" => "background-color:blue; color: white",
                _ => "background-color:red; color: white",
            };

            var name = language.ToLower() switch
            {
                "fsharp" => "F#",
                "python" => "Python",
                _ => "Unknown",
            };

            return $@"<code style='{style}'>{name}</code>";

        }
    }
}

