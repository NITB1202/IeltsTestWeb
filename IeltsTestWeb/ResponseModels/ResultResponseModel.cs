namespace IeltsTestWeb.ResponseModels
{
    public class ResultResponseModel
    {
        public int ResultId { get; set; }

        public int? Score { get; set; }

        public int AccountId { get; set; }

        public int TestId { get; set; }

        public string TestAccess { get; set; } = null!;

        public DateTime? DateMake { get; set; }

        public TimeOnly? CompleteTime { get; set; }
    }
}
