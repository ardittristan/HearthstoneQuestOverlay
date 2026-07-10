// ReSharper disable InconsistentNaming

using System.ComponentModel;

namespace QuestOverlayPlugin.HSReflection.Enums;

[GenerateEnumDescriptor]
public enum QuestPoolType
{
    [Description("Invalid")] INVALID,
    [Description("Daily")] DAILY,
    [Description("Weekly")] WEEKLY,
    [Description("Battlegrounds")] BATTLEGROUNDS = 18,
    [Description("Event Daily")] DAILY_EVENT = 132,
    [Description("Event Step")] STEP_EVENT = 133,
    [Description("Event Daily")] DAILY1_EVENT = 134,
    [Description("Event Step")] STEP1_EVENT = 135,
    [Description("Event Daily")] DAILY2_EVENT = 136,
    [Description("Event Step")] STEP2_EVENT = 137
}