using System;
using System.Linq;

namespace MMS.Application.Services;

public class SupportService(
    ISupportRepository supportRepository,
    IUnitOfWork unitOfWork,
    AutoMapperResult autoMapper,
    IBlobStorageService blobStorageService,
    IUserService userService,
    IUserContextService userContextService,
    INotificationService notificationService,
    IEmailService emailService) : ISupportService
{
    public async Task<ApiResponse<SupportTicketDto>> AddAsync(AddSupportTicketDto request)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            var support = autoMapper.Map<Support>(request);
            support.Status = SupportStatus.Pending;
            support.CustomerId = Guid.Empty;
            support.CustomerName = String.Empty;

            if (userContextService.UserId.HasValue)
            {
                var userResult = await userService.GetUserByIdAsync(userContextService.UserId.Value);
                if (userResult.Data != null)
                {
                    var name = $"{userResult.Data.FirstName} {userResult.Data.LastName}".Trim();

                    support.UserName = String.IsNullOrWhiteSpace(name) ? userResult.Data.Username : name;
                    support.UserId = userResult.Data.UserId;
                }
            }


            // Fill Machine + Customer Data
            if (request.MachineId.HasValue && request.MachineId!= Guid.Empty)
            {
                var machineResult = await unitOfWork.MachineRepository.GetAsync(request.MachineId.Value);

                if (machineResult.IsLeft)
                {
                    return new ApiResponse<SupportTicketDto>(
                        StatusCodes.Status404NotFound,
                        $"Machine with ID {request.MachineId} does not exist."
                    );
                }

                var machine = machineResult.IfRight();
                support.CustomerId = machine!.CustomerId;
                support.MachineName = machine.MachineName;
                support.MachineSerialNumber = machine.SerialNumber;
                // ✅ Fetch customer by CustomerId
                var customerResult = await unitOfWork.CustomerRepository.GetAsync(machine.CustomerId);

                if (customerResult.IsLeft)
                {
                    return new ApiResponse<SupportTicketDto>(
                        StatusCodes.Status404NotFound,
                        $"Customer with ID {machine.CustomerId} does not exist."
                    );
                }

                var customer = customerResult.IfRight();
                support.CustomerName = customer!.Name; 
            }

            if (request.Attachment is not null)
            {
                var uploadedUri = await blobStorageService.UploadAsync(request.Attachment);
                support.Urls = uploadedUri.AbsoluteUri;
            }
            else
            {
                support.Urls = string.Empty;
            }

            await unitOfWork.SupportRepository.AddAsync(support);
            await unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();

            // ✅ Notify Super Admin + Customer Admin
            //await SendSupportTicketAlertsAsync(support,support.CustomerId);

            return new ApiResponse<SupportTicketDto>(
                StatusCodes.Status201Created,
                nameof(Support) + ResponseMessages.Added,
                autoMapper.GetResult<Support, SupportTicketDto>(support)
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<SupportTicketDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }

    private async Task SendSupportTicketAlertsAsync(Support support, Guid? customerId)
    {
        var allUsersResponse = await userService.GetAllUsersAsync(new UserQueryParameters { Top = int.MaxValue},true); // true to show also super admin
        if (allUsersResponse.Data is null || !allUsersResponse.Data.Any())
            return;

        var allUsers = allUsersResponse.Data;

        var superAdmins = allUsers
            .Where(u => u.Role != null &&
                        u.Role.Equals(ApplicationRoles.RoleSystemAdmin, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var customerAdmins = allUsers
            .Where(u => u.Role != null &&
                        u.Role.Equals(ApplicationRoles.RoleCustomerAdmin, StringComparison.OrdinalIgnoreCase) &&
                        u.CustomerIds != null &&
                        u.CustomerIds.Contains(support.CustomerId.ToString()))
            .ToList();

        // ✅ Merge recipients safely (remove duplicates by UserId)
        var recipients = superAdmins
            .Concat(customerAdmins)
            .GroupBy(u => u.UserId)
            .Select(g => g.First())
            .ToList();

        if (!recipients.Any())
            return;

        foreach (var user in recipients)
        {
            // 🔹 1. Send email alert
            if (!string.IsNullOrEmpty(user.Email))
            {
                await emailService.SendSupportTicketCreatedEmailAsync(
                    toEmail: user.Email,
                    userName: user.Username,
                    supportId: support.Id,
                    customerId: support.CustomerId
                );
            }

            // 🔹 2. Save + send notification (DB + Push via FCM)
            if (user.UserId != Guid.Empty)
            {
                var notificationRequest = new AddNotificationDto(
                    Title: "New Support Ticket",
                    Body: $"A new support ticket has been created for customer {support.CustomerId}.",
                    Recipients: new List<string> { user.UserId.ToString() },
                    MachineId: null,
                    MachineName: null,
                    CustomerId: support.CustomerId,
                    Priority: null,
                    Link: null,
                    NotificationTypes: NotificationTypes.Alert,
                    UserIds: new List<Guid?> { user.UserId }
                );

                await notificationService.AddAsync(notificationRequest,true);
            }
        }

    }

    public async Task<ApiResponse<SupportTicketDto>> MarkAsResolvedAsync(Guid supportId)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            var supportResult = await unitOfWork.SupportRepository.GetAsync(supportId);

            if (supportResult.IsLeft)
            {
                return new ApiResponse<SupportTicketDto>(
                    StatusCodes.Status404NotFound,
                    $"Support ticket with ID {supportId} does not exist."
                );
            }

            var support = supportResult.IfRight();
            support!.Status = SupportStatus.Resolved;

            await unitOfWork.SupportRepository.UpdateAsync(supportId,support);
            await unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ApiResponse<SupportTicketDto>(
                StatusCodes.Status200OK,
                nameof(Support) + " marked as resolved.",
                autoMapper.GetResult<Support, SupportTicketDto>(support)
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<SupportTicketDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<string>> DeleteAsync(Guid id)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            var result = await supportRepository.GetAsync(id);

            if (result.IsLeft || result.Equals == null)
            {
                return new ApiResponse<string>(
                    StatusCodes.Status404NotFound,
                    $"Support record with ID {id} not found."
                );
            }

            await supportRepository.DeleteAsync(id);
            await unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ApiResponse<string>(
                StatusCodes.Status200OK,
                nameof(Support) + ResponseMessages.Deleted
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<string>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred while deleting support: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<SupportTicketDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var result = await supportRepository.GetAsync(id);

            return await result.MatchAsync(
                async support =>
                {
                    var dto = autoMapper.Map<SupportTicketDto>(support);

                    return new ApiResponse<SupportTicketDto>(
                        StatusCodes.Status200OK,
                        nameof(Support) + ResponseMessages.Get,
                        dto
                    );
                },
                error => Task.FromResult(new ApiResponse<SupportTicketDto>(
                    StatusCodes.Status404NotFound,
                    $"Support with ID {id} not found."
                ))
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<SupportTicketDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }


    public async Task<ApiResponse<IEnumerable<SupportTicketDto>>> GetListAsync(
    Guid customerId,
    PageParameters pageParameters)
    {
        try
        {
            var currentUserId = userContextService.UserId;
            var userResult = await userService.GetCurrentUserAsync();

            if (userResult.Data == null)
            {
                return new ApiResponse<IEnumerable<SupportTicketDto>>(
                    StatusCodes.Status403Forbidden,
                    "User could not be resolved."
                );
            }

            var userRole = userResult.Data.Role;

            

            var allowedCustomerIds = userContextService.CustomerIds?
                .Select(Guid.Parse)
                .ToList() ?? [];

            Expression<Func<Support, bool>> filter;

            // ✅ ✅ ✅ SYSTEM ADMIN → ALL DATA, OPTIONAL customerId FILTER
            if (!string.IsNullOrEmpty(userRole) &&
                userRole.Equals(ApplicationRoles.RoleSystemAdmin, StringComparison.OrdinalIgnoreCase))
            {
                if (customerId != Guid.Empty)
                    filter = s => s.CustomerId == customerId;   // ✅ FIXED
                else
                    filter = s => true;
            }

            // ✅ CUSTOMER ADMIN → ONLY THEIR ALLOWED CUSTOMERS + OPTIONAL customerId
            else if (!string.IsNullOrEmpty(userRole) &&
                userRole.Equals(ApplicationRoles.RoleCustomerAdmin, StringComparison.OrdinalIgnoreCase))
            {
                if (!allowedCustomerIds.Any())
                {
                    return new ApiResponse<IEnumerable<SupportTicketDto>>(
                        StatusCodes.Status403Forbidden,
                        "No customer access assigned to this user."
                    );
                }

                // ✅ customerId provided → must be within allowed list
                if (customerId != Guid.Empty)
                {
                    if (!allowedCustomerIds.Contains(customerId))
                    {
                        return new ApiResponse<IEnumerable<SupportTicketDto>>(
                            StatusCodes.Status403Forbidden,
                            "You do not have access to this customer."
                        );
                    }

                    filter = s => s.CustomerId == customerId;   // ✅ FIXED
                }
                else
                {
                    filter = s => allowedCustomerIds.Contains(s.CustomerId);  // ✅ ALL assigned customers
                }
            }

            // ✅ NORMAL USER → ONLY THEIR OWN TICKETS + OPTIONAL customerId
            else
            {
                if (!currentUserId.HasValue)
                {
                    return new ApiResponse<IEnumerable<SupportTicketDto>>(
                        StatusCodes.Status401Unauthorized,
                        "User is not authenticated."
                    );
                }

                if (customerId != Guid.Empty)
                {
                    filter = s =>
                        s.UserId == currentUserId.Value &&
                        s.CustomerId == customerId;   // ✅ FIXED
                }
                else
                {
                    filter = s => s.UserId == currentUserId.Value;
                }
            }

            var supports = await supportRepository.GetListAsync(
                pageParameters,
                filter,
                [],
                q => q.OrderByDescending(s => s.CreatedAt)
            );

            var dtoList = autoMapper.Map<IEnumerable<SupportTicketDto>>(supports);

            return new ApiResponse<IEnumerable<SupportTicketDto>>(
                StatusCodes.Status200OK,
                nameof(Support) + ResponseMessages.GetAll,
                dtoList
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<SupportTicketDto>>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<SupportTicketDto>> UpdateAsync(Guid id, UpdateSupportTicketDto request)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            var result = await supportRepository.GetAsync(id);

            return await result.MatchAsync(
                async support =>
                {
                    autoMapper.Map(request, support);
                    await unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new ApiResponse<SupportTicketDto>(
                        StatusCodes.Status200OK,
                        nameof(Support) + ResponseMessages.Updated,
                        autoMapper.GetResult<Support, SupportTicketDto>(support)
                    );
                },
                error => Task.FromResult(new ApiResponse<SupportTicketDto>(
                    StatusCodes.Status404NotFound, 
                    $"Support with ID {id} not found."
                ))
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<SupportTicketDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }
}
