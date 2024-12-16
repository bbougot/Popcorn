using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Popcorn.Extensions
{
    public static class ObservableDrainExtensions
    {
        public static IObservable<TOut> Drain<TSource, TOut>(this IObservable<TSource> source,
            Func<TSource, IObservable<TOut>> selector)
        {
            return Observable.Defer(() =>
            {
                BehaviorSubject<Unit> queue = new BehaviorSubject<Unit>(new Unit());
                var stack = new Stack<TSource>();

                return source
                    .Do(item => stack.Push(item))
                    .Zip(queue, (v, q) => v)
                    .Select(_ => stack.Pop())
                    .SelectMany(v => selector(v)
                        .Do(_ =>
                        {

                        }, () =>
                        {
                            queue.OnNext(new Unit());
                        })
                    );
            });
        }
    }
}
