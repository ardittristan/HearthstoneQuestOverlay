using HearthMirror.Mono;

namespace HSReflection.Util;

public static class MonoUtil
{
    public static dynamic? ToMonoClass(this uint pClass) =>
        pClass == 0 ? null : new MonoClass(Reflection.Mirror.View, pClass);

    public static dynamic? ToMonoClassField(this uint pField) =>
        pField == 0 ? null : new MonoClassField(Reflection.Mirror.View, pField);

    public static dynamic? ToMonoObject(this uint pObject) =>
        pObject == 0 ? null : new MonoObject(Reflection.Mirror.View, pObject);

    public static dynamic? ToMonoStruct(this uint pStruct, object mClass) =>
        pStruct == 0 ? null : new MonoStruct(Reflection.Mirror.View, (MonoClass)mClass, pStruct);

    public static dynamic? ToMonoType(this uint pType) =>
        pType == 0 ? null : new MonoType(Reflection.Mirror.View, pType);
}