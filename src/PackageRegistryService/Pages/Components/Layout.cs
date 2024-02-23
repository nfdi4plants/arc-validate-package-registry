namespace PackageRegistryService.Pages.Components
{
    public static class Layout
    {
        public static string Render(string activeNavbarItem, string title, string content)
        {
            return $@"<!DOCTYPE html>
<html>
    <head>
        <meta charset=""utf-8"">
        <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
        <meta name=""color-scheme"" content=""light dark"" />
        <link rel=""stylesheet"" href=""/css/pico.cyan.min.css"" />
        <title>{title}</title>
    </head>
    <body>
        <header class=""container"">
            {Navbar.Render(active: activeNavbarItem)}
        </header>
        <main class=""container"">
            {content}
        </main>
        <footer></footer>
    </body>
</html>";
        }
    }
}
