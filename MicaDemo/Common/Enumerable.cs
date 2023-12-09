using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace MicaDemo.Common
{
    public static class Enumerable
    {
        /// <summary>
        /// Performs the specified action on each element of the <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source"></param>
        /// <param name="action">The <see cref="Action{T}"/> delegate to perform on each element of the <see cref="List{T}"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="action"/> is null.</exception>
        public static void ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource> action)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (source is List<TSource> list)
            {
                list.ForEach(action);
            }
            else if (source is ImmutableList<TSource> immutableList)
            {
                immutableList.ForEach(action);
            }
            else
            {
                foreach (TSource item in source)
                {
                    action(item);
                }
            }
        }
    }
}
