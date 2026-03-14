using AutoMapper;
using LeaveManagementSystem.Data;
using LeaveManagementSystem.Application.Models.LeaveTypes;

namespace LeaveManagementSystem.Application.MappingProfiles
{
    public class LeaveTypeAutoMapperProfile : Profile
    {
        public LeaveTypeAutoMapperProfile()
        {
            CreateMap<LeaveType, LeaveTypeReadOnlyVM>();
            //FORM(user input) - DB MODEL
            CreateMap<LeaveTypeCreateVM, LeaveType>();
            CreateMap<LeaveTypeEditVM, LeaveType>().ReverseMap();
        }

    }

}
