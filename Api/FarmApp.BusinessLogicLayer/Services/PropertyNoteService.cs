using AutoMapper;
using FarmApp.BusinessLogicLayer.Services.Interfaces;
using FarmApp.DataAccessLayer.Repositories.Interfaces;
using FarmApp.Entities.Entity;
using FarmApp.Shared.Enums;
using FarmApp.Shared.Enums.PushNotification;
using FarmApp.Shared.Exceptions;
using FarmApp.Shared.Helpers;
using FarmApp.ViewModels.Media;
using FarmApp.ViewModels.PropertyNotes;
using static FarmApp.ViewModels.Notifications.NotificationAssetPaths;

namespace FarmApp.BusinessLogicLayer.Services;

public class PropertyNoteService : IPropertyNoteService
{
    private readonly IPropertyNoteRepository _propertyNoteRepository;
    private readonly IPropertyRepository _propertyRepository;
    private readonly IPropertyNoteMediaRepository _propertyNoteMediaRepository;
    private readonly IUserService _userService;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _fileStorageService;
    private readonly IPushNotificationQueueRepository _pushNotificationQueueRepository;
    
    public PropertyNoteService(
        IPropertyNoteRepository propertyNoteRepository, 
        IPropertyRepository propertyRepository,
        IUserService userService, 
        IMapper mapper,
        IFileStorageService fileStorageService,
        IPropertyNoteMediaRepository propertyNoteMediaRepository,
        IPushNotificationQueueRepository pushNotificationQueueRepository)
    {
        _propertyNoteRepository = propertyNoteRepository;
        _propertyRepository = propertyRepository;
        _userService = userService;
        _mapper = mapper;
        _fileStorageService = fileStorageService;
        _propertyNoteMediaRepository = propertyNoteMediaRepository;
        _pushNotificationQueueRepository = pushNotificationQueueRepository;
    }
    
    public async Task<PropertyNoteModel?> CreateAsync(CreatePropertyNoteModel model)
    {
        var currentUserId = _userService.GetCurrentUserId();
        if (currentUserId is null) throw new ServerException(Shared.Constants.Constants.ErrorMessages.USER_DOES_NOT_EXIST);

        var propertyEntity = await _propertyRepository.GetByIdAsync(model.PropertyId);
        if (propertyEntity is null || propertyEntity.UserId != currentUserId)
            throw new ServerException(Shared.Constants.Constants.ErrorMessages.PROPERTY_DOES_NOT_EXIST);

        var propertyNoteEntity = _mapper.Map<CreatePropertyNoteModel, PropertyNoteEntity>(model);
        
        await _propertyNoteRepository.CreateAsync(propertyNoteEntity);

        if (model.TempUploadResults != null && model.TempUploadResults.Any())
            await CommitMediaFiles(model.TempUploadResults, propertyNoteEntity);

        if (propertyNoteEntity.NotificationsEnabled && 
            propertyNoteEntity.NotifyBeforeEnd != null && 
            propertyNoteEntity.NotifyBeforeStart != null)
        {
            var pushBeforeStart = new PushNotificationQueueEntity()
            {
                PropertyNoteId = propertyNoteEntity.Id,
                Type = PushNotificationType.BeforePropertyNoteStart,
                UserId = $"user_{currentUserId}",
                SendAt = propertyNoteEntity.StartTime.AddMinutes(-(int)propertyNoteEntity.NotifyBeforeStart.Value),
                Status = PushNotificationStatus.Pending,
                CreatedAt = DateTime.Now
            };
            var pushBeforeEnd = new PushNotificationQueueEntity()
            {
                PropertyNoteId = propertyNoteEntity.Id,
                Type = PushNotificationType.BeforePropertyNoteEnd,
                UserId = $"user_{currentUserId}",
                SendAt = propertyNoteEntity.StartTime.AddMinutes(-(int)propertyNoteEntity.NotifyBeforeEnd.Value),
                Status = PushNotificationStatus.Pending,
                CreatedAt = DateTime.Now
            };

            await _pushNotificationQueueRepository.AddRangeAsync([pushBeforeStart,pushBeforeEnd]);
        }

        return _mapper.Map<PropertyNoteEntity, PropertyNoteModel>(propertyNoteEntity);
    }

    public async Task<PropertyNoteModel?> UpdateAsync(string id, UpdatePropertyNoteModel model)
    {
        var currentUserId = _userService.GetCurrentUserId();
        if (currentUserId is null) throw new ServerException(Shared.Constants.Constants.ErrorMessages.USER_DOES_NOT_EXIST);
        
        var propertyNoteEntity = await _propertyNoteRepository.GetByIdAsync(id, x => x.Property);
        if (propertyNoteEntity is null || propertyNoteEntity.Property.UserId != currentUserId) 
            throw new ServerException(Shared.Constants.Constants.ErrorMessages.PROPERTY_NOTE_DOES_NOT_EXISTS);
        
        propertyNoteEntity.Header = model.Header;
        propertyNoteEntity.Text = model.Text;
        propertyNoteEntity.StatusId = model.StatusId;
        propertyNoteEntity.StartTime = model.StartTime;
        propertyNoteEntity.EndTime = model.EndTime;
        propertyNoteEntity.NotificationsEnabled = model.NotificationsEnabled;
        propertyNoteEntity.NotifyBeforeStart = model.NotifyBeforeStart;
        propertyNoteEntity.NotifyBeforeEnd = model.NotifyBeforeEnd;

        await _propertyNoteRepository.UpdateAsync(propertyNoteEntity);

        if (model.NotificationsEnabled && model.NotifyBeforeStart != null && model.NotifyBeforeEnd != null)
        {
            var reminders = await _pushNotificationQueueRepository.GetAsync(x => x.PropertyNoteId == id);

            var pushBeforeStart = reminders.FirstOrDefault(x => x.Type == PushNotificationType.BeforePropertyNoteStart);
            var pushBeforeEnd = reminders.FirstOrDefault(x => x.Type == PushNotificationType.BeforePropertyNoteEnd);

            await UpdateReminder(pushBeforeStart, model.StartTime, model.NotifyBeforeStart, currentUserId, id, PushNotificationType.BeforePropertyNoteStart);
            await UpdateReminder(pushBeforeEnd, model.EndTime, model.NotifyBeforeEnd, currentUserId, id, PushNotificationType.BeforePropertyNoteEnd);
        }


        if (model.TempUploadResults != null && model.TempUploadResults.Any())
            await CommitMediaFiles(model.TempUploadResults, propertyNoteEntity);
        
        var propertyNoteModel = _mapper.Map<PropertyNoteEntity, PropertyNoteModel>(propertyNoteEntity);
        return propertyNoteModel;
    }

    public async Task DeleteAsync(string id)
    {
        var currentUserId = _userService.GetCurrentUserId();
        if (currentUserId is null)
            throw new ServerException(Shared.Constants.Constants.ErrorMessages.USER_DOES_NOT_EXIST);
        
        var propertyNoteEntity = await _propertyNoteRepository.GetByIdAsync(id, x=> x.Property);
        if (propertyNoteEntity is null || propertyNoteEntity.Property.UserId != currentUserId)
            throw new ServerException(Shared.Constants.Constants.ErrorMessages.PROPERTY_NOTE_DOES_NOT_EXISTS);

        var medias = await _propertyNoteMediaRepository.GetAsync(x => x.PropertyNoteId == id);
        foreach (var media in medias)
        {
            await _fileStorageService.DeleteAsync(media.RelativePath);
        }
        await _propertyNoteRepository.DeleteAsync(propertyNoteEntity);
    }

    public async Task<PropertyNoteModel?> GetByIdAsync(string propertyNoteId)
    {
        var currentUserId = _userService.GetCurrentUserId();
        if (currentUserId is null)
            throw new ServerException(Shared.Constants.Constants.ErrorMessages.USER_DOES_NOT_EXIST);
        
        var propertyNoteEntity = await _propertyNoteRepository.GetByIdAsync(propertyNoteId, x=> x.Property);
        if (propertyNoteEntity is null || propertyNoteEntity.Property.UserId != currentUserId) return null;
        
        return _mapper.Map<PropertyNoteEntity, PropertyNoteModel>(propertyNoteEntity);
    }

    public async Task<List<PropertyNoteModel>> GetAllByDateAsync(string propertyId, DateTime day)
    {
        var currentUserId = _userService.GetCurrentUserId();
        if (currentUserId is null)
            throw new ServerException(Shared.Constants.Constants.ErrorMessages.USER_DOES_NOT_EXIST);

        var propertyEntity = await _propertyRepository.GetByIdAsync(propertyId);
        if (propertyEntity is null || propertyEntity.UserId != currentUserId)
            throw new ServerException(Shared.Constants.Constants.ErrorMessages.PROPERTY_DOES_NOT_EXIST);
        
        var properties = await _propertyNoteRepository
            .GetAsync(x => x.PropertyId == propertyId && 
                           x.StartTime.Year == day.Year &&
                           x.StartTime.Month == day.Month &&
                           x.StartTime.Day == day.Day);
        
        return _mapper.Map<List<PropertyNoteEntity>, List<PropertyNoteModel>>(properties);
    }

    private async Task CommitMediaFiles(IEnumerable<TempUploadResult> temps, PropertyNoteEntity propertyNote)
    {
        var commitedFiles = await _fileStorageService.CommitAsync(temps, propertyNote.Id);

        if (propertyNote.PreviewMediaId == null && commitedFiles.Any())
            propertyNote.PreviewMediaId = commitedFiles.First().MediaId;

        foreach(var commit in commitedFiles)
        {

            await _propertyNoteMediaRepository.CreateAsync(new PropertyNoteMedia
            {
                Id = commit.MediaId,
                MediaType = MediaTypeResolver.ResolveFromExtension(commit.RelativePath),
                RelativePath = commit.RelativePath,
                ContentType = MediaTypeResolver.GetMimeTypeFromExtension(commit.RelativePath),
                PropertyNoteId = propertyNote.Id
            });
        }    
    }

    private async Task UpdateReminder(PushNotificationQueueEntity? reminder, DateTime targetTime,
        NotificationOffset? offset,
        string userId, string propertyNoteId, PushNotificationType type)
    {
        if (!offset.HasValue || offset == NotificationOffset.None)
        {
            if (reminder != null)
            {
                reminder.Status = PushNotificationStatus.Canceled;
                await _pushNotificationQueueRepository.UpdateAsync(reminder);
            }

            return;
        }

        var sendAt = targetTime.AddMinutes(-(int)offset.Value);

        var status = sendAt > DateTime.Now
            ? PushNotificationStatus.Pending
            : PushNotificationStatus.Canceled;

        if (reminder == null)
        {
            reminder = new PushNotificationQueueEntity
            {
                CreatedAt = DateTime.Now,
                PropertyNoteId = propertyNoteId,
                UserId = $"user_{userId}",
                Type = type,
                SendAt = sendAt,
                Status = status
            };

            await _pushNotificationQueueRepository.CreateAsync(reminder);
        }
        else
        {
            reminder.SendAt = sendAt;
            reminder.Status = status;

            await _pushNotificationQueueRepository.UpdateAsync(reminder);
        }
    }
}