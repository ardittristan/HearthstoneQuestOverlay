using HearthMirror.Mono;

namespace HSReflection.Util;

public static class MonoUtil
{
    public static dynamic? ToMonoClass(this uint pClass) =>
        pClass.ToMonoClassInternal();

    internal static MonoClass? ToMonoClassInternal(this uint pClass) =>
        pClass == 0 ? null : new MonoClass(Reflection.Mirror.View, pClass);

    public static dynamic? ToMonoClassField(this uint pField) =>
        pField.ToMonoClassFieldInternal();

    internal static MonoClassField? ToMonoClassFieldInternal(this uint pField) =>
        pField == 0 ? null : new MonoClassField(Reflection.Mirror.View, pField);

    public static dynamic? ToMonoObject(this uint pObject) =>
        pObject.ToMonoObjectInternal();

    internal static MonoObject? ToMonoObjectInternal(this uint pObject) =>
        pObject == 0 ? null : new MonoObject(Reflection.Mirror.View, pObject);

    public static dynamic? ToMonoStruct(this uint pStruct, object mClass) =>
        pStruct.ToMonoStructInternal((MonoClass)mClass);

    internal static MonoStruct? ToMonoStructInternal(this uint pStruct, MonoClass mClass) =>
        pStruct == 0 ? null : new MonoStruct(Reflection.Mirror.View, mClass, pStruct);

    public static dynamic? ToMonoType(this uint pType) =>
        pType.ToMonoTypeInternal();

    internal static MonoType? ToMonoTypeInternal(this uint pType) =>
        pType == 0 ? null : new MonoType(Reflection.Mirror.View, pType);
}