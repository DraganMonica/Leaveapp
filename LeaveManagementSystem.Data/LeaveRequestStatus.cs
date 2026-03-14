using LeaveManagementSystem.Data.Configurations;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace LeaveManagementSystem.Data
{
    
    public class LeaveRequestStatus: BaseEntity
    {
        [StringLength(50)]
        public string Name { get; set; }
    }
}