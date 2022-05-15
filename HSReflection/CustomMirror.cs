using HearthMirror;

#nullable disable

namespace HSReflection
{
    internal partial class CustomMirror : Mirror
    {
        private dynamic _bgsClient;

        internal new void Clean()
        {
            _bgsClient = null;
            base.Clean();
        }
    }
}

#nullable restore
