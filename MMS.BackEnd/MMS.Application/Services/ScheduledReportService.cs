namespace MMS.Application.Services;

public class ScheduledReportService(
    ICustomerReportSettingRepository reportSettingRepository,
    IHistoricalStatsRepository historicalStatsRepository,
    IUnitOfWork unitOfWork,
    IEmailService emailService,
    IReportGenerateService reportGenerateService,
    IBlobStorageService blobStorageService,
    AutoMapperResult mapper,
    ILogger<ScheduledReportService> logger) : IScheduledReportService
{
    public async Task<IEnumerable<CustomerReportSetting>> GetDueReportSettingsAsync(DateTime runDate)
    {
        var allSettings = await reportSettingRepository.GetActiveScheduledSettingsAsync();

        var dueSettings = new List<CustomerReportSetting>();

        foreach (var setting in allSettings)
        {
            if (IsDueForExecution(setting, runDate))
            {
                dueSettings.Add(setting);
            }
        }

        return dueSettings;
    }

    public async Task<bool> GenerateAndSendReportAsync(CustomerReportSetting setting, DateTime runDate)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            // Calculate date range based on frequency
            var (startDate, endDate) = CalculateDateRange(setting.Frequency, runDate);

            logger.LogInformation("📊 Generating report '{ReportName}' for period {StartDate} to {EndDate}",
                setting.ReportName, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

            // Fetch historical stats for each machine
            var machineData = new Dictionary<Guid, List<HistoricalStats>>();
            var machineDetails = new Dictionary<Guid, MachineDto>();

            foreach (var machineId in setting.MachineIds)
            {
                var stats = await historicalStatsRepository.GetByMachineIdAndDateRangeAsync(
                    machineId, startDate, endDate);
                machineData.Add(machineId, stats);

                var machineResult = await unitOfWork.MachineRepository.GetAsync(machineId);
                if (machineResult.IsRight)
                {
                    machineDetails.Add(machineId, mapper.Map<MachineDto>(machineResult.IfRight()));
                }
                else
                {
                    logger.LogWarning("⚠️ Machine {MachineId} not found, skipping", machineId);
                }
            }

            if (machineDetails.Count == 0)
            {
                logger.LogWarning("⚠️ No valid machines found for report '{ReportName}'", setting.ReportName);
                return false;
            }

            // Calculate report data
            var reportDatas = new Dictionary<Guid, ReportData>();
            foreach (var kv in machineData)
            {
                if (!machineDetails.ContainsKey(kv.Key)) continue;

                var reportData = CalculateReportData(setting.ReportType, machineDetails[kv.Key], kv.Value);
                reportDatas.Add(kv.Key, reportData);
            }

            // Generate report file
            var fileBytes = await reportGenerateService.GenerateAsync(
                setting.Format.ToString(), reportDatas, setting.ReportType, setting.ReportName);

            var (fileExtension, contentType) = GetFileFormatInfo(setting.Format);
            var fileName = $"{setting.ReportName}_{DateTime.UtcNow:yyyyMMddHHmmss}.{fileExtension}";

            // Upload to blob storage
            var blobLink = await blobStorageService.UploadReportAsync(
                fileBytes, fileName, BlobStorageConstants.ReportsFolder);

            // Send email
            var emailSent = await emailService.SendReportAsync(
                setting.Email, setting.ReportName, fileBytes, setting.Format.ToString());

            // Save CustomerReport record
            transaction = await unitOfWork.BeginTransactionAsync();

            var customerReport = new CustomerReport
            {
                Id = Guid.NewGuid(),
                CustomerReportSettingId = setting.Id,
                CustomerId = setting.CustomerId,
                ReportName = setting.ReportName,
                BlobLink = blobLink.ToString(),
                Format = setting.Format,
                IsSent = emailSent,
                GeneratedDate = DateTime.UtcNow
            };

            await unitOfWork.CustomerReportRepository.AddAsync(customerReport);
            await unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();

            logger.LogInformation("✅ Report '{ReportName}' generated and sent successfully. EmailSent={EmailSent}",
                setting.ReportName, emailSent);

            return true;
        }
        catch (Exception ex)
        {
            if (transaction != null)
                await transaction.RollbackAsync();

            logger.LogError(ex, "❌ Failed to generate report '{ReportName}'", setting.ReportName);
            return false;
        }
        finally
        {
            transaction?.Dispose();
        }
    }

    private bool IsDueForExecution(CustomerReportSetting setting, DateTime runDate)
    {
        return setting.Frequency switch
        {
            ReportFrequency.Daily => true,                              // Run every day at 12:00 UTC
            ReportFrequency.Weekly => runDate.DayOfWeek == DayOfWeek.Monday,  // Run only on Mondays at 12:00 UTC
            ReportFrequency.Monthly => runDate.Day == 1,                // Run only on 1st of month at 12:00 UTC
            _ => false
        };
    }

    private static (DateTime StartDate, DateTime EndDate) CalculateDateRange(ReportFrequency frequency, DateTime runDate)
    {
        var previousDay = runDate.Date.AddDays(-1);
        return frequency switch
        {
            // Daily: Generate report for yesterday
            ReportFrequency.Daily => (previousDay, previousDay.AddDays(1).AddSeconds(-1)),

            // Weekly: Generate report for previous calendar week (Monday to Sunday)
            ReportFrequency.Weekly => CalculatePreviousWeekRange(runDate),

            // Monthly: Generate report for previous month (1st to last day)
            ReportFrequency.Monthly => CalculatePreviousMonthRange(runDate),

            _ => (runDate.Date.AddDays(-1), runDate.Date.AddDays(-1))
        };
    }

    private static (DateTime StartDate, DateTime EndDate) CalculatePreviousWeekRange(DateTime runDate)
    {
        // Find the Monday of the current week
        int daysFromMonday = ((int)runDate.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        DateTime currentWeekMonday = runDate.Date.AddDays(-daysFromMonday);

        // Go back to the previous week's Monday and Sunday
        DateTime previousWeekMonday = currentWeekMonday.AddDays(-7);
        DateTime previousWeekSunday = previousWeekMonday.AddDays(6);

        return (previousWeekMonday, previousWeekSunday);
    }

    private static (DateTime StartDate, DateTime EndDate) CalculatePreviousMonthRange(DateTime runDate)
    {
        // Get the first day of the previous month
        DateTime firstDayOfCurrentMonth = new DateTime(runDate.Year, runDate.Month, 1);
        DateTime firstDayOfPreviousMonth = firstDayOfCurrentMonth.AddMonths(-1);

        // Get the last day of the previous month
        DateTime lastDayOfPreviousMonth = firstDayOfCurrentMonth.AddDays(-1);

        return (firstDayOfPreviousMonth, lastDayOfPreviousMonth);
    }

    private static (string Extension, string ContentType) GetFileFormatInfo(ReportFormat format)
    {
        return format switch
        {
            ReportFormat.PDF => ("pdf", "application/pdf"),
            ReportFormat.CSV => ("csv", "text/csv"),
            ReportFormat.EXCEL => ("xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"),
            _ => ("dat", "application/octet-stream")
        };
    }

    private ReportData CalculateReportData(ReportType[] reportTypes, MachineDto machineDto, List<HistoricalStats> stats)
    {
        if (stats == null || !stats.Any())
        {
            return new ReportData
            {
                MachineDetails = new Dictionary<string, string>
                {
                    { "SerialNumber", machineDto.SerialNumber },
                    { "MachineName", machineDto.MachineName },
                    { "MachineModel", machineDto.MachineModel }
                },
                Summary = new Dictionary<string, double>(),
                DailyRows = new List<Dictionary<string, object>>()
            };
        }

        var summary = new Dictionary<string, double>();
        foreach (var typeEnum in reportTypes)
        {
            var type = typeEnum.ToString();
            summary[type] = CalculateMetricValue(typeEnum, stats);
        }

        var grouped = stats.GroupBy(s => s.GeneratedDate.Date).OrderBy(g => g.Key);
        var dailyRows = new List<Dictionary<string, object>>();
      
        foreach (var group in grouped)
        {
            var row = new Dictionary<string, object>
            {
                ["Date"] = group.Key
            };

            foreach (var typeEnum in reportTypes)
            {
                var type = typeEnum.ToString();
                row[type] = CalculateMetricValue(typeEnum, group);
            }
            dailyRows.Add(row);
        }

        return new ReportData
        {
            MachineDetails = new Dictionary<string, string>
            {
                { "SerialNumber", machineDto.SerialNumber! },
                { "MachineName", machineDto.MachineName },
                { "MachineModel", machineDto.MachineModel }
            },
            Summary = summary,
            DailyRows = dailyRows
        };
    }

    private static double CalculateMetricValue(ReportType type, IEnumerable<HistoricalStats> stats)
    {
        return type.ToString() switch
        {
            "OEE" => stats.Average(s => s.OEE),
            "Availability" => stats.Average(s => s.Availability),
            "Performance" => stats.Average(s => s.Performance),
            "Quality" => stats.Average(s => s.Quality),
            "Utilization" => stats.Average(s => s.Utilization),
            "Downtime" => stats.Sum(s => s.DownTime),
            "RequiredQty" => stats.Sum(s => s.RequiredQty),
            "CompletedQty" => stats.Sum(s => s.QtyCompleted),
            "GoodQty" => stats.Sum(s => s.QtyGood),
            "BadQty" => stats.Sum(s => s.QtyBad),
            _ => 0
        };
    }
}
