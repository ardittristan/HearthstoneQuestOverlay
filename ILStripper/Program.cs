ILStrip stripper = new(args[0])
{
    CollectUsageFromReferences = true,
    RemoveUnknownResources = true
};

stripper.EntryPoints.Add(args[1]);

stripper.ScanUsedClasses();

stripper.ScanUnusedClasses();

stripper.UnusedTypes.RemoveAll(x => x?.BaseType?.FullName == "System.Attribute");

stripper.CleanupUnusedClasses();

stripper.CleanupUnusedResources();

stripper.CleanupUnusedReferences();

stripper.MakeInternalExclusions.Add(args[1]);

stripper.MakeInternal();

stripper.Save(args[0]);