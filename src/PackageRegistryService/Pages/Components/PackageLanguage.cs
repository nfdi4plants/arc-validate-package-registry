namespace PackageRegistryService.Pages.Components
{
    public class PackageLanguage
    {
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
    }
}


