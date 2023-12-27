namespace QLShell;

public class TaskLimiter
{
    public static TaskLimiter Instance => _limiter ?? throw new InvalidOperationException("TaskLimiter not created");

    private static TaskLimiter? _limiter;

    private readonly SemaphoreSlim _semaphore;

    private TaskLimiter(int maxTasks)
    {
        _semaphore = new SemaphoreSlim(maxTasks);
    }

    public static void Create(int maxTasks)
    {
        _limiter = new TaskLimiter(Math.Max(Math.Min(maxTasks, Environment.ProcessorCount), 1));
    }

    public async Task ProcessAsync(Func<Task> task, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            await task();
        }
        finally
        {
            _semaphore.Release();
        }
    }
}