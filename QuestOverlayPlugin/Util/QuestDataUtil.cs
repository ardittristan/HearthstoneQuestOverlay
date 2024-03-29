﻿using System.Diagnostics.CodeAnalysis;
using HSReflection;
using HSReflection.Objects;

namespace QuestOverlayPlugin.Util;

public static class QuestDataUtil
{
    public static List<Quest>? QuestData;

    [MemberNotNullWhen(true, nameof(QuestData))]
    public static async Task<bool> UpdateQuestDataAsync(bool force)
    {
        if (QuestData == null || force)
            QuestData = await Task.Run(Reflection.GetQuests);

        return QuestData != null;
    }

    [MemberNotNullWhen(true, nameof(QuestData))]
    public static bool UpdateQuestData(bool force)
    {
        if (QuestData == null || force)
            QuestData = (List<Quest>?)Reflection.GetQuests();

        return QuestData != null;
    }
}