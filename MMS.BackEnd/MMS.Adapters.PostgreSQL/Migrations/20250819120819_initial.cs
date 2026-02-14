using System;
using System.Collections.Generic;
using MMS.Application.Models.SQL;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MMS.Adapters.PostgreSQL.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("805052a9-e25a-4cab-9a2b-032ab18d432e")),
                    ApiRequest = table.Column<string>(type: "jsonb", nullable: true),
                    ApiResponse = table.Column<string>(type: "jsonb", nullable: true),
                    Exception = table.Column<string>(type: "text", nullable: true),
                    Level = table.Column<string>(type: "text", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: true),
                    Deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomerBillingAddresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("3a2c8c27-653a-4ae7-ab30-33dfc6a24c08")),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Region = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ZipCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerBillingAddresses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("0dc5df1e-96f2-4294-a060-cdf88144f58f")),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PhoneCountryCode = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    TimeZone = table.Column<string>(type: "text", nullable: false),
                    Country = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<string>(type: "text", nullable: false),
                    Street = table.Column<string>(type: "text", nullable: false),
                    PostalCode = table.Column<string>(type: "text", nullable: false),
                    Region = table.Column<string>(type: "text", nullable: false),
                    ImageUrls = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Shifts = table.Column<List<Shift>>(type: "jsonb", nullable: false),
                    Deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomerSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("347f8e76-9dd7-4706-9362-9fb374d7f5f0")),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    RenewalType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerSubscriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("5f6c6b7e-2afc-416a-bee6-4b352d12a553")),
                    Invoicenumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Payment = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Amout = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Tax = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Paymentmethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PaymentGatewayTrxId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CustomerSubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    BillingAdressId = table.Column<Guid>(type: "uuid", nullable: false),
                    Deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("325819ed-9a79-455e-b325-94f5d714c9db")),
                    Topic = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    DataPayload = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true),
                    ReadStatus = table.Column<int>(type: "integer", nullable: false),
                    click_action = table.Column<int>(type: "integer", nullable: true),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActionId = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("747568f1-f304-4392-8f70-50434d1790b7")),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    BillingCycle = table.Column<int>(type: "integer", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Features = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: false),
                    Deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                });

     
            migrationBuilder.CreateTable(
                name: "CustomerDashboards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("13dd8e7c-ec9f-48d6-80bc-973da538aef3")),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    RefreshInterval = table.Column<int>(type: "integer", nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    Theme = table.Column<int>(type: "integer", maxLength: 200, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Layout = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerDashboards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerDashboards_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CustomerReportSetting",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("66a95568-0615-431d-927d-dcd0810b680d")),
                    ReportName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Email = table.Column<List<string>>(type: "text[]", nullable: false),
                    Format = table.Column<string>(type: "text", nullable: false),
                    Frequency = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCustomReport = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    WeekDays = table.Column<string[]>(type: "text[]", nullable: false),
                    ReportPeriodStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReportPeriodEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MachineIds = table.Column<string[]>(type: "text[]", nullable: false),
                    ReportType = table.Column<string[]>(type: "text[]", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerReportSetting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerReportSetting_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Machines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("b759b73f-1984-42a8-ba17-bc0dd7537593")),
                    MachineName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    MachineModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Manufacturer = table.Column<string>(type: "text", nullable: true),
                    SerialNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Location = table.Column<string>(type: "text", nullable: true),
                    InstallationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CommunicationProtocol = table.Column<int>(type: "integer", nullable: false),
                    MachineType = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Machines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Machines_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Widgets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("fe15bb6a-cf12-47bd-8710-8616c6522380")),
                    WidgetType = table.Column<int>(type: "integer", nullable: false),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    Config = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: false),
                    DashboardId = table.Column<Guid>(type: "uuid", nullable: false),
                    Deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Widgets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Widgets_CustomerDashboards_DashboardId",
                        column: x => x.DashboardId,
                        principalTable: "CustomerDashboards",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CustomerReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("a2a2307c-54e0-416b-a879-32d6074d46e3")),
                    CustomerReportSettingId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportName = table.Column<string>(type: "text", nullable: false),
                    BlobLink = table.Column<string>(type: "text", nullable: false),
                    Format = table.Column<int>(type: "integer", nullable: false),
                    IsSent = table.Column<bool>(type: "boolean", nullable: false),
                    GeneratedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerReports_CustomerReportSetting_CustomerReportSetting~",
                        column: x => x.CustomerReportSettingId,
                        principalTable: "CustomerReportSetting",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerReports_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MachineMaintenances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("0f9a811c-885f-4c51-862d-f8582c9812df")),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    MaintenanceTaskName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsFinished = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    MachineId = table.Column<Guid>(type: "uuid", nullable: false),
                    Deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineMaintenances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MachineMaintenances_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MachineSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("a4b59edc-3d39-49ce-9c72-47ef9425b472")),
                    CycleStop = table.Column<bool>(type: "boolean", nullable: false),
                    SoundAlert = table.Column<bool>(type: "boolean", nullable: false),
                    CycleStartInterlock = table.Column<bool>(type: "boolean", nullable: false),
                    PartsPerCycle = table.Column<int>(type: "integer", nullable: true),
                    AutomaticPartsCounter = table.Column<bool>(type: "boolean", nullable: false),
                    MaxFeedrate = table.Column<decimal>(type: "numeric", nullable: true),
                    MaxSpindleSpeed = table.Column<int>(type: "integer", nullable: true),
                    MaxCycleDuration = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    StopInterval = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    DownTimeReasons = table.Column<List<string>>(type: "text[]", nullable: false),
                    Alerts = table.Column<List<Alert>>(type: "jsonb", nullable: false),
                    MachineId = table.Column<Guid>(type: "uuid", nullable: false),
                    Deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MachineSettings_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserMachines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("8622de74-9481-4cf8-9ce2-7dfda13bf17d")),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    MachineId = table.Column<Guid>(type: "uuid", nullable: false),
                    Deleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMachines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserMachines_Machines_MachineId",
                        column: x => x.MachineId,
                        principalTable: "Machines",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "CustomerSubscriptions",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "CustomerId", "Deleted", "DeletedBy", "EndDate", "InvoiceId", "IsActive", "RenewalType", "StartDate", "Status", "SubscriptionId", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new Guid("88888888-1111-1111-1111-111111111111"), null, null, new Guid("11111111-1111-1111-1111-111111111111"), null, null, new DateTime(2024, 1, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("99999999-2222-2222-2222-222222222222"), true, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Active", new Guid("45b429a4-f21b-4be4-981b-a4ecee7244b1"), null, null });

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "Id", "City", "Country", "CreatedAt", "CreatedBy", "Deleted", "DeletedBy", "Email", "ImageUrls", "Name", "PhoneCountryCode", "PhoneNumber", "PostalCode", "Region", "Shifts", "Status", "Street", "TimeZone", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "Karachi", "Pakistan", null, null, null, null, "customer1@example.com", "https://apcstorageaccount.blob.core.windows.net/mms-container/2025_06_19_pexels-pixabay-69932.jpg", "Customer 1", "+92", "3001234561", "74200", "Sindh", new List<Shift>(), 0, "1st Avenue", "Asia/Karachi", null, null },
                    { new Guid("11111111-1111-1111-1111-111111111112"), "Lahore", "Pakistan", null, null, null, null, "customer2@example.com", "https://apcstorageaccount.blob.core.windows.net/mms-container/2025_06_19_pexels-pixabay-69932.jpg", "Customer 2", "+92", "3001234562", "54000", "Punjab", new List<Shift>(), 0, "2nd Avenue", "Asia/Karachi", null, null }
                });

            migrationBuilder.InsertData(
                table: "Subscriptions",
                columns: new[] { "Id", "BillingCycle", "CreatedAt", "CreatedBy", "Currency", "Deleted", "DeletedBy", "Features", "Name", "Price", "Status", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("45b429a4-f21b-4be4-981b-a4ecee7244b1"), 0, null, null, "USD", null, null, new Dictionary<string, object>(), "Plan 1", 9.99m, 0, null, null },
                    { new Guid("45b429a4-f21b-4be4-981b-a4ecee7244b2"), 0, null, null, "USD", null, null, new Dictionary<string, object>(), "Plan 2", 19.99m, 0, null, null }
                });

            migrationBuilder.InsertData(
                table: "CustomerDashboards",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "CustomerId", "Deleted", "DeletedBy", "IsDefault", "Layout", "Name", "RefreshInterval", "Status", "Theme", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("c1277139-895e-4efa-a9c0-c544406a2ce1"), null, null, new Guid("11111111-1111-1111-1111-111111111111"), null, null, false, new Dictionary<string, object>(), "Dashboard 1", 60, 0, 1, null, null },
                    { new Guid("c1277139-895e-4efa-a9c0-c544406a2ce2"), null, null, new Guid("11111111-1111-1111-1111-111111111112"), null, null, false, new Dictionary<string, object>(), "Dashboard 2", 60, 0, 1, null, null }
                });

            migrationBuilder.InsertData(
                table: "Machines",
                columns: new[] { "Id", "CommunicationProtocol", "CreatedAt", "CreatedBy", "CustomerId", "Deleted", "DeletedBy", "ImageUrl", "InstallationDate", "Location", "MachineModel", "MachineName", "MachineType", "Manufacturer", "SerialNumber", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("b9cdaa9c-b1b0-48ce-af8e-fda9677a8d91"), 2, null, null, new Guid("11111111-1111-1111-1111-111111111111"), null, null, null, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Location 1", "Model X", "Machine 1", 2, "Manufacturer A", "SN001", null, null },
                    { new Guid("b9cdaa9c-b1b0-48ce-af8e-fda9677a8d92"), 2, null, null, new Guid("11111111-1111-1111-1111-111111111112"), null, null, null, new DateTime(2023, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Location 2", "Model X", "Machine 2", 3, "Manufacturer B", "SN002", null, null }
                });

            migrationBuilder.InsertData(
                table: "MachineMaintenances",
                columns: new[] { "Id", "Category", "CreatedAt", "CreatedBy", "Deleted", "DeletedBy", "EndTime", "ImageUrl", "MachineId", "MaintenanceTaskName", "Notes", "Reason", "StartTime", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new Guid("aaaaaaa1-1111-1111-1111-111111111111"), 6, null, null, null, null, null, null, new Guid("b9cdaa9c-b1b0-48ce-af8e-fda9677a8d91"), "Task 1", null, null, new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc), null, null });

            migrationBuilder.InsertData(
                table: "MachineSettings",
                columns: new[] { "Id", "Alerts", "AutomaticPartsCounter", "CreatedAt", "CreatedBy", "CycleStartInterlock", "CycleStop", "Deleted", "DeletedBy", "DownTimeReasons", "MachineId", "MaxCycleDuration", "MaxFeedrate", "MaxSpindleSpeed", "PartsPerCycle", "SoundAlert", "Status", "StopInterval", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new Guid("99999999-1111-1111-1111-111111111111"), new List<Alert>(), false, null, null, false, false, null, null, new List<string> { "Power Failure" }, new Guid("b9cdaa9c-b1b0-48ce-af8e-fda9677a8d91"), new TimeOnly(0, 0, 0), null, null, null, false, 0, new TimeOnly(0, 0, 0), null, null });

            migrationBuilder.InsertData(
                table: "Widgets",
                columns: new[] { "Id", "Config", "CreatedAt", "CreatedBy", "DashboardId", "Deleted", "DeletedBy", "SourceType", "UpdatedAt", "UpdatedBy", "WidgetType" },
                values: new object[] { new Guid("ddddddd1-1111-1111-1111-111111111111"), new Dictionary<string, object>(), null, null, new Guid("c1277139-895e-4efa-a9c0-c544406a2ce1"), null, null, 0, null, null, 3 });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDashboards_CustomerId",
                table: "CustomerDashboards",
                column: "CustomerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerReports_CustomerId",
                table: "CustomerReports",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerReports_CustomerReportSettingId",
                table: "CustomerReports",
                column: "CustomerReportSettingId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerReportSetting_CustomerId",
                table: "CustomerReportSetting",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_MachineMaintenances_MachineId",
                table: "MachineMaintenances",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_Machines_CustomerId",
                table: "Machines",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_MachineSettings_MachineId",
                table: "MachineSettings",
                column: "MachineId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserMachines_MachineId",
                table: "UserMachines",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_UserMachines_UserId_MachineId",
                table: "UserMachines",
                columns: new[] { "UserId", "MachineId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Widgets_DashboardId",
                table: "Widgets",
                column: "DashboardId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationLogs");

            migrationBuilder.DropTable(
                name: "CustomerBillingAddresses");

            migrationBuilder.DropTable(
                name: "CustomerReports");

            migrationBuilder.DropTable(
                name: "CustomerSubscriptions");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "MachineMaintenances");

            migrationBuilder.DropTable(
                name: "MachineSettings");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Subscriptions");


            migrationBuilder.DropTable(
                name: "UserMachines");

            migrationBuilder.DropTable(
                name: "Widgets");

            migrationBuilder.DropTable(
                name: "CustomerReportSetting");

            migrationBuilder.DropTable(
                name: "Machines");

            migrationBuilder.DropTable(
                name: "CustomerDashboards");

            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
