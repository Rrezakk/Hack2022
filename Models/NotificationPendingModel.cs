namespace Hack2022.Models
{
    public class NotificationPendingModel
    {
        public NotificationPendingModel(NotificationModel model)
        {
            utcTimeTarget = new DateTimeOffset(model.targetTime).ToUnixTimeSeconds();
            utcTimeWithOffset = new DateTimeOffset(model.targetTime).ToUnixTimeSeconds() - new DateTimeOffset((model.preDelay)).ToUnixTimeSeconds();
            Id = model.Id;
            Tittle = "event triggered ";
            Description = model.Text;
            //var leftSet = new DateTimeOffset(v.).ToUnixTimeSeconds();
            //var rightSet = new DateTimeOffset(v.preDelay).ToUnixTimeSeconds();
        }
        public string Id { get; set; }
        public string Tittle { get; set; }
        public string Description { get; set; }
        public long utcTimeTarget { get; set; }
        public long utcTimeWithOffset { get; set; }
        //public DateTime 
        public NotificationViewModel ToViewModel()
        {
            return null; //new NotificationViewModel();
        }
    }
}
