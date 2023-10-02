﻿// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
using ScryDotNet;
using static HSReflection.Reflection;

namespace HSReflection.Util;

public static class Services
{
    public static MonoObject IErrorService => GetService("IErrorService");
    public static MonoObject GameDownloadManager => GetService("Hearthstone.Streaming.GameDownloadManager");
    public static MonoObject Network => GetService("Network");
    public static MonoObject DownloadableDbfCache => GetService("DownloadableDbfCache");
    public static MonoObject EventTimingManager => GetService("EventTimingManager");
    public static MonoObject GameMgr => GetService("GameMgr");
    public static MonoObject DraftManager => GetService("DraftManager");
    public static MonoObject AdventureProgressMgr => GetService("AdventureProgressMgr");
    public static MonoObject AchieveManager => GetService("AchieveManager");
    public static MonoObject AchievementManager => GetService("Hearthstone.Progression.AchievementManager");
    public static MonoObject QuestManager => GetService("Hearthstone.Progression.QuestManager");
    public static MonoObject RewardTrackManager => GetService("Hearthstone.Progression.RewardTrackManager");
    public static MonoObject SpecialEventManager => GetService("Hearthstone.Progression.SpecialEventManager");
    public static MonoObject GenericRewardChestNoticeManager => GetService("GenericRewardChestNoticeManager");
    public static MonoObject AccountLicenseMgr => GetService("AccountLicenseMgr");
    public static MonoObject FixedRewardsMgr => GetService("FixedRewardsMgr");
    public static MonoObject ReturningPlayerMgr => GetService("ReturningPlayerMgr");
    public static MonoObject FreeDeckMgr => GetService("FreeDeckMgr");
    public static MonoObject DemoMgr => GetService("DemoMgr");
    public static MonoObject NetCache => GetService("NetCache");
    public static MonoObject GameDbf => GetService("GameDbf");
    public static MonoObject DebugConsole => GetService("DebugConsole");
    public static MonoObject TavernBrawlManager => GetService("TavernBrawlManager");
    public static MonoObject IAssetLoader => GetService("IAssetLoader");
    public static MonoObject IAliasedAssetResolver => GetService("IAliasedAssetResolver");
    public static MonoObject LoginManager => GetService("LoginManager");
    public static MonoObject CardBackManager => GetService("CardBackManager");
    public static MonoObject CheatMgr => GetService("CheatMgr");
    public static MonoObject Cheats => GetService("Cheats");
    public static MonoObject ReconnectMgr => GetService("ReconnectMgr");
    public static MonoObject DisconnectMgr => GetService("DisconnectMgr");
    public static MonoObject HealthyGamingMgr => GetService("HealthyGamingMgr");
    public static MonoObject SoundManager => GetService("SoundManager");
    public static MonoObject MusicManager => GetService("MusicManager");
    public static MonoObject RAFManager => GetService("RAFManager");
    public static MonoObject InactivePlayerKicker => GetService("InactivePlayerKicker");
    public static MonoObject ClientLocationManager => GetService("ClientLocationManager");
    public static MonoObject AdTrackingManager => GetService("AdTrackingManager");
    public static MonoObject SpellManager => GetService("SpellManager");
    public static MonoObject LocationServices => GetService("LocationServices");
    public static MonoObject WifiInfo => GetService("WifiInfo");
    public static MonoObject FiresideGatheringManager => GetService("FiresideGatheringManager");
    public static MonoObject GameplayErrorManager => GetService("GameplayErrorManager");
    public static MonoObject IFontTable => GetService("Blizzard.T5.Fonts.IFontTable");
    public static MonoObject UniversalInputManager => GetService("UniversalInputManager");
    public static MonoObject ScreenEffectsMgr => GetService("ScreenEffectsMgr");
    public static MonoObject ShownUIMgr => GetService("ShownUIMgr");
    public static MonoObject PerformanceAnalytics => GetService("PerformanceAnalytics");
    public static MonoObject PopupDisplayManager => GetService("PopupDisplayManager");
    public static MonoObject IGraphicsManager => GetService("IGraphicsManager");
    public static MonoObject MobilePermissionsManager => GetService("MobilePermissionsManager");
    public static MonoObject ShaderTime => GetService("ShaderTime");
    public static MonoObject MobileCallbackManager => GetService("MobileCallbackManager");
    public static MonoObject FullScreenFXMgr => GetService("FullScreenFXMgr");
    public static MonoObject SceneMgr => GetService("SceneMgr");
    public static MonoObject SetRotationManager => GetService("SetRotationManager");
    public static MonoObject Cinematic => GetService("Cinematic");
    public static MonoObject WidgetRunner => GetService("Hearthstone.UI.WidgetRunner");
    public static MonoObject SpriteAtlasProvider => GetService("Hearthstone.UI.SpriteAtlasProvider");
    public static MonoObject IProductDataService => GetService("Hearthstone.Store.IProductDataService");
    public static MonoObject HearthstoneCheckout => GetService("HearthstoneCheckout");
    public static MonoObject NetworkReachabilityManager => GetService("NetworkReachabilityManager");
    public static MonoObject VersionConfigurationService => GetService("Hearthstone.Core.Streaming.VersionConfigurationService");
    [Obsolete] public static MonoObject DeeplinkService => throw new NotSupportedException("Hearthstone.Core.Deeplinking.DeeplinkService");
    public static MonoObject ILoginService => GetService("Hearthstone.Login.ILoginService");
    public static MonoObject PartyManager => GetService("PartyManager");
    public static MonoObject PlayerMigrationManager => GetService("PlayerMigrationManager");
    public static MonoObject CoinManager => GetService("CoinManager");
    public static MonoObject QuestToastManager => GetService("Hearthstone.Progression.QuestToastManager");
    public static MonoObject RewardXpNotificationManager => GetService("Hearthstone.Progression.RewardXpNotificationManager");
    public static MonoObject IMaterialService => GetService("Blizzard.T5.MaterialService.IMaterialService");
    public static MonoObject InGameMessageScheduler => GetService("Hearthstone.InGameMessage.InGameMessageScheduler");
    public static MonoObject DisposablesCleaner => GetService("Blizzard.T5.AssetManager.DisposablesCleaner");
    public static MonoObject PrefabInstanceLoadTracker => GetService("PrefabInstanceLoadTracker");
    public static MonoObject ExternalUrlService => GetService("ExternalUrlService");
    public static MonoObject DiamondRenderToTextureService => GetService("DiamondRenderToTextureService");
    public static MonoObject MessagePopupDisplay => GetService("Hearthstone.InGameMessage.UI.MessagePopupDisplay");
    public static MonoObject PrivacyGate => GetService("PrivacyGate");
    public static MonoObject APIGatewayService => GetService("Hearthstone.APIGateway.APIGatewayService");
    public static MonoObject BreakingNews => GetService("Hearthstone.BreakingNews.BreakingNews");
    public static MonoObject IGameStringsService => GetService("IGameStringsService");
    public static MonoObject LuckyDrawManager => GetService("LuckyDrawManager");
    public static MonoObject CameraManager => GetService("CameraManager");
    public static MonoObject LegendaryHeroRenderToTextureService => GetService("LegendaryHeroRenderToTextureService");
    public static MonoObject ActorNames => GetService("ActorNames");
    public static MonoObject ITouchScreenService => GetService("ITouchScreenService");
}