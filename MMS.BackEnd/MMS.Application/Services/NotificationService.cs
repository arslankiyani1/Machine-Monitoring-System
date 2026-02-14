using System.Linq;

namespace MMS.Application.Services;

public class NotificationService(
    INotificationRepository notificationRepository,
    AutoMapperResult mapper,
    INotificationFirebaseService firebaseService,
    IUserService userService,
    IUnitOfWork unitOfWork) : INotificationService
{
    public async Task<ApiResponse<NotificationDto>> AddAsync(AddNotificationDto request, bool? supportshow)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            var createdNotifications = new List<Notification>();

            // ✅ Helper: Add notification for a user
            async Task<bool> AddNotificationForUser(UserModel user, Notification entity)
            {
                entity.UserId = user.UserId;
                var added = await notificationRepository.AddAsync(entity);

                if (added.IsRight)
                {
                    var notification = added.IfRight();
                    if (notification != null)
                    {
                        createdNotifications.Add(notification);
                        return true;
                    }
                }

                var error = added.IfLeft();
                Console.WriteLine($"❌ Failed to create notification for user {user.UserId}: {error?.Message ?? "Unknown error"}");
                return false;
            }

            // ✅ Helper: Send push to devices
            async Task SendToDevices(List<string> tokens, string title, string body)
            {
                if (tokens.Any())
                {
                    Console.WriteLine($"📱 Sending push to {tokens.Count} device(s)");
                    await firebaseService.SendToMultipleDevicesAsync(tokens, title, body);
                }
                else
                {
                    Console.WriteLine($"⚠️ No FCM tokens found - push notification not sent");
                }
            }

            // ✅ Helper: Get valid FCM tokens
            List<string> GetValidTokens(IEnumerable<UserModel> users)
            {
                const string separator = "||";

                return users
                    .Where(u => u.FcmTokens != null && u.FcmTokens.Count > 0)
                    .SelectMany(u => u.FcmTokens!)
                    .Where(token => !string.IsNullOrWhiteSpace(token))
                    .Select(token =>
                    {
                        var separatorIndex = token.IndexOf(separator, StringComparison.Ordinal);

                        if (separatorIndex >= 0)
                        {
                            var afterSeparator = token.Substring(separatorIndex + separator.Length).Trim();
                            if (afterSeparator.Length > 0)
                                return afterSeparator;

                            var beforeSeparator = token.Substring(0, separatorIndex).Trim();
                            return beforeSeparator.Length > 0 ? beforeSeparator : null;
                        }

                        return token.Trim();
                    })
                    .Where(token => !string.IsNullOrWhiteSpace(token))
                    .Distinct()
                    .ToList()!;
            }

            // ✅ Helper: Fetch users by string IDs
            async Task<List<UserModel>> FetchUsersByIds(List<string> userIds)
            {
                var validUserIds = userIds
                    .Where(id => Guid.TryParse(id, out _))
                    .Select(Guid.Parse)
                    .Distinct()
                    .ToList();

                if (validUserIds.Count == 0)
                    return new List<UserModel>();

                var userTasks = validUserIds.Select(async userId =>
                {
                    try
                    {
                        var userResponse = await userService.GetUserByIdAsync(userId);
                        if (userResponse?.StatusCode == 200 && userResponse.Data != null)
                        {
                            return userResponse.Data;
                        }
                        Console.WriteLine($"⚠️ User not found for UserId: {userId}");
                        return null;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error fetching user {userId}: {ex.Message}");
                        return null;
                    }
                });

                var results = await Task.WhenAll(userTasks);
                return results.Where(u => u != null).Cast<UserModel>().ToList();
            }

            // ✅ Helper: Fetch users by Guid list
            async Task<List<UserModel>> FetchUsersByGuids(List<Guid> userIds)
            {
                if (userIds.Count == 0)
                    return new List<UserModel>();

                var userTasks = userIds.Select(async userId =>
                {
                    try
                    {
                        var userResponse = await userService.GetUserByIdAsync(userId);
                        if (userResponse?.StatusCode == 200 && userResponse.Data != null)
                        {
                            return userResponse.Data;
                        }
                        Console.WriteLine($"⚠️ User not found for UserId: {userId}");
                        return null;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error fetching user {userId}: {ex.Message}");
                        return null;
                    }
                });

                var results = await Task.WhenAll(userTasks);
                return results.Where(u => u != null).Cast<UserModel>().ToList();
            }

            // ═══════════════════════════════════════════════════════════
            // ✅ MAIN LOGIC STARTS HERE
            // ═══════════════════════════════════════════════════════════

            // Option 1: Send to Recipients (list of user IDs as strings)
            if (request.Recipients != null && request.Recipients.Any())
            {
                var targetUsers = await FetchUsersByIds(request.Recipients);

                if (!targetUsers.Any())
                {
                    await transaction.RollbackAsync();
                    return new ApiResponse<NotificationDto>(
                        StatusCodes.Status404NotFound,
                        "No valid users found for the provided Recipients."
                    );
                }

                foreach (var user in targetUsers)
                {
                    var entity = mapper.Map<Notification>(request);
                    entity.ReadStatus = NotificationStatus.Unread;
                    await AddNotificationForUser(user, entity);
                }

                await unitOfWork.SaveChangesAsync();

                var tokens = GetValidTokens(targetUsers);
                await SendToDevices(tokens, request.Title, request.Body);
            }
            // Option 2: Send to UserIds (List<Guid?>)
            else if (request.UserIds != null && request.UserIds.Any(id => id.HasValue && id.Value != Guid.Empty))
            {
                var validUserIds = request.UserIds
                    .Where(id => id.HasValue && id.Value != Guid.Empty)
                    .Select(id => id!.Value)
                    .ToList();

                var targetUsers = await FetchUsersByGuids(validUserIds);

                if (!targetUsers.Any())
                {
                    await transaction.RollbackAsync();
                    return new ApiResponse<NotificationDto>(
                        StatusCodes.Status404NotFound,
                        "No valid users found for the provided UserIds."
                    );
                }

                foreach (var user in targetUsers)
                {
                    var entity = mapper.Map<Notification>(request);
                    entity.ReadStatus = NotificationStatus.Unread;
                    await AddNotificationForUser(user, entity);
                }

                await unitOfWork.SaveChangesAsync();

                var tokens = GetValidTokens(targetUsers);
                await SendToDevices(tokens, request.Title, request.Body);
            }
            // Option 3: Send to all users of a Customer
            else if (!string.IsNullOrWhiteSpace(request.CustomerId.ToString()))
            {
                var allUsersResponse = await userService.GetAllUsersforNotification(new UserQueryParameters());
                var customerUsers = (allUsersResponse.Data ?? new List<UserModel>())
                    .Where(u => u.CustomerIds != null && u.CustomerIds.Contains(request.CustomerId.ToString()))
                    .ToList();

                if (!customerUsers.Any())
                {
                    await transaction.RollbackAsync();
                    return new ApiResponse<NotificationDto>(
                        StatusCodes.Status404NotFound,
                        $"No users found for CustomerId: {request.CustomerId}"
                    );
                }

                foreach (var user in customerUsers)
                {
                    var entity = mapper.Map<Notification>(request);
                    entity.ReadStatus = NotificationStatus.Unread;
                    await AddNotificationForUser(user, entity);
                }

                await unitOfWork.SaveChangesAsync();
                await SendToDevices(GetValidTokens(customerUsers), request.Title, request.Body);
            }
            else
            {
                await transaction.RollbackAsync();
                return new ApiResponse<NotificationDto>(
                    StatusCodes.Status400BadRequest,
                    "Invalid notification request: Must specify Recipients, UserIds, or CustomerId."
                );
            }

            if (!createdNotifications.Any())
            {
                await transaction.RollbackAsync();
                return new ApiResponse<NotificationDto>(
                    StatusCodes.Status400BadRequest,
                    "Failed to create any notifications. Please check the logs for details."
                );
            }

            await transaction.CommitAsync();

            return new ApiResponse<NotificationDto>(
                StatusCodes.Status201Created,
                nameof(Notification) + ResponseMessages.Added,
                mapper.GetResult<Notification, NotificationDto>(createdNotifications.First())
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"❌ Error in AddAsync: {ex.Message}");
            return new ApiResponse<NotificationDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred: {ex.Message}"
            );
        }
    }

    // =========================================================================
    // ✅ FIXED: AddAlertAsync - Directly fetches users by ID
    // =========================================================================
    public async Task<ApiResponse<NotificationDto>> AddAlertAsync(AddNotificationDto request, CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            // ✅ Step 1: Convert string recipients to Guid UserIds
            var userIds = new List<Guid>();

            if (request.Recipients != null && request.Recipients.Any())
            {
                foreach (var recipient in request.Recipients)
                {
                    if (Guid.TryParse(recipient, out var userId))
                    {
                        userIds.Add(userId);
                    }
                    else
                    {
                        Console.WriteLine($"   ⚠️ Recipient '{recipient}' is not a valid Guid. Skipping.");
                    }
                }
            }

            if (!userIds.Any())
            {
                Console.WriteLine($"⚠️ No valid UserIds found for alert notification: {request.Title}");
                return new ApiResponse<NotificationDto>(
                    StatusCodes.Status400BadRequest,
                    "No valid user IDs found in recipients list."
                );
            }

            // =========================================================================
            // ✅ Step 2: DIRECTLY FETCH EACH USER BY ID (This fixes SuperAdmin issue!)
            // =========================================================================
            var targetUsers = new List<UserModel>();

            foreach (var userId in userIds)
            {
                try
                {
                    var userResult = await userService.GetUserByIdAsync(userId);
                    if (userResult?.StatusCode == 200 && userResult.Data != null)
                    {
                        targetUsers.Add(userResult.Data);
                    }
                    else
                    {
                        Console.WriteLine($"   ⚠️ User not found for UserId: {userId}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"   ❌ Error fetching user {userId}: {ex.Message}");
                }
            }

            if (!targetUsers.Any())
            {
                return new ApiResponse<NotificationDto>(
                    StatusCodes.Status404NotFound,
                    "No users found for the provided recipient IDs."
                );
            }
            // =========================================================================
            // ✅ Step 3: Create notification records for each user
            // =========================================================================
            var createdNotifications = new List<Notification>();

            foreach (var user in targetUsers)
            {
                
                    var notification = new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.UserId,
                        Title = request.Title,
                        Body = request.Body,
                        ReadStatus = NotificationStatus.Unread,
                        CustomerId = request.CustomerId,
                        MachineId = request.MachineId,
                        MachineName = request.MachineName,
                        Link = request.Link,
                        Priority = request.Priority,
                        NotificationTypes = request.NotificationTypes,
                        Recipients = request.Recipients!,
                        
                    };

                    var added = await notificationRepository.AddAsync(notification);
                    var result = added.IfRight();

                    if (result != null)
                    {
                        createdNotifications.Add(result);
                    }
                    else
                    {
                        var error = added.IfLeft();
                    }
            }

            await unitOfWork.SaveChangesAsync();

            // =========================================================================
            // ✅ Step 4: Get FCM tokens and send push notifications
            // =========================================================================
            var tokens = GetValidTokensFromUsers(targetUsers);

            if (tokens.Any())
                await firebaseService.SendToMultipleDevicesAsync(tokens, request.Title, request.Body);
            
            await transaction.CommitAsync();

            return new ApiResponse<NotificationDto>(
                StatusCodes.Status201Created,
                "Alert notification sent successfully",
                createdNotifications.Any()
                    ? mapper.Map<NotificationDto>(createdNotifications.First())
                    : null
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"❌ Error in AddAlertAsync: {ex.Message}");
            return new ApiResponse<NotificationDto>(
                StatusCodes.Status500InternalServerError,
                $"Error while sending alert notification: {ex.Message}"
            );
        }
    }

    // =========================================================================
    // ✅ Helper: Extract valid FCM tokens from users
    // =========================================================================
    private List<string> GetValidTokensFromUsers(IEnumerable<UserModel> users)
    {
        var tokens = new List<string>();

        foreach (var user in users)
        {

            if (user.FcmTokens == null || !user.FcmTokens.Any())
            {
                continue;
            }

            foreach (var fcmToken in user.FcmTokens)
            {
                if (string.IsNullOrWhiteSpace(fcmToken))
                    continue;

                string? extractedToken = null;

                // Format: "deviceId||token" or "token||deviceId"
                if (fcmToken.Contains("||"))
                {
                    var parts = fcmToken.Split(new[] { "||" }, StringSplitOptions.None);

                    // Try second part first (deviceId||token format)
                    if (parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]))
                    {
                        extractedToken = parts[1].Trim();
                    }
                    // Fallback to first part (token||deviceId format)
                    else if (parts.Length > 0 && !string.IsNullOrWhiteSpace(parts[0]))
                    {
                        extractedToken = parts[0].Trim();
                    }
                }
                else
                {
                    // Token stored directly without delimiter
                    extractedToken = fcmToken.Trim();
                }

                if (!string.IsNullOrWhiteSpace(extractedToken))
                {
                    tokens.Add(extractedToken);
                }
            }
        }

        var distinctTokens = tokens.Distinct().ToList();

        return distinctTokens;
    }

    #region Other Methods (unchanged)

    public async Task<ApiResponse<string>> DeleteAsync(Guid notificationId)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            var result = await notificationRepository.GetAsync(notificationId);

            return await result.MatchAsync(
                async notification =>
                {
                    await notificationRepository.DeleteAsync(notificationId);
                    await unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new ApiResponse<string>(
                        StatusCodes.Status200OK,
                        nameof(Notification) + ResponseMessages.Deleted
                    );
                },
                error =>
                {
                    var statusCode = error switch
                    {
                        EntityNotFound => StatusCodes.Status404NotFound,
                        EntitySoftDeleted => StatusCodes.Status410Gone,
                        _ => StatusCodes.Status400BadRequest
                    };

                    return Task.FromResult(new ApiResponse<string>(
                        statusCode,
                        $"Notification with ID {notificationId} not accessible: {error.GetType().Name}"
                    ));
                }
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<string>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred while deleting the notification: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<NotificationDto>> GetByIdAsync(Guid notificationId)
    {
        try
        {
            var result = await notificationRepository.GetAsync(notificationId);

            return await result.MatchAsync(
                async notification =>
                {
                    var dto = mapper.Map<NotificationDto>(notification);
                    return new ApiResponse<NotificationDto>(
                        StatusCodes.Status200OK,
                        nameof(Notification) + ResponseMessages.Get,
                        dto
                    );
                },
                error =>
                {
                    var statusCode = error switch
                    {
                        EntityNotFound => StatusCodes.Status404NotFound,
                        EntitySoftDeleted => StatusCodes.Status410Gone,
                        _ => StatusCodes.Status400BadRequest
                    };

                    return Task.FromResult(new ApiResponse<NotificationDto>(
                        statusCode,
                        $"Notification with ID {notificationId} not accessible: {error.GetType().Name}"
                    ));
                }
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<NotificationDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred while retrieving the notification: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<IEnumerable<NotificationDto>>> GetListAsync(
        PageParameters pageParameters,
        NotificationStatus? status)
    {
        try
        {
            Expression<Func<Notification, bool>> searchExpr = _ => true;
            var filters = new List<Expression<Func<Notification, bool>>>();
            if (status.HasValue)
                filters.Add(n => n.ReadStatus == status);

            var list = await notificationRepository.GetListAsync(
                pageParameters,
                searchExpr,
                filters,
                q => q.OrderByDescending(n => n.CreatedAt)
            );

            return new ApiResponse<IEnumerable<NotificationDto>>(
                StatusCodes.Status200OK,
                nameof(Notification) + ResponseMessages.GetAll,
                mapper.Map<IEnumerable<NotificationDto>>(list)
            );
        }
        catch (Exception ex)
        {
            return new ApiResponse<IEnumerable<NotificationDto>>(
                StatusCodes.Status500InternalServerError,
                "An error occurred while retrieving notifications: " + ex.Message
            );
        }
    }

    public async Task<ApiResponse<NotificationDto>> UpdateAsync(Guid notificationId, UpdateNotificationDto request)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();
        try
        {
            var result = await notificationRepository.GetAsync(notificationId);

            return await result.MatchAsync(
                async existing =>
                {
                    mapper.Map(request, existing);

                    var updated = await notificationRepository.UpdateAsync(notificationId, existing);
                    await unitOfWork.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new ApiResponse<NotificationDto>(
                        StatusCodes.Status200OK,
                        nameof(Notification) + ResponseMessages.Updated,
                         mapper.GetResult<Notification, NotificationDto>(updated)
                    );
                },
                error =>
                {
                    var statusCode = error switch
                    {
                        EntityNotFound => StatusCodes.Status404NotFound,
                        EntitySoftDeleted => StatusCodes.Status410Gone,
                        _ => StatusCodes.Status400BadRequest
                    };

                    return Task.FromResult(new ApiResponse<NotificationDto>(
                        statusCode,
                        $"Notification with ID {notificationId} not accessible: {error.GetType().Name}"
                    ));
                }
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<NotificationDto>(
                StatusCodes.Status500InternalServerError,
                $"An error occurred while updating the notification: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<NotificationDto>> MarkAsReadAsync(MarkNotificationReadDto dto)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync();

        try
        {
            var result = await notificationRepository.GetAsync(dto.NotificationId);

            return await result.MatchAsync(
                rightFunc: async notification =>
                {
                    if (notification.ReadAt is null)
                    {
                        notification.ReadAt = DateTime.UtcNow;
                        await notificationRepository.UpdateAsync(notification.Id, notification);
                        await unitOfWork.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        return new ApiResponse<NotificationDto>(
                            StatusCodes.Status200OK,
                            "Notification was already marked as read",
                            mapper.Map<NotificationDto>(notification)
                        );
                    }

                    var notificationDto = mapper.Map<NotificationDto>(notification);

                    return new ApiResponse<NotificationDto>(
                        StatusCodes.Status200OK,
                        "Notification marked as read",
                        notificationDto
                    );
                },
                leftFunc: async error => new ApiResponse<NotificationDto>(
                    StatusCodes.Status404NotFound,
                    $"Notification not found: {error.Message}"
                ));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new ApiResponse<NotificationDto>(
                StatusCodes.Status500InternalServerError,
                $"Failed to mark notification as read: {ex.Message}"
            );
        }
    }

    #endregion
}