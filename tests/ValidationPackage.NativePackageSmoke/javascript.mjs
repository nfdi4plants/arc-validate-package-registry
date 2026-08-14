import {
  CommandInputBinding,
  CommandInputParameter,
  CommandInputType,
  CwlPrimitive,
  RollForwardPolicy,
  SemVer,
  ValidationPackageSelection,
  ValidationPackagesConfig,
  ValidationPackageMetadata
} from "@nfdi4plants/validationpackage-model";
import {
  SchemaUris,
  ValidationPackageJson,
  ValidationPackagesConfigYaml
} from "@nfdi4plants/validationpackage-codecs";

const metadata = ValidationPackageMetadata.create(
  "native-package",
  "Native package smoke test",
  "Verifies the installed JavaScript dependency boundary.",
  1,
  2,
  3,
  "FSharp"
);
metadata.Inputs = [
  CommandInputParameter.create(
    "arc-directory",
    CommandInputType.create(CwlPrimitive.String),
    CommandInputBinding.create(undefined, "--arc-directory")
  )
];

const json = ValidationPackageJson.encode(metadata);
const decoded = ValidationPackageJson.decodeOrFail(json);

if (
  decoded.Name !== "native-package" ||
  decoded.MajorVersion !== 1 ||
  decoded.Inputs[0].InputBinding.Prefix !== "--arc-directory"
) {
  throw new Error("ValidationPackage JavaScript package round-trip failed");
}

const config = ValidationPackagesConfig.create([
  ValidationPackageSelection.create(
    "native-package",
    SemVer.create(1, 2, 3),
    RollForwardPolicy.LatestPatch
  )
]);
const configYaml = ValidationPackagesConfigYaml.encode(config);
const decodedConfig = ValidationPackagesConfigYaml.decodeOrFail(configYaml);

if (
  decodedConfig.IsLegacy ||
  decodedConfig.Canonical.ValidationPackages[0].Name !== "native-package" ||
  !configYaml.includes(SchemaUris.ValidationPackagesConfigV1)
) {
  throw new Error("ValidationPackage JavaScript config package round-trip failed");
}
