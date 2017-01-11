namespace QuickBooks.Models.DAL
{
    public class OAuth
    {
        public virtual string RealmId { get; set; }
        public virtual string AccessToken { get; set; }
        public virtual string AccessTokenSecret { get; set; }
    }
}