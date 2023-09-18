using System.Collections;

namespace Quva.Services.Loading.Helper;

public static class CartesianProductContainer
{
    // Begründung für static: Compiler Error CS1106:
    // Extension methods must be defined as static methods in a non-generic static class.

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
                if (slot.MoveNext())
                {
                    // we could increase the current enumerator without reset so stop here
                    break;
                }
                if (slot == slots.Last())
                {
                    // stop when the last enumerator resets
                    yield break;
                }
                // reset the slot if it couldn't move next
                slot.Reset();
                // move to the next enumerator if this reseted
                slot.MoveNext();
                continue;
            }
        }
    }

}