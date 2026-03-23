using System.ComponentModel.DataAnnotations;

namespace Chat.Gateway.Options;

public sealed class ServiceUrlsOptions
{
    public const string SectionName = "ServiceUrls";

    [Required, Url]
    public string Messaging { get; init; } = default!;

    [Required, Url]
    public string Conversation { get; init; } = default!;

    [Required, Url]
    public string Users { get; init; } = default!;

    [Required, Url]
    public string Auth { get; init; } = default!;
}
