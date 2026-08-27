using AutoMapper;
using FarmApp.BusinessLogicLayer.Services.Interfaces;
using FarmApp.DataAccessLayer.Repositories.Interfaces;
using FarmApp.ViewModels.Pagination;
using FarmApp.ViewModels.Steads;
using Microsoft.EntityFrameworkCore;

namespace FarmApp.BusinessLogicLayer.Services;

public class SteadService : ISteadService
{
    private readonly ISteadRepository _steadRepository;
    private readonly IPropertyNoteRepository _propertyNoteRepository;
    private readonly IUserService _userService;
    private readonly IMapper _mapper;

    public SteadService(
        ISteadRepository steadRepository, 
        IMapper mapper, 
        IUserService userService,
        IPropertyNoteRepository propertyNoteRepository)
    {
        _steadRepository = steadRepository;
        _mapper = mapper;
        _userService = userService;
        _propertyNoteRepository = propertyNoteRepository;
    }

    public async Task<SteadModel> GetByIdAsync(string id)
    {
        var stead = await _steadRepository.GetByIdAsync(id);
        var result = _mapper.Map<SteadModel>(stead);
        
        // DONT REMOVE
        // result.CadNum = null;
        // result.Address = null;
        
        return result;
    }
}
