# Smart Inventory and Purchasing Management System

A web-based inventory and purchasing management system built with ASP.NET Core MVC, C#, Entity Framework Core, and SQL Server.

## Project Overview

Smart Inventory System helps businesses manage their products, suppliers, and purchase orders through a centralized web application.

The system allows users to monitor stock quantities, identify low-stock products, create purchase orders, and update inventory when orders are received.

## Features

### Product Management
- Add, edit, view, and delete products.
- Search products by name.
- Track product prices and quantities.
- Set minimum stock levels.
- View low-stock alerts.

### Supplier Management
- Add, edit, view, and delete suppliers.
- Store supplier contact information.
- Search and manage supplier records.

### Purchase Order Management
- Create purchase orders and assign suppliers.
- Add products and quantities to purchase orders.
- Calculate order totals automatically.
- Remove products from pending orders.
- Receive purchase orders and automatically update inventory quantities.
- Prevent modifications after an order is received.

### Dashboard
- View inventory statistics.
- Monitor low-stock products.
- Access important system sections.

## Technologies Used

- C#
- ASP.NET Core MVC
- .NET 10
- Entity Framework Core
- SQL Server
- Bootstrap
- HTML
- CSS
- JavaScript

## Database Setup

1. Install SQL Server.
2. Open the project in Visual Studio.
3. Update the `DefaultConnection` connection string in `appsettings.Development.json` with your SQL Server instance name.
4. Open the Package Manager Console.
5. Run the following command:

```powershell
Update-Database
```

## Run the Project

1. Open `SmartInventorySystem.sln` in Visual Studio.
2. Make sure SQL Server is running.
3. Build the solution.
4. Run the application using `Ctrl + F5`.

## Project Structure

- `Controllers` – Application controllers.
- `Data` – Database context.
- `Migrations` – Entity Framework database migrations.
- `Model` – Application data models.
- `Views` – MVC views.
- `wwwroot` – Static files such as CSS, JavaScript, and libraries.

## Developer

Developed as a practical ASP.NET Core MVC project to apply concepts of backend development, database management, and web application development.