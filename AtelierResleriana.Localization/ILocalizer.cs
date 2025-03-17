using System.Text.Json.Nodes;

namespace AtelierResleriana.Localization
{
    public interface ILocalizer
    {
        string Locale { get; }
        Task<string> LocalizeAsync(string text, CancellationToken cancellationToken = default);
        Task<IEnumerable<GeneralText>> LocalizeAsync(IEnumerable<GeneralText> generalTexts, CancellationToken cancellationToken = default);
        Task<IEnumerable<DialogueText>> LocalizeAsync(IEnumerable<DialogueText> dialogueTexts, CancellationToken cancellationToken = default);
        Task<IEnumerable<DialogueText>> LocalizeAsync(IEnumerable<DialogueText> dialogueTexts, IEnumerable<LocalizedDialogueText> localizedDialogueTexts, CancellationToken cancellationToken = default);
        Task<IEnumerable<DialogueText>> LocalizeAsync(IEnumerable<DialogueText> dialogueTexts, IEnumerable<LocalizedDialogueText> localizedDialogueTexts, string storySummary, CancellationToken cancellationToken = default);
        Task<IEnumerable<JsonObject>> LocalizeAsync(string masterDataFileName, IEnumerable<JsonObject> entities, IEnumerable<MasterDataLocalizationExample> localizationExamples, CancellationToken cancellationToken = default);
        Task<string> SummarizeAsync(IEnumerable<DialogueText> dialogueTexts, CancellationToken cancellationToken = default);
    }
}
