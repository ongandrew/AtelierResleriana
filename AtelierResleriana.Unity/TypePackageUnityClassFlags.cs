namespace AtelierResleriana.Unity
{
    [Flags]
    public enum TypePackageUnityClassFlags
    {
        NONE = 0,
        IsAbstract = 1,
        IsSealed = 2,
        IsEditorOnly = 4,
        IsReleaseOnly = 8,
        IsStripped = 16,
        Reserved = 32,
        HasEditorRootNode = 64,
        HasReleaseRootNode = 128
    }
}
