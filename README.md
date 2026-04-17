#  Leave Management System

An ASP.NET Core MVC web application for managing employee leave requests, 
allocations, and approvals within an organization.

##  Features

- **Role-based access** — Admin, Manager, Employee
- **Leave Types** — Create and manage leave categories (Vacation, Maternity, etc.)
- **Leave Allocations** — Automatically allocate leave days per employee per period
- **Leave Requests** — Employees submit requests, managers approve/reject
- **Period Management** — Define yearly leave periods
- **Email Notifications** — Notifications on request status changes

##  Tech Stack

 Layer -Technology 

Backend - ASP.NET Core MVC (.NET 8) 
ORM - Entity Framework Core 
Auth - ASP.NET Core Identity 
Mapping - AutoMapper 
Logging - Serilog 
UI - [Spike Bootstrap](https://wrappixel.com/templates/spike-free-bootstrap-admin/)


##  Layouts

The app uses **two separate layouts**:

- **_Layout.cshtml** — Standard Bootstrap layout used for employee-facing pages 
  (login, register, leave requests)
- **_AdminLayout.cshtml** — Professional admin dashboard based on 
  **Spike Free Bootstrap Admin** template by WrapPixel, used for employee management, allocations, leave type configuration

## Project Structure

LeaveManagementSystem/
├── LeaveManagementSystem.Web        # MVC Controllers & Views
├── LeaveManagementSystem.Application # Services, Models, AutoMapper
├── LeaveManagementSystem.Data       # EF Core, Entities, Migrations
└── LeaveManagementSystem.Common     # Shared constants (Roles, etc.)


## Setup & Run

1. Clone the repository : git clone https://github.com/DraganMonica/LeaveManagementSystem.git
2. Update appsettings.json with your connection string:
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=LeaveManagementDB;..."
  }
}

3. Apply migrations
4. Run the application


##  Default Admin Account

After seeding, log in with:
- **Email:** admin@leaveapp.com
- **Password:** *(set in your seed configuration)*

##  Notes

- Leave allocations are calculated based on remaining months in the current period
- Maternity leave requires medical document upload *(in progress)*
- Admins can manually trigger allocation for employees with missing leave types
