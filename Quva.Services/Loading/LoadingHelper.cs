using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quva.Services.Loading;

public static class CartesianProductContainer
{
    // https://stackoverflow.com/questions/14643009/generate-all-possible-combinations-from-multiple-arrays-c-vb-net
    // does not work
    public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
    {
        IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };
        return sequences.Aggregate(
            emptyProduct,
            (accumulator, sequence) =>
                from accseq in accumulator
                from item in sequence
                select accseq.Concat(new[] { item }));
    }


    // https://codereview.stackexchange.com/questions/122699/finding-a-cartesian-product-of-multiple-lists
    // Works
    public static IEnumerable Cartesian(this IEnumerable<IEnumerable> items)
    {
        var slots = items
           // initialize enumerators
           .Select(x => x.GetEnumerator())
           // get only those that could start in case there is an empty collection
           .Where(x => x.MoveNext())
           .ToArray();

        while (true)
        {
            // yield current values
            yield return slots.Select(x => x.Current);

            // increase enumerators
            foreach (var slot in slots)
            {
                // reset the slot if it couldn't move next
                if (!slot.MoveNext())
                {
                    // stop when the last enumerator resets
                    if (slot == slots.Last()) { yield break; }
                    slot.Reset();
                    slot.MoveNext();
                    // move to the next enumerator if this reseted
                    continue;
                }
                // we could increase the current enumerator without reset so stop here
                break;
            }
        }
    }

}