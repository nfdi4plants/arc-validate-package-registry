namespace PackageRegistryService.Pages.Components
{
    public class About
    {
        public static string Render()
        {
            return @"<section>
<h1><strong>AVPR:</strong> ARC validation package registry</h1>
<p>The AVPR indexes and hosts validation packages for <strong>A</strong>nnotated <strong>R</strong>esearch <strong>C</strong>ontexts (ARCs) in a central registry. You can learn more about the concept of ARCs <a href=""https://nfdi4plants.org"">here</a></p>
<p>It is intended for 2 main use cases:
<ol>
<li>Summarize available packages and make them discoverable and inspectable for users that want to incorporate them in their <strong>C</strong>ontinuous <strong>Q</strong>uality <strong>C</strong>ontrol (CQC) workflows locally or on the <a href=""git.nfdi4plants.org"">DataHUB</a>. This is done by providing a <a href=""#browser"">package browser website</a></li>
<li>Provide programmatic access for downloading and veryfing validation packages. This is done by providing <a href=""#api"">Public API Endpoints</a></li>
</ol>
<hr/>
</section>

<section>
<h2 id=""browser""><a class=""contrast"" href=""#browser"">ARC validation package browser</a></h2>
<p>The <a href=""/packages"">ARC validation package browser</a> provides a overview of all available validation packages. You can browse the list of packages and view package details such as tags, author(s), release notes, and the validation code.</p>
<p>For each package, you can also find a link to the respective script in the package staging area, where you can propose changes or new versions.</p>
<hr/>
</section>
<section>
<h2 id=""api""><a class=""contrast"" href=""#api"">ARC validation package API</a></h2>
<p>For downstream tools that want to programmatically query and download available validation packages, AVPR provides a extensively documented public API.</p>
<p>OpenAPI specs and documentation of this API can be accessed <a href=""/swagger"">here</a></p>
<p>Note that packages published on AVPR are intended to be <strong>immutable after publication</strong>. Therefore, any endpoints that can push changes to the database are authorization-only, and not intended to be used by any user-facing tool.</p>
<p>If you have good reasons to request an API key for authorizing to these endpoints, feel free to <a href=""https://github.com/nfdi4plants/arc-validate-package-registry/issues"">contact us via an GitHub issue</a>.</p>
<hr/>
</section>

<section>
<h2 id=""faq""><a class=""contrast"" href=""#faq"">Frequently asked questions</a></h2>
<blockquote>
<b>What is an ARC validation package?</b>
</blockquote>
<p>A validation package bundles a collection of validation cases that an ARC MUST pass to qualify as valid in regard to the validation package with instructions on how to perform the validation and summarize the results.</p>
<p>Validation packages are part of the ARC specification 2.0 draft which can be previewed <a href=""https://github.com/nfdi4plants/ARC-specification/blob/v2.0.0/ARC%20specification.md"">here</a></p>
<p>Validation packages hosted on AVPR are implemented as scripts using the reference implementation <a href=""https://nfdi4plants.github.io/arc-validate/ARCExpect/design.html"">ARCExpect</a></p>
<blockquote>
<b>How can I use a validation package?</b>
</blockquote>
<p>While you can download it directly from the AVPR browser, we strongly recommend installing validation packages either with <a href=""https://github.com/nfdi4plants/arc-validate"">arc-validate</a> - DataPLANT's reference implementation for managing and executing validation packages, or use CQC pipelines on the DataPLANT's reference <a href=""https://git.nfdi4plants.org/explore"">PLANTDataHUB instance</a></p>
<blockquote>
<b>What is Continuous Quality Control (CQC)?</b>
</blockquote>
<p>Continuous Quality Control is a core concept of DataPLANT's research data management stack. In short, it refers to continuously collecting and reporting selected quality metrics of an ARC during its whole lifecycle. This can cover quality of metadata annotations, exportability to endpoint repositories, and much more. One way of achieving this is canstantly validating an ARC against validation packages.</p>
<p>For more information on CQC, please have a look at our <a href=""https://doi.org/10.1111/tpj.16474"">PLANTDataHUB paper</a></p>
<blockquote>
<b>How can i contribute my own validation package?</b>
</blockquote>
<p>In short, you create a validation package by submitting it to the <a href=""https://github.com/nfdi4plants/arc-validate-package-registry/tree/main/src/PackageRegistryService/StagingArea"">AVPR staging area on GitHub</a>. Once it is reviewed and approved, it will be published and available for download and use by others.</p>
<p>The process of submission with the correct metadata is extensively documented <a href=""https://github.com/nfdi4plants/arc-validate-package-registry?tab=readme-ov-file#validation-package-staging-area"">here</a></p>
<p>The process of writing validation packages with the ARCExpect reference implementation is documented <a href=""https://nfdi4plants.github.io/arc-validate/ARCExpect/design.html"">here</a></p>
<blockquote>
<b>How can i validate my ARC locally?</b>
</blockquote>
<p><a href=""https://github.com/nfdi4plants/arc-validate"">arc-validate</a> can be used locally to manage validation packages and validate ARCs against them.</p>
<hr/>
</section>
";
        }
    }
}
