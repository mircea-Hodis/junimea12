namespace DataModelLayer.ViewModels.UserManagement
{
    public class UserBanViewModel
    {
        public string BannedUserId { get; set; }
        public int BanWeeksDuration { get; set; }
        public long? FacebookId { get; set; }
        public string BannedEmail { get; set; }
        public bool IsPermanentBan { get; set; }
    }

    public class GetUserBanRequest
    {
        public string UserId { get; set; }
    }

    public class UnbanUserViewModel
    {
        public string UserId { get; set; }
    }

}
