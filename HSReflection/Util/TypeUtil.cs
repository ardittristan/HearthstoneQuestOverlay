using System.Reflection;

#nullable enable

namespace HSReflection.Util;

public static class TypeUtil
{
    public static Type? FindType(string fullName)
    {
        return
            AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.FullName!.Equals(fullName));
    }

    public static object? CreateClass(string fullClassName, IEnumerable<KeyValuePair<string, object>> fields)
    {
        return fullClassName switch
        {
            "System.DateTime" => typeof(DateTime)
                .GetConstructor(
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new[] { typeof(ulong) },
                    null
                )?.Invoke(new object[] { (ulong)fields.First(pair => pair.Key == "dateData").Value }),
            _ => null
        };
    }
}

#nullable restore
