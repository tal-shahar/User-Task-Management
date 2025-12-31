# Task Management API Tests

This test project contains comprehensive tests to prevent database schema mismatches and ensure all required fields are properly set when creating tasks.

## Test Coverage

### Integration Tests (`Integration/TaskCreationIntegrationTests.cs`)
- **CreateTask_ShouldSucceedWithAllRequiredDatabaseFields**: Verifies that task creation sets all required database fields, including the critical `UserTelephone` field
- **CreateTask_ShouldNotViolateDatabaseConstraints**: Ensures no database constraint violations occur
- **CreateTask_ShouldHandleMultipleTasksForSameUser**: Tests batch creation scenarios
- **CreateTask_ShouldSetUserTelephoneToEmptyStringWhenNotProvided**: Specifically tests the `UserTelephone` field that caused the original error

### Service Tests (`Services/TaskServiceTests.cs`)
- **CreateTaskAsync_ShouldSetAllRequiredDatabaseFields**: Verifies TaskService sets all required fields
- **CreateTaskAsync_ShouldNotAllowNullRequiredFields**: Ensures no null values in required fields
- **CreateTaskAsync_ShouldHandleUserWithoutFullName**: Tests fallback logic for missing user data
- **CreateTaskAsync_ShouldThrowExceptionWhenUserNotFound**: Error handling tests
- **CreateTaskAsync_ShouldSetUserTelephoneField**: Specifically validates `UserTelephone` is set

### Repository Tests (`Repositories/TaskRepositoryTests.cs`)
- **CreateAsync_ShouldSaveTaskWithAllRequiredFields**: Repository-level validation
- **CreateAsync_ShouldSetCreatedAtAndUpdatedAt**: Timestamp validation
- **CreateAsync_ShouldNotAllowNullRequiredFields**: Null constraint validation

### Model Tests (`Models/TaskModelTests.cs`)
- **Task_ShouldHaveAllRequiredProperties**: Verifies model structure
- **Task_UserTelephone_ShouldNotBeNull**: Validates `UserTelephone` property exists
- **Task_ShouldInitializeWithDefaultValues**: Default value validation

## Running Tests

```bash
cd Backend/TaskManagementAPI/TaskManagementAPI.Tests
dotnet test
```

## What These Tests Prevent

These tests specifically prevent the error that occurred:
- **Error**: `Cannot insert the value NULL into column 'UserTelephone'`
- **Root Cause**: Database schema required `UserTelephone` but the model/service didn't set it
- **Prevention**: Tests verify that `UserTelephone` is always set (even if empty string) before database operations

## Test Infrastructure

- Uses **In-Memory Database** for fast, isolated tests
- Uses **xUnit** testing framework
- Each test class implements `IDisposable` to clean up test data

