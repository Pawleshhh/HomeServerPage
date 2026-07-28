using System;
using System.Collections.Generic;
using System.Text;

namespace HomeServerPage.Shared.Extensions;

public static class FpExtensions
{
    extension<T>(T value)
    {

        /// <summary>
        /// Pipes the value into the given function, similar to F#'s |&gt; operator.
        /// </summary>
        public TResult Pipe<TResult>(Func<T, TResult> func) => func(value);

        /// <summary>
        /// Executes the given action against the value and returns the original value unchanged.
        /// Useful for side effects (e.g. logging) inside a pipeline.
        /// </summary>
        public T Tap(Action<T> action)
        {
            action(value);
            return value;
        }

        /// <summary>
        /// Applies the given function only when the predicate is true, otherwise returns the original value.
        /// </summary>
        public T PipeIf(bool condition, Func<T, T> func) => condition ? func(value) : value;

        /// <summary>
        /// Applies the given function only when the predicate is true, otherwise returns the original value.
        /// </summary>
        public T PipeIf(Func<T, bool> predicate, Func<T, T> func) => predicate(value) ? func(value) : value;

    }

    extension<T1, T2>((T1, T2) values)
    {

        /// <summary>
        /// Pipes the tuple values into the given function, similar to F#'s |&gt; operator.
        /// </summary>
        public TResult Pipe<TResult>(Func<T1, T2, TResult> func) => func(values.Item1, values.Item2);

        /// <summary>
        /// Executes the given action against the tuple values and returns the original tuple unchanged.
        /// Useful for side effects (e.g. logging) inside a pipeline.
        /// </summary>
        public (T1, T2) Tap(Action<T1, T2> action)
        {
            action(values.Item1, values.Item2);
            return values;
        }

    }

    extension<T1, T2, T3>((T1, T2, T3) values)
    {

        /// <summary>
        /// Pipes the tuple values into the given function, similar to F#'s |&gt; operator.
        /// </summary>
        public TResult Pipe<TResult>(Func<T1, T2, T3, TResult> func) => func(values.Item1, values.Item2, values.Item3);

        /// <summary>
        /// Executes the given action against the tuple values and returns the original tuple unchanged.
        /// Useful for side effects (e.g. logging) inside a pipeline.
        /// </summary>
        public (T1, T2, T3) Tap(Action<T1, T2, T3> action)
        {
            action(values.Item1, values.Item2, values.Item3);
            return values;
        }

    }

    extension<T, TResult>(Func<T, TResult> func)
    {

        /// <summary>
        /// Composes two functions into one, similar to F#'s &gt;&gt; operator.
        /// </summary>
        public Func<T, TNext> Then<TNext>(Func<TResult, TNext> next) => value => next(func(value));

    }

    extension<T1, T2, TResult>(Func<T1, T2, TResult> func)
    {

        /// <summary>
        /// Composes two functions into one, similar to F#'s &gt;&gt; operator.
        /// </summary>
        public Func<T1, T2, TNext> Then<TNext>(Func<TResult, TNext> next) => (a, b) => next(func(a, b));

    }

    extension<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func)
    {

        /// <summary>
        /// Composes two functions into one, similar to F#'s &gt;&gt; operator.
        /// </summary>
        public Func<T1, T2, T3, TNext> Then<TNext>(Func<TResult, TNext> next) => (a, b, c) => next(func(a, b, c));

    }
}
