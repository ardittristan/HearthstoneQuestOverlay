using System.Collections.Generic;
using Cfg.Net;

namespace Texture2DExtractor.Config
{
    public class Cfg : CfgNode
    {
        [Cfg]
        public List<FileVersion> VersionCache { get; set; }
    }
}
