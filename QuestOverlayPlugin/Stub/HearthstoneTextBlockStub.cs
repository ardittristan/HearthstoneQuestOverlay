namespace QuestOverlayPlugin.Stub
{
#if DEBUG

    using System.Windows;
    using System.Windows.Markup;

    internal class HearthstoneTextBlockStub : OutlinedTextBlockStub, IComponentConnector
    {
        private bool _contentLoaded;

        public HearthstoneTextBlockStub() => InitializeComponent();
        
        public void InitializeComponent()
        {
            if (_contentLoaded) return;
            _contentLoaded = true;
            TextWrapping = TextWrapping.NoWrap;
            FontSize = 20;
        }
        
        void IComponentConnector.Connect(int connectionId, object target) => _contentLoaded = true;
    }

#else

    using Hearthstone_Deck_Tracker;

    internal class HearthstoneTextBlockStub : HearthstoneTextBlock {}

#endif
}