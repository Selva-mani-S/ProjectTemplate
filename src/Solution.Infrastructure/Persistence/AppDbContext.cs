namespace Solution.Infrastructure.Persistence;

using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Solution.Domain.Entities;
using Solution.Domain.Enums;

public class AppDbContext : DbContext, IDataProtectionKeyContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();
    public DbSet<ApplicationAuditLog> AuditLogs => Set<ApplicationAuditLog>();
    public DbSet<SampleDomainEntity> SampleEntities => Set<SampleDomainEntity>();

    public DbSet<Trade> Trades => Set<Trade>();
    public DbSet<State> States => Set<State>();
    public DbSet<IafPersonalData> PersonalData => Set<IafPersonalData>();
    public DbSet<IafFamilyData> FamilyData => Set<IafFamilyData>();
    public DbSet<IafAdditionalData> AdditionalData => Set<IafAdditionalData>();
    public DbSet<IafReunionEvent> ReunionEvents => Set<IafReunionEvent>();
    public DbSet<IafReunion> Reunions => Set<IafReunion>();
    public DbSet<IafPayment> Payments => Set<IafPayment>();
    public DbSet<IafAnnouncement> Announcements => Set<IafAnnouncement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("app");

        // Audit Log
        modelBuilder.Entity<ApplicationAuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UserId).HasMaxLength(100);
            entity.Property(e => e.Username).HasMaxLength(256);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.ActionDetails).HasMaxLength(2000);
        });

        // Trade Master
        modelBuilder.Entity<Trade>(entity =>
        {
            entity.ToTable("Trade");
            entity.HasKey(e => e.TradeId);
            entity.Property(e => e.TradeId).HasMaxLength(2).IsUnicode(false);
            entity.Property(e => e.TradeName).HasMaxLength(100).IsRequired();
        });

        // State Master
        modelBuilder.Entity<State>(entity =>
        {
            entity.ToTable("State");
            entity.HasKey(e => e.StateId);
            entity.Property(e => e.StateId).HasMaxLength(2).IsUnicode(false);
            entity.Property(e => e.StateName).HasMaxLength(100).IsRequired();
        });

        // IAF_PersonalData
        modelBuilder.Entity<IafPersonalData>(entity =>
        {
            entity.ToTable("IAF_PersonalData");
            entity.HasKey(e => e.ServiceNo);
            entity.Property(e => e.ServiceNo).HasMaxLength(6).IsUnicode(false);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.TradeId).HasMaxLength(2).IsUnicode(false);
            entity.Property(e => e.Address1).HasMaxLength(50);
            entity.Property(e => e.Address2).HasMaxLength(50);
            entity.Property(e => e.StateId).HasMaxLength(2).IsUnicode(false);
            entity.Property(e => e.MobileNo).HasMaxLength(13);
            entity.Property(e => e.EmailId).HasMaxLength(150);
            entity.Property(e => e.TechnicalSkills).HasMaxLength(250);
            entity.Property(e => e.PlacesWorked).HasMaxLength(250);
            entity.Property(e => e.AdditionalDetails).HasMaxLength(250);
            entity.Property(e => e.Achievements).HasMaxLength(200);
            entity.Property(e => e.VrsReason).HasMaxLength(200);

            entity.HasOne(e => e.Trade)
                .WithMany(t => t.Members)
                .HasForeignKey(e => e.TradeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.State)
                .WithMany(s => s.Members)
                .HasForeignKey(e => e.StateId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // IAF_FamilyData
        modelBuilder.Entity<IafFamilyData>(entity =>
        {
            entity.ToTable("IAF_FamilyData");
            entity.HasKey(e => e.FamilyMemberId);
            entity.Property(e => e.FamilyMemberId).ValueGeneratedOnAdd();
            entity.Property(e => e.ServiceNo).HasMaxLength(6).IsUnicode(false).IsRequired();
            entity.Property(e => e.MemberName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Relationship).HasMaxLength(30);
            entity.Property(e => e.Gender).HasMaxLength(1).IsUnicode(false);
            entity.Property(e => e.Occupation).HasMaxLength(100);
            entity.Property(e => e.Remarks).HasMaxLength(200);

            entity.HasOne(e => e.PersonalData)
                .WithMany(p => p.FamilyMembers)
                .HasForeignKey(e => e.ServiceNo)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // IAF_AdditionalData
        modelBuilder.Entity<IafAdditionalData>(entity =>
        {
            entity.ToTable("IAF_AdditionalData");
            entity.HasKey(e => e.ServiceNo);
            entity.Property(e => e.ServiceNo).HasMaxLength(6).IsUnicode(false);

            entity.HasOne(e => e.PersonalData)
                .WithOne(p => p.AdditionalData)
                .HasForeignKey<IafAdditionalData>(e => e.ServiceNo)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // IAF_ReunionEvent
        modelBuilder.Entity<IafReunionEvent>(entity =>
        {
            entity.ToTable("IAF_ReunionEvent");
            entity.HasKey(e => e.EventId);
            entity.Property(e => e.EventId).ValueGeneratedOnAdd();
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Batch).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Venue).HasMaxLength(250).IsRequired();
            entity.Property(e => e.BaseFeeAmount).HasPrecision(18, 2);
            entity.Property(e => e.Description).HasMaxLength(2000);
        });

        // IAF_Reunion
        modelBuilder.Entity<IafReunion>(entity =>
        {
            entity.ToTable("IAF_Reunion");
            entity.HasKey(e => e.ReunionId);
            entity.Property(e => e.ReunionId).ValueGeneratedOnAdd();
            entity.Property(e => e.ServiceNo).HasMaxLength(6).IsUnicode(false).IsRequired();
            entity.Property(e => e.AccompDetails).HasMaxLength(100);
            entity.Property(e => e.PlaceOfArrival).HasMaxLength(20);
            entity.Property(e => e.Accommodation).HasMaxLength(100);
            entity.Property(e => e.PlaceOfDeparture).HasMaxLength(50);
            entity.Property(e => e.TShirt).HasMaxLength(10);
            entity.Property(e => e.RegistrationStatus).HasConversion<string>().HasMaxLength(30);

            entity.HasOne(e => e.Event)
                .WithMany(ev => ev.Registrations)
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.PersonalData)
                .WithMany(p => p.Reunions)
                .HasForeignKey(e => e.ServiceNo)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.EventId, e.ServiceNo }).IsUnique();
        });

        // IAF_Payment
        modelBuilder.Entity<IafPayment>(entity =>
        {
            entity.ToTable("IAF_Payment");
            entity.HasKey(e => e.PaymentId);
            entity.Property(e => e.PaymentId).ValueGeneratedOnAdd();
            entity.Property(e => e.ServiceNo).HasMaxLength(6).IsUnicode(false).IsRequired();
            entity.Property(e => e.AmountPaid).HasPrecision(18, 2);
            entity.Property(e => e.PaymentMode).HasConversion<string>().HasMaxLength(30);
            entity.Property(e => e.AcknowledgementNumber).HasMaxLength(100);
            entity.Property(e => e.PaymentStatus).HasConversion<string>().HasMaxLength(30);
            entity.Property(e => e.PaymentProofPath).HasMaxLength(500);
            entity.Property(e => e.AdditionalRemarks).HasMaxLength(500);
            entity.Property(e => e.VerifiedBy).HasMaxLength(256);
            entity.Property(e => e.RejectionReason).HasMaxLength(500);

            entity.HasOne(e => e.Event)
                .WithMany(ev => ev.Payments)
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.PersonalData)
                .WithMany(p => p.Payments)
                .HasForeignKey(e => e.ServiceNo)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.AcknowledgementNumber);
            entity.HasIndex(e => e.PaymentStatus);
        });

        // IAF_Announcement
        modelBuilder.Entity<IafAnnouncement>(entity =>
        {
            entity.ToTable("IAF_Announcement");
            entity.HasKey(e => e.AnnouncementId);
            entity.Property(e => e.AnnouncementId).ValueGeneratedOnAdd();
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.AuthorName).HasMaxLength(100);

            entity.HasOne(e => e.Event)
                .WithMany()
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Master Data Seeding
        SeedInitialMasterData(modelBuilder);
    }

    private static void SeedInitialMasterData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Trade>().HasData(
            new Trade { TradeId = "AD", TradeName = "Administrative" },
            new Trade { TradeId = "AC", TradeName = "Accounts" },
            new Trade { TradeId = "CO", TradeName = "Communications / Signals" },
            new Trade { TradeId = "ED", TradeName = "Education" },
            new Trade { TradeId = "EL", TradeName = "Electrical Fitters" },
            new Trade { TradeId = "EN", TradeName = "Engine Fitters" },
            new Trade { TradeId = "LO", TradeName = "Logistics" },
            new Trade { TradeId = "ME", TradeName = "Mechanical Transport" },
            new Trade { TradeId = "MD", TradeName = "Medical" },
            new Trade { TradeId = "RA", TradeName = "Radar Fitters" },
            new Trade { TradeId = "SE", TradeName = "Security" },
            new Trade { TradeId = "WF", TradeName = "Weapon Fitters" }
        );

        modelBuilder.Entity<State>().HasData(
            new State { StateId = "AP", StateName = "Andhra Pradesh" },
            new State { StateId = "AR", StateName = "Arunachal Pradesh" },
            new State { StateId = "AS", StateName = "Assam" },
            new State { StateId = "BR", StateName = "Bihar" },
            new State { StateId = "CG", StateName = "Chhattisgarh" },
            new State { StateId = "DL", StateName = "Delhi" },
            new State { StateId = "GA", StateName = "Goa" },
            new State { StateId = "GJ", StateName = "Gujarat" },
            new State { StateId = "HR", StateName = "Haryana" },
            new State { StateId = "HP", StateName = "Himachal Pradesh" },
            new State { StateId = "JH", StateName = "Jharkhand" },
            new State { StateId = "KA", StateName = "Karnataka" },
            new State { StateId = "KL", StateName = "Kerala" },
            new State { StateId = "MP", StateName = "Madhya Pradesh" },
            new State { StateId = "MH", StateName = "Maharashtra" },
            new State { StateId = "MN", StateName = "Manipur" },
            new State { StateId = "ML", StateName = "Meghalaya" },
            new State { StateId = "MZ", StateName = "Mizoram" },
            new State { StateId = "NL", StateName = "Nagaland" },
            new State { StateId = "OD", StateName = "Odisha" },
            new State { StateId = "PB", StateName = "Punjab" },
            new State { StateId = "RJ", StateName = "Rajasthan" },
            new State { StateId = "SK", StateName = "Sikkim" },
            new State { StateId = "TN", StateName = "Tamil Nadu" },
            new State { StateId = "TS", StateName = "Telangana" },
            new State { StateId = "TR", StateName = "Tripura" },
            new State { StateId = "UP", StateName = "Uttar Pradesh" },
            new State { StateId = "UK", StateName = "Uttarakhand" },
            new State { StateId = "WB", StateName = "West Bengal" }
        );

        modelBuilder.Entity<IafReunionEvent>().HasData(
            new IafReunionEvent
            {
                EventId = 1,
                Title = "Indian Air Force Batch 1986 Grand Reunion",
                Batch = "1986",
                EventDate = new DateTime(2027, 2, 20),
                EventEndDate = new DateTime(2027, 2, 22),
                Venue = "Air Force Officers Mess & Convention Centre, Chennai",
                RegistrationDeadline = new DateTime(2027, 1, 31),
                BaseFeeAmount = 5000.00m,
                IsActive = true,
                Description = "Grand 40-Year Reunion celebrating the Indian Air Force Batch of 1986 veterans, spouses, and families.",
                CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        modelBuilder.Entity<IafAnnouncement>().HasData(
            new IafAnnouncement
            {
                AnnouncementId = 1,
                EventId = 1,
                Title = "Welcome Batch 1986 Veterans! Registration is Open",
                Content = "Welcome to the official registry and reunion management portal for IAF Batch 1986. Please register your attendance, accompanying family members, travel logistics, and T-shirt size. Ensure to record your fee payment confirmation before the deadline.",
                IsPublished = true,
                PublishedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                AuthorName = "Reunion Organizers"
            }
        );
    }
}
