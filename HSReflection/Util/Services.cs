﻿// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
using static HSReflection.Reflection;

namespace HSReflection.Util;

public static class Services
{
    public static MonoWrapper AccountLicenseMgr => new(GetService("AccountLicenseMgr"));
    public static MonoWrapper AchieveManager => new(GetService("AchieveManager"));
    public static MonoWrapper AchievementManager => new(GetService("Hearthstone.Progression.AchievementManager"));
    [Obsolete] public static MonoWrapper ActorNames => new(GetService("ActorNames"));
    public static MonoWrapper AdTrackingManager => new(GetService("AdTrackingManager"));
    public static MonoWrapper AdventureProgressMgr => new(GetService("AdventureProgressMgr"));
    public static MonoWrapper APIGatewayService => new(GetService("Hearthstone.APIGateway.APIGatewayService"));
    public static MonoWrapper Benchmarker = new(GetService("Benchmarker"));
    public static MonoWrapper BreakingNews => new(GetService("Hearthstone.BreakingNews.BreakingNews"));
    public static MonoWrapper CameraManager => new(GetService("CameraManager"));
    public static MonoWrapper CardBackManager => new(GetService("CardBackManager"));
    public static MonoWrapper CheatMgr => new(GetService("CheatMgr"));
    public static MonoWrapper Cheats => new(GetService("Cheats"));
    public static MonoWrapper Cinematic => new(GetService("Cinematic"));
    public static MonoWrapper ClientLocationManager => new(GetService("ClientLocationManager"));
    public static MonoWrapper CoinManager => new(GetService("CoinManager"));
    public static MonoWrapper DebugConsole => new(GetService("DebugConsole"));
    [Obsolete] public static MonoWrapper DeeplinkService => throw new NotSupportedException("Hearthstone.Core.Deeplinking.DeeplinkService");
    public static MonoWrapper DemoMgr => new(GetService("DemoMgr"));
    public static MonoWrapper DiamondRenderToTextureService => new(GetService("DiamondRenderToTextureService"));
    public static MonoWrapper DisconnectMgr => new(GetService("DisconnectMgr"));
    public static MonoWrapper DisposablesCleaner => new(GetService("Blizzard.T5.AssetManager.DisposablesCleaner"));
    public static MonoWrapper DownloadableDbfCache => new(GetService("DownloadableDbfCache"));
    public static MonoWrapper DraftManager => new(GetService("DraftManager"));
    public static MonoWrapper EventTimingManager => new(GetService("EventTimingManager"));
    public static MonoWrapper ExternalUrlService => new(GetService("ExternalUrlService"));
    public static MonoWrapper FiresideGatheringManager => new(GetService("FiresideGatheringManager"));
    public static MonoWrapper FixedRewardsMgr => new(GetService("FixedRewardsMgr"));
    public static MonoWrapper FreeDeckMgr => new(GetService("FreeDeckMgr"));
    public static MonoWrapper FullScreenFXMgr => new(GetService("FullScreenFXMgr"));
    public static MonoWrapper GameDbf => new(GetService("GameDbf"));
    public static MonoWrapper GameDownloadManager => new(GetService("Hearthstone.Streaming.GameDownloadManager"));
    public static MonoWrapper GameMgr => new(GetService("GameMgr"));
    public static MonoWrapper GameplayErrorManager => new(GetService("GameplayErrorManager"));
    public static MonoWrapper GenericRewardChestNoticeManager => new(GetService("GenericRewardChestNoticeManager"));
    public static MonoWrapper HealthyGamingMgr => new(GetService("HealthyGamingMgr"));
    public static MonoWrapper HearthstoneCheckout => new(GetService("HearthstoneCheckout"));
    public static MonoWrapper IAliasedAssetResolver => new(GetService("IAliasedAssetResolver"));
    public static MonoWrapper IAssetLoader => new(GetService("IAssetLoader"));
    public static MonoWrapper IErrorService => new(GetService("IErrorService"));
    public static MonoWrapper IFontTable => new(GetService("Blizzard.T5.Fonts.IFontTable"));
    public static MonoWrapper IGameStringsService => new(GetService("IGameStringsService"));
    public static MonoWrapper IGraphicsManager => new(GetService("IGraphicsManager"));
    public static MonoWrapper ILoginService => new(GetService("Hearthstone.Login.ILoginService"));
    public static MonoWrapper IMaterialService => new(GetService("Blizzard.T5.MaterialService.IMaterialService"));
    public static MonoWrapper InactivePlayerKicker => new(GetService("InactivePlayerKicker"));
    public static MonoWrapper InGameMessageScheduler => new(GetService("Hearthstone.InGameMessage.InGameMessageScheduler"));
    public static MonoWrapper IProductDataService => new(GetService("Hearthstone.Store.IProductDataService"));
    public static MonoWrapper ITouchScreenService => new(GetService("ITouchScreenService"));
    public static MonoWrapper LegendaryHeroRenderToTextureService => new(GetService("LegendaryHeroRenderToTextureService"));
    public static MonoWrapper LocationServices => new(GetService("LocationServices"));
    public static MonoWrapper LoginManager => new(GetService("LoginManager"));
    public static MonoWrapper LuckyDrawManager => new(GetService("LuckyDrawManager"));
    public static MonoWrapper MessagePopupDisplay => new(GetService("Hearthstone.InGameMessage.UI.MessagePopupDisplay"));
    public static MonoWrapper MobileCallbackManager => new(GetService("MobileCallbackManager"));
    public static MonoWrapper MobilePermissionsManager => new(GetService("MobilePermissionsManager"));
    public static MonoWrapper MusicManager => new(GetService("MusicManager"));
    public static MonoWrapper NetCache => new(GetService("NetCache"));
    public static MonoWrapper Network => new(GetService("Network"));
    public static MonoWrapper NetworkReachabilityManager => new(GetService("NetworkReachabilityManager"));
    public static MonoWrapper PartyManager => new(GetService("PartyManager"));
    public static MonoWrapper PerformanceAnalytics => new(GetService("PerformanceAnalytics"));
    public static MonoWrapper PlayerExperimentManager => new(GetService("Hearthstone.PlayerExperiments.PlayerExperimentManager"));
    public static MonoWrapper PlayerMigrationManager => new(GetService("PlayerMigrationManager"));
    public static MonoWrapper PopupDisplayManager => new(GetService("PopupDisplayManager"));
    public static MonoWrapper PrefabInstanceLoadTracker => new(GetService("PrefabInstanceLoadTracker"));
    public static MonoWrapper PrivacyGate => new(GetService("PrivacyGate"));
    public static MonoWrapper QuestManager => new(GetService("Hearthstone.Progression.QuestManager"));
    public static MonoWrapper QuestToastManager => new(GetService("Hearthstone.Progression.QuestToastManager"));
    public static MonoWrapper RAFManager => new(GetService("RAFManager"));
    public static MonoWrapper ReconnectMgr => new(GetService("ReconnectMgr"));
    public static MonoWrapper ReturningPlayerMgr => new(GetService("ReturningPlayerMgr"));
    public static MonoWrapper RewardTrackManager => new(GetService("Hearthstone.Progression.RewardTrackManager"));
    public static MonoWrapper RewardXpNotificationManager => new(GetService("Hearthstone.Progression.RewardXpNotificationManager"));
    public static MonoWrapper SceneMgr => new(GetService("SceneMgr"));
    public static MonoWrapper ScreenEffectsMgr => new(GetService("ScreenEffectsMgr"));
    public static MonoWrapper SetRotationManager => new(GetService("SetRotationManager"));
    public static MonoWrapper ShaderTime => new(GetService("ShaderTime"));
    public static MonoWrapper ShownUIMgr => new(GetService("ShownUIMgr"));
    public static MonoWrapper SoundManager => new(GetService("SoundManager"));
    public static MonoWrapper SpecialEventManager => new(GetService("Hearthstone.Progression.SpecialEventManager"));
    public static MonoWrapper SpellManager => new(GetService("SpellManager"));
    public static MonoWrapper SpriteAtlasProvider => new(GetService("Hearthstone.UI.SpriteAtlasProvider"));
    public static MonoWrapper TavernBrawlManager => new(GetService("TavernBrawlManager"));
    public static MonoWrapper UniversalInputManager => new(GetService("UniversalInputManager"));
    public static MonoWrapper VersionConfigurationService => new(GetService("Hearthstone.Core.Streaming.VersionConfigurationService"));
    public static MonoWrapper WidgetRunner => new(GetService("Hearthstone.UI.WidgetRunner"));
    public static MonoWrapper WifiInfo => new(GetService("WifiInfo"));
}