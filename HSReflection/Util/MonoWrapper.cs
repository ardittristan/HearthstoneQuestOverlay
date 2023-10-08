using System.Diagnostics;
using ScryDotNet;

namespace HSReflection.Util;

[DebuggerDisplay("{Value}")]
public class MonoWrapper
{
    public dynamic? Value { get; }

    public MonoWrapper(dynamic? value)
    {
        Value = value!;
    }

    public MonoWrapper[] AsArray() => ((dynamic[])Value!).Select(v => new MonoWrapper(v)).ToArray();

    public MonoWrapper? this[string key]
    {
        get
        {
            try
            {
                switch (Value)
                {
                    case MonoObject mObj:
                        Dictionary<string, object> mObjFields = mObj.getFields();
                        return mObjFields.Count == 0
                            ? new MonoWrapper(mObj[key])
                            : new MonoWrapper(mObjFields[key]);
                    case MonoStruct mStr:
                        Dictionary<string, object> mStrFields = mStr.getFields();
                        return mStrFields.Count == 0
                            ? new MonoWrapper(mStr[key])
                            : new MonoWrapper(mStrFields[key]);
                    default:
                        return null;
                }
            }
            catch
            {
                return null;
            }
        }
    }

    public static bool operator ==(MonoWrapper? obj1, MonoWrapper? obj2) =>
        obj1?.Value is null
            ? obj2?.Value is null
            : obj1.Equals(obj2);

    public static bool operator !=(MonoWrapper obj1, MonoWrapper obj2) => !(obj1 == obj2);

    // ReSharper disable once BaseObjectEqualsIsObjectEquals
    public override bool Equals(object? obj) => base.Equals(obj);

    public override int GetHashCode() => Value?.GetHashCode();
}