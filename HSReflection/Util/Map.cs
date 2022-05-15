using System.Collections.Generic;

#nullable enable

namespace HSReflection.Util
{
    public static class Map
    {
        public static dynamic? GetValue(dynamic map, dynamic key)
        {
            if (map["keySlots"] == null)
            {
                foreach (var curEntry in map["entries"])
                {
                    if (curEntry == null) continue;
                    if (key == curEntry["key"]) return curEntry["value"];
                }
            }
            else
            {
                int i = 0;
                foreach (var curKey in map["keySlots"])
                {
                    if (curKey == null) continue;
                    if (key == curKey) return map["valueSlots"][i];

                    i++;
                }
            }

            return null;
        }

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(dynamic map)
        {

            Dictionary<TKey, TValue> dictionary = new();

            if (map["keySlots"] == null)
            {
                foreach (dynamic curEntry in map["entries"])
                {
                    if (curEntry == null) continue;
                    TValue value = TypeUtil.CreateClass(curEntry["value"].Class.FullName, curEntry["value"].Fields);

                    try
                    {
                        dictionary.Add((TKey)curEntry["key"], value);
                    }
                    catch (System.ArgumentException)
                    {
                    }
                }
            }
            else
            {
                int i = 0;
                foreach (dynamic key in map["keySlots"])
                {
                    if (key == null) continue;
                    TValue value =
                        TypeUtil.CreateClass(map["valueSlots"][i].Class.FullName, map["valueSlots"][i].Fields);

                    try
                    {
                        dictionary.Add((TKey)key, value);
                    }
                    catch (System.ArgumentException)
                    {
                    }

                    i++;
                }
            }

            return dictionary;
        }
    }
}

#nullable restore
