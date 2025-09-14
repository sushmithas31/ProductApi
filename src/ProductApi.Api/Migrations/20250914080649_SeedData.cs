using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductApi.Api.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO Products (ProductId, Name, Description, Price, StockAvailable, Category, CreatedAt, UpdatedAt)
                VALUES 
                (NEXT VALUE FOR ProductIdSequence, 'iPhone 15 Pro', 'Latest Apple smartphone with A17 Pro chip', 999.99, 50, 'Electronics', GETUTCDATE(), GETUTCDATE()),
                (NEXT VALUE FOR ProductIdSequence, 'MacBook Pro 16""', 'Powerful laptop for professionals with M3 chip', 2499.99, 25, 'Electronics', GETUTCDATE(), GETUTCDATE()),
                (NEXT VALUE FOR ProductIdSequence, 'AirPods Pro', 'Wireless earbuds with active noise cancellation', 249.99, 100, 'Electronics', GETUTCDATE(), GETUTCDATE()),
                (NEXT VALUE FOR ProductIdSequence, 'Office Chair Pro', 'Ergonomic office chair with lumbar support', 299.99, 30, 'Furniture', GETUTCDATE(), GETUTCDATE()),
                (NEXT VALUE FOR ProductIdSequence, 'Standing Desk', 'Height adjustable standing desk', 599.99, 15, 'Furniture', GETUTCDATE(), GETUTCDATE()),
                (NEXT VALUE FOR ProductIdSequence, 'Gaming Mouse', 'High-precision gaming mouse with RGB lighting', 79.99, 75, 'Gaming', GETUTCDATE(), GETUTCDATE()),
                (NEXT VALUE FOR ProductIdSequence, 'Mechanical Keyboard', 'Cherry MX switches mechanical keyboard', 149.99, 40, 'Gaming', GETUTCDATE(), GETUTCDATE()),
                (NEXT VALUE FOR ProductIdSequence, 'Coffee Maker', 'Programmable drip coffee maker', 89.99, 60, 'Appliances', GETUTCDATE(), GETUTCDATE()),
                (NEXT VALUE FOR ProductIdSequence, 'Blender Pro', 'High-speed blender for smoothies and soups', 199.99, 35, 'Appliances', GETUTCDATE(), GETUTCDATE()),
                (NEXT VALUE FOR ProductIdSequence, 'Yoga Mat', 'Non-slip yoga mat with carrying strap', 29.99, 80, 'Fitness', GETUTCDATE(), GETUTCDATE())
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM Products WHERE ProductId BETWEEN 100001 AND 100010");
        }
    }
}
