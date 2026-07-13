"""
---
Name: isa-ro-crate
MajorVersion: 0
MinorVersion: 0
PatchVersion: 1
Summary: ISA-RO-Crate validation package for submission
Description: |
  This python package validates ARCs for conformance with the ISA-RO-Crate profile.
Publish: false
Authors:
  - FullName: Florian Wetzels
    Email: wetzels@cs.uni-kl.de
    Affiliation: RPTU
    AffiliationLink: https://rptu.de
Tags:
  - Name: isa-ro-crate
ReleaseNotes: |
  - proof-of-concept implementation of SHACL validation
---
"""

# /// script
# requires-python = ">=3.14"
# dependencies = [
#     "anybadge",
#     "arctrl==3.0.0b16",
#     "argparse",
#     "junit-xml",
#     "pyshacl",
#     "rdflib"
# ]
# ///

from pyexpat.errors import messages
import anybadge, argparse, json
from arctrl import ARC
from junit_xml import TestSuite, TestCase
from pathlib import Path
import pyshacl
from rdflib import Graph, Namespace
from rdflib.namespace import RDF

SH = Namespace("http://www.w3.org/ns/shacl#")

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

report_file: str = f"{args.output}/.arc-validate-results/validation_report.xml"
summary_file: str = f"{args.output}/.arc-validate-results/validation_summary.json"
badge_file: str = f"{args.output}/.arc-validate-results/badge.svg"

Path(f"{args.output}/.arc-validate-results").mkdir(parents=True, exist_ok=True)

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
        "Name": "isa-ro-crate",
        "Version": "0.0.1",
        "Summary": "ARC Validation for ISA-RO-Crate data repository",
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
    ts = TestSuite("ISA-RO-Crate validation tests", test_cases)
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
    value = 'Passed' if success else 'Failed' 
    badge = anybadge.Badge('ISA-RO-Crate', value=value, default_color=color)
    badge.write_badge(badge_file, overwrite=True)

def validate_arc():
    arc = ARC.load(args.input).ToROCrateJsonString(2)
    # add_test_case("Investigation", None if check_investigation(arc) else "Investigation validation failed.")
    sc,sm = check_studies(arc)
    if sc:
        add_test_case("Study", None)
    else:
        for m in sm:
            add_test_case("Study", m[0] + " failed on node " + m[2] + ": " + m[1])
    ac,am = check_assays(arc)
    if ac:
        add_test_case("Assay", None)
    else:
        for m in am:
            add_test_case("Assay", m[0] + " failed on node " + m[2] + ": " + m[1])
    pc,pm = check_persons(arc)
    if pc:
        add_test_case("Person", None)
    else:
        for m in pm:
            add_test_case("Person", m[0] + " failed on node " + m[2] + ": " + m[1])
    return sc and ac and pc

person_ttl ="""
@prefix ro: <./> .
@prefix ro-crate: <https://github.com/crs4/rocrate-validator/profiles/ro-crate/> .
@prefix bioschemas: <https://bioschemas.org/> .
@prefix bioschemas-prop: <https://bioschemas.org/properties/> .
@prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .
@prefix schema: <http://schema.org/> .
@prefix sh: <http://www.w3.org/ns/shacl#> .
@prefix validator: <https://github.com/crs4/rocrate-validator/> .
@prefix xsd: <http://www.w3.org/2001/XMLSchema#> .
@prefix ex: <http://example.com/ns#> .

ex:PersonMustHaveGivenName a sh:NodeShape ;
    sh:name "Person MUST have a given name" ;
    sh:description "A Person MUST have a given name" ;
    sh:targetClass schema:Person ;
    sh:property [
        a sh:PropertyShape ;
        sh:path schema:givenName ;
        sh:datatype xsd:string ;
        sh:minCount 1 ;
        sh:not [
            sh:hasValue ""
        ] ;
        sh:description "Check that person does have non-empty given name and it's a string" ;
        sh:message "Person entity MUST have a non-empty given name of type string" ;
        sh:severity sh:Violation ;
    ] 
.
"""

assay_ttl ="""
@prefix ro: <./> .
@prefix ro-crate: <https://github.com/crs4/rocrate-validator/profiles/ro-crate/> .
@prefix isa-ro-crate: <https://github.com/crs4/rocrate-validator/profiles/isa-ro-crate/> .
@prefix bioschemas: <https://bioschemas.org/> .
@prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .
@prefix schema: <http://schema.org/> .
@prefix sh: <http://www.w3.org/ns/shacl#> .
@prefix validator: <https://github.com/crs4/rocrate-validator/> .
@prefix xsd: <http://www.w3.org/2001/XMLSchema#> .
@prefix ex: <http://example.com/ns#> .

# WIP
ex:AssayMustHaveBaseDescriptors a sh:NodeShape ;
    sh:name "Assay MUST have base properties" ;
    sh:description "An Assay MUST have identifier" ;
    sh:targetClass schema:Dataset ;
    sh:or(
        [
            sh:property [
                a sh:PropertyShape ;
                sh:path schema:additionalType ;
                sh:not [
                    sh:hasValue "Assay"
                ] ;
                sh:severity sh:Violation ;
            ]
        ]
        [
            sh:property [
                a sh:PropertyShape ;
                sh:path schema:identifier ;
                sh:datatype xsd:string ;
                sh:minCount 1 ;
                sh:maxCount 1 ; 
                sh:not [
                    sh:hasValue ""
                ] ;
                sh:severity sh:Violation ;
            ]
        ]
    ) ;
    sh:message "Assay entity MUST have a non-empty identifier of type string" ;
.
"""

study_ttl ="""
@prefix ro: <./> .
@prefix ro-crate: <https://github.com/crs4/rocrate-validator/profiles/ro-crate/> .
@prefix isa-ro-crate: <https://github.com/crs4/rocrate-validator/profiles/isa-ro-crate/> .
@prefix bioschemas: <https://bioschemas.org/> .
@prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .
@prefix schema: <http://schema.org/> .
@prefix sh: <http://www.w3.org/ns/shacl#> .
@prefix validator: <https://github.com/crs4/rocrate-validator/> .
@prefix xsd: <http://www.w3.org/2001/XMLSchema#> .
@prefix ex: <http://example.com/ns#> .

ex:StudyMustHaveIdentifier a sh:NodeShape ;
    sh:name "Study MUST have identifier" ;
    sh:description "A Study MUST have identifier" ;
    sh:message "Study entity MUST have a non-empty identifier of type string" ;
    sh:targetClass schema:Dataset ;
    sh:or(
        [
            sh:property [
                a sh:PropertyShape ;
                sh:path schema:additionalType ;
                sh:not [
                    sh:hasValue "Study"
                ] ;
                sh:severity sh:Violation ;
            ]
        ]
        [
            sh:property [
                a sh:PropertyShape ;
                sh:path schema:identifier ;
                sh:datatype xsd:string ;
                sh:minCount 1 ;
                sh:maxCount 1 ; 
                sh:not [
                    sh:hasValue ""
                ] ;
                sh:severity sh:Violation ;
            ] 
        ]
    ) ;
.

ex:StudyMustHaveName a sh:NodeShape ;
    sh:name "Study MUST have name" ;
    sh:message "Study entity MUST have a non-empty name of type string" ;
    sh:targetClass schema:Dataset ;
    sh:or(
        [
            sh:property [
                a sh:PropertyShape ;
                sh:path schema:additionalType ;
                sh:not [
                    sh:hasValue "Study"
                ] ;
                sh:severity sh:Violation ;
            ]
        ]
        [
            sh:property [
                a sh:PropertyShape ;
                sh:path schema:name ;
                sh:datatype xsd:string ;
                sh:minCount 1 ;
                sh:maxCount 1 ; 
                sh:not [
                    sh:hasValue ""
                ] ;
                sh:severity sh:Violation ;
            ]
        ]
    ) ;
.
"""

def check_persons(isa_json):
    conforms, results_graph, results_text = pyshacl.validate(
        isa_json,
        shacl_graph=person_ttl,
        data_graph_format="json-ld",   # Required for string input
        shacl_graph_format="turtle"   # Required for string input
    )
    tuples = []
    for result in results_graph.subjects(RDF.type, SH.ValidationResult):
        shape = results_graph.value(result, SH.sourceShape)
        message = results_graph.value(result, SH.resultMessage)
        node = results_graph.value(result, SH.focusNode)

        tuples.append((str(shape).split("#")[-1], str(message), str(node)))
    # print(conforms, tuples)
    return conforms, tuples

def check_investigation(isa_json):
    # TODO implement investigation checks
    return True
def check_studies(isa_json):
    conforms, results_graph, results_text = pyshacl.validate(
        isa_json,
        shacl_graph=study_ttl,
        data_graph_format="json-ld",   # Required for string input
        shacl_graph_format="turtle"   # Required for string input
    )
    tuples = []
    for result in results_graph.subjects(RDF.type, SH.ValidationResult):
        shape = results_graph.value(result, SH.sourceShape)
        message = results_graph.value(result, SH.resultMessage)
        node = results_graph.value(result, SH.focusNode)

        tuples.append((str(shape).split("#")[-1], str(message), str(node)))
    # print(conforms, tuples)
    return conforms, tuples
def check_assays(isa_json):
    conforms, results_graph, results_text = pyshacl.validate(
        isa_json,
        shacl_graph=assay_ttl,
        data_graph_format="json-ld",   # Required for string input
        shacl_graph_format="turtle"   # Required for string input
    )
    tuples = []
    for result in results_graph.subjects(RDF.type, SH.ValidationResult):
        shape = results_graph.value(result, SH.sourceShape)
        message = results_graph.value(result, SH.resultMessage)
        node = results_graph.value(result, SH.focusNode)

        tuples.append((str(shape).split("#")[-1], str(message), str(node)))
    # print(conforms, tuples)
    return conforms, tuples

# TODO: lazy
valid = True
try:
    valid = validate_arc()
except Exception:
    valid = False
    # TODO fake 1 failed test
    print("No ARC found!")
    add_test_case("Error", "Given input directory is not an ARC.")

print(valid)
create_badge(valid)
create_report()
create_summary()

exit(0)
