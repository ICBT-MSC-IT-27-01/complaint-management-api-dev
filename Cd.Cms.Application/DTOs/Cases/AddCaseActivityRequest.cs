namespace Cd.Cms.Application.DTOs.Cases
{
    public class AddCaseActivityRequest
    {
        public string ActivityType { get; set; } = "Note";
        public string Description { get; set; } = string.Empty;
        public string Visibility { get; set; } = "Private";
        public bool NotifyClient { get; set; }
    }
}
