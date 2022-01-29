using Cfg.Net;

namespace Texture2DExtractor.Config
{
    public class FileVersion : CfgNode
    {
        [Cfg(unique = true)]
        public string FileName { get; set; }
        [Cfg]
        public string Version { get; set; }
    }
}
