PACKAGE_METADATA = """
---
Name: bioprofilekit
MajorVersion: 0
MinorVersion: 0
PatchVersion: 1
Summary: BioProfileKit validation package for explorative data analysis
Description: |
  BioProfileKit is a web application for explorative data analysis of biological data. 
  This python package validates ARCs for containment of relevant data for the BioProfileKit web application.
Publish: true
Authors:
  - FullName: Sonja Diedrich
    Email: sonja.diedrich@computational.bio.uni-giessen.de
    Affiliation: JLU Giessen
    AffiliationLink: https://www.uni-giessen.de
  - FullName: Maria Hansen
    Email: maria.hansen@computational.bio.uni-giessen.de
    Affiliation: JLU Giessen
    AffiliationLink: https://www.uni-giessen.de
  - FullName: Julian Hahnfeld
    Email: julian.hahnfeld@computational.bio.uni-giessen.de
    Affiliation: JLU Giessen
    AffiliationLink: https://www.uni-giessen.de
  - FullName: Heinrich Lukas Weil
    Email: weil@rptu.de
    Affiliation: RPTU Kaiserslautern-Landau
    AffiliationLink: https://www.rptu.de
  - FullName: Jonathan Bauer
    Email: jonathan.bauer@rz.uni-freiburg.de
    Affiliation: University of Freiburg
    AffiliationLink: https://uni-freiburg.de
Tags:
  - Name: BioProfileKit
  - Name: explorative-data-analysis
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


parser = argparse.ArgumentParser(description="Validate an ARC for BioProfileKit submission.")
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

def arc_has_datamap() -> None:
    datamaps = arc.Studies.iter


if arc_error is not None:
    testList = [test_case("load ARC", lambda: Expect.is_true(False, f"Failed to load ARC: {arc_error}"))]
else:
    testList = [
        test_case("load ARC", lambda: None),
        test_case("Title", arc_has_title),
    ]

package = Setup.validation_package_from_script(
    __file__,
    critical=[
        test_list(
            "BioProfileKit ARC validation",
            testList
        )
    ],
)

Execute.validation_pipeline(package, output_dir)
