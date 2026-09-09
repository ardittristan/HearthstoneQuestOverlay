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

    // todo: unsure about 3 and 5+
    [Description("Event Daily")] DAILY_EVENT = 132,
    [Description("Event Step")] STEP_EVENT,
    [Description("Event Daily")] DAILY1_EVENT,
    [Description("Event Step")] STEP1_EVENT,
    [Description("Event Daily")] DAILY2_EVENT,
    [Description("Event Step")] STEP2_EVENT,
    [Description("Event Daily")] DAILY3_EVENT,
    [Description("Event Step")] STEP3_EVENT,
    [Description("Event Daily")] DAILY4_EVENT,
    [Description("Event Step")] STEP4_EVENT,
    [Description("Event Daily")] DAILY5_EVENT,
    [Description("Event Step")] STEP5_EVENT,
    [Description("Event Daily")] DAILY6_EVENT,
    [Description("Event Step")] STEP6_EVENT,
    [Description("Event Daily")] DAILY7_EVENT,
    [Description("Event Step")] STEP7_EVENT,
    [Description("Event Daily")] DAILY8_EVENT,
    [Description("Event Step")] STEP8_EVENT,
    [Description("Event Daily")] DAILY9_EVENT,
    [Description("Event Step")] STEP9_EVENT
}