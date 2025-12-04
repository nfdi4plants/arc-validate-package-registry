"""
---
Name: edal
MajorVersion: 0
MinorVersion: 0
PatchVersion: 1
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
Tags:
  - Name: e!DAL
  - Name: data-submission
ReleaseNotes: |
  - Initial 0.0.1 release
    - First working prototype
CQCHookEndpoint: https://mira.ipk-gatersleben.de/submit
---
"""

# /// script
# requires-python = ">=3.14"
# dependencies = [
#     "anybadge",
#     "arctrl==3.0.0b13",
#     "argparse",
#     "junit-xml",
# ]
# ///

import anybadge, argparse, json
from arctrl import ARC
from junit_xml import TestSuite, TestCase
from pathlib import Path

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

report_file: str = f"{args.output}/validation_report.xml"
summary_file: str = f"{args.output}/validation_summary.json"
badge_file: str = f"{args.output}/badge.svg"

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
        "Name": "edal",
        "Version": "0.0.1",
        "Summary": "ARC Validation for e!DAL data repository",
        "Description": "See summary :)"
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
    ts = TestSuite("e!DAL validation tests", test_cases)
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
    badge = anybadge.Badge('e!DAL', value=value, default_color=color)
    badge.write_badge(badge_file, overwrite=True)

def validate_arc():
    arc = ARC.load(args.input)
    add_test_case("Title", None if arc.Title != "" else "No title found.")
    add_test_case("Description", None if arc.Description != "" else "No description found.")
    add_test_case("Contacts", None if check_contacts(arc.Contacts) else "No contacts found.")
    add_test_case("License", None if arc.License else "No license found.")

def check_contacts(contacts):
    if len(contacts) == 0:
        return False
    for c in contacts:
        if c.FirstName == "":
            print(f"No first name found for: {c}")
            return False
        if c.LastName == "":
            print(f"No last name found for: {c}")
            return False
        if c.Affiliation == "":
            print(f"No affiliation found for: {c}")
            return False
        if c.EMail == "":
            print(f"No email found for: {c}")
            return False
        if c.ORCID == "":
            print(f"No ORCID found for: {c}")
            return False
        print(f"Valid contact: {c}")
    
    return True

create_badge(validate_arc())
create_report()
create_summary()
