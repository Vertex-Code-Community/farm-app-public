

namespace FarmApp.Shared.Extensions;

public static class LinqExtensions
{
    public static IEnumerable<TA> Except<TA, TB, TK>(this IEnumerable<TA> a, IEnumerable<TB> b, Func<TA, TK> selectKeyA, Func<TB, TK> selectKeyB)
    {
        return a.Where(aItem => !b.Select(bItem => selectKeyB(bItem)).Contains(selectKeyA(aItem), null));
    }
}
