namespace StuffableCore.Settings.Editor
{
    internal interface IEditorModule<T> where T : ISettings
    {

        T GetDefaultEditor(StuffableCategorySettings stuffableCategorySettings);

    }
}
