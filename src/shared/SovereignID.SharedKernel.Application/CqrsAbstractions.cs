namespace SovereignID.SharedKernel.Application;

public interface ICommand<TResult>;

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
