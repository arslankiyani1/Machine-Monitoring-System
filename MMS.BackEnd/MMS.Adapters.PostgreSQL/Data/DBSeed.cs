using MMS.Application.Enum;

namespace MMS.Adapters.PostgreSQL.Data;

public static class DbSeed
{
    public static void SeedDatabase(ModelBuilder modelBuilder)
    {
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>().HasData(
            new Customer
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Customer 1",
                Email = "customer1@example.com",
                PhoneCountryCode = "+92",
                PhoneNumber = "3001234561",
                TimeZone = "Asia/Karachi",
                Country = "Pakistan",
                City = "Karachi",
                Street = "1st Avenue",
                PostalCode = "74200",
                ImageUrls = "https://apcstorageaccount.blob.core.windows.net/mms-container/2025_06_19_pexels-pixabay-69932.jpg",
                Region = "Sindh",
                State = "Sindh",
                Status = CustomerStatus.Active,
                Shifts = []
            },
            new Customer
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111112"),
                Name = "Customer 2",
                Email = "customer2@example.com",
                PhoneCountryCode = "+92",
                PhoneNumber = "3001234562",
                TimeZone = "Asia/Karachi",
                Country = "Pakistan",
                City = "Lahore",
                Street = "2nd Avenue",
                PostalCode = "54000",
                ImageUrls = "https://apcstorageaccount.blob.core.windows.net/mms-container/2025_06_19_pexels-pixabay-69932.jpg",
                Region = "Punjab",
                State = "Sindh",
                Status = CustomerStatus.Active,
                Shifts = [],
            }
        );

        modelBuilder.Entity<CustomerDashboard>().HasData(
            new CustomerDashboard
            {
                Id = Guid.Parse("c1277139-895e-4efa-a9c0-c544406a2ce1"),
                CustomerId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Dashboard 1",
                RefreshInterval = 60,
                IsDefault = false,
                Theme = DashboardTheme.Dark,
                Status = DashboardStatus.Active,
                Layout = []
            },
            new CustomerDashboard
            {
                Id = Guid.Parse("c1277139-895e-4efa-a9c0-c544406a2ce2"),
                CustomerId = Guid.Parse("11111111-1111-1111-1111-111111111112"),
                Name = "Dashboard 2",
                RefreshInterval = 60,
                IsDefault = false,
                Theme = DashboardTheme.Dark,
                Status = DashboardStatus.Active,
                Layout = []
            }
        );

        //modelBuilder.Entity<CustomerReport>().HasData(
        //    new CustomerReport
        //    {
        //        Id = Guid.Parse("77777777-1111-1111-1111-111111111111"),
        //        CustomerId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        //        ReportName = "Report 1",
        //        Email = "report1@example.com",
        //        ReportType = ReportType.Downtime,
        //        Format = ReportFormat.PDF,
        //        Frequency = ReportFrequency.Monthly,
        //        Status = ReportStatus.Active,
        //        WeekDays = ["Monday", "Tuesday"],
        //        Filters = []
        //    },
        //    new CustomerReport
        //    {
        //        Id = Guid.Parse("77777777-1111-1111-1111-111111111112"),
        //        CustomerId = Guid.Parse("11111111-1111-1111-1111-111111111112"),
        //        ReportName = "Report 2",
        //        Email = "report2@example.com",
        //        ReportType = ReportType.Downtime,
        //        Format = ReportFormat.PDF,
        //        Frequency = ReportFrequency.Monthly,
        //        Status = ReportStatus.Active,
        //        WeekDays = ["Wednesday", "Thursday"],
        //        Filters = []
        //    }
        //);

        modelBuilder.Entity<Subscription>().HasData(
            new Subscription
            {
                Id = Guid.Parse("45b429a4-f21b-4be4-981b-a4ecee7244b1"),
                Name = "Plan 1",
                Price = 9.99m,
                BillingCycle = BillingCycle.Monthly,
                Status = SubscriptionStatus.Active,
                Features = []
            },
            new Subscription
            {
                Id = Guid.Parse("45b429a4-f21b-4be4-981b-a4ecee7244b2"),
                Name = "Plan 2",
                Price = 19.99m,
                BillingCycle = BillingCycle.Monthly,
                Status = SubscriptionStatus.Active,
                Features = []
            }
        );

        modelBuilder.Entity<CustomerSubscription>().HasData(
            new CustomerSubscription
            {
                Id = Guid.Parse("88888888-1111-1111-1111-111111111111"),
                CustomerId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                SubscriptionId = Guid.Parse("45b429a4-f21b-4be4-981b-a4ecee7244b1"),
                InvoiceId = Guid.Parse("99999999-2222-2222-2222-222222222222"),
                StartDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(2024, 1, 31, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true, 
                RenewalType = RenewalType.weekly,
                Status = "Active"
            }
        );

        modelBuilder.Entity<Machine>().HasData(
            new Machine
            {
                Id = Guid.Parse("b9cdaa9c-b1b0-48ce-af8e-fda9677a8d91"),
                CustomerId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                MachineName = "Machine 1",
                MachineModel = "Model X",
                Manufacturer = "Manufacturer A",
                SerialNumber = "SN001",
                Location = "Location 1",
                InstallationDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CommunicationProtocol = CommunicationProtocol.Modbus,
                MachineType = MachineType.Milling,
            },
            new Machine
            {
                Id = Guid.Parse("b9cdaa9c-b1b0-48ce-af8e-fda9677a8d92"),
                CustomerId = Guid.Parse("11111111-1111-1111-1111-111111111112"),
                MachineName = "Machine 2",
                MachineModel = "Model X",
                Manufacturer = "Manufacturer B",
                SerialNumber = "SN002",
                Location = "Location 2",
                InstallationDate = new DateTime(2023, 2, 1, 0, 0, 0, DateTimeKind.Utc),
                CommunicationProtocol = CommunicationProtocol.Modbus,
                MachineType = MachineType.Press,
            }
        );

        modelBuilder.Entity<MachineMaintenanceTask>().HasData(
            new MachineMaintenanceTask
            {
                Id = Guid.Parse("aaaaaaa1-1111-1111-1111-111111111111"),
                MachineId = Guid.Parse("b9cdaa9c-b1b0-48ce-af8e-fda9677a8d91"),
                MaintenanceTaskName = "Task 1",
                StartTime = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                IsFinished = false,
                Category = MaintenanceTaskCategory.Emergency
            }
        );

        modelBuilder.Entity<Widget>().HasData(
            new Widget
            {
                Id = Guid.Parse("ddddddd1-1111-1111-1111-111111111111"),
                DashboardId = Guid.Parse("c1277139-895e-4efa-a9c0-c544406a2ce1"),
                WidgetType = WidgetType.Text,
                SourceType = WidgetSourceType.Api,
                Config = []
            }
        );
    }
}