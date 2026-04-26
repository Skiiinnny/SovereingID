using System.Diagnostics.CodeAnalysis;

[assembly: ExcludeFromCodeCoverage]

namespace SovereignID.SharedKernel.Application;

/// <summary>
/// Marker for state-changing application commands. The <typeparamref name="TResult"/> type parameter
/// links each command type to the result type of <see cref="ICommandHandler{TCommand, TResult}"/>
/// through the constraint <c>where TCommand : ICommand&lt;TResult&gt;</c>.
/// </summary>
/// <typeparam name="TResult">Result type produced by the handler for this command.</typeparam>
[SuppressMessage("csharpsquid", "S2326", Justification = "TResult binds the command type to the handler result type per solution-architecture (ICommandHandler generic constraint).")]
public interface ICommand<TResult>;

/// <summary>
/// Marker for read-only application queries. The <typeparamref name="TResult"/> type parameter
/// links each query type to the result type of <see cref="IQueryHandler{TQuery, TResult}"/>
/// through the constraint <c>where TQuery : IQuery&lt;TResult&gt;</c>.
/// </summary>
/// <typeparam name="TResult">Result type returned by the handler for this query.</typeparam>
[SuppressMessage("csharpsquid", "S2326", Justification = "TResult binds the query type to the handler result type per solution-architecture (IQueryHandler generic constraint).")]
public interface IQuery<TResult>;

public interface ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    Task<TResult> HandleAsync(TCommand input, CancellationToken cancellationToken);
}

public interface IQueryHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery input, CancellationToken cancellationToken);
}
