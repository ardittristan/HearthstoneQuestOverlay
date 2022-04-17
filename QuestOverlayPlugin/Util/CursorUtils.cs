using System.IO;

namespace QuestOverlayPlugin.Util
{
    public static class CursorUtils
    {
        public static Stream GetTransformedCur(string path, byte hotspotX, byte hotspotY)
        {
            FileStream s = File.OpenRead(path);

            byte[] buffer = new byte[s.Length];

            s.Read(buffer, 0, (int)s.Length);

            MemoryStream ms = new MemoryStream();

            buffer[10] = hotspotX;
            buffer[12] = hotspotY;

            ms.Write(buffer, 0, (int)s.Length);

            ms.Position = 0;

            return ms;
        }
    }
}
