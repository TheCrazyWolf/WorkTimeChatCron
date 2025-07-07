namespace WorkTimeChat.Models;

public class WorkTimeConfig
{
    public long[] ChatId { get; set; } = null!;
    public string AccessToken { get; set; } = null!;
    public string ChatTurnOnMessage { get; set; } = null!;
    public string[] JobTurnOnParams { get; set; } = null!;
    public string ChatTurnOffMessage { get; set; } = null!;
    public string[] JobTurnOffParams { get; set; } = null!;
}