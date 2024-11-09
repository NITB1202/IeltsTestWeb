namespace IeltsTestWeb.ResponseModels
{
    public class AccountResponseModel
    {
        public int AccountId { get; set; }
        public string Email { get; set; } = null!;
        public int RoleId { get; set; }
        public string? AvatarLink { get; set; }
        public decimal? Goal { get; set; }
        public Boolean IsActive { get; set; }
    }
}
