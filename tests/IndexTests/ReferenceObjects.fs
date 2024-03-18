module ReferenceObjects

open Utils
open AVPRIndex
open AVPRIndex.Domain

let testDate = System.DateTimeOffset.Parse("01/01/2024")

module Author = 
    
    let mandatoryFields = Author(FullName = "test")

    let allFields =
        Author(
            FullName = "test",
            Email = "test@test.test",
            Affiliation = "testaffiliation",
            AffiliationLink = "test.com"
        )

module OntologyAnnotation = 
    
    let mandatoryFields = OntologyAnnotation(Name = "test")

    let allFields = OntologyAnnotation(
        Name = "test",
        TermSourceREF = "REF",
        TermAccessionNumber = "TAN"
    )

module Frontmatter = 

    let validMandatoryFrontmatter = """(*
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
*)"""                                                                         .ReplaceLineEndings("\n")

    let validMandatoryFrontmatterExtracted = """
Name: valid
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
Summary: My package does the thing.
Description: |
  My package does the thing.
  It does it very good, it does it very well.
  It does it very fast, it does it very swell.
"""                                                                         .ReplaceLineEndings("\n")


    let validFullFrontmatter = """(*
---
Name: valid
MajorVersion: 2
MinorVersion: 0
PatchVersion: 0
Summary: My package does the thing.
Description: |
  My package does the thing.
  It does it very good, it does it very well.
  It does it very fast, it does it very swell.
Publish: true
Authors:
  - FullName: John Doe
    Email: j@d.com
    Affiliation: University of Nowhere
    AffiliationLink: https://nowhere.edu
  - FullName: Jane Doe
    Email: jj@d.com
    Affiliation: University of Somewhere
    AffiliationLink: https://somewhere.edu
Tags:
  - Name: validation
  - Name: my-tag
    TermSourceREF: my-ontology
    TermAccessionNumber: MO:12345
ReleaseNotes: |
  - initial release
    - does the thing
    - does it well
---
*)"""                                                                         .ReplaceLineEndings("\n")

    let validFullFrontmatterExtracted = """
Name: valid
MajorVersion: 2
MinorVersion: 0
PatchVersion: 0
Summary: My package does the thing.
Description: |
  My package does the thing.
  It does it very good, it does it very well.
  It does it very fast, it does it very swell.
Publish: true
Authors:
  - FullName: John Doe
    Email: j@d.com
    Affiliation: University of Nowhere
    AffiliationLink: https://nowhere.edu
  - FullName: Jane Doe
    Email: jj@d.com
    Affiliation: University of Somewhere
    AffiliationLink: https://somewhere.edu
Tags:
  - Name: validation
  - Name: my-tag
    TermSourceREF: my-ontology
    TermAccessionNumber: MO:12345
ReleaseNotes: |
  - initial release
    - does the thing
    - does it well
"""                                                                         .ReplaceLineEndings("\n")

    let invalidStartFrontmatter = """(
---
Name: invalid
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
Summary: My package does the thing.
Description: |
  My package does the thing.
  It does it very good, it does it very well.
  It does it very fast, it does it very swell.
---
*)"""                                                                         .ReplaceLineEndings("\n")

    let invalidEndFrontmatter = """(*
---
Name: invalid
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
Summary: My package does the thing.
Description: |
  My package does the thing.
  It does it very good, it does it very well.
  It does it very fast, it does it very swell.---*)""".ReplaceLineEndings("\n")

    let invalidMissingMandatoryFrontmatter = """(*
---
Name: invalid
MinorVersion: 0
PatchVersion: 0
Summary: My package does the thing.
Description: |
  My package does the thing.
  It does it very good, it does it very well.
  It does it very fast, it does it very swell.
---
*)"""                                                                         .ReplaceLineEndings("\n")

    let invalidMissingMandatoryFrontmatterExtracted = """
Name: invalid
MinorVersion: 0
PatchVersion: 0
Summary: My package does the thing.
Description: |
  My package does the thing.
  It does it very good, it does it very well.
  It does it very fast, it does it very swell.
"""                                                                         .ReplaceLineEndings("\n")

module Metadata =
    
    
    let validMandatoryFrontmatter = 
    
        ValidationPackageMetadata(
            Name = "valid",
            MajorVersion = 1,
            MinorVersion = 0,
            PatchVersion = 0,
            Summary = "My package does the thing.",
            Description = """My package does the thing.
It does it very good, it does it very well.
It does it very fast, it does it very swell.
""".ReplaceLineEndings("\n")
        )
       
    let validFullFrontmatter = 
        ValidationPackageMetadata(
            Name = "valid",
            MajorVersion = 2,
            MinorVersion = 0,
            PatchVersion = 0,
            Summary = "My package does the thing.",
            Publish = true,
            Description = """My package does the thing.
It does it very good, it does it very well.
It does it very fast, it does it very swell.
""".ReplaceLineEndings("\n"),
            Authors = [|
                Author(
                    FullName = "John Doe",
                    Email = "j@d.com",
                    Affiliation = "University of Nowhere",
                    AffiliationLink = "https://nowhere.edu"
                )
                Author(
                    FullName = "Jane Doe",
                    Email = "jj@d.com",
                    Affiliation = "University of Somewhere",
                    AffiliationLink = "https://somewhere.edu"
                )
            |],
            Tags = [|
                OntologyAnnotation(Name = "validation")
                OntologyAnnotation(Name = "my-tag", TermSourceREF = "my-ontology", TermAccessionNumber = "MO:12345")
            |],
            ReleaseNotes = """- initial release
  - does the thing
  - does it well
""".ReplaceLineEndings("\n")
        )

    let invalidMissingMandatoryFrontmatter = 
        ValidationPackageMetadata(
            Name = "invalid",
            MajorVersion = -1,
            MinorVersion = 0,
            PatchVersion = 0,
            Summary = "My package does the thing.",
            Description = """My package does the thing.
It does it very good, it does it very well.
It does it very fast, it does it very swell.
""".ReplaceLineEndings("\n")
        )

module ValidationPackageIndex =
    
    let validMandatoryFrontmatter = 
        ValidationPackageIndex.create(
            repoPath = "fixtures/valid@1.0.0.fsx",
            fileName = "valid@1.0.0.fsx",
            lastUpdated = testDate,
            contentHash = (Frontmatter.validMandatoryFrontmatter |> md5hash),
            metadata = Metadata.validMandatoryFrontmatter
        )

    let validFullFrontmatter = 
        ValidationPackageIndex.create(
            repoPath = "fixtures/valid@2.0.0.fsx",
            fileName = "valid@2.0.0.fsx",
            lastUpdated = testDate,
            contentHash = (Frontmatter.validFullFrontmatter |> md5hash),
            metadata = Metadata.validFullFrontmatter
        )

    let invalidMissingMandatoryFrontmatter = 
        ValidationPackageIndex.create(
            repoPath = "fixtures/invalid@0.0.fsx",
            fileName = "invalid@0.0.fsx",
            lastUpdated = testDate,
            contentHash = (Frontmatter.invalidMissingMandatoryFrontmatter |> md5hash),
            metadata = Metadata.invalidMissingMandatoryFrontmatter
        )