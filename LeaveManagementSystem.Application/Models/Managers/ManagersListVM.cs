using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaveManagementSystem.Application.Models.Managers
{
    public class ManagersListVM
    {
        public List<ManagerVM> Managers { get; set; }
        public List<ManagerVM> GeneralManagers { get; set; }
    }
}
