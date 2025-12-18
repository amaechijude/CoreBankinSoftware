# Core Banking Software

A comprehensive microservices-based banking solution built with .NET 9, featuring customer profile management, account services, API gateway, and gRPC communication.

## 🏗️ Architecture Overview

This solution implements a modern microservices architecture with the following components:

- **API Gateway** - YARP-based reverse proxy for routing and load balancing
- **Customer Profile Service** - User authentication, onboarding, and biometric verification
- **Account Services** - Core banking account management and operations
- **gRPC Contracts** - Shared service contracts for inter-service communication

## 📁 Solution Structure

```
CoreBankingSoftware/
├── AccountServices/              # Account management microservice
├── CustomerProfile/              # Customer profile and authentication service
├── YarpApiGateWay/              # API Gateway with YARP
├── gRPC/Contracts/               # Shared gRPC contracts
└── Directory.Packages.props     # Centralized package management
```

## 🚀 Services

### 1. API Gateway (YarpApiGateWay)
**Technology**: YARP (Yet Another Reverse Proxy)  
**Port**: Default routing to Customer Service on port 5039

**Features**:
- Request routing and load balancing
- Service discovery and health checks
- Request/response transformation
- Rate limiting and security policies

**Configuration**:
- Routes `/customer/{**catch-all}` to Customer Service cluster
- Configurable destination addresses

### 2. Customer Profile Service (CustomerProfile.API)
**Technology**: ASP.NET Core 9, PostgreSQL, JWT Authentication  
**Port**: 5039

**Key Features**:
- **User Onboarding**: Phone number and email-based registration
- **Authentication**: JWT token-based authentication system
- **BVN Verification**: Integration with QuickVerify for BVN validation
- **Face Recognition**: Biometric verification using FaceAiSharp
- **SMS Integration**: Twilio-based SMS notifications
- **Document Processing**: OCR capabilities with Tesseract

**Technologies Used**:
- Entity Framework Core with PostgreSQL
- Serilog for structured logging
- FluentValidation for input validation
- FaceAiSharp for biometric verification
- Twilio for SMS services

**API Documentation**: Scalar-based OpenAPI documentation

### 3. Account Services (AccountServices)
**Technology**: ASP.NET Core 9, SQL Server, gRPC

**Key Features**:
- **Account Management**: Create, retrieve, and manage bank accounts
- **Account Types**: Support for different account types (Savings, etc.)
- **Account Status**: Active, inactive, and closed account states
- **Phone-based Accounts**: 10-digit account numbers derived from phone numbers
- **Balance Management**: Account balance tracking and updates

**Domain Model**:
- `Account` entity with comprehensive account information
- Account status and type enums
- Phone number to account number conversion
- Audit fields for tracking changes

**Technologies Used**:
- Entity Framework Core with SQL Server
- FluentValidation for business rules
- Swagger for API documentation
- gRPC for service communication

### 4. gRPC Contracts (SharedGrpcContracts)
**Technology**: Protocol Buffers, gRPC

**Services**:
- `AccountGrpcApiService` - Account management operations
- `CreateAccount` - Create new bank accounts
- `GetAccountById` - Retrieve account by customer ID
- `GetAccountByNumber` - Retrieve account by phone number

**Message Types**:
- Account creation and retrieval requests/responses
- Error handling and success indicators
- Account status and balance information

## 🛠️ Technology Stack

### Core Technologies
- **.NET 9** - Latest .NET framework
- **ASP.NET Core** - Web API framework
- **Entity Framework Core** - ORM for data access
- **gRPC** - High-performance RPC framework
- **YARP** - Reverse proxy for API Gateway

### Databases
- **PostgreSQL** - Customer Profile Service
- **SQL Server** - Account Services

### Authentication & Security
- **JWT Bearer Tokens** - Authentication
- **ASP.NET Identity** - User management
- **Password Hashing** - Secure password storage

### External Services
- **Twilio** - SMS notifications
- **QuickVerify** - BVN verification
- **FaceAiSharp** - Biometric face recognition

### Development Tools
- **Serilog** - Structured logging
- **FluentValidation** - Input validation
- **Swagger/Scalar** - API documentation
- **Docker** - Containerization support

## 🚀 Getting Started

### Prerequisites
- .NET 9 SDK
- PostgreSQL (for Customer Profile Service)
- Docker (optional, for containerized deployment)

### Running the Solution

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd CoreBankingSoftware
   ```

2. **Restore packages**
   ```bash
   dotnet restore
   ```

3. **Configure databases**
   - Update connection strings in `appsettings.json` files
   - Run Entity Framework migrations

4. **Start services**
   ```bash
   # Start API Gateway
   cd YarpApiGateWay
   dotnet run

   # Start Customer Profile Service
   cd CustomerProfile/src/CustomerProfile.API
   dotnet run

   # Start Account Services
   cd AccountServices
   dotnet run
   ```

### Docker Deployment
Each service includes Dockerfile for containerized deployment:

```bash
# Build and run with Docker Compose
docker-compose up --build
```

## 📚 API Documentation

### Customer Profile Service
- **Base URL**: `http://localhost:5039`
- **Documentation**: `http://localhost:5039/scalar/v1` (Development)
- **Authentication**: JWT Bearer tokens required for protected endpoints

### Account Services
- **Base URL**: `http://localhost:5000` (default)
- **Documentation**: `http://localhost:5000/swagger` (Development)
- **gRPC**: Available for inter-service communication

### API Gateway
- **Base URL**: `http://localhost:5000` (default)
- **Routes**: `/customer/*` → Customer Profile Service

## 🔧 Configuration

### Environment Variables
- Database connection strings
- JWT secret keys
- External service API keys (Twilio, QuickVerify)
- Service discovery endpoints

### Service Configuration
- YARP routing configuration in `RouteClusterConfiguration.cs`
- Database contexts and connection strings
- Authentication and authorization policies
- Logging and monitoring settings

## 🧪 Testing

The solution includes test projects for unit testing:
- `CustomerProfile/tests/TestProject1`
- `CustomerProfile/tests/TestProject2`

Run tests with:
```bash
dotnet test
```

## 📋 Service Flow

### Customer Onboarding
1. User provides phone number and email
2. System validates and sends OTP via SMS
3. User verifies OTP and sets up profile
4. JWT token generated for authentication

### BVN Verification
1. User provides BVN number
2. System calls QuickVerify service
3. BVN data retrieved and stored
4. Face verification using biometric comparison

### Account Creation
1. Customer profile must exist
2. Account created with phone-based account number
3. Account linked to customer profile
4. Initial balance set to zero

## 🔒 Security Features

- JWT-based authentication
- Password hashing with ASP.NET Identity
- Input validation with FluentValidation
- Secure biometric data storage
- HTTPS enforcement
- CORS configuration

## 📊 Monitoring and Logging

- **Structured Logging**: Serilog with console and file sinks
- **Log Levels**: Information, Warning, Error
- **Log Rotation**: Hourly rotation with size limits
- **Application Insights**: Ready for cloud monitoring

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🆘 Support

For support and questions:
- Create an issue in the repository
- Check the service-specific documentation
- Review the API documentation endpoints

---

**Built with ❤️ using .NET 9 and modern microservices architecture**