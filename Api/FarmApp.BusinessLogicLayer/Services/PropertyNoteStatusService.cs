using AutoMapper;
using FarmApp.BusinessLogicLayer.Services.Interfaces;
using FarmApp.DataAccessLayer.Repositories.Interfaces;
using FarmApp.Entities.Entity;
using FarmApp.Shared.Exceptions;
using FarmApp.ViewModels.PropertyNoteStatuses;

namespace FarmApp.BusinessLogicLayer.Services
{
    public class PropertyNoteStatusService : IPropertyNoteStatusService
    {
        private readonly IUserService _userService;
        private readonly IPropertyNoteStatusRepository _statusRepository;
        private readonly IMapper _mapper;
        public PropertyNoteStatusService(IUserService userService,
            IPropertyNoteStatusRepository statusRepository,
            IMapper mapper)
        {
            _statusRepository = statusRepository;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<PropertyNoteStatusModel> CreateAsync(CreatePropertyNoteStatusModel model)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            if (currentUser == null)
                throw new ServerException(Shared.Constants.Constants.ErrorMessages.USER_DOES_NOT_EXIST,System.Net.HttpStatusCode.Unauthorized);

            var result = _mapper.Map<CreatePropertyNoteStatusModel, PropertyNoteStatusEntity>(model);
            result.IsDefault = false;
            result.UserId = currentUser.Id;

            await _statusRepository.CreateAsync(result);

            return _mapper.Map<PropertyNoteStatusEntity, PropertyNoteStatusModel>(result);

        }

        public async Task<List<PropertyNoteStatusModel>> GetPropertyNoteStatusesAsync()
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            if (currentUser == null)
                throw new ServerException(Shared.Constants.Constants.ErrorMessages.USER_DOES_NOT_EXIST, System.Net.HttpStatusCode.Unauthorized);

            var statuses = await _statusRepository.GetAsync(x => x.IsDefault == true || x.UserId == currentUser.Id);

            return _mapper.Map<List<PropertyNoteStatusEntity>,List<PropertyNoteStatusModel>>(statuses);
        }

        public async Task<PropertyNoteStatusModel> GetStatusByIdAsync(int id)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            if (currentUser == null)
                throw new ServerException(Shared.Constants.Constants.ErrorMessages.USER_DOES_NOT_EXIST, System.Net.HttpStatusCode.Unauthorized);

            var status = await _statusRepository.GetByIdAsync(id);
            if (status == null)
                throw new ServerException(Shared.Constants.Constants.ErrorMessages.STATUS_DOES_NOT_EXIST);

            return _mapper.Map<PropertyNoteStatusEntity,PropertyNoteStatusModel>(status);
        }

        public async Task<PropertyNoteStatusModel> UpdateAsync(int id,UpdatePropertyNoteStatusModel model)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            if (currentUser == null)
                throw new ServerException(Shared.Constants.Constants.ErrorMessages.USER_DOES_NOT_EXIST, System.Net.HttpStatusCode.Unauthorized);

            var status = await _statusRepository.GetByIdAsync(id);
            if (status == null)
                throw new ServerException(Shared.Constants.Constants.ErrorMessages.STATUS_DOES_NOT_EXIST);

            status.Name = model.Name;
            status.TextColorHex = model.TextColorHex;
            status.BGColorHex = model.BGColorHex;

            await _statusRepository.UpdateAsync(status);

            return _mapper.Map<PropertyNoteStatusEntity,PropertyNoteStatusModel>(status);
        }

        public async Task DeleteAsync(int id)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            if (currentUser == null)
                throw new ServerException(Shared.Constants.Constants.ErrorMessages.USER_DOES_NOT_EXIST, System.Net.HttpStatusCode.Unauthorized);

            var status = await _statusRepository.GetByIdAsync(id);
            if (status == null)
                throw new ServerException(Shared.Constants.Constants.ErrorMessages.STATUS_DOES_NOT_EXIST);

            if (status.IsDefault)
                throw new ServerException(Shared.Constants.Constants.ErrorMessages.CANNOT_DELETE_DEFAULT_STATUS);

            await _statusRepository.DeleteAsync(status);
        }
    }
}
