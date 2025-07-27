using System.Diagnostics.CodeAnalysis;

namespace QuestOverlayPlugin.HSReflection.Util;

internal static class DynamicUtil
{
    [return: MaybeNull]
    public static T TryCast<T>(dynamic obj)
    {
        try
        {
            return (T)obj;
        }
        catch
        {
            return default;
        }
    }
}