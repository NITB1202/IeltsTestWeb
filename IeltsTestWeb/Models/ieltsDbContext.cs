using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace IeltsTestWeb.Models;

public partial class ieltsDbContext : DbContext
{
    public ieltsDbContext()
    {
    }

    public ieltsDbContext(DbContextOptions<ieltsDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Constant> Constants { get; set; }

    public virtual DbSet<DiagramQuestionList> DiagramQuestionLists { get; set; }

    public virtual DbSet<Explanation> Explanations { get; set; }

    public virtual DbSet<ListeningSection> ListeningSections { get; set; }

    public virtual DbSet<MatchQuestionList> MatchQuestionLists { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<QuestionList> QuestionLists { get; set; }

    public virtual DbSet<ReadingSection> ReadingSections { get; set; }

    public virtual DbSet<Result> Results { get; set; }

    public virtual DbSet<ResultDetail> ResultDetails { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Sound> Sounds { get; set; }

    public virtual DbSet<Test> Tests { get; set; }

    public virtual DbSet<UserTest> UserTests { get; set; }

    public virtual DbSet<UserTestDetail> UserTestDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PRIMARY");

            entity.ToTable("Account");

            entity.HasIndex(e => e.Email, "email").IsUnique();

            entity.HasIndex(e => e.RoleId, "role_id");

            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.AvatarLink)
                .HasMaxLength(255)
                .HasColumnName("avatar_link");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Goal)
                .HasPrecision(2, 1)
                .HasDefaultValueSql("'5.0'")
                .HasColumnName("goal");
            entity.Property(e => e.IsActive)
                .HasDefaultValueSql("'1'")
                .HasColumnName("isActive");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.RoleId)
                .HasDefaultValueSql("'1'")
                .HasColumnName("role_id");

            entity.HasOne(d => d.Role).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("Account_ibfk_1");
        });

        modelBuilder.Entity<Constant>(entity =>
        {
            entity.HasKey(e => e.Name).HasName("PRIMARY");

            entity.ToTable("Constant");

            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Value)
                .HasPrecision(10, 2)
                .HasColumnName("value");
        });

        modelBuilder.Entity<DiagramQuestionList>(entity =>
        {
            entity.HasKey(e => e.DqlistId).HasName("PRIMARY");

            entity.ToTable("DiagramQuestionList");

            entity.HasIndex(e => e.QlistId, "qlist_id");

            entity.Property(e => e.DqlistId).HasColumnName("dqlist_id");
            entity.Property(e => e.ImageLink)
                .HasMaxLength(255)
                .HasColumnName("image_link");
            entity.Property(e => e.QlistId).HasColumnName("qlist_id");

            entity.HasOne(d => d.Qlist).WithMany(p => p.DiagramQuestionLists)
                .HasForeignKey(d => d.QlistId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("DiagramQuestionList_ibfk_1");
        });

        modelBuilder.Entity<Explanation>(entity =>
        {
            entity.HasKey(e => e.ExId).HasName("PRIMARY");

            entity.ToTable("Explanation");

            entity.HasIndex(e => e.QuestionId, "question_id");

            entity.Property(e => e.ExId).HasColumnName("ex_id");
            entity.Property(e => e.Content)
                .HasMaxLength(1000)
                .HasColumnName("content");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");

            entity.HasOne(d => d.Question).WithMany(p => p.Explanations)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Explanation_ibfk_1");
        });

        modelBuilder.Entity<ListeningSection>(entity =>
        {
            entity.HasKey(e => e.LsectionId).HasName("PRIMARY");

            entity.ToTable("ListeningSection");

            entity.HasIndex(e => e.SoundId, "sound_id");

            entity.Property(e => e.LsectionId).HasColumnName("lsection_id");
            entity.Property(e => e.SectionOrder).HasColumnName("section_order");
            entity.Property(e => e.SoundId).HasColumnName("sound_id");
            entity.Property(e => e.TimeStamp)
                .HasColumnType("time")
                .HasColumnName("time_stamp");
            entity.Property(e => e.Transcript)
                .HasMaxLength(10000)
                .HasColumnName("transcript");

            entity.HasOne(d => d.Sound).WithMany(p => p.ListeningSections)
                .HasForeignKey(d => d.SoundId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ListeningSection_ibfk_1");

            entity.HasMany(d => d.Qlists).WithMany(p => p.Lsections)
                .UsingEntity<Dictionary<string, object>>(
                    "ListeningSectionQuestionList",
                    r => r.HasOne<QuestionList>().WithMany()
                        .HasForeignKey("QlistId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("ListeningSection_QuestionList_ibfk_2"),
                    l => l.HasOne<ListeningSection>().WithMany()
                        .HasForeignKey("LsectionId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("ListeningSection_QuestionList_ibfk_1"),
                    j =>
                    {
                        j.HasKey("LsectionId", "QlistId")
                            .HasName("PRIMARY")
                            .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                        j.ToTable("ListeningSection_QuestionList");
                        j.HasIndex(new[] { "QlistId" }, "qlist_id");
                        j.IndexerProperty<int>("LsectionId").HasColumnName("lsection_id");
                        j.IndexerProperty<int>("QlistId").HasColumnName("qlist_id");
                    });
        });

        modelBuilder.Entity<MatchQuestionList>(entity =>
        {
            entity.HasKey(e => e.MqlistId).HasName("PRIMARY");

            entity.ToTable("MatchQuestionList");

            entity.HasIndex(e => e.QlistId, "qlist_id");

            entity.Property(e => e.MqlistId).HasColumnName("mqlist_id");
            entity.Property(e => e.ChoiceList)
                .HasMaxLength(255)
                .HasColumnName("choice_list");
            entity.Property(e => e.QlistId).HasColumnName("qlist_id");

            entity.HasOne(d => d.Qlist).WithMany(p => p.MatchQuestionLists)
                .HasForeignKey(d => d.QlistId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("MatchQuestionList_ibfk_1");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PRIMARY");

            entity.ToTable("Question");

            entity.Property(e => e.QuestionId).HasColumnName("question_id");
            entity.Property(e => e.Answer)
                .HasMaxLength(1000)
                .HasColumnName("answer");
            entity.Property(e => e.ChoiceList)
                .HasMaxLength(1000)
                .HasColumnName("choice_list");
            entity.Property(e => e.Content)
                .HasMaxLength(1000)
                .HasColumnName("content");
            entity.Property(e => e.QlistId).HasColumnName("qlist_id");
        });

        modelBuilder.Entity<QuestionList>(entity =>
        {
            entity.HasKey(e => e.QlistId).HasName("PRIMARY");

            entity.ToTable("QuestionList");

            entity.Property(e => e.QlistId).HasColumnName("qlist_id");
            entity.Property(e => e.Content)
                .HasMaxLength(1000)
                .HasColumnName("content");
            entity.Property(e => e.QlistType)
                .HasColumnType("enum('multiple_choice','matching','true_false','complete','diagram')")
                .HasColumnName("qlist_type");
            entity.Property(e => e.Qnum).HasColumnName("qnum");
        });

        modelBuilder.Entity<ReadingSection>(entity =>
        {
            entity.HasKey(e => e.RsectionId).HasName("PRIMARY");

            entity.ToTable("ReadingSection");

            entity.HasIndex(e => e.TestId, "test_id");

            entity.Property(e => e.RsectionId).HasColumnName("rsection_id");
            entity.Property(e => e.Content)
                .HasMaxLength(10000)
                .HasColumnName("content");
            entity.Property(e => e.ImageLink)
                .HasMaxLength(255)
                .HasColumnName("image_link");
            entity.Property(e => e.TestId).HasColumnName("test_id");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");

            entity.HasOne(d => d.Test).WithMany(p => p.ReadingSections)
                .HasForeignKey(d => d.TestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ReadingSection_ibfk_1");

            entity.HasMany(d => d.Qlists).WithMany(p => p.Rsections)
                .UsingEntity<Dictionary<string, object>>(
                    "ReadingSectionQuestionList",
                    r => r.HasOne<QuestionList>().WithMany()
                        .HasForeignKey("QlistId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("ReadingSection_QuestionList_ibfk_2"),
                    l => l.HasOne<ReadingSection>().WithMany()
                        .HasForeignKey("RsectionId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("ReadingSection_QuestionList_ibfk_1"),
                    j =>
                    {
                        j.HasKey("RsectionId", "QlistId")
                            .HasName("PRIMARY")
                            .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                        j.ToTable("ReadingSection_QuestionList");
                        j.HasIndex(new[] { "QlistId" }, "qlist_id");
                        j.IndexerProperty<int>("RsectionId").HasColumnName("rsection_id");
                        j.IndexerProperty<int>("QlistId").HasColumnName("qlist_id");
                    });
        });

        modelBuilder.Entity<Result>(entity =>
        {
            entity.HasKey(e => e.ResultId).HasName("PRIMARY");

            entity.ToTable("Result");

            entity.HasIndex(e => e.AccountId, "account_id");

            entity.Property(e => e.ResultId).HasColumnName("result_id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.CompleteTime)
                .HasColumnType("time")
                .HasColumnName("complete_time");
            entity.Property(e => e.DateMake)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("date_make");
            entity.Property(e => e.Score)
                .HasDefaultValueSql("'0'")
                .HasColumnName("score");
            entity.Property(e => e.TestAccess)
                .HasColumnType("enum('public','private')")
                .HasColumnName("test_access");
            entity.Property(e => e.TestId).HasColumnName("test_id");

            entity.HasOne(d => d.Account).WithMany(p => p.Results)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Result_ibfk_1");
        });

        modelBuilder.Entity<ResultDetail>(entity =>
        {
            entity.HasKey(e => e.DetailId).HasName("PRIMARY");

            entity.HasIndex(e => e.QuestionId, "question_id");

            entity.HasIndex(e => e.ResultId, "result_id");

            entity.Property(e => e.DetailId).HasColumnName("detail_id");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");
            entity.Property(e => e.QuestionOrder).HasColumnName("question_order");
            entity.Property(e => e.QuestionState)
                .HasColumnType("enum('right','wrong')")
                .HasColumnName("question_state");
            entity.Property(e => e.ResultId).HasColumnName("result_id");
            entity.Property(e => e.UserAnswer)
                .HasMaxLength(1000)
                .HasColumnName("user_answer");

            entity.HasOne(d => d.Question).WithMany(p => p.ResultDetails)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ResultDetails_ibfk_2");

            entity.HasOne(d => d.Result).WithMany(p => p.ResultDetails)
                .HasForeignKey(d => d.ResultId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ResultDetails_ibfk_1");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PRIMARY");

            entity.ToTable("Role");

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Sound>(entity =>
        {
            entity.HasKey(e => e.SoundId).HasName("PRIMARY");

            entity.ToTable("Sound");

            entity.HasIndex(e => e.TestId, "test_id");

            entity.Property(e => e.SoundId).HasColumnName("sound_id");
            entity.Property(e => e.SoundLink)
                .HasMaxLength(255)
                .HasColumnName("sound_link");
            entity.Property(e => e.TestId).HasColumnName("test_id");

            entity.HasOne(d => d.Test).WithMany(p => p.Sounds)
                .HasForeignKey(d => d.TestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Sound_ibfk_1");
        });

        modelBuilder.Entity<Test>(entity =>
        {
            entity.HasKey(e => e.TestId).HasName("PRIMARY");

            entity.ToTable("Test");

            entity.Property(e => e.TestId).HasColumnName("test_id");
            entity.Property(e => e.MonthEdition).HasColumnName("month_edition");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.TestSkill)
                .HasColumnType("enum('reading','listening')")
                .HasColumnName("test_skill");
            entity.Property(e => e.TestType)
                .HasColumnType("enum('general','academic')")
                .HasColumnName("test_type");
            entity.Property(e => e.UserCompletedNum)
                .HasDefaultValueSql("'0'")
                .HasColumnName("user_completed_num");
            entity.Property(e => e.YearEdition).HasColumnName("year_edition");
        });

        modelBuilder.Entity<UserTest>(entity =>
        {
            entity.HasKey(e => e.UtestId).HasName("PRIMARY");

            entity.ToTable("UserTest");

            entity.HasIndex(e => e.AccountId, "account_id");

            entity.Property(e => e.UtestId).HasColumnName("utest_id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.DateCreate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("date_create");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.TestSkill)
                .HasColumnType("enum('listening','reading')")
                .HasColumnName("test_skill");
            entity.Property(e => e.TestType)
                .HasColumnType("enum('general','academic')")
                .HasColumnName("test_type");

            entity.HasOne(d => d.Account).WithMany(p => p.UserTests)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("UserTest_ibfk_1");
        });

        modelBuilder.Entity<UserTestDetail>(entity =>
        {
            entity.HasKey(e => e.TdetailId).HasName("PRIMARY");

            entity.HasIndex(e => e.UtestId, "utest_id");

            entity.Property(e => e.TdetailId).HasColumnName("tdetail_id");
            entity.Property(e => e.SectionId).HasColumnName("section_id");
            entity.Property(e => e.UtestId).HasColumnName("utest_id");

            entity.HasOne(d => d.Utest).WithMany(p => p.UserTestDetails)
                .HasForeignKey(d => d.UtestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("UserTestDetails_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
