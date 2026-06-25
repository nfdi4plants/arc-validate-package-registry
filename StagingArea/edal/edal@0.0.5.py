"""
---
Name: edal
MajorVersion: 0
MinorVersion: 0
PatchVersion: 5
Summary: e!DAL validation package for submission
Description: |
  This python package validates ARCs for the e!DAL
  PGP research data repository.
Publish: true
Authors:
  - FullName: Jonathan Bauer
    Email: bauer@nfdi4plants.org
    Affiliation: University of Freiburg
    AffiliationLink: https://uni-freiburg.de
  - FullName: Heinrich Lukas Weil
    Email: weil@rptu.de
    Affiliation: RPTU Kaiserslautern-Landau
    AffiliationLink: https://www.rptu.de
Tags:
  - Name: e!DAL
  - Name: data-submission
ReleaseNotes: |
  - fixed folder output path again
CQCHookEndpoint: https://mira.ipk-gatersleben.de/submit
---
"""

# /// script
# requires-python = ">=3.11"
# dependencies = [
#     "arctrl==3.1.0",
#     "arcexpect==0.0.3",
# ]
# ///

from __future__ import annotations

import argparse
from pathlib import Path

from arctrl import ARC, start_as_task
from arcexpect import Execute, Expect, Setup, test_case, test_list


parser = argparse.ArgumentParser(description="Validate an ARC for e!DAL submission.")
parser.add_argument("-i", "--input", required=True, type=Path, help="Path to the ARC directory")
parser.add_argument("-o", "--output", required=False, type=Path, help="Directory for validation results")
args = parser.parse_args()

output_dir = str(args.output if args.output else args.input)

arc: ARC | None = None
arc_error: str | None = None

try:
    arc = ARC.load(str(args.input))
except Exception as e:
    arc_error = str(e)


def arc_has_title() -> None:
    Expect.is_true(arc.Title != "", "No title found.")

def arc_has_description() -> None:
    Expect.is_true(arc.Description != "", "No description found.")

def arc_has_valid_contacts() -> None:
    contacts = arc.Contacts
    Expect.is_true(len(contacts) > 0, "No contacts found.")
    for c in contacts:
        Expect.is_true(c.FirstName != "", f"No first name found for contact: {c}")
        Expect.is_true(c.LastName != "", f"No last name found for contact: {c}")
        Expect.is_true(c.Affiliation != "", f"No affiliation found for contact: {c}")
        Expect.is_true(c.EMail != "", f"No email found for contact: {c}")
        Expect.is_true(c.ORCID != "", f"No ORCID found for contact: {c}")

def arc_has_license() -> None:
    Expect.is_true(bool(arc.License), "No license found.")


if arc_error is not None:
    testList = [test_case("load ARC", lambda: Expect.is_true(False, f"Failed to load ARC: {arc_error}"))]
else:
    testList = [
        test_case("load ARC", lambda: None),
        test_case("Title", arc_has_title),
        test_case("Description", arc_has_description),
        test_case("Contacts", arc_has_valid_contacts),
        test_case("License", arc_has_license),
    ]

package = Setup.validation_package_from_script(
    __file__,
    critical=[
        test_list(
            "e!DAL ARC validation",
            testList
        )
    ],
)

Execute.validation_pipeline(package, output_dir)
