namespace PackageRegistryService.Pages
{
    public class Layout
    {
        public static string Render(string title, string content)
        {
            return $@"<!DOCTYPE html>
<html>
    <head>
        <meta charset=""utf-8"" />
        <meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
        <link rel=""stylesheet"" href=""~/css/pico.min.css"">
        <title>{title}</title>
    </head>
    <body>
        {content}
    </body>
</html>";
        }
    }
}
