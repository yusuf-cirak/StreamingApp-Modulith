namespace BuildingBlocks.SharedKernel;

public interface IOption;

public record Option<T> : IOption
{
    public bool HasValue { get; }
    
    private readonly T _content;

    private Option(T content)
    {
        _content = content;
        HasValue = true;
    }

    internal Option()
    {
        _content = default!;
        HasValue = false;
    }

    public static Option<T> Create(T? value) => value is not null ? Some(value) : None();
    public static Option<T> Some(T value) => new(value);

    public static Option<T> None() => OptionCache<T>.None;

    public bool TryGetValue(out T value)
    {
        value = _content;
        return HasValue;
    }
    
    public T GetValueOrDefault() => _content;
    public T GetValueOrFail() => HasValue ? _content : throw new NoneException($"Expected value of type {typeof(T).Name} but got None");

    public TResult Match<TResult>(Func<TResult> none, Func<T, TResult> some) => HasValue ? some(_content) : none();

    public Option<TResult> Map<TResult>(Func<T, TResult> map)
        => HasValue ? Option<TResult>.Some(map(_content)) : Option<TResult>.None();

    public Option<TResult> Bind<TResult>(Func<T, Option<TResult>> bind)
        => HasValue ? bind(_content) : Option<TResult>.None();


    public static implicit operator Option<T>(T? value) => value is not null ? Some(value) : None();
}

file static class OptionCache<T>
{
    public static readonly Option<T> None = new();
}

public static class OptionExtensions
{
    public static Option<T> ToOption<T>(this T? value) => Option<T>.Create(value);

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


public sealed class NoneException(string message) : Exception(message);