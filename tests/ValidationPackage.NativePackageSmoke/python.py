from validation_package_model import (
    CommandInputBinding,
    CommandInputParameter,
    CommandInputType,
    CwlPrimitive,
    RollForwardPolicy,
    SemVer,
    ValidationPackageSelection,
    ValidationPackagesConfig,
    ValidationPackageMetadata,
)
from validation_package_codecs import (
    SchemaUris,
    ValidationPackageJson,
    ValidationPackagesConfigYaml,
)

metadata = ValidationPackageMetadata.create(
    "native-package",
    "Native package smoke test",
    "Verifies the installed Python dependency boundary.",
    1,
    2,
    3,
    "FSharp",
)
metadata.Inputs = [
    CommandInputParameter.create(
        "arc-directory",
        CommandInputType.create(CwlPrimitive.String),
        CommandInputBinding.create(None, "--arc-directory"),
    )
]

json = ValidationPackageJson.encode(metadata)
decoded = ValidationPackageJson.decode_or_fail(json)

assert decoded.Name == "native-package"
assert decoded.MajorVersion == 1
assert decoded.Inputs[0].InputBinding.Prefix == "--arc-directory"

config = ValidationPackagesConfig.create(
    [
        ValidationPackageSelection.create(
            "native-package",
            SemVer.create(1, 2, 3),
            RollForwardPolicy.LatestPatch,
        )
    ]
)
config_yaml = ValidationPackagesConfigYaml.encode(config)
decoded_config = ValidationPackagesConfigYaml.decode_or_fail(config_yaml)

assert not decoded_config.IsLegacy
assert decoded_config.Canonical.ValidationPackages[0].Name == "native-package"
assert SchemaUris.ValidationPackagesConfigV1 in config_yaml
