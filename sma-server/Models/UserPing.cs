namespace SMAServer.Models
{
    public class UserPing
    {
        public string ID { get; set; }

        public string Branch { get; set; }

        public string Status { get; set; }

        public DateTime Pinged { get; set; }

        public DateTime Created { get; set; }

        public int PingsCount { get; set; }
    }
}
