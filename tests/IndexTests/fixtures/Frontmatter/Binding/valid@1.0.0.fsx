let [<Literal>]PACKAGE_METADATA = """(*
---
Name: valid
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
Summary: My package does the thing.
Description: |
  My package does the thing.
  It does it very good, it does it very well.
  It does it very fast, it does it very swell.
---
*)"""

open System.IO
open System
open System.Text

let f = 
    File.ReadAllText(@"C:\Users\schne\source\repos\nfdi4plants\arc-validate-package-registry\tests\IndexTests\fixtures\Frontmatter\Binding\valid@2.0.0.fsx")
        .ReplaceLineEndings("\n")

/// the frontmatter start string if the package uses yaml frontmatter as comment
let [<Literal>] frontMatterCommentStart = "(*\n---"
/// the frontmatter end string if the package uses yaml frontmatter as comment
let [<Literal>] frontMatterCommentEnd = "---\n*)"

/// the frontmatter start string if the package uses yaml frontmatter as a string binding to be re-used in the package code
let [<Literal>] frontmatterBindingStart = "let [<Literal>]PACKAGE_METADATA = \"\"\"(*\n---"
/// the frontmatter end string if the package uses yaml frontmatter as a string binding to be re-used in the package code
let [<Literal>] frontmatterBindingEnd = "---\n*)\"\"\""


let containsCommentFrontmatter (str: string) =
    str.StartsWith(frontMatterCommentStart, StringComparison.Ordinal) && str.Contains(frontMatterCommentEnd)

let containsBindingFrontmatter (str: string) =
    str.StartsWith(frontmatterBindingStart, StringComparison.Ordinal) && str.Contains(frontmatterBindingEnd)

let tryExtractFromString (str: string) =
    let norm = str.ReplaceLineEndings("\n")
    if containsCommentFrontmatter norm then
        norm.Substring(
            frontMatterCommentStart.Length, 
            (norm.IndexOf(frontMatterCommentEnd, StringComparison.Ordinal) - frontMatterCommentStart.Length))
        |> Some
    elif containsBindingFrontmatter norm then
        norm.Substring(
            frontmatterBindingStart.Length, 
            (norm.IndexOf(frontmatterBindingEnd, StringComparison.Ordinal) - frontmatterBindingStart.Length))
        |> Some
    else 
        None

containsBindingFrontmatter f
tryExtractFromString f