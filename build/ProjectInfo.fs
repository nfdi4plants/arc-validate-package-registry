module ProjectInfo

open System.IO

let configuration = "Release"

let mainSolution = "arc-validate-package-registry.slnx"
let stagingSolution = "PackageStagingArea.slnx"

let modelProject = "src/ValidationPackage.Model/ValidationPackage.Model.fsproj"
let codecsProject = "src/ValidationPackage.Codecs/ValidationPackage.Codecs.fsproj"
let clientProject = "src/AVPRClient/AVPRClient.csproj"
let clientInteropProject = "src/AVPRClient.Interop/AVPRClient.Interop.csproj"

let modelTestsProject = "tests/ValidationPackage.Model.Tests/ValidationPackage.Model.Tests.fsproj"
let modelPackageSmokeProject = "tests/ValidationPackage.Model.PackageSmoke/ValidationPackage.Model.PackageSmoke.fsproj"

let codecsTestsProject = "tests/ValidationPackage.Codecs.Tests/ValidationPackage.Codecs.Tests.fsproj"
let codecsJavaScriptTestsProject =
    "tests/ValidationPackage.Codecs.Tests/javascript/ValidationPackage.Codecs.Tests.Javascript.fsproj"
let codecsPythonTestsProject =
    "tests/ValidationPackage.Codecs.Tests/python/ValidationPackage.Codecs.Tests.Python.fsproj"
let codecsPackageSmokeProject = "tests/ValidationPackage.Codecs.PackageSmoke/ValidationPackage.Codecs.PackageSmoke.fsproj"

let artifactsDir = "artifacts"
let portableArtifactsDir = Path.Combine(artifactsDir, "portable")
let packageDir = Path.Combine(artifactsDir, "packages")
let packageCacheDir = Path.Combine(artifactsDir, "package-cache")
