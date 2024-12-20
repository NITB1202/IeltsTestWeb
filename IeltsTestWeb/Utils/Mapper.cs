using IeltsTestWeb.Models;
using IeltsTestWeb.ResponseModels;

namespace IeltsTestWeb.Utils
{
    public class Mapper
    {
        public static AccountResponseModel AccountToResponseModel(Account account)
        {
            return new AccountResponseModel
            {
                AccountId = account.AccountId,
                Email = account.Email,
                RoleId = account.RoleId,
                AvatarLink = account.AvatarLink,
                Goal = account.Goal,
                IsActive = account.IsActive
            };
        }
        public static ExplanationResponseModel ExplanationToResponseModel(Explanation model)
        {
            return new ExplanationResponseModel
            {
                ExId = model.ExId,
                Content = model.Content,
                QuestionId = model.QuestionId
            };
        }
        public static QuestionResponseModel QuestionToResponseModel(Question model)
        {
            return new QuestionResponseModel
            {
                QuestionId = model.QuestionId,
                QlistId = model.QlistId,
                Content = model.Content,
                ChoiceList = model.ChoiceList,
                Answer = model.Answer
            };
        }
        public static QuestionListResponseModel QuestionListToResponseModel(QuestionList model)
        {
            return new QuestionListResponseModel
            {
                Id = model.QlistId,
                Type = model.QlistType,
                Content = model.Content,
                Qnum = model.Qnum
            };
        }
        public static ResultResponseModel ResultToResponseModel(Result model)
        {
            return new ResultResponseModel
            {
                ResultId = model.ResultId,
                AccountId = model.AccountId,
                TestId = model.TestId,
                TestAccess = model.TestAccess,
                DateMake = model.DateMake,
                CompleteTime = model.CompleteTime,
                Score = model.Score
            };
        }
        public static ResultDetailResponseModel ResultDetailToResponseModel(ResultDetail model)
        {
            return new ResultDetailResponseModel
            {
                DetailId = model.DetailId,
                ResultId = model.ResultId,
                QuestionOrder = model.QuestionOrder,
                QuestionId = model.QuestionId,
                UserAnswer = model.UserAnswer,
                QuestionState = model.QuestionState
            };
        }
        public static TestResponseModel TestToResponseModel(Test model)
        {
            return new TestResponseModel
            {
                TestId = model.TestId,
                TestType = model.TestType,
                TestSkill = model.TestSkill,
                Name = model.Name,
                MonthEdition = model.MonthEdition,
                YearEdition = model.YearEdition,
                UserCompletedNum = model.UserCompletedNum
            };
        }
        public static UserTestResponseModel UserTestToResponseModel(UserTest model)
        {
            return new UserTestResponseModel
            {
                Id = model.UtestId,
                AccountId = model.AccountId,
                Name = model.Name,
                DateCreate = model.DateCreate,
                TestType = model.TestType,
                TestSkill = model.TestSkill
            };
        }
        public static DetailResponseModel DetailToResponseModel(UserTestDetail model)
        {
            return new DetailResponseModel
            {
                Id = model.TdetailId,
                TestId = model.UtestId,
                SectionId = model.SectionId
            };
        }

        public static ReadingSectionResponseModel ReadingSectionToResponseModel(ReadingSection model)
        {
            return new ReadingSectionResponseModel
            {
                Id = model.RsectionId,
                ImageLink = model.ImageLink,
                Title = model.Title,
                Content = model.Content,
                TestId = model.TestId
            };
        }
        public static ListeningSectionResponseModel ListeningSectionToResponseModel(ListeningSection model)
        {
            return new ListeningSectionResponseModel
            {
                Id = model.LsectionId,
                SectionOrder = model.SectionOrder,
                TimeStamp = model.TimeStamp,
                Transcript = model.Transcript,
                SoundId = model.SoundId
            };
        }
    }
}
