namespace Solution.Tests;

using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Solution.Application.Common.Interfaces;
using Solution.Application.DTOs;
using Solution.Domain.Enums;
using Solution.Infrastructure.Persistence;
using Solution.Web.Extensions;
using Xunit;

public class IafCoreTests
{
    private static ServiceProvider CreateTestServiceProvider()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["UseInMemoryDatabase"] = "true"
        }).Build();

        services.AddLogging(b => b.AddConsole().SetMinimumLevel(LogLevel.Warning));
        services.AddInfrastructureServices(config);
        services.AddSecurityAndIdentityServices();
        services.AddApplicationServices();

        var sp = services.BuildServiceProvider();

        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();

        return sp;
    }

    [Fact]
    public async Task MemberService_CreateAndRetrieve_Succeeds()
    {
        using var sp = CreateTestServiceProvider();
        using var scope = sp.CreateScope();
        var memberService = scope.ServiceProvider.GetRequiredService<IMemberService>();

        var dto = new CreateMemberDto
        {
            ServiceNo = "661234",
            Name = "Sgt Rajesh Kumar",
            Dob = new DateTime(1965, 5, 12),
            TradeId = "RA",
            StateId = "TN",
            MobileNo = "9876543210",
            TechnicalSkills = "Radar Systems",
            PlacesWorked = "Tambaram, Bareilly",
            Achievements = "Commendation by AOC-in-C",
            VrsReason = "Family personal reasons",
            AdditionalDetails = "Life member of AFRO"
        };

        bool created = await memberService.CreateMemberAsync(dto);
        Assert.True(created);

        var member = await memberService.GetMemberDetailsAsync("661234");
        Assert.NotNull(member);
        Assert.Equal("Sgt Rajesh Kumar", member.Name);
        Assert.Equal("Radar Fitters", member.TradeName);
        Assert.Equal("Tamil Nadu", member.StateName);

        // Add Family Member
        bool famAdded = await memberService.AddFamilyMemberAsync(new FamilyMemberDto
        {
            ServiceNo = "661234",
            MemberName = "Sunita Kumar",
            Relationship = "Wife",
            Gender = "Female",
            Occupation = "Teacher",
            Remarks = "Attending reunion"
        });
        Assert.True(famAdded);

        var memberWithFamily = await memberService.GetMemberDetailsAsync("661234");
        Assert.NotNull(memberWithFamily);
        Assert.Single(memberWithFamily.FamilyMembers);
        Assert.Equal("Sunita Kumar", memberWithFamily.FamilyMembers[0].MemberName);

        // Photo slot test
        byte[] dummyPhoto = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10];
        await memberService.SavePhotoAsync("661234", 1, dummyPhoto);

        var fetchedPhoto = await memberService.GetPhotoAsync("661234", 1);
        Assert.NotNull(fetchedPhoto);
        Assert.Equal(dummyPhoto.Length, fetchedPhoto.Length);
    }

    [Fact]
    public async Task ImportService_ValidAndDuplicate_HandledCorrectly()
    {
        using var sp = CreateTestServiceProvider();
        using var scope = sp.CreateScope();
        var importService = scope.ServiceProvider.GetRequiredService<IImportService>();

        string csvContent = "ServiceNo,Name,Trade,State,MobileNo\n" +
                            "771122,WO Arun Sharma,RA,TN,9876543211\n" +
                            "771123,JWO K Raman,EL,KL,9876543212\n";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
        var preview = await importService.ParseAndValidateFileAsync(stream, "members.csv");

        Assert.Equal(2, preview.TotalRecords);
        Assert.Equal(2, preview.ValidCount);
        Assert.Equal(0, preview.InvalidCount);

        var validRows = preview.Rows.Where(r => r.IsValid && !r.IsDuplicate).ToList();
        var result = await importService.CommitImportAsync(validRows);
        Assert.Equal(2, result.SuccessfullyImported);

        // Try importing the same service numbers again
        using var stream2 = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
        var preview2 = await importService.ParseAndValidateFileAsync(stream2, "members.csv");

        Assert.Equal(2, preview2.TotalRecords);
        Assert.Equal(0, preview2.ValidCount);
        Assert.Equal(2, preview2.DuplicateCount);
    }

    [Fact]
    public async Task ReunionRegistration_And_PaymentWorkflow_Succeeds()
    {
        using var sp = CreateTestServiceProvider();
        using var scope = sp.CreateScope();
        var memberService = scope.ServiceProvider.GetRequiredService<IMemberService>();
        var reunionService = scope.ServiceProvider.GetRequiredService<IReunionService>();
        var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();

        // 1. Create a member
        await memberService.CreateMemberAsync(new CreateMemberDto
        {
            ServiceNo = "881100",
            Name = "MWO Suresh Nair",
            TradeId = "EL",
            StateId = "KL",
            MobileNo = "9988776655"
        });

        // 2. Register for Reunion
        var regDto = new SubmitReunionRegistrationDto
        {
            EventId = 1,
            ServiceNo = "881100",
            NoOfPersons = 2,
            AccompDetails = "Wife: Meena Nair",
            DateOfArrival = DateTime.Today.AddMonths(3).AddHours(9),
            PlaceOfArrival = "Airport",
            Accommodation = "Officers Mess",
            DateOfDeparture = DateTime.Today.AddMonths(3).AddDays(2).AddHours(16),
            PlaceOfDeparture = "Central",
            TShirt = "XL",
            Drinker = 1,
            VegNonVeg = 0
        };

        var (regSuccess, regMsg) = await reunionService.RegisterOrUpdateAsync(regDto);
        Assert.True(regSuccess, regMsg);

        var reg = await reunionService.GetRegistrationAsync(1, "881100");
        Assert.NotNull(reg);
        Assert.Equal(2, reg.NoOfPersons);
        Assert.Equal("XL", reg.TShirt);
        Assert.Equal(RegistrationStatus.Submitted, reg.RegistrationStatus);

        // 3. Member submits payment confirmation (Yes, I have paid)
        var pmtDto = new SubmitPaymentDto
        {
            EventId = 1,
            ServiceNo = "881100",
            HasPaid = true,
            AmountPaid = 5000.00m,
            PaymentMode = PaymentMode.UPI,
            AcknowledgementNumber = "UPI9988112233",
            PaymentDate = DateTime.Today,
            AdditionalRemarks = "Paid via Google Pay"
        };

        var (pmtSuccess, pmtMsg, pmtId) = await paymentService.SubmitPaymentAsync(pmtDto);
        Assert.True(pmtSuccess, pmtMsg);
        Assert.True(pmtId > 0);

        // Crucial business rule: Submitting payment details does NOT automatically mark verified!
        var pmt = await paymentService.GetPaymentByIdAsync(pmtId);
        Assert.NotNull(pmt);
        Assert.Equal(PaymentStatus.PendingVerification, pmt.PaymentStatus);

        // 4. Admin verifies payment
        var (verSuccess, verMsg) = await paymentService.VerifyPaymentAsync(new VerifyPaymentDto
        {
            PaymentId = pmtId,
            IsApproved = true,
            VerifierName = "AdminUser"
        });
        Assert.True(verSuccess, verMsg);

        var verifiedPmt = await paymentService.GetPaymentByIdAsync(pmtId);
        Assert.NotNull(verifiedPmt);
        Assert.Equal(PaymentStatus.Verified, verifiedPmt.PaymentStatus);
        Assert.Equal("AdminUser", verifiedPmt.VerifiedBy);
        Assert.NotNull(verifiedPmt.VerifiedAtUtc);
    }

    [Fact]
    public async Task PaymentService_Rejection_RecordsReasonAndAllowsResubmission()
    {
        using var sp = CreateTestServiceProvider();
        using var scope = sp.CreateScope();
        var memberService = scope.ServiceProvider.GetRequiredService<IMemberService>();
        var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();

        await memberService.CreateMemberAsync(new CreateMemberDto
        {
            ServiceNo = "992211",
            Name = "Cpl Deepu Varghese",
            TradeId = "RA",
            StateId = "KL"
        });

        var (pmtSuccess, _, pmtId) = await paymentService.SubmitPaymentAsync(new SubmitPaymentDto
        {
            EventId = 1,
            ServiceNo = "992211",
            HasPaid = true,
            AmountPaid = 5000m,
            PaymentMode = PaymentMode.BankTransfer,
            AcknowledgementNumber = "NEFT-INCORRECT",
            PaymentDate = DateTime.Today
        });
        Assert.True(pmtSuccess);

        // Admin rejects payment
        var (rejSuccess, rejMsg) = await paymentService.VerifyPaymentAsync(new VerifyPaymentDto
        {
            PaymentId = pmtId,
            IsApproved = false,
            RejectionReason = "Reference not found in bank ledger.",
            VerifierName = "Treasurer"
        });
        Assert.True(rejSuccess, rejMsg);

        var rejectedPmt = await paymentService.GetPaymentByIdAsync(pmtId);
        Assert.NotNull(rejectedPmt);
        Assert.Equal(PaymentStatus.Rejected, rejectedPmt.PaymentStatus);
        Assert.Equal("Reference not found in bank ledger.", rejectedPmt.RejectionReason);

        // Member resubmits with corrected transaction number
        var (resubmitSuccess, _, resubmitId) = await paymentService.SubmitPaymentAsync(new SubmitPaymentDto
        {
            EventId = 1,
            ServiceNo = "992211",
            HasPaid = true,
            AmountPaid = 5000m,
            PaymentMode = PaymentMode.BankTransfer,
            AcknowledgementNumber = "NEFT-CORRECT-123",
            PaymentDate = DateTime.Today
        });
        Assert.True(resubmitSuccess);

        var resubmittedPmt = await paymentService.GetPaymentByIdAsync(resubmitId);
        Assert.NotNull(resubmittedPmt);
        Assert.Equal(PaymentStatus.PendingVerification, resubmittedPmt.PaymentStatus);
        Assert.Equal("NEFT-CORRECT-123", resubmittedPmt.AcknowledgementNumber);
    }

    [Fact]
    public async Task DashboardAndReports_All14Reports_GenerateSuccessfully()
    {
        using var sp = CreateTestServiceProvider();
        using var scope = sp.CreateScope();
        var memberService = scope.ServiceProvider.GetRequiredService<IMemberService>();
        var reunionService = scope.ServiceProvider.GetRequiredService<IReunionService>();
        var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
        var dashboardService = scope.ServiceProvider.GetRequiredService<IDashboardService>();
        var reportService = scope.ServiceProvider.GetRequiredService<IReportService>();

        // Populate sample data
        await memberService.CreateMemberAsync(new CreateMemberDto
        {
            ServiceNo = "111222",
            Name = "MWO Sanjeev Sharma",
            TradeId = "RA",
            StateId = "TN",
            MobileNo = "9112233445"
        });

        await reunionService.RegisterOrUpdateAsync(new SubmitReunionRegistrationDto
        {
            EventId = 1,
            ServiceNo = "111222",
            NoOfPersons = 2,
            AccompDetails = "Spouse",
            TShirt = "XXL",
            VegNonVeg = 0,
            Drinker = 0,
            PlaceOfArrival = "Airport",
            DateOfArrival = DateTime.Today.AddMonths(3)
        });

        var (_, _, pmtId) = await paymentService.SubmitPaymentAsync(new SubmitPaymentDto
        {
            EventId = 1,
            ServiceNo = "111222",
            HasPaid = true,
            AmountPaid = 5000m,
            PaymentMode = PaymentMode.UPI,
            AcknowledgementNumber = "UPI-REP-111"
        });

        await paymentService.VerifyPaymentAsync(new VerifyPaymentDto
        {
            PaymentId = pmtId,
            IsApproved = true,
            VerifierName = "Admin"
        });

        // Test Admin Dashboard stats
        var stats = await dashboardService.GetAdminStatsAsync(1);
        Assert.True(stats.TotalMembersInRegistry >= 1);
        Assert.True(stats.TotalExpectedAttendees >= 2);
        Assert.True(stats.TotalVerifiedAmountCollected >= 5000m);
        Assert.Equal(1, stats.TShirtSizeDistribution["XXL"]);

        // Test All 14 Reports in both Excel and CSV formats
        for (int reportNo = 1; reportNo <= 14; reportNo++)
        {
            byte[] excelBytes = await reportService.GenerateReportExcelAsync(reportNo, 1);
            Assert.NotNull(excelBytes);
            Assert.True(excelBytes.Length > 0, $"Report {reportNo} Excel generated empty bytes");
            // Check Excel zip magic header: 'P' 'K' (0x50, 0x4B)
            Assert.Equal(0x50, excelBytes[0]);
            Assert.Equal(0x4B, excelBytes[1]);

            byte[] csvBytes = await reportService.GenerateReportCsvAsync(reportNo, 1);
            Assert.NotNull(csvBytes);
            Assert.True(csvBytes.Length > 0, $"Report {reportNo} CSV generated empty bytes");
            string csvText = Encoding.UTF8.GetString(csvBytes);
            Assert.False(string.IsNullOrWhiteSpace(csvText), $"Report {reportNo} CSV generated blank text");
        }
    }
}
