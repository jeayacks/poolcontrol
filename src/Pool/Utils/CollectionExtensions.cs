//-----------------------------------------------------------------------
// <copyright file="CollectionExtensions.cs" company="JeYacks">
//     Copyright (c) JeYacks. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Pool
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Contains extension methods for list types.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Recursively select elements and its children collection, specified by <paramref name="collectionSelector"/>,
        /// and returns the elements of the children collection, specified by <paramref name="resultSelector"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <typeparam name="TResult">The type of the elements of the children collection.</typeparam>
        /// <param name="source">An <see cref="IEnumerable{T}"/> to filter.</param>
        /// <param name="collectionSelector">A transform function to produce a children collection from each element.</param>
        /// <param name="resultSelector">A transform function to produce a result element value from each children collection.</param>
        /// <returns>The collection of children elements.</returns>
        public static IEnumerable<TResult> RecursiveSelect<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TSource>> collectionSelector, Func<TSource, TResult> resultSelector)
        {
            using (IEnumerator<TSource> enumeratorSource = source.GetEnumerator())
            {
                while (enumeratorSource.MoveNext())
                {
                    if (enumeratorSource.Current != null)
                    {
                        yield return resultSelector(enumeratorSource.Current);

                        IEnumerable<TSource> collection = collectionSelector(enumeratorSource.Current);
                        if (collection != null)
                        {
                            foreach (TResult element in RecursiveSelect(collection, collectionSelector, resultSelector))
                            {
                                yield return element;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Recursively select elements and its children collection, specified by <paramref name="collectionSelector"/>,
        /// and returns all the elements in source and in its children collection.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">An <see cref="IEnumerable{T}"/> to filter.</param>
        /// <param name="collectionSelector">A transform function to produce a children collection from each element.</param>
        /// <returns>The collection of elements in source and its children collection.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Need to use Func<TSource, IEnumerable<TSource>> for the collection selector")]
        public static IEnumerable<TSource> RecursiveSelect<TSource>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TSource>> collectionSelector)
        {
            return source.RecursiveSelect(collection => collectionSelector(collection), result => result);
        }
    }
}