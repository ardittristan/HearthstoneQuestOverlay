using HSReflection;
using HSReflection.Objects;

namespace QuestOverlayPlugin.Util;

public static class QuestDataUtil
{
    public static List<Quest>? QuestData;

    public static bool UpdateQuestData(bool force)
    {
        if (QuestData == null || force)
            QuestData = Reflection.GetQuests();

        return QuestData != null;
    }
}