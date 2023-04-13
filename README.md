# TwinCipherGuard Library

TwinCipherGuard is a library for encrypting and decrypting data using a twin-cipher approach with Azure Key Vault. It provides an easy-to-use API for generating and managing encryption keys and performing encryption and decryption using DEK and MEK. 

## Features

- Generate Data Encryption Keys (DEKs) based on user's identifier.
- Generate encrypted DEKs using Azure Key Vault.
- Encrypt and decrypt data using the generated DEKs and MEK stored in Azure Key Vault. 

## Getting Started

### Prerequisites

- .NET Core 6  or later
- Azure Key Vault account with configured access policies

### Installation

Install the TwinCipherGuard Library from NuGet:

```sh
dotnet add package TwinCipherGuardLib --version 1.0.0
```

### Configuration
Add the following configuration to your appsettings.json file:

```sh
{
  "AzureKeyVault": {
    "ClientId": "YOUR_AZURE_AD_APP_CLIENT_ID",
    "ClientSecret": "YOUR_AZURE_AD_APP_CLIENT_SECRET",
    "TenantId": "YOUR_AZURE_AD_TENANT_ID",
    "VaultUri": "https://YOUR_KEY_VAULT_NAME.vault.azure.net/"
  }
}
```

###  Usage
In your application, set up the dependency injection container to use the TwinCipherGuard Library:

```cs
// Add using directives
using TwinCipherGuardLib;
using TwinCipherGuardLib.Configuration;

// Configure services
services.AddOptions()
    .Configure<AzureKeyVaultSettings>(Configuration.GetSection("AzureKeyVault"))
    .AddSingleton<ITwinCipherGuard, TwinCipherGuard>();
```

Now, you can use the ITwinCipherGuard interface in your application to perform encryption and decryption operations:

```cs
// Add using directive
using TwinCipherGuardLib;

// Inject ITwinCipherGuard in your class constructor
public class MyClass
{
    private readonly ITwinCipherGuard _twinCipherGuard;

    public MyClass(ITwinCipherGuard twinCipherGuard)
    {
        _twinCipherGuard = twinCipherGuard;
    }

    // Use _twinCipherGuard to perform encryption and decryption operations
}
```


### License
This project is licensed under the General Public License (GPL)

