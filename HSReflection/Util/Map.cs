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
    }
}

#nullable restore
