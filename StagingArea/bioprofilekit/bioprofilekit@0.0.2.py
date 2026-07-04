"""
---
Name: bioprofilekit
MajorVersion: 0
MinorVersion: 0
PatchVersion: 2
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
  - updated arcexpect to ensure correct creation of payload
CQCHookEndpoint: https://bioprofilekit.computational.bio/ready
---
"""

# /// script
# requires-python = ">=3.11"
# dependencies = [
#     "arctrl==3.1.0",
#     "arcexpect==0.0.4",
# ]
# ///

from __future__ import annotations

import argparse
from pathlib import Path

from arctrl import ARC, start_as_task, DataContext, JsonController, Datamap
from arcexpect import Execute, Expect, Setup, test_case, test_list
from collections.abc import Iterable, Mapping

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

def get_datamaps() -> list[Datamap]:
    datamaps = []
    for study in arc.Studies:
        if study.Datamap is not None:
            datamaps.append(study.Datamap)
    for assay in arc.Assays:
        if assay.Datamap is not None:
            datamaps.append(assay.Datamap)
    for workflow in arc.Workflows:
        if workflow.Datamap is not None:
            datamaps.append(workflow.Datamap)   
    for run in arc.Runs:
        if run.Datamap is not None:
            datamaps.append(run.Datamap)
    return datamaps

# def datacontext_is_relevant(datacontext : DataContext) -> bool:
#     return datacontext.Explication.NameText != "" and datacontext.FilePath != None  
    
def arc_has_datamap() -> None:
    datamaps = get_datamaps()
    Expect.is_true(len(datamaps) > 0, "No datamap found in the ARC.")


if arc_error is not None:
    testList = [test_case("load ARC", lambda: Expect.is_true(False, f"Failed to load ARC: {arc_error}"))]
else:
    testList = [
        test_case("load ARC", lambda: None),
        test_case("Datamap", arc_has_datamap),
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

payload : Mapping[str, str]= dict()

if arc is not None:
    if len(get_datamaps()) > 0:
        for datamap in get_datamaps():
            payload[str(datamap.__hash__())] = JsonController.Datamap().to_json_string(datamap, 0)

Execute.validation_pipeline(package, output_dir, payload = payload)
