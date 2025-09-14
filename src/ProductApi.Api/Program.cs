using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using ProductApi.Api.Data;
using ProductApi.Api.Extensions;
using ProductApi.Api.Repositories.Implementations;
using ProductApi.Api.Repositories.Interfaces;
using ProductApi.Api.Services.Implementations;
using ProductApi.Api.Services.Interfaces;
using ProductApi.Api.Models; // Add this using statement
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure Entity Framework with SQL Server
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? ""; 

Console.WriteLine($"Using connection string: {connectionString}");

builder.Services.AddDbContext<ProductApiDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register repositories
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// Register services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductIdGeneratorService, ProductIdGeneratorService>();

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Product API",
        Version = "v1",
        Description = "A production-ready .NET 8 Web API for managing products with CRUD operations and stock management."
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddProblemDetails();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

var app = builder.Build();

// Simple Database Setup
await SetupDatabase(connectionString);

app.UseGlobalExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("Application starting...");
app.Run();

static async Task SetupDatabase(string connectionString)
{
    try
    {
        // Step 1: Check if database exists, create if not
        var masterConnectionString = connectionString.Replace("Database=ProductApiDb_Dev", "Database=master");

        using var masterConnection = new Microsoft.Data.SqlClient.SqlConnection(masterConnectionString);
        await masterConnection.OpenAsync();

        using var checkDbCommand = masterConnection.CreateCommand();
        checkDbCommand.CommandText = "SELECT COUNT(*) FROM sys.databases WHERE name = 'ProductApiDb_Dev'";
        var dbExists = (int)(await checkDbCommand.ExecuteScalarAsync() ?? 0) > 0;

        if (!dbExists)
        {
            Console.WriteLine("Creating database ProductApiDb_Dev...");
            using var createDbCommand = masterConnection.CreateCommand();
            createDbCommand.CommandText = "CREATE DATABASE ProductApiDb_Dev";
            await createDbCommand.ExecuteNonQueryAsync();
            Console.WriteLine("Database created successfully");
        }
        else
        {
            Console.WriteLine("Database already exists");
        }

        // Step 2: Create tables and sequence
        using var appConnection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
        await appConnection.OpenAsync();

        using var checkTableCommand = appConnection.CreateCommand();
        checkTableCommand.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Products'";
        var tableExists = (int)(await checkTableCommand.ExecuteScalarAsync() ?? 0) > 0;

        if (!tableExists)
        {
            Console.WriteLine("Creating tables using Entity Framework Code First...");

            // Use Entity Framework to create tables from entity models
            var optionsBuilder = new DbContextOptionsBuilder<ProductApiDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            using var context = new ProductApiDbContext(optionsBuilder.Options);
            await context.Database.EnsureCreatedAsync();

            // Create the sequence for 6-digit product IDs
            Console.WriteLine("Creating ProductIdSequence...");
            using var sequenceCommand = appConnection.CreateCommand();
            sequenceCommand.CommandText = @"
                IF NOT EXISTS (SELECT * FROM sys.sequences WHERE name = 'ProductIdSequence')
                BEGIN
                    CREATE SEQUENCE ProductIdSequence 
                    AS INT 
                    START WITH 100000 
                    INCREMENT BY 1 
                    MINVALUE 100000 
                    MAXVALUE 999999
                    CYCLE;
                END";
            await sequenceCommand.ExecuteNonQueryAsync();

            Console.WriteLine("Tables and sequence created successfully");

            // Add sample products
            Console.WriteLine("Adding sample products...");
            await SeedSampleProducts(context);
        }
        else
        {
            Console.WriteLine("Tables already exist");

            // Ensure sequence exists even if tables exist
            using var checkSequenceCommand = appConnection.CreateCommand();
            checkSequenceCommand.CommandText = "SELECT COUNT(*) FROM sys.sequences WHERE name = 'ProductIdSequence'";
            var sequenceExists = (int)(await checkSequenceCommand.ExecuteScalarAsync() ?? 0) > 0;

            if (!sequenceExists)
            {
                Console.WriteLine("Creating missing ProductIdSequence...");
                using var sequenceCommand = appConnection.CreateCommand();
                sequenceCommand.CommandText = @"
                    CREATE SEQUENCE ProductIdSequence 
                    AS INT 
                    START WITH 100000 
                    INCREMENT BY 1 
                    MINVALUE 100000 
                    MAXVALUE 999999
                    CYCLE;";
                await sequenceCommand.ExecuteNonQueryAsync();
            }

            // Check if we need sample data
            var optionsBuilder = new DbContextOptionsBuilder<ProductApiDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            using var context = new ProductApiDbContext(optionsBuilder.Options);
            var hasProducts = await context.Products.AnyAsync();

            if (!hasProducts)
            {
                Console.WriteLine("Table is empty, adding sample products...");
                await SeedSampleProducts(context);
            }
            else
            {
                var productCount = await context.Products.CountAsync();
                Console.WriteLine($"Products table has {productCount} existing products");
            }
        }

        Console.WriteLine("Database setup completed");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database setup failed: {ex.Message}");
        throw;
    }
}

static async Task SeedSampleProducts(ProductApiDbContext context)
{
    try
    {
        var sampleProductsData = new[]
        {
            new { Name = "iPhone 15 Pro", Description = "Latest Apple smartphone with A17 Pro chip", Price = 999.99m, Stock = 50, Category = "Electronics" },
            new { Name = "MacBook Pro 16\"", Description = "Powerful laptop for professionals with M3 chip", Price = 2499.99m, Stock = 25, Category = "Electronics" },
            new { Name = "AirPods Pro", Description = "Wireless earbuds with active noise cancellation", Price = 249.99m, Stock = 100, Category = "Electronics" },
            new { Name = "Office Chair Pro", Description = "Ergonomic office chair with lumbar support", Price = 299.99m, Stock = 30, Category = "Furniture" },
            new { Name = "Standing Desk", Description = "Height adjustable standing desk", Price = 599.99m, Stock = 15, Category = "Furniture" },
            new { Name = "Gaming Mouse", Description = "High-precision gaming mouse with RGB lighting", Price = 79.99m, Stock = 75, Category = "Gaming" },
            new { Name = "Mechanical Keyboard", Description = "Cherry MX switches mechanical keyboard", Price = 149.99m, Stock = 40, Category = "Gaming" },
            new { Name = "Coffee Maker", Description = "Programmable drip coffee maker", Price = 89.99m, Stock = 60, Category = "Appliances" },
            new { Name = "Blender Pro", Description = "High-speed blender for smoothies and soups", Price = 199.99m, Stock = 35, Category = "Appliances" },
            new { Name = "Yoga Mat", Description = "Non-slip yoga mat with carrying strap", Price = 29.99m, Stock = 80, Category = "Fitness" }
        };

        var connection = context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        foreach (var productData in sampleProductsData)
        {
            // Generate next ID using sequence
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT NEXT VALUE FOR ProductIdSequence";
            var nextId = Convert.ToInt32(await command.ExecuteScalarAsync());

            var product = new Product
            {
                ProductId = nextId,
                Name = productData.Name,
                Description = productData.Description,
                Price = productData.Price,
                StockAvailable = productData.Stock,
                Category = productData.Category,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.Products.Add(product);
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"Successfully added {sampleProductsData.Length} sample products with IDs starting from 100000");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to seed sample products: {ex.Message}");
        throw;
    }
}