using System.Collections.Generic;

#nullable enable

namespace HSReflection.Util
{
    public static class Map
    {
        public static dynamic? GetValue(dynamic map, dynamic key)
        {
            int i = 0;
            foreach (var curKey in map["keySlots"])
            {
                if (key == curKey) return map["valueSlots"][i];

                i++;
            }

            return null;
        }

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(dynamic map)
        {

            Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
            
            int i = 0;
            foreach (dynamic key in map["keySlots"])
            {
                TValue value = TypeUtil.CreateClass(map["valueSlots"][i].Class.FullName, map["valueSlots"][i].Fields);

                try
                {
                    dictionary.Add((TKey)key, value);
                } 
                catch (System.ArgumentException) { }

                i++;
            }

            return dictionary;
        }
    }
}

#nullable restore
