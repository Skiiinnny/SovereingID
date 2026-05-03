## MODIFIED Requirements

### Requirement: Application layer uses lightweight CQRS abstractions

`SovereignID.SharedKernel.Application` SHALL define the marker and handler
interfaces used by every BC's Application layer:

- `ICommand<TResult>` — marker for state-changing intents.
- `IQuery<TResult>` — marker for read-only intents.
- `ICommandHandler<TCommand, TResult>` and
  `IQueryHandler<TQuery, TResult>` — async handler contracts that take a
  `CancellationToken`.

The type parameters on `ICommand<TResult>` and `IQuery<TResult>` exist to
link each command or query type to the result type of its handler through
the generic constraints on `ICommandHandler<TCommand, TResult>` and
`IQueryHandler<TQuery, TResult>` (for example,
`where TCommand : ICommand<TResult>`). These marker interfaces SHALL
therefore be permitted to contain no instance members; that SHALL not be
treated as a meaningless or unused generic parameter in the
architecture sense.

Each BC SHALL express its use cases as command/query records implementing
these markers, and as handler classes implementing the corresponding
handler interface. No external mediator dependency (e.g., MediatR) is
permitted for this convention.

#### Scenario: Handler contracts are async with cancellation

- **WHEN** an inspector reads the signatures of `ICommandHandler<,>` and
  `IQueryHandler<,>`
- **THEN** their `HandleAsync` method returns `Task<TResult>` and accepts
  a `CancellationToken`

#### Scenario: No external mediator package referenced

- **WHEN** the architecture tests inspect any `SovereignID.*` project
  outside `legacy/`
- **THEN** none of them reference a NuGet package whose id starts with
  `MediatR`

#### Scenario: Command and query markers bind result types

- **WHEN** a developer implements a new command or query in a BC
- **THEN** the command or query type implements `ICommand<TResult>` or
  `IQuery<TResult>` where `TResult` is the return type of the
  corresponding handler, matching the `where` clause on
  `ICommandHandler<,>` or `IQueryHandler<,>`

## ADDED Requirements

### Requirement: Optional public assembly anchor in each layer project

Each of `SovereignID.<BC>.Domain`, `SovereignID.<BC>.Application`, and
`SovereignID.<BC>.Infrastructure` for every bounded context, and each of
`SovereignID.SharedKernel.Domain`, `SovereignID.SharedKernel.Application`,
and `SovereignID.SharedKernel.Infrastructure`, MAY expose a single
public, intentionally minimal type (conventionally named with the suffix
`Marker`) whose only purpose is to be referenced for assembly identity,
compiler checks, and architecture rules that require a peer project
reference.

The type SHALL document that purpose in a standard XML doc comment. If
static analysis reports an "empty class" or equivalent issue for that
type, a suppression with an explicit rule id at the type or file level
SHALL be used and SHALL repeat the same rationale, rather than adding
distractor members to the public API.

#### Scenario: Reader understands the anchor

- **WHEN** a maintainer opens a `*Marker` type in any layer project
- **THEN** the XML documentation explains that the type exists to anchor
  the assembly and satisfy reference-based architecture rules

#### Scenario: Suppression is local and explicit

- **WHEN** a quality tool still flags the anchor as empty
- **THEN** the suppression names the tool rule and the architectural
  reason in line with the documentation above
