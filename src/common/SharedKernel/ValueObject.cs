using LanguageExt.Common;

namespace SharedKernel;

public abstract record ValueObject<T>(T Value) where T : notnull
{
    public static implicit operator T(ValueObject<T> valueObject) => valueObject.Value;
}