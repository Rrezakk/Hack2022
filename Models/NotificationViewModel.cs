namespace Hack2022.Models
{
    public class NotificationViewModel
    {
        public string title { get; set; }
        public string description { get; set; }

        public NotificationViewModel(string title, string description)
        {
            this.description = description;
            this.title = title;
        }
    }
}
