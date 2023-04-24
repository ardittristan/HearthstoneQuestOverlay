using System.Configuration;

namespace TextureExtractor.Properties;

internal sealed class Settings : ApplicationSettingsBase
{
    public static Settings Default { get; } = (Settings)Synchronized(new Settings());

    [UserScopedSetting]
    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public SerializableStringDictionary VersionStore
    {
        get
        {
            this[nameof(VersionStore)] ??= new SerializableStringDictionary();
            return (SerializableStringDictionary)this[nameof(VersionStore)];
        }
    }
}