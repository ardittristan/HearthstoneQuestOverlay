namespace HSReflection.Util;

public static class Map
{
    public static dynamic? GetValue(MonoWrapper? map, dynamic key)
    {
        if (map == null) return null;

        if (map["keySlots"] == null)
            foreach (MonoWrapper? curEntry in map["_entries"]!.AsArray())
            {
                if (curEntry.Value == null) continue;
                if (key == curEntry["key"]!.Value) return curEntry["value"]!.Value;
            }
        else
        {
            int i = 0;
            foreach (MonoWrapper? curKey in map["keySlots"]!.AsArray())
            {
                if (curKey.Value == null) continue;
                if (key == curKey.Value) return map["valueSlots"]!.Value[i];

                i++;
            }
        }

        return null;
    }
}