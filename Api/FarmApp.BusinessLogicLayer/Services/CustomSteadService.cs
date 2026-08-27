using AutoMapper;
using FarmApp.BusinessLogicLayer.Services.Interfaces;
using FarmApp.DataAccessLayer.Repositories.Interfaces;
using FarmApp.Entities.Entity;
using FarmApp.Shared.Exceptions;
using FarmApp.ViewModels.CustomSteads;

namespace FarmApp.BusinessLogicLayer.Services;

public class CustomSteadService : ICustomSteadService
{
    private readonly ICustomSteadRepository _customSteadRepository;
    private readonly ISteadRepository _steadRepository;
    private readonly IUserService _userService;
    private readonly IMapper _mapper;
    
    public CustomSteadService(
        ICustomSteadRepository customSteadRepository, 
        ISteadRepository steadRepository,
        IUserService userService, 
        IMapper mapper)
    {
        _steadRepository = steadRepository;
        _customSteadRepository = customSteadRepository;
        _userService = userService;
        _mapper = mapper;
    }
    
    public async Task<CustomSteadModel?> CreateAsync(CreateCustomSteadModel createModel)
    {
        var currentUserId = _userService.GetCurrentUserId();
        if (currentUserId is null)
            throw new ServerException(Shared.Constants.Constants.ErrorMessages.USER_DOES_NOT_EXIST);

        if (createModel.SteadId is not null)
        {
            var steadEntity = await _steadRepository.GetByIdAsync(createModel.SteadId);
            if (steadEntity is null)
                throw new ServerException(Shared.Constants.Constants.ErrorMessages.STEAD_DOES_NOT_EXISTS);
        }
        
        var customSteadEntity = new CustomSteadEntity
        {
            UserId = currentUserId,
            SteadId = createModel.SteadId,
            Coordinates = createModel.Coordinates
        };

        await _customSteadRepository.CreateAsync(customSteadEntity);
        var customSteadModel = _mapper.Map<CustomSteadEntity, CustomSteadModel>(customSteadEntity);
        return customSteadModel;
    }

    public async Task<CustomSteadModel?> UpdateAsync(string id, UpdateCustomSteadModel model)
    {
        var customSteadEntity = await _customSteadRepository.GetByIdAsync(id);
        if (customSteadEntity is null) 
            throw new ServerException(Shared.Constants.Constants.ErrorMessages.STEAD_DOES_NOT_EXISTS);

        customSteadEntity.Coordinates = model.Coordinates;
        await _customSteadRepository.UpdateAsync(customSteadEntity);
        
        var customSteadModel = _mapper.Map<CustomSteadEntity, CustomSteadModel>(customSteadEntity);
        return customSteadModel;
    }

    public async Task DeleteAsync(string id)
    {
        var currentUserId = _userService.GetCurrentUserId();
        if (currentUserId is null)
            throw new ServerException(Shared.Constants.Constants.ErrorMessages.USER_DOES_NOT_EXIST);
        
        var steadEntity = await _customSteadRepository.GetByIdAsync(id);
        if (steadEntity is null || steadEntity.UserId != currentUserId)
            throw new ServerException(Shared.Constants.Constants.ErrorMessages.STEAD_DOES_NOT_EXISTS);

        await _customSteadRepository.DeleteAsync(steadEntity);
    }

    public async Task<CustomSteadModel> GetByIdAsync(string id)
    {
        var customSteadEntity = await _customSteadRepository.GetByIdAsync(id);
        if (customSteadEntity is null)
            throw new ServerException(Shared.Constants.Constants.ErrorMessages.STEAD_DOES_NOT_EXISTS);
        
        var customSteadModel = _mapper.Map<CustomSteadEntity, CustomSteadModel>(customSteadEntity);
        return customSteadModel;
    }

    public async Task<List<CustomSteadModel>> GetAllOfCurrentUserAsync()
    {
        var currentUserId = _userService.GetCurrentUserId();
        if (currentUserId is null)
            throw new ServerException(Shared.Constants.Constants.ErrorMessages.USER_DOES_NOT_EXIST);

        var customSteadEntity = await _customSteadRepository.GetAsync(x => x.UserId == currentUserId);
        var customSteadModels = _mapper.Map<List<CustomSteadEntity>, List<CustomSteadModel>>(customSteadEntity);

        return customSteadModels;
    }
}