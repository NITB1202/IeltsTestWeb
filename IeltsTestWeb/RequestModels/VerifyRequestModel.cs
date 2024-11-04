namespace IeltsTestWeb.RequestModels
{
    public class VerifyRequestModel
    {
        public string Email { get; set; } =  null!;
        public string VerificationCode { get; set; } = null!;
    }
}
