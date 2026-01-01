# blazor-identity-verification
Blazor Server application for identity verification using Azure Computer Vision and Azure Blob Storage

## Features

- **ID Document Upload**: Upload component with file validation (supports image files up to 10MB)
- **Webcam Selfie Capture**: Real-time webcam access using JavaScript interop and MediaDevices API
- **Azure Computer Vision Integration**: Face detection and matching using Azure AI Vision service
- **Azure Blob Storage**: Secure storage of verification images
- **Database Persistence**: SQL Server database to store verification attempts with confidence scores

## Architecture

### Models
- **VerificationResult**: Contains verification status, confidence score, and message
- **VerificationAttempt**: Entity for database persistence with document path, selfie path, verification status, and confidence score

### Services
- **IIdentityVerificationService**: Main service interface for identity verification workflow
- **AzureVisionService**: Implements face detection and comparison using Azure Computer Vision
- **BlobStorageService**: Handles image upload/download to Azure Blob Storage

### Database
- **VerificationAttempts Table**: Stores all verification attempts with indexed fields for efficient querying

## Prerequisites

1. **.NET 10 SDK** or later
2. **Azure Account** with:
   - Azure Computer Vision resource
   - Azure Storage Account
3. **SQL Server** (LocalDB, Express, or full version)

## Setup Instructions

### 1. Azure Resources Setup

#### Azure Computer Vision
1. Create an Azure Computer Vision resource in Azure Portal
2. Navigate to "Keys and Endpoint"
3. Copy the endpoint URL and key

#### Azure Blob Storage
1. Create a Storage Account in Azure Portal
2. Navigate to "Access keys"
3. Copy the connection string

### 2. Configuration

Update `appsettings.json` with your Azure credentials:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=IdentityVerificationDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "AzureVision": {
    "Endpoint": "https://your-resource-name.cognitiveservices.azure.com/",
    "Key": "your-azure-vision-key-here"
  },
  "AzureStorage": {
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=your-storage-account;AccountKey=your-account-key;EndpointSuffix=core.windows.net",
    "ContainerName": "verification-images"
  }
}
```

### 3. Database Setup

Run the SQL script to create the database table:

```bash
# Using Entity Framework Core Migrations (recommended)
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Or manually execute the SQL script in `Database/CreateVerificationAttemptsTable.sql`:

```sql
CREATE TABLE VerificationAttempts (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    DocumentPath NVARCHAR(500) NOT NULL,
    SelfiePath NVARCHAR(500) NOT NULL,
    IsVerified BIT NOT NULL,
    ConfidenceScore FLOAT NOT NULL,
    AttemptDate DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
```

### 4. Build and Run

```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run
```

The application will be available at `https://localhost:5001` or `http://localhost:5000`

## Usage

1. **Navigate to Identity Verification** page from the menu
2. **Upload ID Document**: Click to select an image file from your device
3. **Capture Selfie**: 
   - Click "Start Camera" to activate your webcam
   - Position yourself in the frame
   - Click "Capture Photo" to take the selfie
4. **Verify Identity**: Click the "Verify Identity" button to process the verification
5. **View Results**: See the verification status, confidence score, and detailed message

## Security Considerations

- Images are stored securely in Azure Blob Storage with private access
- Database stores only paths to images, not the actual image data
- All verification attempts are logged with timestamps for audit purposes
- Sensitive configuration (API keys, connection strings) should be stored in Azure Key Vault or User Secrets for production

## Project Structure

```
IdentityVerification/
├── Components/
│   ├── Layout/
│   │   ├── MainLayout.razor
│   │   └── NavMenu.razor
│   ├── Pages/
│   │   ├── DocumentUpload.razor      # ID document upload component
│   │   ├── SelfieCapture.razor       # Webcam selfie capture component
│   │   └── Verification.razor        # Main verification page
│   └── App.razor
├── Data/
│   └── ApplicationDbContext.cs       # Entity Framework DbContext
├── Database/
│   └── CreateVerificationAttemptsTable.sql
├── Models/
│   ├── VerificationAttempt.cs        # Database entity
│   └── VerificationResult.cs         # Verification result model
├── Services/
│   ├── IIdentityVerificationService.cs
│   ├── IdentityVerificationService.cs
│   ├── IAzureVisionService.cs
│   ├── AzureVisionService.cs
│   ├── IBlobStorageService.cs
│   └── BlobStorageService.cs
├── wwwroot/
│   └── js/
│       └── camera.js                 # JavaScript interop for camera access
├── appsettings.json
└── Program.cs
```

## API Endpoints

This is a Blazor Server application with no REST API endpoints. All interactions happen through the Blazor UI components.

## Dependencies

- **Azure.AI.Vision.ImageAnalysis** (1.0.0-beta.3): Azure Computer Vision SDK
- **Azure.Storage.Blobs**: Azure Blob Storage SDK
- **Microsoft.EntityFrameworkCore.SqlServer**: Entity Framework Core for SQL Server
- **Microsoft.EntityFrameworkCore.Tools**: EF Core tools for migrations

## License

This project is provided as-is for demonstration purposes.
