using AutoMapper;
using FarmApp.BusinessLogicLayer.Services.Interfaces;
using FarmApp.DataAccessLayer.Repositories.Interfaces;
using FarmApp.Entities.Entity;
using FarmApp.Shared.Exceptions;
using FarmApp.ViewModels.Properties;

namespace FarmApp.BusinessLogicLayer.Services;

public class PropertyService : IPropertyService
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;

    public PropertyService(
        IPropertyRepository propertyRepository,
        IMapper mapper,
        IUserService userService)
    {
        _propertyRepository = propertyRepository;
        _mapper = mapper;
        _userService = userService;
    }

    public async Task<PropertyModel?> CreateAsync(CreatePropertyModel model)
    {
        var currentUserId = _userService.GetCurrentUserId();
        if (currentUserId is null) throw new ServerException(Shared.Constants.Constants.ErrorMessages.USER_DOES_NOT_EXIST, System.Net.HttpStatusCode.Unauthorized);

        var userProperties = await GetAllOfUserAsync();
        var previouslyIncluded = userProperties
            .SelectMany(x => x.PropertySteads)
            .Any(x => model.SteadIds.Any(e => e == x.SteadId) || 
                      model.CustomSteadIds.Any(e => e == x.CustomSteadId));
        
        if (previouslyIncluded) throw new ServerException("Ділянку вже додано до іншого поля");

        var propertyEntity = new PropertyEntity
        {
            UserId = currentUserId,
            MultipolygonSerialized = model.MultipolygonSerialized,
            Area = model.Area,
            Name = model.Name,
            PropertySteads = model.SteadIds.Select(steadId => new PropertyAndSteadEntity
            {
                SteadId = steadId
            }).Concat(model.CustomSteadIds.Select(customSteadId => new PropertyAndSteadEntity
            {
                CustomSteadId = customSteadId
            })).ToList()
        };

        await _propertyRepository.CreateAsync(propertyEntity);
        return _mapper.Map<PropertyEntity, PropertyModel>(propertyEntity);
    }
    
    public async Task<PropertyModel?> GetByIdAsync(string id)
    {
        var currentUserId = _userService.GetCurrentUserId();
        if (currentUserId is null) throw new ServerException(Shared.Constants.Constants.ErrorMessages.USER_DOES_NOT_EXIST);
        
        var propertyEntity = await _propertyRepository.GetByIdAsync(id, 
            x => x.PropertySteads,
            x => x.PropertyNotes);
        
        if (propertyEntity is null || propertyEntity.UserId != currentUserId) 
            throw new ServerException(Shared.Constants.Constants.ErrorMessages.PROPERTY_DOES_NOT_EXIST);
        
        var propertyModel = _mapper.Map<PropertyModel>(propertyEntity);
        return propertyModel;
    }

    public async Task<PropertyModel?> UpdateAsync(string id, UpdatePropertyModel model)
    {
        var currentUserId = _userService.GetCurrentUserId();
        if (currentUserId is null) throw new ServerException(Shared.Constants.Constants.ErrorMessages.USER_DOES_NOT_EXIST);
        
        var propertyEntity = await _propertyRepository.GetByIdAsync(id);
        
        if (propertyEntity is null || propertyEntity.UserId != currentUserId) 
            throw new ServerException(Shared.Constants.Constants.ErrorMessages.PROPERTY_DOES_NOT_EXIST);

        propertyEntity.Name = model.Name;
        await _propertyRepository.UpdateAsync(propertyEntity);
        
        var propertyModel = _mapper.Map<PropertyModel>(propertyEntity);
        return propertyModel;
    }

    public async Task<PropertyPreviewModel?> GetPreviewByIdAsync(string id)
    {
        var currentUserId = _userService.GetCurrentUserId();
        if (currentUserId is null) throw new ServerException(Shared.Constants.Constants.ErrorMessages.USER_DOES_NOT_EXIST);

        var propertyEntity = await _propertyRepository.GetByIdAsync(id, x => x.PropertyNotes);
        
        if (propertyEntity is null || propertyEntity.UserId != currentUserId) 
            throw new ServerException(Shared.Constants.Constants.ErrorMessages.PROPERTY_DOES_NOT_EXIST);
        
        var propertyPreviewModel = _mapper.Map<PropertyPreviewModel>(propertyEntity);
        return propertyPreviewModel;
    }

    public async Task DeleteAsync(string id)
    {
        var currentUserId = _userService.GetCurrentUserId();
        if (currentUserId is null)
            throw new ServerException(Shared.Constants.Constants.ErrorMessages.USER_DOES_NOT_EXIST);
        
        var propertyEntity = await _propertyRepository.GetByIdAsync(id);
        if (propertyEntity is null || propertyEntity.UserId != currentUserId)
            throw new ServerException(Shared.Constants.Constants.ErrorMessages.PROPERTY_DOES_NOT_EXIST);

        await _propertyRepository.DeleteAsync(propertyEntity);
    }

    public async Task<List<PropertyModel>> GetAllOfUserAsync()
    {
        var currentUser = await _userService.GetCurrentUserAsync();
        if (currentUser is null) throw new ServerException(Shared.Constants.Constants.ErrorMessages.USER_DOES_NOT_EXIST);

        var propertyEntities = await _propertyRepository
            .GetAsync(x => x.UserId == currentUser.Id, 
                x => x.PropertySteads,
                x => x.PropertyNotes);
        
        var propertyModels = _mapper.Map<List<PropertyModel>>(propertyEntities);
        return propertyModels;
    }
}
