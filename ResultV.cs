#nullable enable

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace ScrubJay.Results;

/// <summary>
/// Represents the result of an operation as either:<br/>
/// <see cref="Ok">Result.Ok(Value)</see><br/>
/// <see cref="Error">Result.Error(Exception)</see><br/>
/// </summary>
/// <typeparam name="TValue">
/// The <see cref="Type"/> of Value in <c>Result.Ok(Value)</c>
/// </typeparam>
public readonly struct Result<TValue> :
    IEqualityOperators<Result<TValue>, Result<TValue>, bool>,
    IEqualityOperators<Result<TValue>, Result, bool>,
    IEqualityOperators<Result<TValue>, bool, bool>,
    IBitwiseOperators<Result<TValue>, Result<TValue>, bool>,
    IBitwiseOperators<Result<TValue>, Result, bool>,
    IBitwiseOperators<Result<TValue>, bool, bool>,
    IEquatable<Result<TValue>>,
    IEquatable<Result>,
    IEnumerable<TValue>,
    IFormattable
{
    public static implicit operator Result<TValue>(TValue value) => Ok(value);
    public static implicit operator Result<TValue>(Exception? exception) => Error(exception);
    public static implicit operator Result(Result<TValue> result) => result.IsError(out var ex) ? Result.Error(ex) : Result.Ok();
    public static implicit operator bool(Result<TValue> result) => result._ok;

    public static bool operator true(Result<TValue> result) => result._ok;
    public static bool operator false(Result<TValue> result) => !result._ok;
    public static bool operator !(Result<TValue> result) => !result._ok;

    [Obsolete("Do not use ~ with Result<TValue>", true)]
    public static bool operator ~(Result<TValue> _)
        => throw new NotSupportedException($"Cannot apply ~ to a Result<{typeof(TValue).Name}>");

    public static bool operator ==(Result<TValue> left, Result<TValue> right) => left.Equals(right);
    public static bool operator ==(Result<TValue> result, Result r) => result.Equals(r);
    public static bool operator ==(Result<TValue> result, TValue value) => result.Equals(value);
    public static bool operator ==(Result<TValue> result, Exception? error) => result.Equals(error);
    public static bool operator ==(Result<TValue> result, bool isOk) => result.Equals(isOk);
    public static bool operator ==(Result r, Result<TValue> result) => result.Equals(r);
    public static bool operator ==(TValue value, Result<TValue> result) => result.Equals(value);
    public static bool operator ==(Exception? error, Result<TValue> result) => result.Equals(error);
    public static bool operator ==(bool isOk, Result<TValue> result) => result.Equals(isOk);

    public static bool operator !=(Result<TValue> left, Result<TValue> right) => !left.Equals(right);
    public static bool operator !=(Result<TValue> result, Result r) => !result.Equals(r);
    public static bool operator !=(Result<TValue> result, TValue value) => !result.Equals(value);
    public static bool operator !=(Result<TValue> result, Exception? error) => !result.Equals(error);
    public static bool operator !=(Result<TValue> result, bool isOk) => !result.Equals(isOk);
    public static bool operator !=(Result r, Result<TValue> result) => !result.Equals(r);
    public static bool operator !=(TValue value, Result<TValue> result) => !result.Equals(value);
    public static bool operator !=(Exception? error, Result<TValue> result) => !result.Equals(error);
    public static bool operator !=(bool isOk, Result<TValue> result) => !result.Equals(isOk);

    public static bool operator |(Result<TValue> left, Result<TValue> right) => left._ok || right._ok;
    public static bool operator |(Result<TValue> result, Result r) => result._ok || r.IsOk();
    public static bool operator |(Result<TValue> result, bool isOk) => isOk || result._ok;
    public static bool operator |(Result r, Result<TValue> result) => result._ok || r.IsOk();
    public static bool operator |(bool isOk, Result<TValue> result) => isOk || result._ok;

    public static bool operator &(Result<TValue> left, Result<TValue> right) => left._ok && right._ok;
    public static bool operator &(Result<TValue> result, Result r) => result._ok && r.IsOk();
    public static bool operator &(Result<TValue> result, bool isOk) => isOk && result._ok;
    public static bool operator &(Result r, Result<TValue> result) => result._ok && r.IsOk();
    public static bool operator &(bool isOk, Result<TValue> result) => isOk && result._ok;
    
    public static bool operator ^(Result<TValue> left, Result<TValue> right) => left._ok ^ right._ok;
    public static bool operator ^(Result<TValue> result, Result r) => result._ok ^ r.IsOk();
    public static bool operator ^(Result<TValue> result, bool isOk) => isOk ^ result._ok;
    public static bool operator ^(Result r, Result<TValue> result) => result._ok ^ r.IsOk();
    public static bool operator ^(bool isOk, Result<TValue> result) => isOk ^ result._ok;

    /// <summary>
    /// Returns an <c>Ok</c> Result
    /// </summary>
    /// <param name="value">The value to associate with the <c>Ok</c> Result</param>
    /// <returns>
    /// <c>Result&lt;TValue&gt;.Ok(</c><paramref name="value"/><c>)</c>
    /// </returns>
    public static Result<TValue> Ok(TValue value)
    {
        return new(true, value, null);
    }

    /// <summary>
    /// Returns an <c>Error</c> <see cref="Result{T}"/> with the given <paramref name="exception"/>
    /// </summary>
    /// <param name="exception">
    /// An optional <see cref="Exception"/> with additional information related to the failure
    /// </param>
    /// <returns><c>Result&lt;TValue&gt;.Error(</c><see cref="Exception"/><c>)</c></returns>
    public static Result<TValue> Error(Exception? exception)
    {
        return new(false, default!, exception);
    }

    public static Result<TValue> NotNull([AllowNull] TValue value)
    {
        if (value is not null)
        {
            return Ok(value);
        }
        else
        {
            return Error(new ArgumentNullException(nameof(value)));
        }
    }

    private readonly bool _ok;
    private readonly TValue _value;
    private readonly Exception? _exception;

    internal Result(bool ok, TValue value, Exception? exception)
    {
        _ok = ok;
        _value = value;
        _exception = exception;
    }

    /// <summary>
    /// Gets an attached <see cref="Exception"/> or creates a new <see cref="Exception"/>
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Exception GetOrCreateException()
    {
        return _exception ?? new Exception("Error");
    }

    /// <summary>
    /// If this <see cref="Result{TValue}"/> is <c>Ok</c>, returns the <c>Value</c>,<br/>
    /// otherwise <c>throws</c> the <c>Error</c>'s <see cref="Exception"/>
    /// </summary>
    /// <returns>
    /// The Value from <c>Ok(Value)</c>
    /// </returns>
    /// <exception cref="Exception">
    /// The Exception from <c>Error(Exception)</c>
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TValue OkValueOrThrowError() => _ok ? _value : throw GetOrCreateException();

    /// <summary>
    /// If this <see cref="Result{TValue}"/> is <c>Error(Exception)</c>, <c>throw Exception</c>
    /// </summary>
    /// <exception cref="Exception">
    /// The Exception from <c>Error(Exception)</c>
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ThrowIfError()
    {
        if (!_ok)
            throw GetOrCreateException();
    }

    /// <summary>
    /// Is this an <c>Ok</c> <see cref="Result{T}"/>?
    /// </summary>
    /// <returns>
    /// <c>true</c> if this is <c>Result.Ok(Value)</c>; otherwise, <c>false</c>
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsOk() => _ok;

    /// <summary>
    /// Is this an <c>Ok</c> <see cref="Result{T}"/>?
    /// </summary>
    /// <param name="value">
    /// If this is an <c>Ok</c>, the associated value; otherwise, <c>default</c>
    /// </param>
    /// <returns>
    /// <c>true</c> if this is <c>Result.Ok(Value)</c>; otherwise, <c>false</c>
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsOk([MaybeNullWhen(false)] out TValue value)
    {
        value = _value;
        return _ok;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsOk([MaybeNullWhen(false)] out TValue okValue, [MaybeNullWhen(true), NotNullWhen(false)] out Exception? errorException)
    {
        okValue = _value;
        errorException = GetOrCreateException();
        return _ok;
    }

    /// <summary>
    /// Is this an <c>Error</c> <see cref="Result{T}"/>?
    /// </summary>
    /// <returns>
    /// <c>true</c> if this is <c>Result.Error(Exception)</c>; otherwise, <c>false</c>
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsError() => !_ok;

    /// <summary>
    /// Is this an <c>Error</c> <see cref="Result{T}"/>?
    /// </summary>
    /// <param name="exception">
    /// If this is an <c>Error</c>, the attached or a new <see cref="Exception"/>; otherwise, <c>null</c>
    /// </param>
    /// <returns>
    /// <c>true</c> if this is <c>Result.Error(Exception)</c>; otherwise, <c>false</c>
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsError([NotNullWhen(true)] out Exception? exception)
    {
        if (_ok)
        {
            exception = null;
            return false;
        }

        exception = GetOrCreateException();
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsError([NotNullWhen(true)] out Exception? errorException, [MaybeNullWhen(true)] out TValue okValue)
    {
        okValue = _value;
        errorException = GetOrCreateException();
        return !_ok;
    }

    /// <summary>
    /// Performs a matching operation on this <see cref="Result{T}"/>,
    /// executing <paramref name="onOk"/> if this is an <c>Ok</c> Result and
    /// executing <paramref name="onError"/> if this is an <c>Error</c> Result
    /// </summary>
    /// <param name="onOk">
    /// The <see cref="Action{TValue}">Action&lt;TValue&gt;</see> that will be invoked if this is <c>Result.Ok(Value)</c>
    /// </param>
    /// <param name="onError">
    /// The <see cref="Action{Exception}">Action&lt;Exception&gt;</see> that will be invoked if this is <c>Result.Error(Exception)</c>
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Match(Action<TValue> onOk, Action<Exception> onError)
    {
        if (_ok)
        {
            onOk(_value);
        }
        else
        {
            onError(GetOrCreateException());
        }
    }

    /// <summary>
    /// Performs a matching operation on this <see cref="Result{T}"/>,
    /// executing <paramref name="onOk"/> if this is an <c>Ok</c> Result and
    /// executing <paramref name="onError"/> if this is an <c>Error</c> Result,
    /// returning a <typeparamref name="TReturn"/> value
    /// </summary>
    /// <param name="onOk">
    /// The <see cref="Func{TValue,TResult}">Func&lt;TValue,TReturn&gt;</see> that will be invoked if this is <c>Result.Ok()</c>
    /// </param>
    /// <param name="onError">
    /// The <see cref="Func{Exception,TResult}">Func&lt;Exception,TReturn&gt;</see> that will be invoked if this is <c>Result.Error(Exception)</c>
    /// </param>
    /// <typeparam name="TReturn">
    /// The <see cref="Type"/> of the return value
    /// </typeparam>
    /// <returns>
    /// The <typeparamref name="TReturn"/> value from either <paramref name="onOk"/> or <paramref name="onError"/>
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TReturn Match<TReturn>(Func<TValue, TReturn> onOk, Func<Exception, TReturn> onError)
    {
        if (_ok)
        {
            return onOk(_value);
        }
        else
        {
            return onError(GetOrCreateException());
        }
    }
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<TValue> GetEnumerator()
    {
        if (_ok)
        {
            yield return _value;
        }
    }

    public bool Equals(Result<TValue> result) => Match(
        okValue => result.IsOk(out var resultValue) && EqualityComparer<TValue>.Default.Equals(okValue!, resultValue!),
        // ReSharper disable once UnusedParameter.Local
        error => result.IsError());

    public bool Equals(Result r) => this.IsOk() == r.IsOk();
    public bool Equals(TValue? value) => IsOk(out var okValue) && EqualityComparer<TValue>.Default.Equals(okValue!, value!);
    public bool Equals(Exception? _) => IsError();
    public bool Equals(bool isOk) => IsOk();

    public override bool Equals(object? obj)
    {
        return obj switch
        {
            Result<TValue> result => Equals(result),
            Result r => Equals(r),
            TValue value => Equals(value),
            Exception exception => Equals(exception),
            bool isOk => Equals(isOk),
            _ => false,
        };
    }

    public override int GetHashCode() => _ok ? HashCode.Combine<TValue>(_value) : 0;
    
    public string ToString(string? format, IFormatProvider? provider = default)
    {
        return Match(value =>
        {
            string? valueStr;
            // ReSharper disable once MergeCastWithTypeCheck
            if (value is IFormattable)
            {
                valueStr = ((IFormattable)value).ToString(format, provider);
            }
            else
            {
                valueStr = value?.ToString();
            }

            return $"Ok<{typeof(TValue).Name}>({valueStr})";
        }, error => $"Error([{error.GetType().Name}]: {error.Message})");
    }

    public override string ToString() => Match(
        value => $"Ok<{typeof(TValue).Name}>({value})",
        error => $"Error([{error.GetType().Name}]: {error.Message})");
}