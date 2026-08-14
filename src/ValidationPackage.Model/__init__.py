"""Portable metadata and CWL input model for ARC validation packages."""

from enum import IntEnum

from .author import Author
from .cwl import CommandInputBinding, CommandInputParameter, CommandInputType
from .ontology_annotation import OntologyAnnotation
from .semantic_version import SemVer
from .validation_package_identity import ValidationPackageIdentity
from .validation_package_metadata import ValidationPackageMetadata
from .validation_packages_config import (
    LegacyValidationPackageSelection,
    LegacyValidationPackagesConfig,
    ValidationPackageInput,
    ValidationPackageInputValue,
    ValidationPackageSelection,
    ValidationPackagesConfig,
)


class CwlPrimitive(IntEnum):
    Boolean = 0
    Int = 1
    Long = 2
    Float = 3
    Double = 4
    String = 5


class RollForwardPolicy(IntEnum):
    Disable = 0
    LatestPatch = 1
    LatestMinor = 2


class ValidationPackageInputValueKind(IntEnum):
    Null = 0
    Boolean = 1
    Integer = 2
    FloatingPoint = 3
    String = 4

__all__ = [
    "Author",
    "CommandInputBinding",
    "CommandInputParameter",
    "CommandInputType",
    "CwlPrimitive",
    "OntologyAnnotation",
    "LegacyValidationPackageSelection",
    "LegacyValidationPackagesConfig",
    "RollForwardPolicy",
    "SemVer",
    "ValidationPackageIdentity",
    "ValidationPackageInput",
    "ValidationPackageInputValue",
    "ValidationPackageInputValueKind",
    "ValidationPackageMetadata",
    "ValidationPackageSelection",
    "ValidationPackagesConfig",
]
