#r "nuget: YamlDotNet, 15.1.2"
#r "../src/AVPRIndex/bin/Debug/net8.0/AVPRIndex.dll"

open AVPRIndex
open AVPRIndex.Domain
open AVPRIndex.Frontmatter
open YamlDotNet.Serialization

open System
open System.IO
open System.Text

let yamlDeserializer = 
    DeserializerBuilder()
        .WithNamingConvention(NamingConventions.PascalCaseNamingConvention.Instance)
        .Build()

let p = @"C:\Users\schne\source\repos\nfdi4plants\arc-validate-package-registry\src\PackageRegistryService\StagingArea\pride\pride@1.0.0.fsx"

ValidationPackageMetadata.extractFromScript(p)

let f = File.ReadAllText(p).ReplaceLineEndings()

f.ReplaceLineEndings().StartsWith(Frontmatter.frontMatterStart)

f.ReplaceLineEndings().Contains(Frontmatter.frontMatterEnd)

let fm = 
    f.Substring(
        frontMatterStart.Length, 
        (f.IndexOf(Frontmatter.frontMatterEnd, StringComparison.Ordinal) - frontMatterEnd.Length)
    )

yamlDeserializer.Deserialize<ValidationPackageMetadata>(fm)