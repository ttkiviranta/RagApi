# RagMaui.Tests

Unit tests for the RagMaui .NET MAUI application.

## Overview

This test project provides unit testing capabilities for the RagMaui cross-platform application. Due to MAUI's multi-platform nature, this project focuses on testing business logic, view models, and services that don't require platform-specific implementations.

## Test Structure

### 📁 **TestInfrastructure/**
- `TestViewModelBase.cs` - Simple ViewModelBase implementation for testing
- `MockRagApiService.cs` - Mock implementation of API services with test models

### 📁 **ViewModels/**
- `ViewModelBaseTests.cs` - Tests for MVVM base functionality (PropertyChanged, etc.)

### 📁 **Services/**
- `RagApiServiceTests.cs` - Tests for API service mock implementations

## Testing Strategy

### ✅ **What is Tested**
- **Business Logic**: ViewModels, data models, utility functions
- **Service Contracts**: Mock implementations of API services
- **MVVM Patterns**: PropertyChanged notifications, command patterns
- **Data Binding Logic**: Property setters and getters

### ❌ **What is NOT Tested (Requires Alternative Approaches)**
- **UI Components**: XAML pages and controls
- **Platform-Specific Code**: Authentication, file system access
- **Navigation**: Shell navigation and page transitions
- **Device Features**: Camera, geolocation, platform APIs

## Running Tests

### Command Line
```bash
cd RagMaui.Tests
dotnet test
```

### Visual Studio
- Open Test Explorer (Test → Test Explorer)
- Build the solution
- Run All Tests

### Test Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Alternative Testing Approaches for MAUI

Since this project only covers unit testing of business logic, here are additional testing strategies for MAUI applications:

### 1. **UI Testing with Appium**
```xml
<PackageReference Include="Appium.WebDriver" Version="5.0.0-rc.1" />
```

### 2. **Integration Testing**
- Test API integrations with actual endpoints
- Database integration testing
- Azure services integration

### 3. **Manual Testing Checklist**
- Platform-specific functionality (iOS, Android, Windows)
- Authentication flows
- File upload/download
- Navigation between pages
- Device rotation and responsive design

### 4. **Platform-Specific Testing**
For testing platform-specific implementations:
- Create separate test projects for each platform
- Use platform-specific testing frameworks
- Test actual device/emulator functionality

## Test Dependencies

- **xUnit** - Testing framework
- **FluentAssertions** - Assertion library for readable tests
- **Moq** - Mocking framework (if needed for more complex scenarios)
- **CommunityToolkit.Mvvm** - MVVM helpers

## Best Practices

1. **Mock External Dependencies**: Use mock services for API calls
2. **Test Business Logic**: Focus on ViewModels and data processing
3. **Avoid UI Dependencies**: Keep tests independent of XAML/platform code
4. **Test Commands**: Verify CanExecute and Execute behavior
5. **Property Testing**: Ensure PropertyChanged events fire correctly

## Known Limitations

- Cannot directly test MAUI-specific features due to platform dependencies
- Authentication testing requires mocking Microsoft Identity Client
- File I/O operations need platform-specific testing
- UI automation requires separate tools like Appium or platform-specific frameworks

## Future Improvements

- Add integration tests with real API endpoints
- Implement Appium-based UI tests
- Add performance testing for large data operations
- Create test data builders for complex scenarios