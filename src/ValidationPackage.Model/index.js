export * from "./Author.js";
export * from "./Cwl.js";
export const CwlPrimitive = Object.freeze({
  Boolean: 0,
  Int: 1,
  Long: 2,
  Float: 3,
  Double: 4,
  String: 5
});
export * from "./OntologyAnnotation.js";
export * from "./SemanticVersion.js";
export * from "./ValidationPackageIdentity.js";
export * from "./ValidationPackageMetadata.js";
export * from "./ValidationPackagesConfig.js";
export const RollForwardPolicy = Object.freeze({
  Disable: 0,
  LatestPatch: 1,
  LatestMinor: 2
});
export const ValidationPackageInputValueKind = Object.freeze({
  Null: 0,
  Boolean: 1,
  Integer: 2,
  FloatingPoint: 3,
  String: 4
});
