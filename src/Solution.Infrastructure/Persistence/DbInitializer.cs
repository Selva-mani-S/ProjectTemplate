namespace Solution.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Solution.Domain.Entities;
using Solution.Domain.Enums;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        using IServiceScope scope = serviceProvider.CreateScope();
        var appDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var identityDb = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();

        await appDb.Database.EnsureCreatedAsync().ConfigureAwait(false);
        await identityDb.Database.EnsureCreatedAsync().ConfigureAwait(false);

        // 1. Ensure Master Trades exist
        if (!await appDb.Trades.AnyAsync().ConfigureAwait(false))
        {
            var trades = new List<Trade>
            {
                new() { TradeId = "AD", TradeName = "Administrative" },
                new() { TradeId = "AC", TradeName = "Accounts" },
                new() { TradeId = "CO", TradeName = "Communications / Signals" },
                new() { TradeId = "ED", TradeName = "Education" },
                new() { TradeId = "EL", TradeName = "Electrical Fitters" },
                new() { TradeId = "EN", TradeName = "Engine Fitters" },
                new() { TradeId = "LO", TradeName = "Logistics" },
                new() { TradeId = "ME", TradeName = "Mechanical Transport" },
                new() { TradeId = "MD", TradeName = "Medical" },
                new() { TradeId = "RA", TradeName = "Radar Fitters" },
                new() { TradeId = "SE", TradeName = "Security" },
                new() { TradeId = "WF", TradeName = "Weapon Fitters" }
            };
            appDb.Trades.AddRange(trades);
            await appDb.SaveChangesAsync().ConfigureAwait(false);
        }

        // 2. Ensure Master States exist
        if (!await appDb.States.AnyAsync().ConfigureAwait(false))
        {
            var states = new List<State>
            {
                new() { StateId = "AP", StateName = "Andhra Pradesh" },
                new() { StateId = "AR", StateName = "Arunachal Pradesh" },
                new() { StateId = "AS", StateName = "Assam" },
                new() { StateId = "BR", StateName = "Bihar" },
                new() { StateId = "CG", StateName = "Chhattisgarh" },
                new() { StateId = "DL", StateName = "Delhi" },
                new() { StateId = "GA", StateName = "Goa" },
                new() { StateId = "GJ", StateName = "Gujarat" },
                new() { StateId = "HR", StateName = "Haryana" },
                new() { StateId = "HP", StateName = "Himachal Pradesh" },
                new() { StateId = "JK", StateName = "Jammu & Kashmir" },
                new() { StateId = "JH", StateName = "Jharkhand" },
                new() { StateId = "KA", StateName = "Karnataka" },
                new() { StateId = "KL", StateName = "Kerala" },
                new() { StateId = "MP", StateName = "Madhya Pradesh" },
                new() { StateId = "MH", StateName = "Maharashtra" },
                new() { StateId = "MN", StateName = "Manipur" },
                new() { StateId = "ML", StateName = "Meghalaya" },
                new() { StateId = "MZ", StateName = "Mizoram" },
                new() { StateId = "NL", StateName = "Nagaland" },
                new() { StateId = "OD", StateName = "Odisha" },
                new() { StateId = "PB", StateName = "Punjab" },
                new() { StateId = "RJ", StateName = "Rajasthan" },
                new() { StateId = "SK", StateName = "Sikkim" },
                new() { StateId = "TN", StateName = "Tamil Nadu" },
                new() { StateId = "TS", StateName = "Telangana" },
                new() { StateId = "TR", StateName = "Tripura" },
                new() { StateId = "UP", StateName = "Uttar Pradesh" },
                new() { StateId = "UK", StateName = "Uttarakhand" },
                new() { StateId = "WB", StateName = "West Bengal" },
                new() { StateId = "AN", StateName = "Andaman & Nicobar Islands" },
                new() { StateId = "CH", StateName = "Chandigarh" },
                new() { StateId = "DN", StateName = "Dadra & Nagar Haveli and Daman & Diu" },
                new() { StateId = "LA", StateName = "Ladakh" },
                new() { StateId = "LD", StateName = "Lakshadweep" },
                new() { StateId = "PY", StateName = "Puducherry" }
            };
            appDb.States.AddRange(states);
            await appDb.SaveChangesAsync().ConfigureAwait(false);
        }

        // 3. Ensure Default Reunion Event exists
        IafReunionEvent? defaultEvent = await appDb.ReunionEvents.FirstOrDefaultAsync(e => e.IsActive).ConfigureAwait(false);
        if (defaultEvent == null)
        {
            defaultEvent = new IafReunionEvent
            {
                Title = "Indian Air Force Batch 1986 Grand Reunion",
                Batch = "1986",
                EventDate = new DateTime(2027, 2, 20),
                EventEndDate = new DateTime(2027, 2, 22),
                Venue = "Air Force Officers Mess & Convention Centre, Chennai",
                RegistrationDeadline = new DateTime(2027, 1, 31),
                BaseFeeAmount = 5000.00m,
                IsActive = true,
                Description = "Grand 40-Year Reunion celebrating the Indian Air Force Batch of 1986 veterans, spouses, and families.",
                CreatedAtUtc = DateTime.UtcNow
            };
            appDb.ReunionEvents.Add(defaultEvent);
            await appDb.SaveChangesAsync().ConfigureAwait(false);
        }

        // 4. Ensure Default Announcements exist
        if (!await appDb.Announcements.AnyAsync().ConfigureAwait(false))
        {
            appDb.Announcements.Add(new IafAnnouncement
            {
                EventId = defaultEvent.EventId,
                Title = "Welcome Batch 1986 Veterans! Registration is Open",
                Content = "Welcome to the official registry and reunion management portal for IAF Batch 1986. Please register your attendance, accompanying family members, travel logistics, and T-shirt size. Ensure to record your fee payment confirmation before the deadline.",
                IsPublished = true,
                PublishedAtUtc = DateTime.UtcNow,
                AuthorName = "Reunion Organizers"
            });
            await appDb.SaveChangesAsync().ConfigureAwait(false);
        }

        // 5. Ensure Initial IAF Batch 1986 Member Data exists
        //if (!await appDb.PersonalData.AnyAsync().ConfigureAwait(false))
        //{
        //    var initialMembers = new List<IafPersonalData>
        //    {
        //        new()
        //        {
        //            ServiceNo = "661234",
        //            Name = "Sgt Rajesh Kumar",
        //            Dob = new DateTime(1965, 5, 12),
        //            TradeId = "RA",
        //            Address1 = "Plot 42, Air Force Enclave",
        //            Address2 = "Tambaram",
        //            StateId = "TN",
        //            MobileNo = "+919876543210",
        //            EmailId = "rajesh.kumar@iaf1986.local",
        //            TechnicalSkills = "Radar Systems, Avionics Troubleshooting",
        //            PlacesWorked = "AFS Tambaram, AFS Bareilly, AFS Hindan",
        //            Achievements = "Commendation by AOC-in-C Maintenance Command",
        //            VrsReason = "Family personal reasons after 20 years exemplary service"
        //        },
        //        new()
        //        {
        //            ServiceNo = "661235",
        //            Name = "WO Arun Sharma",
        //            Dob = new DateTime(1964, 8, 22),
        //            TradeId = "CO",
        //            Address1 = "House 18, Vayu Vihar",
        //            Address2 = "Subroto Park",
        //            StateId = "DL",
        //            MobileNo = "+919876543211",
        //            EmailId = "arun.sharma@iaf1986.local",
        //            TechnicalSkills = "High Frequency Telecom Systems, Cryptography",
        //            PlacesWorked = "Air HQ, AFS Palam, AFS Leh",
        //            Achievements = "Chief of Air Staff Commendation Badge",
        //            VrsReason = "Completed 22 years pensionable colour service"
        //        },
        //        new()
        //        {
        //            ServiceNo = "661236",
        //            Name = "JWO K Raman",
        //            Dob = new DateTime(1966, 11, 4),
        //            TradeId = "EL",
        //            Address1 = "TC 14/120, Kowdiar",
        //            Address2 = "Thiruvananthapuram",
        //            StateId = "KL",
        //            MobileNo = "+919876543212",
        //            EmailId = "k.raman@iaf1986.local",
        //            TechnicalSkills = "Aircraft Electrical Power Distribution",
        //            PlacesWorked = "Southern Air Command, AFS Sulur",
        //            Achievements = "Flight Safety Trophy 1998",
        //            VrsReason = "Superannuation"
        //        },
        //        new()
        //        {
        //            ServiceNo = "661237",
        //            Name = "MWO Suresh Nair",
        //            Dob = new DateTime(1963, 2, 19),
        //            TradeId = "EN",
        //            Address1 = "77, HAL 2nd Stage",
        //            Address2 = "Indiranagar",
        //            StateId = "KA",
        //            MobileNo = "+919876543213",
        //            EmailId = "suresh.nair@iaf1986.local",
        //            TechnicalSkills = "Aero Engine Overhaul & Testing",
        //            PlacesWorked = "BRD Kanpur, AFS Yelahanka",
        //            Achievements = "Best Maintenance Crew Award 2002",
        //            VrsReason = "Medical downgrade upon reaching age 52"
        //        },
        //        new()
        //        {
        //            ServiceNo = "661238",
        //            Name = "Cpl Deepu Varghese",
        //            Dob = new DateTime(1967, 7, 30),
        //            TradeId = "LO",
        //            Address1 = "B-204, Vayu Sena Society",
        //            Address2 = "Viman Nagar",
        //            StateId = "MH",
        //            MobileNo = "+919876543214",
        //            EmailId = "deepu.varghese@iaf1986.local",
        //            TechnicalSkills = "Aviation Inventory & Supply Chain Logistics",
        //            PlacesWorked = "AFS Pune, AFS Jamnagar",
        //            Achievements = "Unit Level Commendation",
        //            VrsReason = "Civil employment offer"
        //        }
        //    };

        //    appDb.PersonalData.AddRange(initialMembers);
        //    await appDb.SaveChangesAsync().ConfigureAwait(false);

        //    // Add sample family members
        //    appDb.FamilyData.AddRange(
        //        new IafFamilyData
        //        {
        //            ServiceNo = "661234",
        //            MemberName = "Sunita Kumar",
        //            Relationship = "Wife",
        //            Gender = "F",
        //            Occupation = "Teacher",
        //            Remarks = "Attending reunion"
        //        },
        //        new IafFamilyData
        //        {
        //            ServiceNo = "661234",
        //            MemberName = "Rohan Kumar",
        //            Relationship = "Son",
        //            Gender = "M",
        //            Occupation = "Software Engineer",
        //            Remarks = "Attending reunion"
        //        },
        //        new IafFamilyData
        //        {
        //            ServiceNo = "661235",
        //            MemberName = "Shalini Sharma",
        //            Relationship = "Wife",
        //            Gender = "F",
        //            Occupation = "Homemaker",
        //            Remarks = "Attending reunion"
        //        }
        //    );
        //    await appDb.SaveChangesAsync().ConfigureAwait(false);
        //}
    }
}
