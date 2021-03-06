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
    }
}

#nullable restore
