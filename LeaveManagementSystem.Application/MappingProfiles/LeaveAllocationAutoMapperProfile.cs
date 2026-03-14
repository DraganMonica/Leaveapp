using AutoMapper;
using LeaveManagementSystem.Application.Models.LeaveAllocations;
using LeaveManagementSystem.Application.Models.Periods;
using LeaveManagementSystem.Data;


namespace LeaveManagementSystem.Application.MappingProfiles
{
    public class LeaveAllocationAutoMapperProfile : Profile
    {
        public LeaveAllocationAutoMapperProfile()
        {
            // transform entitatea din DB într-un obiect pentru UI.
            CreateMap<LeaveAllocation, LeaveAllocationVM>();
            CreateMap<LeaveAllocation, LeaveAllocationEditVM>();
            CreateMap<ApplicationUser, EmployeeListVM>();
            CreateMap<Period, PeriodVM>();
        }
    }
}
