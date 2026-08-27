using System.Linq.Expressions;
using AutoMapper;
using FarmApp.BusinessLogicLayer.Services;
using FarmApp.BusinessLogicLayer.Services.Interfaces;
using FarmApp.DataAccessLayer.Repositories.Interfaces;
using FarmApp.Entities.Entity;
using FarmApp.Shared.Enums;
using FarmApp.Shared.Exceptions;
using FarmApp.ViewModels.Media;
using FarmApp.ViewModels.PropertyNotes;
using Moq;

namespace FarmApp.Tests;

[TestFixture]
public class PropertyNoteServiceTests
{
    private Mock<IPropertyNoteRepository> _propertyNoteRepoMock;
    private Mock<IPropertyRepository> _propertyRepoMock;
    private Mock<IPropertyNoteMediaRepository> _propertyNoteMediaRepoMock;
    private Mock<IUserService> _userServiceMock;
    private Mock<IMapper> _mapperMock;
    private Mock<IFileStorageService> _fileStorageServiceMock;
    private Mock<IPushNotificationQueueRepository> _pushNotificationQueueRepoMock;
    private PropertyNoteService _service;

    private const string UserId = "user-1";
    private const string PropertyId = "property-1";
    private const string NoteId = "note-1";

    [SetUp]
    public void SetUp()
    {
        _propertyNoteRepoMock = new Mock<IPropertyNoteRepository>();
        _propertyRepoMock = new Mock<IPropertyRepository>();
        _propertyNoteMediaRepoMock = new Mock<IPropertyNoteMediaRepository>();
        _userServiceMock = new Mock<IUserService>();
        _mapperMock = new Mock<IMapper>();
        _fileStorageServiceMock = new Mock<IFileStorageService>();
        _pushNotificationQueueRepoMock = new Mock<IPushNotificationQueueRepository>();

        _service = new PropertyNoteService(
            _propertyNoteRepoMock.Object,
            _propertyRepoMock.Object,
            _userServiceMock.Object,
            _mapperMock.Object,
            _fileStorageServiceMock.Object,
            _propertyNoteMediaRepoMock.Object,
            _pushNotificationQueueRepoMock.Object);
    }

    #region CreateAsync

    [Test]
    public void CreateAsync_UserNotFound_ThrowsServerException()
    {
        _userServiceMock.Setup(x => x.GetCurrentUserId()).Returns((string?)null);

        var model = new CreatePropertyNoteModel { PropertyId = PropertyId };

        Assert.ThrowsAsync<ServerException>(() => _service.CreateAsync(model));
    }

    [Test]
    public void CreateAsync_PropertyNotFound_ThrowsServerException()
    {
        _userServiceMock.Setup(x => x.GetCurrentUserId()).Returns(UserId);
        _propertyRepoMock.Setup(x => x.GetByIdAsync(PropertyId)).ReturnsAsync((PropertyEntity?)null);

        var model = new CreatePropertyNoteModel { PropertyId = PropertyId };

        Assert.ThrowsAsync<ServerException>(() => _service.CreateAsync(model));
    }

    [Test]
    public void CreateAsync_PropertyBelongsToOtherUser_ThrowsServerException()
    {
        _userServiceMock.Setup(x => x.GetCurrentUserId()).Returns(UserId);
        _propertyRepoMock.Setup(x => x.GetByIdAsync(PropertyId))
            .ReturnsAsync(new PropertyEntity { Id = PropertyId, UserId = "other-user" });

        var model = new CreatePropertyNoteModel { PropertyId = PropertyId };

        Assert.ThrowsAsync<ServerException>(() => _service.CreateAsync(model));
    }

    [Test]
    public async Task CreateAsync_ValidModel_CreatesNoteAndReturnsModel()
    {
        _userServiceMock.Setup(x => x.GetCurrentUserId()).Returns(UserId);
        _propertyRepoMock.Setup(x => x.GetByIdAsync(PropertyId))
            .ReturnsAsync(new PropertyEntity { Id = PropertyId, UserId = UserId });

        var createModel = new CreatePropertyNoteModel
        {
            Header = "Test Note",
            Text = "Description text",
            StartTime = DateTime.Now.AddDays(1),
            EndTime = DateTime.Now.AddDays(2),
            PropertyId = PropertyId
        };

        var entity = new PropertyNoteEntity { Id = NoteId, PropertyId = PropertyId };
        var expectedModel = new PropertyNoteModel { Id = NoteId, Header = "Test Note" };

        _mapperMock.Setup(x => x.Map<CreatePropertyNoteModel, PropertyNoteEntity>(createModel)).Returns(entity);
        _mapperMock.Setup(x => x.Map<PropertyNoteEntity, PropertyNoteModel>(entity)).Returns(expectedModel);

        var result = await _service.CreateAsync(createModel);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(NoteId));
        _propertyNoteRepoMock.Verify(x => x.CreateAsync(entity), Times.Once);
    }

    [Test]
    public async Task CreateAsync_WithMediaFiles_CommitsMedia()
    {
        _userServiceMock.Setup(x => x.GetCurrentUserId()).Returns(UserId);
        _propertyRepoMock.Setup(x => x.GetByIdAsync(PropertyId))
            .ReturnsAsync(new PropertyEntity { Id = PropertyId, UserId = UserId });

        var temps = new List<TempUploadResult>
        {
            new() { TempMediaId = "temp-1", TempExtension = ".jpg" }
        };

        var createModel = new CreatePropertyNoteModel
        {
            Header = "Note with media",
            Text = "Description text",
            PropertyId = PropertyId,
            TempUploadResults = temps
        };

        var entity = new PropertyNoteEntity { Id = NoteId, PropertyId = PropertyId };
        _mapperMock.Setup(x => x.Map<CreatePropertyNoteModel, PropertyNoteEntity>(createModel)).Returns(entity);
        _mapperMock.Setup(x => x.Map<PropertyNoteEntity, PropertyNoteModel>(entity))
            .Returns(new PropertyNoteModel { Id = NoteId });

        _fileStorageServiceMock.Setup(x => x.CommitAsync(temps, NoteId))
            .ReturnsAsync(new List<CommitResult>
            {
                new() { MediaId = "media-1", RelativePath = "path/file.jpg" }
            });

        await _service.CreateAsync(createModel);

        _fileStorageServiceMock.Verify(x => x.CommitAsync(temps, NoteId), Times.Once);
        _propertyNoteMediaRepoMock.Verify(x => x.CreateAsync(It.IsAny<PropertyNoteMedia>()), Times.Once);
    }

    [Test]
    public async Task CreateAsync_WithNotifications_QueuesPushNotifications()
    {
        _userServiceMock.Setup(x => x.GetCurrentUserId()).Returns(UserId);
        _propertyRepoMock.Setup(x => x.GetByIdAsync(PropertyId))
            .ReturnsAsync(new PropertyEntity { Id = PropertyId, UserId = UserId });

        var createModel = new CreatePropertyNoteModel
        {
            Header = "Notification note",
            Text = "Description text",
            PropertyId = PropertyId,
            NotificationsEnabled = true,
            NotifyBeforeStart = NotificationOffset.ThirtyMinutes,
            NotifyBeforeEnd = NotificationOffset.FifteenMinutes
        };

        var entity = new PropertyNoteEntity
        {
            Id = NoteId,
            PropertyId = PropertyId,
            NotificationsEnabled = true,
            NotifyBeforeStart = NotificationOffset.ThirtyMinutes,
            NotifyBeforeEnd = NotificationOffset.FifteenMinutes,
            StartTime = DateTime.Now.AddDays(1)
        };

        _mapperMock.Setup(x => x.Map<CreatePropertyNoteModel, PropertyNoteEntity>(createModel)).Returns(entity);
        _mapperMock.Setup(x => x.Map<PropertyNoteEntity, PropertyNoteModel>(entity))
            .Returns(new PropertyNoteModel { Id = NoteId });

        await _service.CreateAsync(createModel);

        _pushNotificationQueueRepoMock.Verify(
            x => x.AddRangeAsync(It.Is<IEnumerable<PushNotificationQueueEntity>>(items => items.Count() == 2)),
            Times.Once);
    }

    [Test]
    public async Task CreateAsync_NotificationsDisabled_DoesNotQueuePush()
    {
        _userServiceMock.Setup(x => x.GetCurrentUserId()).Returns(UserId);
        _propertyRepoMock.Setup(x => x.GetByIdAsync(PropertyId))
            .ReturnsAsync(new PropertyEntity { Id = PropertyId, UserId = UserId });

        var createModel = new CreatePropertyNoteModel
        {
            Header = "No notifications",
            Text = "Description text",
            PropertyId = PropertyId,
            NotificationsEnabled = false
        };

        var entity = new PropertyNoteEntity
        {
            Id = NoteId,
            PropertyId = PropertyId,
            NotificationsEnabled = false
        };

        _mapperMock.Setup(x => x.Map<CreatePropertyNoteModel, PropertyNoteEntity>(createModel)).Returns(entity);
        _mapperMock.Setup(x => x.Map<PropertyNoteEntity, PropertyNoteModel>(entity))
            .Returns(new PropertyNoteModel { Id = NoteId });

        await _service.CreateAsync(createModel);

        _pushNotificationQueueRepoMock.Verify(
            x => x.AddRangeAsync(It.IsAny<IEnumerable<PushNotificationQueueEntity>>()),
            Times.Never);
    }

    #endregion

    #region UpdateAsync

    [Test]
    public void UpdateAsync_UserNotFound_ThrowsServerException()
    {
        _userServiceMock.Setup(x => x.GetCurrentUserId()).Returns((string?)null);

        Assert.ThrowsAsync<ServerException>(() =>
            _service.UpdateAsync(NoteId, new UpdatePropertyNoteModel()));
    }

    [Test]
    public void UpdateAsync_NoteNotFound_ThrowsServerException()
    {
        _userServiceMock.Setup(x => x.GetCurrentUserId()).Returns(UserId);
        _propertyNoteRepoMock
            .Setup(x => x.GetByIdAsync(NoteId, It.IsAny<Expression<Func<PropertyNoteEntity, object>>[]>()))
            .ReturnsAsync((PropertyNoteEntity?)null);

        Assert.ThrowsAsync<ServerException>(() =>
            _service.UpdateAsync(NoteId, new UpdatePropertyNoteModel()));
    }

    [Test]
    public void UpdateAsync_NoteBelongsToOtherUser_ThrowsServerException()
    {
        _userServiceMock.Setup(x => x.GetCurrentUserId()).Returns(UserId);
        _propertyNoteRepoMock
            .Setup(x => x.GetByIdAsync(NoteId, It.IsAny<Expression<Func<PropertyNoteEntity, object>>[]>()))
            .ReturnsAsync(new PropertyNoteEntity
            {
                Id = NoteId,
                Property = new PropertyEntity { UserId = "other-user" }
            });

        Assert.ThrowsAsync<ServerException>(() =>
            _service.UpdateAsync(NoteId, new UpdatePropertyNoteModel()));
    }

    [Test]
    public async Task UpdateAsync_ValidModel_UpdatesFieldsAndReturnsModel()
    {
        _userServiceMock.Setup(x => x.GetCurrentUserId()).Returns(UserId);

        var existingEntity = new PropertyNoteEntity
        {
            Id = NoteId,
            Header = "Old Header",
            Text = "Old Text",
            Property = new PropertyEntity { UserId = UserId }
        };

        _propertyNoteRepoMock
            .Setup(x => x.GetByIdAsync(NoteId, It.IsAny<Expression<Func<PropertyNoteEntity, object>>[]>()))
            .ReturnsAsync(existingEntity);

        var updateModel = new UpdatePropertyNoteModel
        {
            Header = "New Header",
            Text = "New Text",
            StartTime = DateTime.Now.AddDays(1),
            EndTime = DateTime.Now.AddDays(2),
            StatusId = 1
        };

        var expectedModel = new PropertyNoteModel { Id = NoteId, Header = "New Header" };
        _mapperMock.Setup(x => x.Map<PropertyNoteEntity, PropertyNoteModel>(existingEntity))
            .Returns(expectedModel);

        var result = await _service.UpdateAsync(NoteId, updateModel);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Header, Is.EqualTo("New Header"));
        Assert.That(existingEntity.Header, Is.EqualTo("New Header"));
        Assert.That(existingEntity.Text, Is.EqualTo("New Text"));
        Assert.That(existingEntity.StatusId, Is.EqualTo(1));
        _propertyNoteRepoMock.Verify(x => x.UpdateAsync(existingEntity), Times.Once);
    }

    [Test]
    public async Task UpdateAsync_WithMediaFiles_CommitsMedia()
    {
        _userServiceMock.Setup(x => x.GetCurrentUserId()).Returns(UserId);

        var existingEntity = new PropertyNoteEntity
        {
            Id = NoteId,
            Property = new PropertyEntity { UserId = UserId }
        };

        _propertyNoteRepoMock
            .Setup(x => x.GetByIdAsync(NoteId, It.IsAny<Expression<Func<PropertyNoteEntity, object>>[]>()))
            .ReturnsAsync(existingEntity);

        var temps = new List<TempUploadResult>
        {
            new() { TempMediaId = "temp-1", TempExtension = ".png" }
        };

        var updateModel = new UpdatePropertyNoteModel
        {
            Header = "Updated",
            Text = "Updated text",
            TempUploadResults = temps
        };

        _fileStorageServiceMock.Setup(x => x.CommitAsync(temps, NoteId))
            .ReturnsAsync(new List<CommitResult>
            {
                new() { MediaId = "media-2", RelativePath = "path/file.png" }
            });

        _mapperMock.Setup(x => x.Map<PropertyNoteEntity, PropertyNoteModel>(existingEntity))
            .Returns(new PropertyNoteModel { Id = NoteId });

        await _service.UpdateAsync(NoteId, updateModel);

        _fileStorageServiceMock.Verify(x => x.CommitAsync(temps, NoteId), Times.Once);
    }

    #endregion

    #region DeleteAsync

    [Test]
    public void DeleteAsync_UserNotFound_ThrowsServerException()
    {
        _userServiceMock.Setup(x => x.GetCurrentUserId()).Returns((string?)null);

        Assert.ThrowsAsync<ServerException>(() => _service.DeleteAsync(NoteId));
    }

    [Test]
    public void DeleteAsync_NoteNotFound_ThrowsServerException()
    {
        _userServiceMock.Setup(x => x.GetCurrentUserId()).Returns(UserId);
        _propertyNoteRepoMock
            .Setup(x => x.GetByIdAsync(NoteId, It.IsAny<Expression<Func<PropertyNoteEntity, object>>[]>()))
            .ReturnsAsync((PropertyNoteEntity?)null);

        Assert.ThrowsAsync<ServerException>(() => _service.DeleteAsync(NoteId));
    }

    [Test]
    public void DeleteAsync_NoteBelongsToOtherUser_ThrowsServerException()
    {
        _userServiceMock.Setup(x => x.GetCurrentUserId()).Returns(UserId);
        _propertyNoteRepoMock
            .Setup(x => x.GetByIdAsync(NoteId, It.IsAny<Expression<Func<PropertyNoteEntity, object>>[]>()))
            .ReturnsAsync(new PropertyNoteEntity
            {
                Id = NoteId,
                Property = new PropertyEntity { UserId = "other-user" }
            });

        Assert.ThrowsAsync<ServerException>(() => _service.DeleteAsync(NoteId));
    }

    [Test]
    public async Task DeleteAsync_ValidNote_DeletesNoteAndMedia()
    {
        _userServiceMock.Setup(x => x.GetCurrentUserId()).Returns(UserId);

        var entity = new PropertyNoteEntity
        {
            Id = NoteId,
            Property = new PropertyEntity { UserId = UserId }
        };

        _propertyNoteRepoMock
            .Setup(x => x.GetByIdAsync(NoteId, It.IsAny<Expression<Func<PropertyNoteEntity, object>>[]>()))
            .ReturnsAsync(entity);

        var medias = new List<PropertyNoteMedia>
        {
            new() { Id = "m1", RelativePath = "path/img1.jpg", PropertyNoteId = NoteId },
            new() { Id = "m2", RelativePath = "path/img2.jpg", PropertyNoteId = NoteId }
        };

        _propertyNoteMediaRepoMock
            .Setup(x => x.GetAsync(It.IsAny<Expression<Func<PropertyNoteMedia, bool>>>()))
            .ReturnsAsync(medias);

        await _service.DeleteAsync(NoteId);

        _fileStorageServiceMock.Verify(x => x.DeleteAsync("path/img1.jpg", default), Times.Once);
        _fileStorageServiceMock.Verify(x => x.DeleteAsync("path/img2.jpg", default), Times.Once);
        _propertyNoteRepoMock.Verify(x => x.DeleteAsync(entity), Times.Once);
    }

    [Test]
    public async Task DeleteAsync_NoteWithNoMedia_DeletesOnlyNote()
    {
        _userServiceMock.Setup(x => x.GetCurrentUserId()).Returns(UserId);

        var entity = new PropertyNoteEntity
        {
            Id = NoteId,
            Property = new PropertyEntity { UserId = UserId }
        };

        _propertyNoteRepoMock
            .Setup(x => x.GetByIdAsync(NoteId, It.IsAny<Expression<Func<PropertyNoteEntity, object>>[]>()))
            .ReturnsAsync(entity);

        _propertyNoteMediaRepoMock
            .Setup(x => x.GetAsync(It.IsAny<Expression<Func<PropertyNoteMedia, bool>>>()))
            .ReturnsAsync(new List<PropertyNoteMedia>());

        await _service.DeleteAsync(NoteId);

        _fileStorageServiceMock.Verify(x => x.DeleteAsync(It.IsAny<string>(), default), Times.Never);
        _propertyNoteRepoMock.Verify(x => x.DeleteAsync(entity), Times.Once);
    }

    #endregion

    #region GetByIdAsync

    [Test]
    public void GetByIdAsync_UserNotFound_ThrowsServerException()
    {
        _userServiceMock.Setup(x => x.GetCurrentUserId()).Returns((string?)null);

        Assert.ThrowsAsync<ServerException>(() => _service.GetByIdAsync(NoteId));
    }

    [Test]
    public async Task GetByIdAsync_NoteNotFound_ReturnsNull()
    {
        _userServiceMock.Setup(x => x.GetCurrentUserId()).Returns(UserId);
        _propertyNoteRepoMock
            .Setup(x => x.GetByIdAsync(NoteId, It.IsAny<Expression<Func<PropertyNoteEntity, object>>[]>()))
            .ReturnsAsync((PropertyNoteEntity?)null);

        var result = await _service.GetByIdAsync(NoteId);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetByIdAsync_NoteBelongsToOtherUser_ReturnsNull()
    {
        _userServiceMock.Setup(x => x.GetCurrentUserId()).Returns(UserId);
        _propertyNoteRepoMock
            .Setup(x => x.GetByIdAsync(NoteId, It.IsAny<Expression<Func<PropertyNoteEntity, object>>[]>()))
            .ReturnsAsync(new PropertyNoteEntity
            {
                Id = NoteId,
                Property = new PropertyEntity { UserId = "other-user" }
            });

        var result = await _service.GetByIdAsync(NoteId);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetByIdAsync_ValidNote_ReturnsMappedModel()
    {
        _userServiceMock.Setup(x => x.GetCurrentUserId()).Returns(UserId);

        var entity = new PropertyNoteEntity
        {
            Id = NoteId,
            Property = new PropertyEntity { UserId = UserId }
        };

        _propertyNoteRepoMock
            .Setup(x => x.GetByIdAsync(NoteId, It.IsAny<Expression<Func<PropertyNoteEntity, object>>[]>()))
            .ReturnsAsync(entity);

        var expectedModel = new PropertyNoteModel { Id = NoteId, Header = "Test" };
        _mapperMock.Setup(x => x.Map<PropertyNoteEntity, PropertyNoteModel>(entity)).Returns(expectedModel);

        var result = await _service.GetByIdAsync(NoteId);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(NoteId));
    }

    #endregion

    #region GetAllByDateAsync

    [Test]
    public void GetAllByDateAsync_UserNotFound_ThrowsServerException()
    {
        _userServiceMock.Setup(x => x.GetCurrentUserId()).Returns((string?)null);

        Assert.ThrowsAsync<ServerException>(() =>
            _service.GetAllByDateAsync(PropertyId, DateTime.Today));
    }

    [Test]
    public void GetAllByDateAsync_PropertyNotFound_ThrowsServerException()
    {
        _userServiceMock.Setup(x => x.GetCurrentUserId()).Returns(UserId);
        _propertyRepoMock.Setup(x => x.GetByIdAsync(PropertyId)).ReturnsAsync((PropertyEntity?)null);

        Assert.ThrowsAsync<ServerException>(() =>
            _service.GetAllByDateAsync(PropertyId, DateTime.Today));
    }

    [Test]
    public void GetAllByDateAsync_PropertyBelongsToOtherUser_ThrowsServerException()
    {
        _userServiceMock.Setup(x => x.GetCurrentUserId()).Returns(UserId);
        _propertyRepoMock.Setup(x => x.GetByIdAsync(PropertyId))
            .ReturnsAsync(new PropertyEntity { Id = PropertyId, UserId = "other-user" });

        Assert.ThrowsAsync<ServerException>(() =>
            _service.GetAllByDateAsync(PropertyId, DateTime.Today));
    }

    [Test]
    public async Task GetAllByDateAsync_ValidRequest_ReturnsMappedNotes()
    {
        _userServiceMock.Setup(x => x.GetCurrentUserId()).Returns(UserId);
        _propertyRepoMock.Setup(x => x.GetByIdAsync(PropertyId))
            .ReturnsAsync(new PropertyEntity { Id = PropertyId, UserId = UserId });

        var entities = new List<PropertyNoteEntity>
        {
            new() { Id = "n1", PropertyId = PropertyId },
            new() { Id = "n2", PropertyId = PropertyId }
        };

        _propertyNoteRepoMock
            .Setup(x => x.GetAsync(It.IsAny<Expression<Func<PropertyNoteEntity, bool>>>()))
            .ReturnsAsync(entities);

        var expectedModels = new List<PropertyNoteModel>
        {
            new() { Id = "n1" },
            new() { Id = "n2" }
        };

        _mapperMock.Setup(x => x.Map<List<PropertyNoteEntity>, List<PropertyNoteModel>>(entities))
            .Returns(expectedModels);

        var result = await _service.GetAllByDateAsync(PropertyId, DateTime.Today);

        Assert.That(result, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetAllByDateAsync_NoNotesForDate_ReturnsEmptyList()
    {
        _userServiceMock.Setup(x => x.GetCurrentUserId()).Returns(UserId);
        _propertyRepoMock.Setup(x => x.GetByIdAsync(PropertyId))
            .ReturnsAsync(new PropertyEntity { Id = PropertyId, UserId = UserId });

        _propertyNoteRepoMock
            .Setup(x => x.GetAsync(It.IsAny<Expression<Func<PropertyNoteEntity, bool>>>()))
            .ReturnsAsync(new List<PropertyNoteEntity>());

        _mapperMock
            .Setup(x => x.Map<List<PropertyNoteEntity>, List<PropertyNoteModel>>(It.IsAny<List<PropertyNoteEntity>>()))
            .Returns(new List<PropertyNoteModel>());

        var result = await _service.GetAllByDateAsync(PropertyId, DateTime.Today);

        Assert.That(result, Is.Empty);
    }

    #endregion

    #region PreviewModel Validation Attributes

    [Test]
    public void PropertyNotePreviewModel_HeaderRequired_ValidationFails()
    {
        var model = new PropertyNotePreviewModel { Header = null!, Text = "Valid description text" };
        var results = ValidateModel(model);

        Assert.That(results, Has.Some.Matches<System.ComponentModel.DataAnnotations.ValidationResult>(
            r => r.ErrorMessage!.Contains("Name")));
    }

    [Test]
    public void PropertyNotePreviewModel_HeaderTooShort_ValidationFails()
    {
        var model = new PropertyNotePreviewModel { Header = "AB", Text = "Valid description text" };
        var results = ValidateModel(model);

        Assert.That(results, Has.Some.Matches<System.ComponentModel.DataAnnotations.ValidationResult>(
            r => r.ErrorMessage!.Contains("3 characters")));
    }

    [Test]
    public void PropertyNotePreviewModel_TextRequired_ValidationFails()
    {
        var model = new PropertyNotePreviewModel { Header = "Valid Header", Text = null! };
        var results = ValidateModel(model);

        Assert.That(results, Has.Some.Matches<System.ComponentModel.DataAnnotations.ValidationResult>(
            r => r.ErrorMessage!.Contains("Description")));
    }

    [Test]
    public void PropertyNotePreviewModel_TextTooShort_ValidationFails()
    {
        var model = new PropertyNotePreviewModel { Header = "Valid Header", Text = "Short" };
        var results = ValidateModel(model);

        Assert.That(results, Has.Some.Matches<System.ComponentModel.DataAnnotations.ValidationResult>(
            r => r.ErrorMessage!.Contains("10 characters")));
    }

    [Test]
    public void PropertyNotePreviewModel_ValidFields_NoValidationErrors()
    {
        var model = new PropertyNotePreviewModel
        {
            Header = "Valid Header",
            Text = "This is a valid description with enough chars"
        };
        var results = ValidateModel(model);

        Assert.That(results, Is.Empty);
    }

    #endregion

    private static List<System.ComponentModel.DataAnnotations.ValidationResult> ValidateModel(object model)
    {
        var context = new System.ComponentModel.DataAnnotations.ValidationContext(model);
        var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        System.ComponentModel.DataAnnotations.Validator.TryValidateObject(model, context, results, true);
        return results;
    }
}
