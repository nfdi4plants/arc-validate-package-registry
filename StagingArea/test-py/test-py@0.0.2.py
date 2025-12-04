PACKAGE_METADATA = """
---
Name: test-py
MajorVersion: 0
MinorVersion: 0
PatchVersion: 2
Publish: true
Summary: this package is here for testing python support.
Description: this package is here for testing python support.
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
  - Name: my-package
  - Name: thing
ReleaseNotes: Add uv inline deps
CQCHookEndpoint: https://avpr.nfdi4plants.org/log-hooks
---
"""

# /// script
# dependencies = [
#   "requests"
# ]
# ///

print("If you can read this in your console, you are executing test-py package v0.0.2!")