using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Solution.Infrastructure.Persistence.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class AddIafCodeFirstEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "app",
                table: "SampleEntities",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.CreateTable(
                name: "IAF_ReunionEvent",
                schema: "app",
                columns: table => new
                {
                    EventId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Batch = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EventDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Venue = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    RegistrationDeadline = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BaseFeeAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IAF_ReunionEvent", x => x.EventId);
                });

            migrationBuilder.CreateTable(
                name: "State",
                schema: "app",
                columns: table => new
                {
                    StateId = table.Column<string>(type: "varchar(2)", unicode: false, maxLength: 2, nullable: false),
                    StateName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_State", x => x.StateId);
                });

            migrationBuilder.CreateTable(
                name: "Trade",
                schema: "app",
                columns: table => new
                {
                    TradeId = table.Column<string>(type: "varchar(2)", unicode: false, maxLength: 2, nullable: false),
                    TradeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trade", x => x.TradeId);
                });

            migrationBuilder.CreateTable(
                name: "IAF_Announcement",
                schema: "app",
                columns: table => new
                {
                    AnnouncementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    PublishedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AuthorName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IAF_Announcement", x => x.AnnouncementId);
                    table.ForeignKey(
                        name: "FK_IAF_Announcement_IAF_ReunionEvent_EventId",
                        column: x => x.EventId,
                        principalSchema: "app",
                        principalTable: "IAF_ReunionEvent",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "IAF_PersonalData",
                schema: "app",
                columns: table => new
                {
                    ServiceNo = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Dob = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Dod = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TradeId = table.Column<string>(type: "varchar(2)", unicode: false, maxLength: 2, nullable: true),
                    Address1 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Address2 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StateId = table.Column<string>(type: "varchar(2)", unicode: false, maxLength: 2, nullable: true),
                    MobileNo = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    TechnicalSkills = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    PlacesWorked = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    AdditionalDetails = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Achievements = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    VrsReason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IAF_PersonalData", x => x.ServiceNo);
                    table.ForeignKey(
                        name: "FK_IAF_PersonalData_State_StateId",
                        column: x => x.StateId,
                        principalSchema: "app",
                        principalTable: "State",
                        principalColumn: "StateId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IAF_PersonalData_Trade_TradeId",
                        column: x => x.TradeId,
                        principalSchema: "app",
                        principalTable: "Trade",
                        principalColumn: "TradeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IAF_AdditionalData",
                schema: "app",
                columns: table => new
                {
                    ServiceNo = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: false),
                    Photo1 = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Photo2 = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Photo3 = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Photo4 = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Photo5 = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IAF_AdditionalData", x => x.ServiceNo);
                    table.ForeignKey(
                        name: "FK_IAF_AdditionalData_IAF_PersonalData_ServiceNo",
                        column: x => x.ServiceNo,
                        principalSchema: "app",
                        principalTable: "IAF_PersonalData",
                        principalColumn: "ServiceNo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IAF_FamilyData",
                schema: "app",
                columns: table => new
                {
                    FamilyMemberId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceNo = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: false),
                    MemberName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Relationship = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Gender = table.Column<string>(type: "varchar(1)", unicode: false, maxLength: 1, nullable: true),
                    Occupation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IAF_FamilyData", x => x.FamilyMemberId);
                    table.ForeignKey(
                        name: "FK_IAF_FamilyData_IAF_PersonalData_ServiceNo",
                        column: x => x.ServiceNo,
                        principalSchema: "app",
                        principalTable: "IAF_PersonalData",
                        principalColumn: "ServiceNo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IAF_Payment",
                schema: "app",
                columns: table => new
                {
                    PaymentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    ServiceNo = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentMode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AcknowledgementNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PaymentProofPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AdditionalRemarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SubmittedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VerifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VerifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IAF_Payment", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_IAF_Payment_IAF_PersonalData_ServiceNo",
                        column: x => x.ServiceNo,
                        principalSchema: "app",
                        principalTable: "IAF_PersonalData",
                        principalColumn: "ServiceNo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IAF_Payment_IAF_ReunionEvent_EventId",
                        column: x => x.EventId,
                        principalSchema: "app",
                        principalTable: "IAF_ReunionEvent",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IAF_Reunion",
                schema: "app",
                columns: table => new
                {
                    ReunionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    ServiceNo = table.Column<string>(type: "varchar(6)", unicode: false, maxLength: 6, nullable: false),
                    NoOfPersons = table.Column<int>(type: "int", nullable: false),
                    AccompDetails = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DateOfArrival = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlaceOfArrival = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Accommodation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DateOfDeparture = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlaceOfDeparture = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TShirt = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Drinker = table.Column<int>(type: "int", nullable: false),
                    VegNonVeg = table.Column<int>(type: "int", nullable: false),
                    RegistrationStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IAF_Reunion", x => x.ReunionId);
                    table.ForeignKey(
                        name: "FK_IAF_Reunion_IAF_PersonalData_ServiceNo",
                        column: x => x.ServiceNo,
                        principalSchema: "app",
                        principalTable: "IAF_PersonalData",
                        principalColumn: "ServiceNo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IAF_Reunion_IAF_ReunionEvent_EventId",
                        column: x => x.EventId,
                        principalSchema: "app",
                        principalTable: "IAF_ReunionEvent",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "app",
                table: "IAF_ReunionEvent",
                columns: new[] { "EventId", "BaseFeeAmount", "Batch", "CreatedAtUtc", "Description", "EventDate", "EventEndDate", "IsActive", "RegistrationDeadline", "Title", "Venue" },
                values: new object[] { 1, 5000.00m, "1986", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Grand 40-Year Reunion celebrating the Indian Air Force Batch of 1986 veterans, spouses, and families.", new DateTime(2027, 2, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2027, 2, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), true, new DateTime(2027, 1, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "Indian Air Force Batch 1986 Grand Reunion", "Air Force Officers Mess & Convention Centre, Chennai" });

            migrationBuilder.InsertData(
                schema: "app",
                table: "State",
                columns: new[] { "StateId", "StateName" },
                values: new object[,]
                {
                    { "AP", "Andhra Pradesh" },
                    { "AR", "Arunachal Pradesh" },
                    { "AS", "Assam" },
                    { "BR", "Bihar" },
                    { "CG", "Chhattisgarh" },
                    { "DL", "Delhi" },
                    { "GA", "Goa" },
                    { "GJ", "Gujarat" },
                    { "HP", "Himachal Pradesh" },
                    { "HR", "Haryana" },
                    { "JH", "Jharkhand" },
                    { "KA", "Karnataka" },
                    { "KL", "Kerala" },
                    { "MH", "Maharashtra" },
                    { "ML", "Meghalaya" },
                    { "MN", "Manipur" },
                    { "MP", "Madhya Pradesh" },
                    { "MZ", "Mizoram" },
                    { "NL", "Nagaland" },
                    { "OD", "Odisha" },
                    { "PB", "Punjab" },
                    { "RJ", "Rajasthan" },
                    { "SK", "Sikkim" },
                    { "TN", "Tamil Nadu" },
                    { "TR", "Tripura" },
                    { "TS", "Telangana" },
                    { "UK", "Uttarakhand" },
                    { "UP", "Uttar Pradesh" },
                    { "WB", "West Bengal" }
                });

            migrationBuilder.InsertData(
                schema: "app",
                table: "Trade",
                columns: new[] { "TradeId", "TradeName" },
                values: new object[,]
                {
                    { "AC", "Accounts" },
                    { "AD", "Administrative" },
                    { "CO", "Communications / Signals" },
                    { "ED", "Education" },
                    { "EL", "Electrical Fitters" },
                    { "EN", "Engine Fitters" },
                    { "LO", "Logistics" },
                    { "MD", "Medical" },
                    { "ME", "Mechanical Transport" },
                    { "RA", "Radar Fitters" },
                    { "SE", "Security" },
                    { "WF", "Weapon Fitters" }
                });

            migrationBuilder.InsertData(
                schema: "app",
                table: "IAF_Announcement",
                columns: new[] { "AnnouncementId", "AuthorName", "Content", "EventId", "IsPublished", "PublishedAtUtc", "Title" },
                values: new object[] { 1, "Reunion Organizers", "Welcome to the official registry and reunion management portal for IAF Batch 1986. Please register your attendance, accompanying family members, travel logistics, and T-shirt size. Ensure to record your fee payment confirmation before the deadline.", 1, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Welcome Batch 1986 Veterans! Registration is Open" });

            migrationBuilder.CreateIndex(
                name: "IX_IAF_Announcement_EventId",
                schema: "app",
                table: "IAF_Announcement",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_IAF_FamilyData_ServiceNo",
                schema: "app",
                table: "IAF_FamilyData",
                column: "ServiceNo");

            migrationBuilder.CreateIndex(
                name: "IX_IAF_Payment_AcknowledgementNumber",
                schema: "app",
                table: "IAF_Payment",
                column: "AcknowledgementNumber");

            migrationBuilder.CreateIndex(
                name: "IX_IAF_Payment_EventId",
                schema: "app",
                table: "IAF_Payment",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_IAF_Payment_PaymentStatus",
                schema: "app",
                table: "IAF_Payment",
                column: "PaymentStatus");

            migrationBuilder.CreateIndex(
                name: "IX_IAF_Payment_ServiceNo",
                schema: "app",
                table: "IAF_Payment",
                column: "ServiceNo");

            migrationBuilder.CreateIndex(
                name: "IX_IAF_PersonalData_StateId",
                schema: "app",
                table: "IAF_PersonalData",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_IAF_PersonalData_TradeId",
                schema: "app",
                table: "IAF_PersonalData",
                column: "TradeId");

            migrationBuilder.CreateIndex(
                name: "IX_IAF_Reunion_EventId_ServiceNo",
                schema: "app",
                table: "IAF_Reunion",
                columns: new[] { "EventId", "ServiceNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IAF_Reunion_ServiceNo",
                schema: "app",
                table: "IAF_Reunion",
                column: "ServiceNo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IAF_AdditionalData",
                schema: "app");

            migrationBuilder.DropTable(
                name: "IAF_Announcement",
                schema: "app");

            migrationBuilder.DropTable(
                name: "IAF_FamilyData",
                schema: "app");

            migrationBuilder.DropTable(
                name: "IAF_Payment",
                schema: "app");

            migrationBuilder.DropTable(
                name: "IAF_Reunion",
                schema: "app");

            migrationBuilder.DropTable(
                name: "IAF_PersonalData",
                schema: "app");

            migrationBuilder.DropTable(
                name: "IAF_ReunionEvent",
                schema: "app");

            migrationBuilder.DropTable(
                name: "State",
                schema: "app");

            migrationBuilder.DropTable(
                name: "Trade",
                schema: "app");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "app",
                table: "SampleEntities",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
