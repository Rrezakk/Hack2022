namespace Hack2022.Models
{
    public class MapSubscriberEventRole
    {
        public string googleId { get; set; } = "";
        public string role { get; set; } = "";
        public MapSubscriberEventRole(){}

        public MapSubscriberEventRole(string googleId, string role)
        {
            this.googleId= googleId;
            this.role =role;
        }
    }
}
