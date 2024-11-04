namespace IeltsTestWeb.RequestModels
{
    public class AccountRequestModel
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public int RoleId { get; set; }
    }
}
