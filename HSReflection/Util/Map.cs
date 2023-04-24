namespace HSReflection.Util;

public static class Map
{
    public static dynamic? GetValue(dynamic map, dynamic key)
    {
        if (map == null) return null;

        if (map["keySlots"] == null)
            foreach (dynamic? curEntry in map["entries"])
            {
                if (curEntry == null) continue;
                if (key == curEntry["key"]) return curEntry["value"];
            }
        else
        {
            int i = 0;
            foreach (dynamic? curKey in map["keySlots"])
            {
                if (curKey == null) continue;
                if (key == curKey) return map["valueSlots"][i];

                i++;
            }
        }

        return null;
    }
}
