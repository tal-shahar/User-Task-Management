# Task Management Application

A full-stack task management application with .NET backend and React frontend.

> **⚠️ Development Setup**: This README contains default credentials and configuration for local development only. Never use these settings in a production environment.

## Prerequisites

Before running the application, make sure you have these installed:

- **.NET 8.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Node.js 16+** and **npm** - [Download here](https://nodejs.org/)
- **SQL Server LocalDB** (usually comes with Visual Studio) or **SQL Server Express**

## Quick Start

### Step 1: Setup Database

1. Open a terminal in the project root directory
2. Navigate to the backend folder:
   ```bash
   cd Backend/TaskManagementAPI
   ```
3. Install Entity Framework Core tools (if not already installed):
   ```bash
   dotnet tool install --global dotnet-ef
   ```
4. Create and update the database:
   ```bash
   dotnet ef database update
   ```

### Step 2: Run the Backend

1. Make sure you're in the `Backend/TaskManagementAPI` directory
2. Run the API:
   ```bash
   dotnet run
   ```
3. The backend will start on **http://localhost:5263**
4. You can access the Swagger API documentation at **http://localhost:5263/swagger**

**Keep this terminal window open!**

### Step 2.5: Create Admin User (Development Only)

> **⚠️ IMPORTANT: This is for LOCAL DEVELOPMENT ONLY. Never use these credentials in production!**

After the backend is running, create the admin user by making a POST request to the seed endpoint:

**Option 1: Using Swagger UI (Easiest)**
1. Go to http://localhost:5263/swagger
2. Find the `POST /api/seed/admin` endpoint
3. Click "Try it out" then "Execute"
4. The admin user will be created

**Option 2: Using curl (Command Line)**
```bash
curl -X POST http://localhost:5263/api/seed/admin
```

**Default Admin Credentials (Development Only):**
- **Username**: `admin`
- **Password**: `admin123`

> **⚠️ SECURITY WARNING**: These are default development credentials with weak passwords. In a production environment, you must:
> - Use strong, unique passwords
> - Change default credentials immediately
> - Use secure password management
> - Never commit credentials to version control
> - Use environment variables or secure configuration management

### Step 3: Run the Frontend

1. Open a **new terminal window**
2. Navigate to the frontend directory:
   ```bash
   cd Backend/TaskManagementAPI/frontend
   ```
3. Install dependencies (first time only):
   ```bash
   npm install
   ```
4. Start the frontend:
   ```bash
   npm start
   ```
5. The frontend will automatically open in your browser at **http://localhost:3000**

**Keep this terminal window open too!**

## Access URLs

- **Frontend Application**: http://localhost:3000
- **Backend API**: http://localhost:5263
- **API Documentation (Swagger)**: http://localhost:5263/swagger

## Troubleshooting

### Database Connection Issues

If you get database connection errors:

1. Make sure SQL Server LocalDB is running
2. Check the connection string in `Backend/TaskManagementAPI/appsettings.json`
3. Try running the migration again:
   ```bash
   cd Backend/TaskManagementAPI
   dotnet ef database update
   ```

### Port Already in Use

If port 5263 or 3000 is already in use:

- **Backend**: Stop any other applications using port 5263, or change the port in `launchSettings.json`
- **Frontend**: Stop any other applications using port 3000, or the React app will ask to use a different port

### Frontend Can't Connect to Backend

- Make sure the backend is running on port 5263
- Check that both services are running in separate terminal windows
- Verify the API URL in the frontend code matches `http://localhost:5263/api`

## Project Structure

```
User-Task-Management/
├── Backend/
│   └── TaskManagementAPI/
│       ├── Controllers/        # API endpoints
│       ├── Services/           # Business logic
│       ├── Repositories/       # Data access
│       ├── Models/             # Database models
│       ├── DTOs/               # Data transfer objects
│       └── frontend/           # React application
```

## Technology Stack

- **Backend**: .NET 8.0, Entity Framework Core, SQL Server
- **Frontend**: React 18, TypeScript, Redux Toolkit
- **Authentication**: JWT Bearer tokens

## Default Login Credentials (Development Only)

> **⚠️ FOR LOCAL DEVELOPMENT ONLY - DO NOT USE IN PRODUCTION**

After creating the admin user (see Step 2.5), you can log in with:

- **Username**: `admin`
- **Password**: `admin123`

These credentials are only for local development and testing. In production, you must:
- Use strong, unique passwords
- Change all default credentials
- Implement proper security measures
- Use secure configuration management

