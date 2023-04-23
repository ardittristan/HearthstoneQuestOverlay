// ReSharper disable once CheckNamespace
internal static class Extensions
{
    public static void RemoveAll<T>(this ICollection<T> @this, Func<T, bool> predicate)
    {
        if (@this is List<T> list)
            list.RemoveAll(new Predicate<T>(predicate));
        else
            foreach (var item in @this.Where(predicate))
                @this.Remove(item);
    }
}