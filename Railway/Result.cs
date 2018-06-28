using System;

namespace Lette.Functional.CSharp
{
    public abstract class Result<T>
    {
        public static Result<T> Ok(T value) => new OkImpl(value);
        public static Result<T> Error(string message) => new ErrorImpl(message);

        public abstract TOut Match<TOut>(Func<T, TOut> ok, Func<string, TOut> error);

        private class OkImpl : Result<T>
        {
            private readonly T _value;

            public OkImpl(T value)
            {
                _value = value;
            }

            public override TOut Match<TOut>(Func<T, TOut> ok, Func<string, TOut> error)
            {
                return ok(_value);
            }

            public override string ToString()
            {
                return "Ok: " + _value;
            }
        }

        private class ErrorImpl : Result<T>
        {
            private readonly string _message;

            public ErrorImpl(string message)
            {
                _message = message;
            }

            public override TOut Match<TOut>(Func<T, TOut> ok, Func<string, TOut> error)
            {
                return error(_message);
            }

            public override string ToString()
            {
                return "Error: " + _message;
            }
        }
    }

    public static class ResultExtensions
    {
        public static Func<Result<TIn>, Result<TOut>> Bind<TIn, TOut>(Func<TIn, Result<TOut>> func)
        {
            return result => result.Match(
                ok:    @in => func(@in),
                error: msg => Result<TOut>.Error(msg));
        }

        public static Func<Result<TIn>, Result<TOut>> Map<TIn, TOut>(Func<TIn, TOut> func)
        {
            return Bind(func.Compose(Result<TOut>.Ok));
        }

        public static Func<Result<T>, Result<T>> Map<T>(Action<T> action)
        {
            return t => t.Match(
                ok: @in =>
                {
                    action(@in);
                    return Result<T>.Ok(@in);
                },
                error: msg => Result<T>.Error(msg));
        }

        public static Func<Result<TIn>, Result<TOut>> TryMap<TIn, TOut>(Func<TIn, TOut> func)
        {
            return result => result.Match(
                ok: @in =>
                {
                    try
                    {
                        return Result<TOut>.Ok(func(@in));
                    }
                    catch (Exception ex)
                    {
                        return Result<TOut>.Error(ex.Message);
                    }
                },
                error: msg => Result<TOut>.Error(msg));
        }

        public static Func<Result<T>, Result<T>> TryMap<T>(Action<T> action)
        {
            return result => result.Match(
                ok: @in =>
                {
                    try
                    {
                        action(@in);
                        return Result<T>.Ok(@in);
                    }
                    catch (Exception ex)
                    {
                        return Result<T>.Error(ex.Message);
                    }
                },
                error: msg => Result<T>.Error(msg));
        }

        public static Result<TOut> Apply<TIn, TOut>(Result<Func<TIn, TOut>> func, Result<TIn> value)
        {
            return func.Match(
                ok:    f   => value.Match(
                    ok:    v   => Result<TOut>.Ok(f(v)),
                    error: msg => Result<TOut>.Error(msg)),
                error: msg => Result<TOut>.Error(msg));
        }

        public static Func<TIn, Maybe<TOut>> ToMaybe<TIn, TOut>(this Func<TIn, Result<TOut>> func)
        {
            return @in => func(@in).Match(
                ok:    @out => Maybe<TOut>.Just(@out),
                error: _    => Maybe<TOut>.Nothing);
        }
    }
}