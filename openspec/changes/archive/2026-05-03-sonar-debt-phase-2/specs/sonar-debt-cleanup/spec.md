## ADDED Requirements

### Requirement: Tracked SonarQube and Roslyn findings are resolved or explicitly justified

The change SHALL address the static-analysis findings that SonarQube
currently lists for the SovereignID .NET solution (including rule families
`csharpsquid` and `external_roslyn` where applicable), specifically:

- Empty public layer anchor types (rule `S2094` on `*Marker` types);
- Generic type parameters on `ICommand<TResult>` and `IQuery<TResult>`
  (rule `S2326` when reported as unused on memberless marker interfaces);
- Suggestions `CA1859` in `SovereignID.Architecture.Tests` as present in
  the last published analysis;
- Suggestion `CA1845` in `SovereignID.Chain.DocumentNotarizer` as present
  in the last published analysis;

Each such item SHALL be implemented per `design.md` or the corresponding
`MODIFIED` / `ADDED` text in `solution-architecture` so that the
repository reflects an intentional design, not a silent waiver.

#### Scenario: No orphan suppressions

- **WHEN** a suppression attribute or `NOSONAR` comment is introduced for
  the rules above
- **THEN** it is adjacent to the affected type or member
- **AND** it states the architectural reason in plain language
- **AND** it is consistent with the `solution-architecture` delta for
  that rule family

#### Scenario: Observable behavior is preserved

- **WHEN** the change is complete
- **THEN** all existing automated tests that passed before the change
  still pass
- **AND** no public API surface of a non-legacy project regresses
  (binary compatibility for consumers)
- **AND** legacy public type signatures that shipped with Phase 1 remain
  unchanged except where a delta spec explicitly allows internal-only
  implementation changes

### Requirement: Architecture tests use concrete collection types where analyzers require them

The `SovereignID.Architecture.Tests` project SHALL be updated so that
private helper methods that currently trigger `CA1859` use the concrete
collection or dictionary types recommended by the analysis, as long as
this does not break test scenarios or allow unintended mutation of shared
state.

#### Scenario: Read-only intent remains clear

- **WHEN** a method is changed from an interface-typed return or
  parameter to a concrete `Dictionary` or `HashSet` type
- **THEN** call sites that must not observe mutation still avoid exposing
  those collections to other tests in a way that would change test
  isolation (for example, by not returning internal mutable static state)
