"""
---
Name: treem-sequencing
MajorVersion: 0
MinorVersion: 0
PatchVersion: 1
Summary: TreeM RNA Sequencing ARC Validation
Description: |
  This python package validates ARCs for the TreeM consortium RNA Sequencing.
Publish: true
Authors:
  - FullName: Kristian Peters
    Email: kristian.peters@computational.bio.uni-giessen.de
    Affiliation: JLU Giessen
    AffiliationLink: https://www.uni-giessen.de
Tags:
  - Name: TreeM
  - Name: RNA-Sequencing
ReleaseNotes: |
  - initial release
---
"""

# /// script
# requires-python = ">=3.14"
# dependencies = [
#     "anybadge",
#     "arctrl==3.0.4",
#     "argparse",
#     "junit-xml",
# ]
# ///

import string

import anybadge, argparse, json
from arctrl import ARC, ArcTable, CompositeHeader, CompositeCell, IOType, OntologyAnnotation
from junit_xml import TestSuite, TestCase
from pathlib import Path
import os

parser = argparse.ArgumentParser()
parser.add_argument(
    '-i', '--input',
    type=str,
    required=True,
    help='Path to the ARC directory'
)
parser.add_argument(
    '-o', '--output',
    type=str,
    required=True,
    help='Output directory'
)
args = parser.parse_args()

Path(args.output).mkdir(parents=True, exist_ok=True)
os.makedirs(f"{args.output}/.arc-validate-results/treem-sequencing@0.0.1", exist_ok=True)

report_file: str = f"{args.output}/.arc-validate-results/treem-sequencing@0.0.1/validation_report.xml"
summary_file: str = f"{args.output}/.arc-validate-results/treem-sequencing@0.0.1/validation_summary.json"
badge_file: str = f"{args.output}/.arc-validate-results/treem-sequencing@0.0.1/badge.svg"
test_cases = []
summary_data = {
    "Critical": {
        "HasFailures": "false",
        "Total": 0,
        "Passed": 0,
        "Failed": 0,
        "Errored": 0
    },
    "NonCritical": {
        "HasFailures": "false",
        "Total": 0,
        "Passed": 0,
        "Failed": 0,
        "Errored": 0
    },
    "ValidationPackage": {
        "Name": "treem-sequencing",
        "Version": "0.0.1",
        "Summary": "TreeM RNA Sequencing ARC Validation",
        "Description": "This python package validates ARCs for the TreeM consortium RNA Sequencing.",
    }
}

def add_test_case(name: str, failure: str = None):
    case = TestCase(name)
    summary_data["Critical"]["Total"] += 1
    if failure:
        case.add_failure_info(message=failure)
        summary_data["Critical"]["HasFailures"] = "true"
        summary_data["Critical"]["Failed"] += 1
    else:
        summary_data["Critical"]["Passed"] += 1
    test_cases.append(case)
    print(summary_data)

def create_report():
    ts = TestSuite("TreeM RNA Sequencing validation tests", test_cases)
    print(TestSuite.to_xml_string([ts]))
    with open(report_file, 'w') as f:
        TestSuite.to_file(f, [ts], prettyprint=True)


def create_summary():
    summary_json = json.dumps(summary_data)

    print(f"Summary JSON: {summary_json}")

    with open(summary_file, 'w') as f:
        json.dump(summary_data, f, indent=4)

def create_badge(success: bool):
    color = 'green' if success else 'red' 
    value = 'Submit' if success else 'Reports' 
    badge = anybadge.Badge('TreeM RNA Sequencing', value=value, default_color=color)
    badge.write_badge(badge_file, overwrite=True)


def tryGetSequencingTable(arc: ARC):
    for a in arc.Assays:
        for t in a.Tables:
            protocolTypeColumn = t.TryGetColumnByHeader(CompositeHeader.protocol_type())
            if protocolTypeColumn:              
                for c in protocolTypeColumn.Cells:
                    if c.AsTerm.NameText == "Sequencing":
                        return t
    return None

def containsNGSInstrument(t : ArcTable):
    hasInstrument = False
    for c in t.GetComponentColumns():
        if c.Header.TryGetTerm().NameText.__contains__("Instrument"):
            hasInstrument = True
    return hasInstrument

def tryGetTreeMIdentiferColumn(t : ArcTable):
    treeMHeader = CompositeHeader.characteristic(OntologyAnnotation("TreeM Sample Identifier"))
    return t.TryGetColumnByHeader(treeMHeader)
    
def treeMIdentifierIsValid(s:str):
    return s.__contains__("-")

def validate_arc():
    print(f"Validating ARC at: {args.input}")
    arc = ARC.load(args.input)    
    print(f"Loaded ARC: {arc.Identifier}")
    sequenceTable = tryGetSequencingTable(arc)
    print(sequenceTable)
    if sequenceTable:
        add_test_case("Instrument", None if containsNGSInstrument(sequenceTable) else "Sequencing table does not contain any instrument component column.")
        treeMColumn = tryGetTreeMIdentiferColumn(sequenceTable)
        if treeMColumn:
            for c in treeMColumn.Cells:
                if not treeMIdentifierIsValid(c.AsTerm.NameText):
                    add_test_case("TreeM Sample Identifier", f"Invalid TreeM Sample Identifier found: {c.AsTerm.NameText}")
                else:
                    add_test_case("TreeM Sample Identifier", None)
        else:
            add_test_case("TreeM Sample IdentifierColumn","No Characteristric[TreeM Sample Identifier] column found")
    else:
        add_test_case("Sequencing Type", "No Table with protocol type 'Sequencing' was found in any of the assays.") 

valid = validate_arc()

valid = True
try:
    valid = validate_arc()
except Exception:
    valid = False


create_badge(valid)
create_report()
create_summary()

exit(0)
