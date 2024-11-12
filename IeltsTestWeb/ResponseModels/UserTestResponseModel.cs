namespace IeltsTestWeb.ResponseModels
{
    public class UserTestResponseModel
    {
        public int Id { get; set; }

        public int AccountId { get; set; }

        public string Name { get; set; } = null!;

        public DateTime? DateCreate { get; set; }

        public string TestType { get; set; } = null!;

        public string TestSkill { get; set; } = null!;
    }
}
