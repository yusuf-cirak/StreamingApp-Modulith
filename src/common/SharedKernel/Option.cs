namespace SharedKernel;

public sealed record Option<T>
{
    private readonly T _content;
    private readonly bool _hasValue;

    private Option(T content)
    {
        _content = content;
        _hasValue = true;
    }

    internal Option()
    {
        _content = default!;
        _hasValue = false;
    }

    public static Option<T> Some(T value) => new(value);

    public static Option<T> None() => OptionCache<T>.None;

    public bool TryGetValue(out T value)
    {
        value = _content;
        return _hasValue;
    }

    public Option<TResult> Map<TResult>(Func<T, TResult> map)
        => _hasValue ? Option<TResult>.Some(map(_content)) : Option<TResult>.None();

    public Option<TResult> Bind<TResult>(Func<T, Option<TResult>> bind)
        => _hasValue ? bind(_content) : Option<TResult>.None();
}

file static class OptionCache<T>
{
    public static readonly Option<T> None = new();
}

public static class OptionExtensions
{
    public static Option<T> FirstOrNone<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        => source
            .Where(predicate)
            .Select(Option<T>.Some)
            .DefaultIfEmpty(Option<T>.None())
            .First();

    public static Option<T> SingleOrNone<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        => source
            .Where(predicate)
            .Select(Option<T>.Some)
            .DefaultIfEmpty(Option<T>.None())
            .Single();

    public static Option<TResult> Select<T, TResult>(this Option<T> option, Func<T, TResult> map)
        => option.Map(map);

    public static Option<TResult> SelectMany<T, TSecond, TResult>(this Option<T> option, Func<T, Option<TSecond>> bind,
        Func<T, TSecond, TResult> map)
        => option.Bind(original => bind(original).Map(result => map(original, result)));

    public static Option<T> Where<T>(this Option<T> option, Func<T, bool> predicate)
        => option.Bind(value => predicate(value) ? option : Option<T>.None());
}