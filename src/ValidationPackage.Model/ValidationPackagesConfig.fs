namespace ValidationPackage.Model

open System
open System.Globalization
open Fable.Core

type RollForwardPolicy =
    | Disable = 0
    | LatestPatch = 1
    | LatestMinor = 2

type ValidationPackageInputValueKind =
    | Null = 0
    | Boolean = 1
    | Integer = 2
    | FloatingPoint = 3
    | String = 4

module private InputValueValidation =

    let isAsciiDigit character =
        character >= '0' && character <= '9'

    let isJsonIntegerLexeme (value: string) =
        if String.IsNullOrEmpty(value) then
            false
        else
            let start = if value[0] = '-' then 1 else 0

            if start = value.Length then
                false
            elif value[start] = '0' then
                start + 1 = value.Length
            else
                value[start] >= '1'
                && value[start] <= '9'
                && value.Substring(start + 1) |> Seq.forall isAsciiDigit

    let isJsonNumberLexeme (value: string) =
        if String.IsNullOrEmpty(value) then
            false
        else
            let mutable index = if value[0] = '-' then 1 else 0

            if index = value.Length then
                false
            else
                if value[index] = '0' then
                    index <- index + 1
                elif value[index] >= '1' && value[index] <= '9' then
                    index <- index + 1

                    while index < value.Length && isAsciiDigit value[index] do
                        index <- index + 1
                else
                    index <- value.Length + 1

                if index <= value.Length && index < value.Length && value[index] = '.' then
                    index <- index + 1
                    let fractionStart = index

                    while index < value.Length && isAsciiDigit value[index] do
                        index <- index + 1

                    if index = fractionStart then
                        index <- value.Length + 1

                if
                    index <= value.Length
                    && index < value.Length
                    && (value[index] = 'e' || value[index] = 'E')
                then
                    index <- index + 1

                    if index < value.Length && (value[index] = '+' || value[index] = '-') then
                        index <- index + 1

                    let exponentStart = index

                    while index < value.Length && isAsciiDigit value[index] do
                        index <- index + 1

                    if index = exponentStart then
                        index <- value.Length + 1

                index = value.Length

    let isFiniteDouble value =
        not (Double.IsNaN value) && not (Double.IsInfinity value)

    let isFiniteDoubleLexeme (value: string) =
        match Double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture) with
        | true, parsed -> isFiniteDouble parsed
        | false, _ -> false

    let isFiniteSingleLexeme (value: string) =
        match Double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture) with
        | true, parsed ->
            isFiniteDouble parsed
            && Math.Abs(parsed) <= 3.4028234663852886e38
        | false, _ -> false

    let fitsSignedInteger (positiveLimit: string) (negativeLimit: string) (value: string) =
        let isNegative = value.StartsWith("-", StringComparison.Ordinal)
        let digits = if isNegative then value.Substring(1) else value
        let limit = if isNegative then negativeLimit else positiveLimit

        digits.Length < limit.Length
        || (digits.Length = limit.Length && PortableString.compareOrdinal digits limit <= 0)

[<AttachMembers>]
type ValidationPackageInputValue private (kind: ValidationPackageInputValueKind, value: string) =

    member _.Kind = kind
    member _.Value = value

    override this.GetHashCode() =
        ValidationPackageInputValue.getHashCode this

    static member getHashCode(inputValue: ValidationPackageInputValue) =
        PortableHash.combineValues [
            int inputValue.Kind
            PortableHash.stringValue inputValue.Value
        ]

    override this.Equals(other) =
        match other with
        | :? ValidationPackageInputValue as inputValue ->
            (this.Kind, this.Value) = (inputValue.Kind, inputValue.Value)
        | _ -> false

    static member nullValue() =
        ValidationPackageInputValue(ValidationPackageInputValueKind.Null, "")

    static member boolean(value: bool) =
        ValidationPackageInputValue(
            ValidationPackageInputValueKind.Boolean,
            if value then "true" else "false"
        )

    static member integer(value: string) =
        if not (InputValueValidation.isJsonIntegerLexeme value) then
            invalidArg "value" $"invalid JSON-compatible integer: {value}"

        ValidationPackageInputValue(ValidationPackageInputValueKind.Integer, value)

    static member floatingPoint(value: string) =
        if
            not (InputValueValidation.isJsonNumberLexeme value)
            || InputValueValidation.isJsonIntegerLexeme value
            || not (InputValueValidation.isFiniteDoubleLexeme value)
        then
            invalidArg "value" $"invalid finite JSON-compatible floating-point number: {value}"

        ValidationPackageInputValue(ValidationPackageInputValueKind.FloatingPoint, value)

    static member string(value: string) =
        if isNull value then
            nullArg "value"

        ValidationPackageInputValue(ValidationPackageInputValueKind.String, value)

    static member isJsonInteger(value: string) =
        InputValueValidation.isJsonIntegerLexeme value

    static member isJsonNumber(value: string) =
        InputValueValidation.isJsonNumberLexeme value

[<AttachMembers>]
type ValidationPackageInput(id: string, value: ValidationPackageInputValue) =

    member _.Id = id
    member _.Value = value

    override this.GetHashCode() =
        ValidationPackageInput.getHashCode this

    static member getHashCode(input: ValidationPackageInput) =
        PortableHash.combineValues [
            PortableHash.stringValue input.Id
            ValidationPackageInputValue.getHashCode input.Value
        ]

    override this.Equals(other) =
        match other with
        | :? ValidationPackageInput as input ->
            (this.Id, this.Value) = (input.Id, input.Value)
        | _ -> false

    static member create(id: string, value: ValidationPackageInputValue) =
        if String.IsNullOrWhiteSpace(id) then
            invalidArg "id" "validation-package input id must be non-empty"

        if isNull (box value) then
            nullArg "value"

        ValidationPackageInput(id, value)

module private SelectionValidation =

    let validateSemanticVersion path (version: SemVer) =
        if isNull (box version) then
            invalidArg path "semantic version is required"

        let canonical = SemVer.toString version

        match SemVer.tryParse canonical with
        | Some parsed when parsed = version -> version
        | _ -> invalidArg path $"must be a full semantic version, but was '{canonical}'"

    let validateInputs (inputs: ValidationPackageInput array) =
        if isNull inputs then
            nullArg "inputs"

        inputs
        |> Array.iteri (fun index input ->
            if isNull (box input) then
                invalidArg "inputs" $"validation-package input at index {index} must not be null"
        )

        inputs
        |> Array.countBy (fun input -> input.Id)
        |> Array.tryFind (fun (_, count) -> count > 1)
        |> Option.iter (fun (id, _) ->
            invalidArg "inputs" $"validation-package input id must be unique, but was duplicated: {id}"
        )

        inputs

    let validateRollForward policy =
        match policy with
        | RollForwardPolicy.Disable
        | RollForwardPolicy.LatestPatch
        | RollForwardPolicy.LatestMinor -> policy
        | _ -> invalidArg "RollForward" $"unsupported roll-forward policy: {policy}"

    let findInput id (inputs: ValidationPackageInput array) =
        inputs |> Array.tryFind (fun input -> input.Id = id)

    let validateValue path (declaration: CommandInputParameter) (value: ValidationPackageInputValue) =
        if value.Kind = ValidationPackageInputValueKind.Null then
            if not declaration.Type.IsNullable then
                invalidArg "inputs" $"{path} does not allow null"
        else
            let compatible =
                match declaration.Type.PrimitiveType, value.Kind with
                | CwlPrimitive.Boolean, ValidationPackageInputValueKind.Boolean -> true
                | CwlPrimitive.String, ValidationPackageInputValueKind.String -> true
                | CwlPrimitive.Int, ValidationPackageInputValueKind.Integer ->
                    InputValueValidation.fitsSignedInteger
                        "2147483647"
                        "2147483648"
                        value.Value
                | CwlPrimitive.Long, ValidationPackageInputValueKind.Integer ->
                    InputValueValidation.fitsSignedInteger
                        "9223372036854775807"
                        "9223372036854775808"
                        value.Value
                | CwlPrimitive.Float, ValidationPackageInputValueKind.Integer
                | CwlPrimitive.Float, ValidationPackageInputValueKind.FloatingPoint ->
                    InputValueValidation.isFiniteSingleLexeme value.Value
                | CwlPrimitive.Double, ValidationPackageInputValueKind.Integer
                | CwlPrimitive.Double, ValidationPackageInputValueKind.FloatingPoint ->
                    InputValueValidation.isFiniteDoubleLexeme value.Value
                | _ -> false

            if not compatible then
                let expected = CommandInputType.toCwlString declaration.Type
                invalidArg "inputs" $"{path} is not compatible with declared type {expected}"

[<AttachMembers>]
type ValidationPackageSelection() =

    let mutable _name = ""
    let mutable _version = Unchecked.defaultof<SemVer>
    let mutable _rollForward = RollForwardPolicy.Disable
    let mutable _inputs: ValidationPackageInput array = Array.empty

    member _.Name
        with get () = _name
        and set value = _name <- value

    member _.Version
        with get () = _version
        and set value = _version <- value

    member _.RollForward
        with get () = _rollForward
        and set value = _rollForward <- SelectionValidation.validateRollForward value

    member _.Inputs
        with get () = _inputs
        and set value = _inputs <- SelectionValidation.validateInputs value

    override this.GetHashCode() =
        ValidationPackageSelection.getHashCode this

    static member getHashCode(selection: ValidationPackageSelection) =
        PortableHash.combineValues [
            PortableHash.stringValue selection.Name
            SemVer.getHashCode selection.Version
            int selection.RollForward
            PortableHash.arrayValue ValidationPackageInput.getHashCode selection.Inputs
        ]

    override this.Equals(other) =
        match other with
        | :? ValidationPackageSelection as selection ->
            (this.Name, this.Version, this.RollForward, this.Inputs) =
                (selection.Name, selection.Version, selection.RollForward, selection.Inputs)
        | _ -> false

    static member create (
        name: string,
        version: SemVer,
        ?RollForward: RollForwardPolicy,
        ?Inputs: ValidationPackageInput array
    ) =
        if String.IsNullOrWhiteSpace(name) then
            invalidArg "name" "validation-package selection name must be non-empty"

        SelectionValidation.validateSemanticVersion "version" version |> ignore

        let selection = ValidationPackageSelection(Name = name, Version = version)
        RollForward |> Option.iter (fun value -> selection.RollForward <- value)
        Inputs |> Option.iter (fun value -> selection.Inputs <- value)
        selection

    static member validateInputs (
        selection: ValidationPackageSelection,
        declarations: CommandInputParameter array
    ) =
        CommandInputParameter.validate declarations |> ignore
        SelectionValidation.validateInputs selection.Inputs |> ignore

        selection.Inputs
        |> Array.iter (fun input ->
            if declarations |> Array.exists (fun declaration -> declaration.Id = input.Id) |> not then
                invalidArg
                    "inputs"
                    $"validation_packages['{selection.Name}'].inputs['{input.Id}'] is not declared by the package"
        )

        declarations
        |> Array.iter (fun declaration ->
            let path =
                $"validation_packages['{selection.Name}'].inputs['{declaration.Id}']"

            match SelectionValidation.findInput declaration.Id selection.Inputs with
            | None when not declaration.Type.IsNullable ->
                invalidArg "inputs" $"{path} is required"
            | None -> ()
            | Some input -> SelectionValidation.validateValue path declaration input.Value
        )

    static member materializeArguments (
        selection: ValidationPackageSelection,
        declarations: CommandInputParameter array
    ) =
        ValidationPackageSelection.validateInputs(selection, declarations)

        declarations
        |> Array.sortWith (fun first second ->
            let positionComparison = compare first.InputBinding.Position second.InputBinding.Position

            if positionComparison <> 0 then
                positionComparison
            else
                PortableString.compareOrdinal first.Id second.Id
        )
        |> Array.collect (fun declaration ->
            match SelectionValidation.findInput declaration.Id selection.Inputs with
            | None -> Array.empty
            | Some input when input.Value.Kind = ValidationPackageInputValueKind.Null -> Array.empty
            | Some input when declaration.Type.PrimitiveType = CwlPrimitive.Boolean ->
                if input.Value.Value = "true" then
                    [| declaration.InputBinding.Prefix |]
                else
                    Array.empty
            | Some input ->
                [| declaration.InputBinding.Prefix; input.Value.Value |]
        )

[<AttachMembers>]
type ValidationPackagesConfig() =

    let mutable _arcSpecification = Unchecked.defaultof<SemVer>
    let mutable _validationPackages: ValidationPackageSelection array = Array.empty

    member _.ArcSpecification
        with get () = _arcSpecification
        and set value = _arcSpecification <- value

    member _.HasArcSpecification = not (isNull (box _arcSpecification))

    member _.ValidationPackages
        with get () = _validationPackages
        and set value = _validationPackages <- value

    override this.GetHashCode() =
        ValidationPackagesConfig.getHashCode this

    static member getHashCode(config: ValidationPackagesConfig) =
        PortableHash.combineValues [
            if config.HasArcSpecification then
                SemVer.getHashCode config.ArcSpecification
            else
                0

            PortableHash.arrayValue ValidationPackageSelection.getHashCode config.ValidationPackages
        ]

    override this.Equals(other) =
        match other with
        | :? ValidationPackagesConfig as config ->
            let arcSpecificationEqual =
                match this.HasArcSpecification, config.HasArcSpecification with
                | false, false -> true
                | true, true -> this.ArcSpecification = config.ArcSpecification
                | _ -> false

            arcSpecificationEqual
            && this.ValidationPackages = config.ValidationPackages
        | _ -> false

    static member validate(config: ValidationPackagesConfig) =
        if isNull (box config) then
            nullArg "config"

        if config.HasArcSpecification then
            SelectionValidation.validateSemanticVersion
                "arc_specification"
                config.ArcSpecification
            |> ignore

        if isNull config.ValidationPackages then
            invalidArg "config" "validation_packages must be an array"

        config.ValidationPackages
        |> Array.iteri (fun index selection ->
            if isNull (box selection) then
                invalidArg "config" $"validation_packages[{index}] must not be null"

            if String.IsNullOrWhiteSpace(selection.Name) then
                invalidArg "config" $"validation_packages[{index}].name must be non-empty"

            SelectionValidation.validateSemanticVersion
                $"validation_packages[{index}].version"
                selection.Version
            |> ignore

            SelectionValidation.validateRollForward selection.RollForward |> ignore
            SelectionValidation.validateInputs selection.Inputs |> ignore
        )

        config.ValidationPackages
        |> Array.countBy (fun selection -> selection.Name)
        |> Array.tryFind (fun (_, count) -> count > 1)
        |> Option.iter (fun (name, _) ->
            invalidArg "config" $"validation package name must be unique, but was duplicated: {name}"
        )

        config

    static member create (
        validationPackages: ValidationPackageSelection array,
        ?ArcSpecification: SemVer
    ) =
        let config = ValidationPackagesConfig(ValidationPackages = validationPackages)
        ArcSpecification |> Option.iter (fun value -> config.ArcSpecification <- value)
        ValidationPackagesConfig.validate config

[<AttachMembers>]
type LegacyValidationPackageSelection() =

    let mutable _name = ""
    let mutable _version = Unchecked.defaultof<SemVer>

    member _.Name
        with get () = _name
        and set value = _name <- value

    member _.Version
        with get () = _version
        and set value = _version <- value

    member _.HasVersion = not (isNull (box _version))

    static member create(name: string, ?Version: SemVer) =
        if String.IsNullOrWhiteSpace(name) then
            invalidArg "name" "legacy validation-package selection name must be non-empty"

        Version
        |> Option.iter (fun version ->
            SelectionValidation.validateSemanticVersion "Version" version |> ignore
        )

        let selection = LegacyValidationPackageSelection(Name = name)
        Version |> Option.iter (fun value -> selection.Version <- value)
        selection

[<AttachMembers>]
type LegacyValidationPackagesConfig() =

    let mutable _arcSpecification = Unchecked.defaultof<SemVer>
    let mutable _validationPackages: LegacyValidationPackageSelection array = Array.empty

    member _.ArcSpecification
        with get () = _arcSpecification
        and set value = _arcSpecification <- value

    member _.HasArcSpecification = not (isNull (box _arcSpecification))

    member _.ValidationPackages
        with get () = _validationPackages
        and set value = _validationPackages <- value

    static member create (
        validationPackages: LegacyValidationPackageSelection array,
        ?ArcSpecification: SemVer
    ) =
        if isNull validationPackages then
            nullArg "validationPackages"

        ArcSpecification
        |> Option.iter (fun version ->
            SelectionValidation.validateSemanticVersion "ArcSpecification" version |> ignore
        )

        validationPackages
        |> Array.iteri (fun index selection ->
            if isNull (box selection) then
                invalidArg "validationPackages" $"legacy validation package at index {index} must not be null"

            if String.IsNullOrWhiteSpace(selection.Name) then
                invalidArg "validationPackages" $"legacy validation package at index {index} requires a name"

            if selection.HasVersion then
                SelectionValidation.validateSemanticVersion
                    $"validationPackages[{index}].Version"
                    selection.Version
                |> ignore
        )

        validationPackages
        |> Array.countBy (fun selection -> selection.Name)
        |> Array.tryFind (fun (_, count) -> count > 1)
        |> Option.iter (fun (name, _) ->
            invalidArg "validationPackages" $"legacy validation package name must be unique, but was duplicated: {name}"
        )

        let config = LegacyValidationPackagesConfig(ValidationPackages = validationPackages)
        ArcSpecification |> Option.iter (fun value -> config.ArcSpecification <- value)
        config
