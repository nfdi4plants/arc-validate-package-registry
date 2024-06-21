namespace AVPRIndex

open System.Text
open System.Text.RegularExpressions

module Globals =

    let [<Literal>] STAGING_AREA_RELATIVE_PATH = "StagingArea"

    // https://semver.org/#is-there-a-suggested-regular-expression-regex-to-check-a-semver-string
    let [<Literal>] SEMVER_REGEX_PATTERN = @"^(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)(?:-(?<prerelease>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+(?<buildmetadata>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$"

    let SEMVER_REGEX = new Regex(SEMVER_REGEX_PATTERN)