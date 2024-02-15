namespace PackageRegistryService.Pages.Components
{
    public static class Navbar
    {
        public static string Render(string active)
        {
            return @"<nav>
  <ul>
    <li><strong>AVPR - a service by <a>DataPLANT</a></strong></li>
  </ul>
  <ul>
    <li><a href=""#"">About</a></li>
    <li><a href=""#"">Services</a></li>
    <li><a href=""#"">Products</a></li>
  </ul>
</nav>";
        }
    }
}
