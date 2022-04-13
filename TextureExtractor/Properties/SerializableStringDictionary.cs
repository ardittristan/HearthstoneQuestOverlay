using System;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace TextureExtractor.Properties
{
    public class SerializableStringDictionary : StringDictionary, IXmlSerializable
    {
        public XmlSchema GetSchema() => null;

        public void ReadXml(XmlReader reader)
        {
            while (reader.Read() &&
                   !(reader.NodeType == XmlNodeType.EndElement && reader.LocalName == GetType().Name))
            {
                var name = reader["Name"];
                if (name == null)
                    throw new FormatException();

                var value = reader["Value"];
                this[name] = value;
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            foreach (DictionaryEntry entry in this)
            {
                writer.WriteStartElement("Pair");
                writer.WriteAttributeString("Name", (string)entry.Key);
                writer.WriteAttributeString("Value", (string)entry.Value);
                writer.WriteEndElement();
            }
        }
    }
}