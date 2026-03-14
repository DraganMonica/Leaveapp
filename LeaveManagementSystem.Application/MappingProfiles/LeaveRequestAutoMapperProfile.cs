using AutoMapper;
using LeaveManagementSystem.Data;
using LeaveManagementSystem.Application.Models.LeaveRequests;
using LeaveManagementSystem.Application.Models.Periods;

namespace LeaveManagementSystem.Application.MappingProfiles
{
    public class LeaveRequestAutoMapperProfile: Profile
    {
        public LeaveRequestAutoMapperProfile()
        {
            // transform entitatea din DB într-un obiect pentru UI.
            CreateMap<LeaveRequestCreateVM, LeaveRequest>();
        }
    }
}
