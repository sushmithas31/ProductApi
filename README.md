# ProductApi - Product Management API

Simple .NET 8 REST API for managing products with CRUD operations and stock management.

## Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB/Express/Full)

## Quick Start

1. **Update connection string** in `src/ProductApi.Api/appsettings.Development.json`:
```json
"DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=ProductApiDb_Dev;Integrated Security=true;TrustServerCertificate=true;"
```

2. **Run the API**:
```bash
cd ProductApi\src\ProductApi.Api
dotnet run
```

3. **Open Swagger**: https://localhost:3001

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/products` | Get all products |
| GET | `/api/products/{id}` | Get product by ID |
| POST | `/api/products` | Create product |
| PUT | `/api/products/{id}` | Update product |
| DELETE | `/api/products/{id}` | Delete product |
| PUT | `/api/products/add-to-stock/{id}/{quantity}` | Add stock |
| PUT | `/api/products/decrement-stock/{id}/{quantity}` | Remove stock |

## Features
- ✅ CRUD operations
- ✅ Stock management  
- ✅ Unique 6-digit ProductIds
- ✅ Input validation
- ✅ Swagger documentation
- ✅ Unit tests



![Swagger Screenshot](./screenshots/swagger.png)

