using System.Diagnostics;
using ScryDotNet;

namespace HSReflection.Util;

[DebuggerDisplay("{Value}")]
public class MonoWrapper
{
    private readonly dynamic? _value;
    public dynamic? Value =>
        _value is MonoClassField cfValue
            ? cfValue.getStaticValue()
            : _value;

    public MonoWrapper(dynamic? value) => _value = value!;

    public MonoWrapper[] AsArray() =>
        ((dynamic[]?)Value)?.Select(v => new MonoWrapper(v)).ToArray() ?? Array.Empty<MonoWrapper>();

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
                    case MonoClass mCls:
                        Dictionary<string, MonoClassField> mClsFields = mCls.getFields();
                        return mClsFields.Count == 0
                            ? new MonoWrapper(mCls[key])
                            : new MonoWrapper(mClsFields[key]);
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

    public override bool Equals(object? obj) => Value?.Equals(obj);

    public override int GetHashCode() => Value?.GetHashCode();
}