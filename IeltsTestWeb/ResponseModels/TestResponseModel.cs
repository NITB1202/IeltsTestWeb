namespace IeltsTestWeb.ResponseModels
{
    public class TestResponseModel
    {
        public int TestId { get; set; }
        public string TestType { get; set; } = null!;

        public string TestSkill { get; set; } = null!;

        public string Name { get; set; } = null!;

        public int MonthEdition { get; set; }

        public int YearEdition { get; set; }

        public int? UserCompletedNum { get; set; }
    }
}
