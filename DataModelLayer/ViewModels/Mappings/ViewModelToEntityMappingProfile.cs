using AutoMapper;
using DataModelLayer.Models.Entities;

namespace DataModelLayer.ViewModels.Mappings
{
      public class ViewModelToEntityMappingProfile : Profile
      {
            public ViewModelToEntityMappingProfile()
            {
                CreateMap<RegistrationViewModel, AppUser>().ForMember(au => au.UserName, map => map.MapFrom(vm => vm.Email));
            }
      }
}
