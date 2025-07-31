using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using ScryDotNet;

namespace QuestOverlayPlugin.HSReflection.Util;

internal static class MonoUtils
{
    #region Iterators

    public static IEnumerable<KeyValuePair<TKey, TValue>> GetDictIterator<TKey, TValue>(this MonoObject? dict) where TKey : notnull
    {
        if (dict == null)
            yield break;

        MonoArray? array = dict.GetArr("_entries")!;
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (array == null)
            yield break;

        for (uint i = 0; i < array.size(); i++)
        {
            MonoStruct kvp = array.GetStruct(i)!;
            yield return new KeyValuePair<TKey, TValue>(kvp.Get<TKey>("key"), kvp.Get<TValue>("value"));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> GetListIterator<T>(this MonoObject? list) =>
        GetArrayIterator<T>(list.GetListAsArr());

    public static IEnumerable<T> GetArrayIterator<T>(this MonoArray? array)
    {
        if (array == null)
            yield break;

        for (uint i = 0; i < array.size(); i++)
            yield return array.Get<T>(i);
    }

    #endregion

    #region List

    [return: NotNullIfNotNull(nameof(obj))]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MonoArray? GetListAsArr(this MonoObject? obj) => obj.GetArr("_items");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint GetListSize(this MonoObject? obj) => obj.GetListAsArr()?.size() ?? 0;

    #endregion

    #region Object

    public static MonoObject? GetObj(this MonoObject? obj, string key) => obj?[key];
    public static MonoArray? GetArr(this MonoObject? obj, string key) => obj?[key];
    public static MonoStruct? GetStruct(this MonoObject? obj, string key) => obj?[key];
    public static T Get<T>(this MonoObject obj, string key) => obj[key];
    public static T? TryGet<T>(this MonoObject? obj, string key) => obj?[key];

    #endregion

    #region Array

    public static MonoObject? GetObj(this MonoArray? arr, uint index) => (MonoObject?)arr?[index];
    public static MonoArray? GetArr(this MonoArray? arr, uint index) => (MonoArray?)arr?[index];
    public static MonoStruct? GetStruct(this MonoArray? arr, uint index) => (MonoStruct?)arr?[index];
    public static T Get<T>(this MonoArray arr, uint index) => (T)arr[index];
    public static T? TryGet<T>(this MonoArray? arr, uint index) => (T?)arr?[index];

    #endregion

    #region Struct

    public static MonoObject? GetObj(this MonoStruct? @struct, string key) => @struct?[key];
    public static MonoArray? GetArr(this MonoStruct? @struct, string key) => @struct?[key];
    public static MonoStruct? GetStruct(this MonoStruct? @struct, string key) => @struct?[key];
    public static T Get<T>(this MonoStruct @struct, string key) => @struct[key];
    public static T? TryGet<T>(this MonoStruct? @struct, string key) => @struct?[key];

    #endregion

    #region Class

    public static MonoObject? GetObj(this MonoClass? @class, string key) => @class?[key];
    public static MonoArray? GetArr(this MonoClass? @class, string key) => @class?[key];
    public static MonoStruct? GetStruct(this MonoClass? @class, string key) => @class?[key];
    public static T Get<T>(this MonoClass @class, string key) => @class[key];
    public static T? TryGet<T>(this MonoClass? @class, string key) => @class?[key];

    #endregion
}