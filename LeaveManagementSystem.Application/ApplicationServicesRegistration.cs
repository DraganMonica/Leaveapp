using LeaveManagementSystem.Application.Services;
using LeaveManagementSystem.Application.Services.Email;
using LeaveManagementSystem.Application.Services.LeaveAllocations;
using LeaveManagementSystem.Application.Services.LeaveRequests;
using LeaveManagementSystem.Application.Services.LeaveTypes;
using LeaveManagementSystem.Application.Services.Periods;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;


namespace LeaveManagementSystem.Application
{
    public static class ApplicationServicesRegistration
    {
        //extending the services that we can register in our web application
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddScoped<ILeaveTypesService, LeaveTypesService>();
            services.AddScoped<ILeaveAllocationsService, LeaveAllocationsService>();
            services.AddScoped<ILeaveRequestsService, LeaveRequestsService>();
            services.AddScoped<IPeriodsService, PeriodsService>();
            services.AddScoped<IUserService, UserService>();
            //vreau un nou client, a noua instanta de fiecare data cand este nevoie, nu o sa folosesc aceeasi instanta pentru toate cererile
            services.AddTransient<IEmailSender, EmailSender>();
            return services;
        }
    }
}
