# TownTrek

A modern ASP.NET Core web application built with .NET Aspire for exploring and discovering local towns and attractions.

## Project Structure

This solution consists of three main projects:

- **TownTrek** - The main ASP.NET Core MVC web application
- **TownTrek.AppHost** - .NET Aspire orchestration host for managing the application lifecycle
- **TownTrek.ServiceDefaults** - Shared service configuration and defaults for observability and health checks

## Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022 or Visual Studio Code
- Docker (for containerization support)

## Getting Started

1. Clone the repository:
   ```bash
   git clone https://github.com/JohnSters/towntrek.git
   cd towntrek
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Run the application using the Aspire AppHost:
   ```bash
   dotnet run --project TownTrek.AppHost
   ```

4. Open your browser and navigate to the URL shown in the console output.

## Development

### Running Locally

The easiest way to run the application is through the Aspire AppHost, which will orchestrate all services and provide a unified dashboard for monitoring.

### Project Dependencies

- **TownTrek** depends on **TownTrek.ServiceDefaults** for shared configuration
- **TownTrek.AppHost** orchestrates the **TownTrek** web application

## Technology Stack

- ASP.NET Core 8.0
- .NET Aspire
- Bootstrap 5
- jQuery
- Entity Framework Core (ready for database integration)

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## License

This project is licensed under the MIT License.