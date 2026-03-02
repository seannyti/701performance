
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.Text.Json.Serialization;
using PowersportsApi.Data;
using PowersportsApi.Models;
using PowersportsApi.Models.Auth;
using PowersportsApi.Models.Admin;
using PowersportsApi.Services;

namespace PowersportsApi;

public class Program
{
    public static async Task Main(string[] args)
    {
        DisplayStartupBanner();

        var builder = WebApplication.CreateBuilder(args);
        var configuration = builder.Configuration;

        builder.Services.AddDbContext<PowersportsDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ProductService>();
        builder.Services.AddScoped<AuthService>();
        builder.Services.AddScoped<FileService>();
        builder.Services.AddScoped<EmailService>();

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? "powersports-jwt-secret-key-2026-at-least-32-chars-long!";
        var key = Encoding.ASCII.GetBytes(secretKey);

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy =>
                policy.RequireRole("Admin", "SuperAdmin"));
            
            options.AddPolicy("SuperAdminOnly", policy =>
                policy.RequireRole("SuperAdmin"));
            
            options.AddPolicy("AuthenticatedOnly", policy =>
                policy.RequireAuthenticatedUser());
        });

        builder.Services.AddCors(options =>
        {
            var allowedOrigins = configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>() 
                ?? new[] { "http://localhost:5173", "http://localhost:5174" };
            
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS", "PATCH")
                      .WithHeaders("Content-Type", "Authorization", "X-Requested-With")
                      .AllowCredentials()
                      .SetPreflightMaxAge(TimeSpan.FromHours(1));
            });
            
            // Separate policy for public read-only endpoints
            options.AddPolicy("PublicReadOnly", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .WithMethods("GET", "OPTIONS")
                      .AllowAnyHeader()
                      .SetPreflightMaxAge(TimeSpan.FromDays(1));
            });
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() 
            { 
                Title = "Powersports Showcase API", 
                Version = "v1.0",
                Description = "API for powersports products including ATVs, dirt bikes, UTVs, snowmobiles, and gear"
            });
        });

        var app = builder.Build();

        DisplaySeparator();
        LogInfo("Powersports Showcase API Server");
        DisplaySeparator();
        LogInfo($"Environment: {app.Environment.EnvironmentName}");
        
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => 
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Powersports API v1.0");
            });
        }

        // Security middleware pipeline
        app.UseMiddleware<PowersportsApi.Middleware.SecurityHeadersMiddleware>();
        app.UseMiddleware<PowersportsApi.Middleware.RequestValidationMiddleware>();
        app.UseMiddleware<PowersportsApi.Middleware.RateLimitingMiddleware>();
        
        app.UseCors("AllowFrontend");
        app.UseStaticFiles();

        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseAuthentication();
        app.UseAuthorization();

        var v1Routes = app.MapGroup("/api/v1").WithTags("API v1");
        var productRoutes = v1Routes.MapGroup("/products").WithTags("Products");

        productRoutes.MapGet("/", async (ProductService productService) =>
        {
            var products = await productService.GetAllProductsAsync();
            return Results.Ok(products);
        })
        .WithName("GetAllProducts")
        .WithSummary("Get all powersports products")
        .Produces<List<Product>>(200);

        productRoutes.MapGet("/{id:int}", async (ProductService productService, int id) =>
        {
            var product = await productService.GetProductByIdAsync(id);
            return product != null ? Results.Ok(product) : Results.NotFound($"Product with ID {id} not found");
        })
        .WithName("GetProductById")
        .WithSummary("Get a specific product by ID")
        .Produces<Product>(200)
        .Produces(404);

        productRoutes.MapGet("/category/{category}", async (ProductService productService, string category) =>
        {
            var products = await productService.GetProductsByCategoryAsync(category);
            return Results.Ok(products);
        })
        .WithName("GetProductsByCategory")
        .WithSummary("Get products by category (ATV, Dirtbike, UTV, Snowmobile, Gear)")
        .Produces<List<Product>>(200);

        productRoutes.MapGet("/featured", async (ProductService productService) =>
        {
            var products = await productService.GetFeaturedProductsAsync();
            return Results.Ok(products);
        })
        .WithName("GetFeaturedProducts")
        .WithSummary("Get featured products for home page display")
        .Produces<List<Product>>(200);

        productRoutes.MapPost("/", async (CreateProductRequest request, ProductService productService, PowersportsDbContext context) =>
        {
            try
            {
                var product = new Product
                {
                    Name = request.Name,
                    Description = request.Description,
                    Price = request.Price,
                    CategoryId = request.CategoryId,
                    ImageUrl = request.ImageUrl ?? string.Empty,
                    IsFeatured = request.IsFeatured,
                    IsActive = request.IsActive,
                    Sku = request.Sku,
                    StockQuantity = request.StockQuantity,
                    LowStockThreshold = request.LowStockThreshold,
                    CostPrice = request.CostPrice,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                context.Products.Add(product);
                await context.SaveChangesAsync();
                
                return Results.Created($"/api/v1/products/{product.Id}", product);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { message = $"Failed to create product: {ex.Message}" });
            }
        })
        .WithName("CreateProduct")
        .WithSummary("Create a new product (Admin+ only)")
        .Produces<Product>(201)
        .Produces(400)
        .RequireAuthorization("AdminOnly");

        productRoutes.MapPut("/{id:int}", async (int id, UpdateProductRequest request, ProductService productService, PowersportsDbContext context, ILogger<Program> logger) =>
        {
            try
            {
                logger.LogInformation("Updating product {Id}: {@Request}", id, request);
                
                var existingProduct = await context.Products.FindAsync(id);
                if (existingProduct == null)
                {
                    return Results.NotFound($"Product with ID {id} not found");
                }
                
                var categoryExists = await context.Categories.AnyAsync(c => c.Id == request.CategoryId);
                if (!categoryExists)
                {
                    return Results.BadRequest(new { message = $"Category with ID {request.CategoryId} not found" });
                }
                
                existingProduct.Name = request.Name;
                existingProduct.Description = request.Description;
                existingProduct.Price = request.Price;
                existingProduct.CategoryId = request.CategoryId;
                if (!string.IsNullOrEmpty(request.ImageUrl))
                {
                    existingProduct.ImageUrl = request.ImageUrl;
                }
                existingProduct.IsFeatured = request.IsFeatured;
                existingProduct.IsActive = request.IsActive;
                existingProduct.Sku = request.Sku;
                existingProduct.StockQuantity = request.StockQuantity;
                existingProduct.LowStockThreshold = request.LowStockThreshold;
                existingProduct.CostPrice = request.CostPrice;
                existingProduct.UpdatedAt = DateTime.UtcNow;
                
                await context.SaveChangesAsync();
                
                logger.LogInformation("Product {Id} updated successfully", id);
                return Results.Ok(existingProduct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating product {Id}: {Message}", id, ex.Message);
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return Results.BadRequest(new { message = $"Failed to update product: {innerMessage}" });
            }
        })
        .WithName("UpdateProduct")
        .WithSummary("Update an existing product (Admin+ only)")
        .Produces<Product>(200)
        .Produces(404)
        .Produces(400)
        .RequireAuthorization("AdminOnly");

        productRoutes.MapDelete("/{id:int}", async (int id, PowersportsDbContext context) =>
        {
            try
            {
                var product = await context.Products.FindAsync(id);
                if (product == null)
                {
                    return Results.NotFound($"Product with ID {id} not found");
                }
                
                context.Products.Remove(product);
                await context.SaveChangesAsync();
                
                return Results.Ok(new { message = $"Product '{product.Name}' deleted successfully" });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { message = $"Failed to delete product: {ex.Message}" });
            }
        })
        .WithName("DeleteProduct")
        .WithSummary("Delete a product (Admin+ only)")
        .Produces(200)
        .Produces(404)
        .Produces(400)
        .RequireAuthorization("AdminOnly");

        productRoutes.MapPatch("/{id:int}/stock", async (int id, AdjustStockRequest request, PowersportsDbContext context) =>
        {
            try
            {
                var product = await context.Products.FindAsync(id);
                if (product == null)
                    return Results.NotFound($"Product with ID {id} not found");

                product.StockQuantity = request.StockQuantity;
                product.LowStockThreshold = request.LowStockThreshold;
                if (request.Sku != null) product.Sku = request.Sku;
                if (request.CostPrice.HasValue) product.CostPrice = request.CostPrice;
                product.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();
                return Results.Ok(new { product.Id, product.StockQuantity, product.LowStockThreshold, product.Sku, product.CostPrice });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { message = $"Failed to update stock: {ex.Message}" });
            }
        })
        .WithName("AdjustProductStock")
        .WithSummary("Update inventory fields for a product (Admin+ only)")
        .Produces(200)
        .Produces(404)
        .Produces(400)
        .RequireAuthorization("AdminOnly");

        var authRoutes = v1Routes.MapGroup("/auth").WithTags("Authentication");

        authRoutes.MapPost("/register", async (RegisterRequest request, AuthService authService, PowersportsDbContext context) =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email) || 
                    string.IsNullOrWhiteSpace(request.Password) ||
                    string.IsNullOrWhiteSpace(request.FirstName) ||
                    string.IsNullOrWhiteSpace(request.LastName))
                {
                    return Results.BadRequest(new { message = "All fields are required" });
                }

                // Check allow_user_registration setting
                var regSetting = await context.SiteSettings
                    .Where(s => s.Key == "allow_user_registration")
                    .Select(s => s.Value)
                    .FirstOrDefaultAsync();
                if (regSetting == "false")
                {
                    return Results.BadRequest(new { message = "User registration is currently disabled." });
                }

                var result = await authService.RegisterAsync(request);
                if (result == null || !result.IsSuccess)
                {
                    return Results.BadRequest(new { message = result?.ErrorMessage ?? "Registration failed" });
                }

                return Results.Ok(result.Data);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
        .WithName("Register")
        .WithSummary("Register a new user account")
        .Produces<AuthResponse>(200)
        .Produces(400);

        authRoutes.MapPost("/login", async (LoginRequest request, AuthService authService, HttpContext httpContext) =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return Results.BadRequest(new { message = "Email and password are required" });
                }

                var ipAddress = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                    ?? httpContext.Connection.RemoteIpAddress?.ToString();

                var result = await authService.LoginAsync(request, ipAddress);
                if (result == null || !result.IsSuccess)
                {
                    if (result?.ErrorMessage == "EMAIL_NOT_VERIFIED")
                        return Results.Json(new { message = "EMAIL_NOT_VERIFIED" }, statusCode: 403);
                    return Results.Unauthorized();
                }

                return Results.Ok(result.Data);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
        .WithName("Login")
        .WithSummary("Login with email and password")
        .Produces<AuthResponse>(200)
        .Produces(400)
        .Produces(401);

        authRoutes.MapGet("/me", async (HttpContext context, AuthService authService) =>
        {
            try
            {
                string? userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                {
                    return Results.Unauthorized();
                }

                var user = await authService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return Results.NotFound();
                }

                return Results.Ok(user);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
        .WithName("GetCurrentUser")
        .WithSummary("Get current authenticated user profile")
        .Produces<UserResponse>(200)
        .Produces(401)
        .Produces(404)
        .RequireAuthorization();

        authRoutes.MapGet("/verify-email", async (string token, PowersportsDbContext context) =>
        {
            if (string.IsNullOrWhiteSpace(token))
                return Results.BadRequest(new { message = "Verification token is required." });

            var user = await context.Users.FirstOrDefaultAsync(u => u.EmailVerificationToken == token);
            if (user == null)
                return Results.BadRequest(new { message = "Invalid verification token." });

            if (user.EmailVerificationTokenExpiry.HasValue && user.EmailVerificationTokenExpiry.Value < DateTime.UtcNow)
                return Results.BadRequest(new { message = "Verification token has expired. Please request a new one." });

            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpiry = null;
            user.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();

            return Results.Ok(new { message = "Email verified successfully! You can now log in." });
        })
        .WithName("VerifyEmail")
        .WithSummary("Verify user email address via token")
        .Produces(200)
        .Produces(400);

        authRoutes.MapPost("/resend-verification", async (ResendVerificationRequest request, PowersportsDbContext context, EmailService emailService) =>
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant());
            if (user == null || user.IsEmailVerified)
                return Results.Ok(new { message = "If that email exists and is unverified, a new verification email has been sent." });

            var newToken = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(48));
            user.EmailVerificationToken = newToken;
            user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
            user.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();

            var siteUrl = await context.SiteSettings
                .Where(s => s.Key == "site_url")
                .Select(s => s.Value)
                .FirstOrDefaultAsync() ?? "http://localhost:3000";

            try { await emailService.SendVerificationEmailAsync(user.Email, user.FullName, newToken, siteUrl); }
            catch { /* best-effort */ }

            return Results.Ok(new { message = "If that email exists and is unverified, a new verification email has been sent." });
        })
        .WithName("ResendVerification")
        .WithSummary("Resend email verification link")
        .Produces(200)
        .Produces(400);

        var categoryRoutes = v1Routes.MapGroup("/categories").WithTags("Categories");

        categoryRoutes.MapGet("/", async (PowersportsDbContext context, bool? includeInactive) =>
        {
            var query = context.Categories.AsQueryable();
            
            if (includeInactive != true)
            {
                query = query.Where(c => c.IsActive);
            }
            
            var categories = await query
                .OrderBy(c => c.Name)
                .ToListAsync();
            return Results.Ok(categories);
        })
        .WithName("GetAllCategories")
        .WithSummary("Get all categories (optionally including inactive)")
        .Produces<List<Category>>(200);

        categoryRoutes.MapGet("/{id:int}", async (int id, PowersportsDbContext context) =>
        {
            var category = await context.Categories
                .Include(c => c.Products.Where(p => p.IsActive))
                .FirstOrDefaultAsync(c => c.Id == id);
            return category != null ? Results.Ok(category) : Results.NotFound($"Category with ID {id} not found");
        })
        .WithName("GetCategoryById")
        .WithSummary("Get a specific category by ID with its products")
        .Produces<Category>(200)
        .Produces(404);

        categoryRoutes.MapPost("/", async (CreateCategoryRequest request, PowersportsDbContext context) =>
        {
            try
            {
                var category = new Category
                {
                    Name = request.Name,
                    Description = request.Description,
                    ImageUrl = request.ImageUrl,
                    IsActive = request.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                context.Categories.Add(category);
                await context.SaveChangesAsync();
                
                return Results.Created($"/api/v1/categories/{category.Id}", category);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { message = $"Failed to create category: {ex.Message}" });
            }
        })
        .WithName("CreateCategory")
        .WithSummary("Create a new category (Admin+ only)")
        .Produces<Category>(201)
        .Produces(400)
        .RequireAuthorization("AdminOnly");

        categoryRoutes.MapPut("/{id:int}", async (int id, UpdateCategoryRequest request, PowersportsDbContext context) =>
        {
            try
            {
                var existingCategory = await context.Categories.FindAsync(id);
                if (existingCategory == null)
                {
                    return Results.NotFound($"Category with ID {id} not found");
                }
                
                existingCategory.Name = request.Name;
                existingCategory.Description = request.Description;
                existingCategory.ImageUrl = request.ImageUrl;
                existingCategory.IsActive = request.IsActive;
                existingCategory.UpdatedAt = DateTime.UtcNow;
                
                await context.SaveChangesAsync();
                
                return Results.Ok(existingCategory);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { message = $"Failed to update category: {ex.Message}" });
            }
        })
        .WithName("UpdateCategory")
        .WithSummary("Update an existing category (Admin+ only)")
        .Produces<Category>(200)
        .Produces(404)
        .Produces(400)
        .RequireAuthorization("AdminOnly");

        categoryRoutes.MapDelete("/{id:int}", async (int id, PowersportsDbContext context) =>
        {
            try
            {
                var category = await context.Categories
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.Id == id);
                    
                if (category == null)
                {
                    return Results.NotFound($"Category with ID {id} not found");
                }
                
                var hasActiveProducts = category.Products.Any(p => p.IsActive);
                if (hasActiveProducts)
                {
                    category.IsActive = false;
                    category.UpdatedAt = DateTime.UtcNow;
                    await context.SaveChangesAsync();
                    return Results.Ok(new { message = $"Category '{category.Name}' has been disabled (has active products)" });
                }
                else
                {
                    context.Categories.Remove(category);
                    await context.SaveChangesAsync();
                    return Results.Ok(new { message = $"Category '{category.Name}' deleted successfully" });
                }
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { message = $"Failed to delete category: {ex.Message}" });
            }
        })
        .WithName("DeleteCategory")
        .WithSummary("Delete/disable a category (Admin+ only)")
        .Produces(200)
        .Produces(404)
        .Produces(400)
        .RequireAuthorization("AdminOnly");

        var userRoutes = v1Routes.MapGroup("/admin/users").WithTags("User Management");

        userRoutes.MapGet("/", async (PowersportsDbContext context) =>
        {
            var users = await context.Users
                .Select(u => new {
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.Phone,
                    u.Role,
                    RoleName = u.Role.GetRoleName(),
                    u.CreatedAt,
                    u.LastLoginAt,
                    u.LastLoginIp,
                    u.IsActive,
                    u.SubscribeNewsletter
                })
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();
            return Results.Ok(users);
        })
        .WithName("GetAllUsers")
        .WithSummary("Get all users (Admin+ only)")
        .Produces(200)
        .RequireAuthorization("AdminOnly");

        userRoutes.MapGet("/{id:int}", async (int id, PowersportsDbContext context) =>
        {
            var user = await context.Users
                .Where(u => u.Id == id)
                .Select(u => new {
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.Phone,
                    u.Role,
                    RoleName = u.Role.GetRoleName(),
                    u.CreatedAt,
                    u.LastLoginAt,
                    u.LastLoginIp,
                    u.IsActive,
                    u.SubscribeNewsletter
                })
                .FirstOrDefaultAsync();
            return user != null ? Results.Ok(user) : Results.NotFound($"User with ID {id} not found");
        })
        .WithName("GetUserById")
        .WithSummary("Get a specific user by ID (Admin+ only)")
        .Produces(200)
        .Produces(404)
        .RequireAuthorization("AdminOnly");

        userRoutes.MapPut("/{id:int}/role", async (int id, UpdateUserRoleRequest request, PowersportsDbContext context, HttpContext httpContext) =>
        {
            try
            {
                var user = await context.Users.FindAsync(id);
                if (user == null)
                {
                    return Results.NotFound($"User with ID {id} not found");
                }
                
                string? currentUserIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (currentUserIdClaim != null && int.TryParse(currentUserIdClaim, out int currentUserId))
                {
                    if (currentUserId == id && request.Role != UserRole.SuperAdmin)
                    {
                        return Results.BadRequest(new { message = "Cannot change your own role from SuperAdmin" });
                    }
                }
                
                user.Role = request.Role;
                user.UpdatedAt = DateTime.UtcNow;
                
                await context.SaveChangesAsync();
                
                return Results.Ok(new { 
                    message = $"User {user.FullName} role updated to {request.Role.GetRoleName()}",
                    user = new {
                        user.Id,
                        user.FullName,
                        user.Role,
                        RoleName = user.Role.GetRoleName()
                    }
                });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { message = $"Failed to update user role: {ex.Message}" });
            }
        })
        .WithName("UpdateUserRole")
        .WithSummary("Update a user's role (SuperAdmin only)")
        .Produces(200)
        .Produces(404)
        .Produces(400)
        .RequireAuthorization("SuperAdminOnly");

        userRoutes.MapPut("/{id:int}/status", async (int id, PowersportsDbContext context, HttpContext httpContext) =>
        {
            try
            {
                var user = await context.Users.FindAsync(id);
                if (user == null)
                {
                    return Results.NotFound($"User with ID {id} not found");
                }
                
                string? currentUserIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (currentUserIdClaim != null && int.TryParse(currentUserIdClaim, out int currentUserId))
                {
                    if (currentUserId == id)
                    {
                        return Results.BadRequest(new { message = "Cannot deactivate your own account" });
                    }
                }
                
                user.IsActive = !user.IsActive;
                user.UpdatedAt = DateTime.UtcNow;
                
                await context.SaveChangesAsync();
                
                var status = user.IsActive ? "activated" : "deactivated";
                return Results.Ok(new { 
                    message = $"User {user.FullName} has been {status}",
                    user = new {
                        user.Id,
                        user.FullName,
                        user.IsActive
                    }
                });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { message = $"Failed to update user status: {ex.Message}" });
            }
        })
        .WithName("UpdateUserStatus")
        .WithSummary("Activate/Deactivate a user account (SuperAdmin only)")
        .Produces(200)
        .Produces(404)
        .Produces(400)
        .RequireAuthorization("SuperAdminOnly");

        userRoutes.MapPost("/", async (CreateAdminUserRequest request, PowersportsDbContext context) =>
        {
            try
            {
                var email = request.Email.ToLowerInvariant();

                if (await context.Users.AnyAsync(u => u.Email == email))
                {
                    return Results.BadRequest(new { message = "A user with this email already exists." });
                }

                var user = new User
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Phone = request.Phone,
                    Role = request.Role,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                context.Users.Add(user);
                await context.SaveChangesAsync();

                return Results.Created($"/api/v1/admin/users/{user.Id}", new {
                    user.Id, user.FirstName, user.LastName, user.Email,
                    user.Phone, user.Role, RoleName = user.Role.GetRoleName(),
                    user.CreatedAt, user.IsActive
                });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { message = $"Failed to create user: {ex.Message}" });
            }
        })
        .WithName("CreateAdminUser")
        .WithSummary("Create a new user account (SuperAdmin only)")
        .Produces(201)
        .Produces(400)
        .RequireAuthorization("SuperAdminOnly");

        userRoutes.MapDelete("/{id:int}", async (int id, PowersportsDbContext context, HttpContext httpContext) =>
        {
            try
            {
                var user = await context.Users.FindAsync(id);
                if (user == null)
                {
                    return Results.NotFound($"User with ID {id} not found");
                }

                string? currentUserIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (currentUserIdClaim != null && int.TryParse(currentUserIdClaim, out int currentUserId) && currentUserId == id)
                {
                    return Results.BadRequest(new { message = "You cannot delete your own account." });
                }

                context.Users.Remove(user);
                await context.SaveChangesAsync();

                return Results.Ok(new { message = $"User {user.FullName} has been permanently deleted." });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { message = $"Failed to delete user: {ex.Message}" });
            }
        })
        .WithName("DeleteUser")
        .WithSummary("Permanently delete a user account (SuperAdmin only)")
        .Produces(200)
        .Produces(404)
        .Produces(400)
        .RequireAuthorization("SuperAdminOnly");

        userRoutes.MapPut("/{id:int}/info", async (int id, UpdateAdminUserInfoRequest request, PowersportsDbContext context) =>
        {
            try
            {
                var user = await context.Users.FindAsync(id);
                if (user == null)
                    return Results.NotFound($"User with ID {id} not found");

                var email = request.Email.Trim().ToLowerInvariant();
                if (await context.Users.AnyAsync(u => u.Email == email && u.Id != id))
                    return Results.BadRequest(new { message = "That email address is already in use by another account." });

                user.FirstName = request.FirstName.Trim();
                user.LastName  = request.LastName.Trim();
                user.Email     = email;
                user.Phone     = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
                user.SubscribeNewsletter = request.SubscribeNewsletter;
                user.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();

                return Results.Ok(new {
                    message = $"User {user.FullName} profile updated.",
                    user = new {
                        user.Id,
                        user.FirstName,
                        user.LastName,
                        user.Email,
                        user.Phone,
                        user.SubscribeNewsletter,
                        user.UpdatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { message = $"Failed to update user: {ex.Message}" });
            }
        })
        .WithName("UpdateUserInfo")
        .WithSummary("Update a user's profile information (Admin+ only)")
        .Produces(200)
        .Produces(400)
        .Produces(404)
        .RequireAuthorization("AdminOnly");

        userRoutes.MapPut("/{id:int}/password", async (int id, ResetUserPasswordRequest request, PowersportsDbContext context, HttpContext httpContext) =>
        {
            try
            {
                var user = await context.Users.FindAsync(id);
                if (user == null)
                    return Results.NotFound($"User with ID {id} not found");

                // Invalidate all active refresh tokens so the user is forced to log in again
                var tokens = await context.RefreshTokens
                    .Where(rt => rt.UserId == id)
                    .ToListAsync();
                context.RefreshTokens.RemoveRange(tokens);

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                user.UpdatedAt    = DateTime.UtcNow;

                await context.SaveChangesAsync();

                return Results.Ok(new { message = $"Password for {user.FullName} has been reset. Active sessions have been revoked." });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { message = $"Failed to reset password: {ex.Message}" });
            }
        })
        .WithName("ResetUserPassword")
        .WithSummary("Reset a user's password and revoke active sessions (SuperAdmin only)")
        .Produces(200)
        .Produces(400)
        .Produces(404)
        .RequireAuthorization("SuperAdminOnly");

        // ─── Admin Dashboard ──────────────────────────────────────────────────
        var dashboardRoutes = v1Routes.MapGroup("/admin/dashboard").WithTags("Admin Dashboard");

        dashboardRoutes.MapGet("/stats", async (PowersportsDbContext context) =>
        {
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

            var totalProducts       = await context.Products.CountAsync(p => p.IsActive);
            var inactiveProducts    = await context.Products.CountAsync(p => !p.IsActive);
            var featuredProducts    = await context.Products.CountAsync(p => p.IsActive && p.IsFeatured);
            var totalCategories     = await context.Categories.CountAsync(c => c.IsActive);
            var totalUsers          = await context.Users.CountAsync(u => u.IsActive && u.Role == UserRole.User);
            var totalAdmins         = await context.Users.CountAsync(u => u.IsActive && (u.Role == UserRole.Admin || u.Role == UserRole.SuperAdmin));
            var recentRegistrations = await context.Users.CountAsync(u => u.CreatedAt >= sevenDaysAgo);

            var recentUsers = await context.Users
                .Where(u => u.IsActive)
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .Select(u => new {
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    RoleName = u.Role.GetRoleName(),
                    u.CreatedAt
                })
                .ToListAsync();

            return Results.Ok(new {
                data = new {
                    totalProducts,
                    inactiveProducts,
                    featuredProducts,
                    totalCategories,
                    totalUsers,
                    totalAdmins,
                    recentRegistrations,
                    recentUsers,
                    generatedAt = DateTime.UtcNow
                }
            });
        })
        .WithName("GetDashboardStats")
        .WithSummary("Get admin dashboard statistics (Admin+ only)")
        .Produces(200)
        .RequireAuthorization("AdminOnly");

        // Public Site Settings endpoint
        v1Routes.MapGet("/settings", async (PowersportsDbContext context) =>
        {
            var settings = await context.SiteSettings
                .Where(s => s.IsActive)
                .Select(s => new { s.Id, s.Key, s.Value, s.DisplayName })
                .ToListAsync();
            return Results.Ok(settings);
        })
        .WithName("GetPublicSiteSettings")
        .WithSummary("Get public site settings")
        .Produces(200);

        // Public Contact Form endpoint
        v1Routes.MapPost("/contact", async (ContactRequest request, EmailService emailService) =>
        {
            var result = await emailService.SendContactFormEmailAsync(
                request.Name,
                request.Email,
                request.Subject ?? "",
                request.Message
            );

            if (result.Success)
            {
                return Results.Ok(new { message = result.Message });
            }
            else
            {
                return Results.BadRequest(new { message = result.Message });
            }
        })
        .WithName("SubmitContactForm")
        .WithSummary("Submit a contact form message")
        .Produces(200)
        .Produces(400);

        var settingsRoutes = v1Routes.MapGroup("/admin/settings").WithTags("Site Settings");

        settingsRoutes.MapGet("/", async (PowersportsDbContext context) =>
        {
            var settings = await context.SiteSettings
                .Where(s => s.IsActive)
                .OrderBy(s => s.Category)
                .ThenBy(s => s.SortOrder)
                .ThenBy(s => s.DisplayName)
                .ToListAsync();
            return Results.Ok(settings);
        })
        .WithName("GetAllSiteSettings")
        .WithSummary("Get all site settings (Admin+ only)")
        .Produces<List<SiteSetting>>(200)
        .RequireAuthorization("AdminOnly");

        settingsRoutes.MapPut("/{id:int}", async (int id, UpdateSiteSettingRequest request, PowersportsDbContext context, HttpContext httpContext) =>
        {
            try
            {
                var setting = await context.SiteSettings.FindAsync(id);
                if (setting == null)
                {
                    return Results.NotFound($"Setting with ID {id} not found");
                }
                
                string? currentUserIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (currentUserIdClaim != null && int.TryParse(currentUserIdClaim, out int currentUserId))
                {
                    setting.LastModifiedBy = currentUserId;
                }
                
                setting.Value = request.Value;
                setting.UpdatedAt = DateTime.UtcNow;
                
                await context.SaveChangesAsync();
                
                return Results.Ok(setting);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { message = $"Failed to update setting: {ex.Message}" });
            }
        })
        .WithName("UpdateSiteSetting")
        .WithSummary("Update a site setting value (Admin+ only)")
        .Produces<SiteSetting>(200)
        .Produces(404)
        .Produces(400)
        .RequireAuthorization("AdminOnly");

        settingsRoutes.MapPost("/", async (CreateSiteSettingRequest request, PowersportsDbContext context, HttpContext httpContext) =>
        {
            try
            {
                // Prevent duplicate keys
                bool exists = await context.SiteSettings.AnyAsync(s => s.Key == request.Key);
                if (exists)
                {
                    return Results.Conflict(new { message = $"A setting with key '{request.Key}' already exists." });
                }

                string? currentUserIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                int currentUserId = currentUserIdClaim != null && int.TryParse(currentUserIdClaim, out int uid) ? uid : 1;

                var setting = new SiteSetting
                {
                    Key = request.Key,
                    DisplayName = request.DisplayName,
                    Value = request.Value,
                    Description = request.Description,
                    Type = request.Type,
                    Category = request.Category,
                    SortOrder = request.SortOrder,
                    IsRequired = request.IsRequired,
                    IsActive = true,
                    LastModifiedBy = currentUserId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                context.SiteSettings.Add(setting);
                await context.SaveChangesAsync();

                return Results.Created($"/api/v1/admin/settings/{setting.Id}", setting);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { message = $"Failed to create setting: {ex.Message}" });
            }
        })
        .WithName("CreateSiteSetting")
        .WithSummary("Create a new site setting (Admin+ only)")
        .Produces<SiteSetting>(201)
        .Produces(409)
        .Produces(400)
        .RequireAuthorization("AdminOnly");

        settingsRoutes.MapPost("/test-email", async (PowersportsDbContext context, HttpContext httpContext, EmailService emailService) =>
        {
            try
            {
                var user = httpContext.User;
                if (!user.Identity?.IsAuthenticated ?? true)
                    return Results.Unauthorized();

                var toEmailParam = httpContext.Request.Query["email"].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(toEmailParam))
                {
                    // Try reading from body JSON
                    using var body = await System.Text.Json.JsonDocument.ParseAsync(httpContext.Request.Body);
                    toEmailParam = body.RootElement.TryGetProperty("email", out var emailEl) ? emailEl.GetString() : null;
                }

                if (string.IsNullOrWhiteSpace(toEmailParam))
                    return Results.BadRequest(new { message = "Email address is required." });

                await emailService.SendTestEmailAsync(toEmailParam);
                return Results.Ok(new { message = $"Test email sent to {toEmailParam}" });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { message = $"Failed to send test email: {ex.Message}" });
            }
        })
        .WithName("TestEmail")
        .WithSummary("Send a test email to verify SMTP configuration")
        .Produces(200)
        .Produces(400)
        .RequireAuthorization("AdminOrSuperAdmin");

        settingsRoutes.MapPost("/reset", async (PowersportsDbContext context, HttpContext httpContext) =>
        {
            try
            {
                string? currentUserIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                int currentUserId = currentUserIdClaim != null && int.TryParse(currentUserIdClaim, out int userId) ? userId : 1;

                var existingSettings = await context.SiteSettings.ToListAsync();
                context.SiteSettings.RemoveRange(existingSettings);
                await context.SaveChangesAsync();

                var defaultSettings = new[]
                {
                    // General Settings
                    new SiteSetting { Key = "site_name", DisplayName = "Site Name", Value = "701 Performance Power", Type = SettingType.Text, Category = "General", SortOrder = 1, IsRequired = true },
                    new SiteSetting { Key = "site_tagline", DisplayName = "Site Tagline", Value = "Your Ultimate Powersports Destination", Type = SettingType.Text, Category = "General", SortOrder = 2 },
                    new SiteSetting { Key = "logo_url", DisplayName = "Logo URL", Value = "/images/logo.png", Type = SettingType.Image, Category = "General", SortOrder = 3 },
                    
                    new SiteSetting { Key = "contact_email", DisplayName = "Contact Email", Value = "info@701performancepower.com", Type = SettingType.Email, Category = "General", SortOrder = 4, IsRequired = true },
                    new SiteSetting { Key = "contact_phone", DisplayName = "Contact Phone", Value = "(701) 555-0100", Type = SettingType.Phone, Category = "General", SortOrder = 5 },
                    new SiteSetting { Key = "contact_address", DisplayName = "Contact Address", Value = "123 Powersports Drive, Fargo, ND 58102", Type = SettingType.TextArea, Category = "General", SortOrder = 6 },
                    
                    new SiteSetting { Key = "facebook_url", DisplayName = "Facebook URL", Value = "", Type = SettingType.Url, Category = "Social Media", SortOrder = 1 },
                    new SiteSetting { Key = "instagram_url", DisplayName = "Instagram URL", Value = "", Type = SettingType.Url, Category = "Social Media", SortOrder = 2 },
                    new SiteSetting { Key = "twitter_url", DisplayName = "Twitter URL", Value = "", Type = SettingType.Url, Category = "Social Media", SortOrder = 3 },
                    new SiteSetting { Key = "youtube_url", DisplayName = "YouTube URL", Value = "", Type = SettingType.Url, Category = "Social Media", SortOrder = 4 },
                    
                    new SiteSetting { Key = "hero_title", DisplayName = "Homepage Hero Title", Value = "Premium Powersports Vehicles & Gear", Type = SettingType.Text, Category = "Homepage", SortOrder = 1 },
                    new SiteSetting { Key = "hero_subtitle", DisplayName = "Homepage Hero Subtitle", Value = "Discover our collection of ATVs, dirt bikes, UTVs, snowmobiles, and premium gear for all your outdoor adventures.", Type = SettingType.TextArea, Category = "Homepage", SortOrder = 2 },
                    
                    new SiteSetting { Key = "products_title", DisplayName = "Products Page Title", Value = "Our Products", Type = SettingType.Text, Category = "Products Page", SortOrder = 1 },
                    new SiteSetting { Key = "products_subtitle", DisplayName = "Products Page Subtitle", Value = "Explore our complete collection of powersports vehicles and gear", Type = SettingType.TextArea, Category = "Products Page", SortOrder = 2 },
                    
                    new SiteSetting { Key = "about_title", DisplayName = "About Page Title", Value = "About Us", Type = SettingType.Text, Category = "About Page", SortOrder = 1 },
                    new SiteSetting { Key = "about_subtitle", DisplayName = "About Page Subtitle", Value = "Your trusted partner for powersports adventures", Type = SettingType.TextArea, Category = "About Page", SortOrder = 2 },
                    
                    new SiteSetting { Key = "contact_title", DisplayName = "Contact Page Title", Value = "Contact Us", Type = SettingType.Text, Category = "Contact Page", SortOrder = 1 },
                    new SiteSetting { Key = "contact_subtitle", DisplayName = "Contact Page Subtitle", Value = "Get in touch with our team for questions, support, or to schedule a visit", Type = SettingType.TextArea, Category = "Contact Page", SortOrder = 2 },
                    
                    new SiteSetting { Key = "faq_title", DisplayName = "FAQ Page Title", Value = "FAQ", Type = SettingType.Text, Category = "FAQ Page", SortOrder = 1 },
                    new SiteSetting { Key = "faq_subtitle", DisplayName = "FAQ Page Subtitle", Value = "Frequently Asked Questions", Type = SettingType.TextArea, Category = "FAQ Page", SortOrder = 2 },
                    new SiteSetting { Key = "faq_content", DisplayName = "FAQ Page Content", Value = "<h2>General Questions</h2><p>Default FAQ content goes here.</p>", Type = SettingType.Html, Category = "FAQ Page", SortOrder = 3 },
                    
                    new SiteSetting { Key = "shipping_title", DisplayName = "Shipping & Returns Page Title", Value = "Shipping & Returns", Type = SettingType.Text, Category = "Shipping Page", SortOrder = 1 },
                    new SiteSetting { Key = "shipping_subtitle", DisplayName = "Shipping & Returns Page Subtitle", Value = "Our shipping and return policies", Type = SettingType.TextArea, Category = "Shipping Page", SortOrder = 2 },
                    new SiteSetting { Key = "shipping_content", DisplayName = "Shipping & Returns Page Content", Value = "<h2>Shipping Policy</h2><p>Default shipping policy content goes here.</p>", Type = SettingType.Html, Category = "Shipping Page", SortOrder = 3 },
                    
                    new SiteSetting { Key = "privacy_title", DisplayName = "Privacy Policy Page Title", Value = "Privacy Policy", Type = SettingType.Text, Category = "Privacy Page", SortOrder = 1 },
                    new SiteSetting { Key = "privacy_subtitle", DisplayName = "Privacy Policy Page Subtitle", Value = "How we handle your personal information", Type = SettingType.TextArea, Category = "Privacy Page", SortOrder = 2 },
                    new SiteSetting { Key = "privacy_content", DisplayName = "Privacy Policy Page Content", Value = "<h2>Privacy Policy</h2><p>Default privacy policy content goes here.</p>", Type = SettingType.Html, Category = "Privacy Page", SortOrder = 3 },
                    
                    new SiteSetting { Key = "terms_title", DisplayName = "Terms of Service Page Title", Value = "Terms of Service", Type = SettingType.Text, Category = "Terms Page", SortOrder = 1 },
                    new SiteSetting { Key = "terms_subtitle", DisplayName = "Terms of Service Page Subtitle", Value = "Terms and conditions for using our services", Type = SettingType.TextArea, Category = "Terms Page", SortOrder = 2 },
                    new SiteSetting { Key = "terms_content", DisplayName = "Terms of Service Page Content", Value = "<h2>Terms of Service</h2><p>Default terms of service content goes here.</p>", Type = SettingType.Html, Category = "Terms Page", SortOrder = 3 },
                    
                    new SiteSetting { Key = "session_timeout", DisplayName = "Session Timeout (minutes)", Value = "10", Type = SettingType.Number, Category = "Advanced", SortOrder = 1 },
                    new SiteSetting { Key = "max_login_attempts", DisplayName = "Max Login Attempts", Value = "5", Type = SettingType.Number, Category = "Advanced", SortOrder = 2 },
                    new SiteSetting { Key = "allow_user_registration", DisplayName = "Allow User Registration", Value = "true", Type = SettingType.Boolean, Category = "Advanced", SortOrder = 3 },
                    new SiteSetting { Key = "require_email_verification", DisplayName = "Require Email Verification", Value = "false", Type = SettingType.Boolean, Category = "Advanced", SortOrder = 4 },
                    new SiteSetting { Key = "enable_two_factor_auth", DisplayName = "Enable Two-Factor Authentication", Value = "false", Type = SettingType.Boolean, Category = "Advanced", SortOrder = 5 },
                    new SiteSetting { Key = "allow_guest_checkout", DisplayName = "Allow Guest Checkout", Value = "false", Type = SettingType.Boolean, Category = "Advanced", SortOrder = 6 },
                    
                    new SiteSetting { Key = "image_quality", DisplayName = "Image Quality (%)", Value = "85", Type = SettingType.Number, Category = "Advanced", SortOrder = 7 },
                    new SiteSetting { Key = "cache_duration", DisplayName = "Cache Duration (hours)", Value = "24", Type = SettingType.Number, Category = "Advanced", SortOrder = 8 },
                    new SiteSetting { Key = "max_image_width", DisplayName = "Max Image Width (px)", Value = "1920", Type = SettingType.Number, Category = "Advanced", SortOrder = 9 },
                    new SiteSetting { Key = "max_image_height", DisplayName = "Max Image Height (px)", Value = "1080", Type = SettingType.Number, Category = "Advanced", SortOrder = 10 },
                    new SiteSetting { Key = "enable_image_optimization", DisplayName = "Enable Image Optimization", Value = "true", Type = SettingType.Boolean, Category = "Advanced", SortOrder = 11 },
                    new SiteSetting { Key = "enable_cdn", DisplayName = "Enable CDN", Value = "false", Type = SettingType.Boolean, Category = "Advanced", SortOrder = 12 },
                    new SiteSetting { Key = "enable_compression", DisplayName = "Enable Compression", Value = "true", Type = SettingType.Boolean, Category = "Advanced", SortOrder = 13 },
                    
                    new SiteSetting { Key = "enable_maintenance_mode", DisplayName = "Enable Maintenance Mode", Value = "false", Type = SettingType.Boolean, Category = "Advanced", SortOrder = 14 },

                    // Email (SMTP)
                    new SiteSetting { Key = "smtp_enabled", DisplayName = "Enable Email (SMTP)", Value = "false", Type = SettingType.Boolean, Category = "Email", SortOrder = 1 },
                    new SiteSetting { Key = "smtp_host", DisplayName = "SMTP Host", Value = "", Type = SettingType.Text, Category = "Email", SortOrder = 2 },
                    new SiteSetting { Key = "smtp_port", DisplayName = "SMTP Port", Value = "587", Type = SettingType.Number, Category = "Email", SortOrder = 3 },
                    new SiteSetting { Key = "smtp_username", DisplayName = "SMTP Username", Value = "", Type = SettingType.Text, Category = "Email", SortOrder = 4 },
                    new SiteSetting { Key = "smtp_password", DisplayName = "SMTP Password", Value = "", Type = SettingType.Text, Category = "Email", SortOrder = 5 },
                    new SiteSetting { Key = "smtp_from_email", DisplayName = "From Email Address", Value = "", Type = SettingType.Email, Category = "Email", SortOrder = 6 },
                    new SiteSetting { Key = "smtp_from_name", DisplayName = "From Name", Value = "701 Performance Power", Type = SettingType.Text, Category = "Email", SortOrder = 7 },
                    new SiteSetting { Key = "smtp_use_ssl", DisplayName = "Use SSL/TLS", Value = "true", Type = SettingType.Boolean, Category = "Email", SortOrder = 8 },
                    new SiteSetting { Key = "site_url", DisplayName = "Site URL", Value = "http://localhost:3000", Type = SettingType.Url, Category = "Email", SortOrder = 9 }
                };

                foreach (var setting in defaultSettings)
                {
                    setting.LastModifiedBy = currentUserId;
                    setting.CreatedAt = DateTime.UtcNow;
                    setting.UpdatedAt = DateTime.UtcNow;
                }

                context.SiteSettings.AddRange(defaultSettings);
                await context.SaveChangesAsync();

                return Results.Ok(new { message = $"Successfully reset {defaultSettings.Length} settings to defaults", count = defaultSettings.Length });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { message = $"Failed to reset settings: {ex.Message}" });
            }
        })
        .WithName("ResetSiteSettings")
        .WithSummary("Reset all site settings to defaults (SuperAdmin only)")
        .Produces(200)
        .Produces(400)
        .RequireAuthorization("SuperAdminOnly");

        ConfigurePhotoEndpoints(v1Routes);

        v1Routes.MapGet("/health", () => Results.Ok(new { 
            status = "healthy", 
            version = "v1.0",
            timestamp = DateTime.UtcNow
        }))
        .WithName("HealthCheck")
        .WithTags("Health");

        app.MapGet("/health", () => Results.Ok(new { 
            status = "healthy", 
            apiVersion = "v1.0",
            timestamp = DateTime.UtcNow
        }))
        .WithName("GlobalHealthCheck")
        .WithTags("System");

        DisplayStartupComplete(app);

        // Apply any pending EF migrations before seeding
        using (var migrationScope = app.Services.CreateScope())
        {
            var db = migrationScope.ServiceProvider.GetRequiredService<PowersportsDbContext>();
            await db.Database.MigrateAsync();
        }

        await SeedDatabase(app);

        app.Run();
    }

    private static void ConfigurePhotoEndpoints(RouteGroupBuilder v1Routes)
    {
        v1Routes.MapPost("/photos/upload", async (
            HttpContext context,
            FileService fileService,
            ILogger<Program> logger) =>
        {
            try
            {
                var form = await context.Request.ReadFormAsync();
                var file = form.Files.FirstOrDefault();
                
                if (file == null || file.Length == 0)
                {
                    logger.LogWarning("No file provided in upload request");
                    return Results.BadRequest(new { message = "No file provided" });
                }

                var entityType = form["entityType"].ToString();
                var entityId = form["entityId"].ToString();

                logger.LogInformation("Upload request: {FileName} ({Size} bytes) for {EntityType} {EntityId}", 
                    file.FileName, file.Length, entityType, entityId);

                if (string.IsNullOrEmpty(entityType) || string.IsNullOrEmpty(entityId))
                {
                    logger.LogWarning("Missing entityType or entityId");
                    return Results.BadRequest(new { message = "EntityType and EntityId are required" });
                }

                var result = await fileService.UploadFileAsync(file, entityType, int.Parse(entityId));
                
                if (result.IsSuccess)
                {
                    logger.LogInformation("Upload successful: {FileName}", file.FileName);
                    return Results.Ok(result.Data);
                }

                logger.LogError("Upload failed: {Error}", result.ErrorMessage);
                return Results.BadRequest(new { 
                    success = false, 
                    message = result.ErrorMessage 
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception during upload: {Message}", ex.Message);
                return Results.Problem($"Upload failed: {ex.Message}");
            }
        })
        .WithName("UploadPhoto")
        .WithTags("Photos")
        .WithSummary("Upload a photo for a product or category")
        .Produces(200)
        .Produces(400)
        .DisableAntiforgery()
        .RequireAuthorization();

        v1Routes.MapDelete("/photos/{entityType}/{entityId}/{fileName}", async (
            string entityType,
            int entityId,
            string fileName,
            FileService fileService) =>
        {
            try
            {
                var result = await fileService.DeleteFileAsync(fileName, entityType, entityId);
                
                if (result.IsSuccess)
                {
                    return Results.Ok(new { 
                        success = true, 
                        message = "File deleted successfully" 
                    });
                }

                return Results.BadRequest(new { 
                    success = false, 
                    message = result.ErrorMessage 
                });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Delete failed: {ex.Message}");
            }
        })
        .WithName("DeletePhoto")
        .WithTags("Photos")
        .WithSummary("Delete a photo for a product or category")
        .Produces(200)
        .Produces(400)
        .RequireAuthorization();

        v1Routes.MapGet("/photos/{entityType}/{entityId}", async (
            string entityType,
            int entityId,
            FileService fileService) =>
        {
            try
            {
                var files = await fileService.GetEntityFilesAsync(entityType, entityId);
                return Results.Ok(new { 
                    success = true, 
                    files = files.Select(f => new {
                        fileName = f.fileName,
                        fileSize = f.fileSize,
                        uploadDate = f.uploadDate,
                        isDefault = f.isDefault
                    })
                });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Failed to retrieve files: {ex.Message}");
            }
        })
        .WithName("GetEntityPhotos")
        .WithTags("Photos")
        .WithSummary("Get all photos for a specific entity (product or category)")
        .Produces(200);

        v1Routes.MapPatch("/photos/{entityType}/{entityId}/{fileName}/default", async (
            string entityType,
            int entityId,
            string fileName,
            FileService fileService) =>
        {
            try
            {
                var result = await fileService.SetDefaultImageAsync(fileName, entityType, entityId);
                
                if (result.IsSuccess)
                {
                    return Results.Ok(new { 
                        success = true, 
                        message = "Default image set successfully" 
                    });
                }

                return Results.BadRequest(new { 
                    success = false, 
                    message = result.ErrorMessage 
                });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Failed to set default image: {ex.Message}");
            }
        })
        .WithName("SetDefaultPhoto")
        .WithTags("Photos")
        .WithSummary("Set a photo as the default for an entity")
        .Produces(200)
        .Produces(400)
        .RequireAuthorization();
    }

    private static void DisplayStartupBanner()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("========================================");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Backend API Server");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("========================================");
        Console.ResetColor();
        Console.WriteLine();
    }

    private static void DisplaySeparator()
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("========================================");
        Console.ResetColor();
    }

    private static void LogInfo(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write($"[{timestamp} INF] ");
        Console.ResetColor();
        Console.WriteLine(message);
    }

    private static void LogWarning(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write($"[{timestamp} WRN] ");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"!!! WARNING: {message} !!!");
        Console.ResetColor();
    }

    private static void DisplayStartupComplete(WebApplication app)
    {
        DisplaySeparator();
        
        var urls = app.Urls.Any() ? string.Join(", ", app.Urls) : "http://localhost:5226";
        
        LogInfo($"Swagger UI: {urls}/swagger");
        LogInfo($"API v1 URL: {urls}/api/v1");
        LogInfo($"Health Check: {urls}/api/v1/health");
        LogInfo($"Global Health: {urls}/health");
        
        DisplaySeparator();
        Console.WriteLine();
    }

    private static async Task SeedDatabase(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PowersportsDbContext>();

        await context.Database.EnsureCreatedAsync();

        var existingSuperAdmin = await context.Users
            .FirstOrDefaultAsync(u => u.Email == "seannytheirish@gmail.com");

        if (existingSuperAdmin == null)
        {
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("SuperAdmin@123");
            
            var superAdmin = new User
            {
                FirstName = "Patrick",
                LastName = "Farrell", 
                Email = "seannytheirish@gmail.com",
                PasswordHash = hashedPassword,
                Role = UserRole.SuperAdmin,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(superAdmin);
            await context.SaveChangesAsync();

            Console.WriteLine("✅ Super Admin user created successfully!");
            Console.WriteLine($"   Email: {superAdmin.Email}");
            Console.WriteLine($"   Name: {superAdmin.FirstName} {superAdmin.LastName}");
            Console.WriteLine($"   Role: {superAdmin.Role}");
        }
        else
        {
            Console.WriteLine("ℹ️  Super Admin user already exists, skipping seeding.");
        }

        Console.WriteLine("Checking for site settings...");
        var settingsCount = await context.SiteSettings.CountAsync();
        Console.WriteLine($"Found {settingsCount} existing settings in database");
        
        var defaultSettings = new[]
            {
                // General Settings
                new SiteSetting { Key = "site_name", DisplayName = "Site Name", Value = "701 Performance Power", Type = SettingType.Text, Category = "General", SortOrder = 1, IsRequired = true },
                new SiteSetting { Key = "site_tagline", DisplayName = "Site Tagline", Value = "Your Ultimate Powersports Destination", Type = SettingType.Text, Category = "General", SortOrder = 2 },
                new SiteSetting { Key = "logo_url", DisplayName = "Logo URL", Value = "/images/logo.png", Type = SettingType.Image, Category = "General", SortOrder = 3 },
                
                // Contact Settings  
                new SiteSetting { Key = "contact_email", DisplayName = "Contact Email", Value = "info@701performancepower.com", Type = SettingType.Email, Category = "General", SortOrder = 4, IsRequired = true },
                new SiteSetting { Key = "contact_phone", DisplayName = "Contact Phone", Value = "(701) 555-0100", Type = SettingType.Phone, Category = "General", SortOrder = 5 },
                new SiteSetting { Key = "contact_address", DisplayName = "Contact Address", Value = "123 Powersports Drive, Fargo, ND 58102", Type = SettingType.TextArea, Category = "General", SortOrder = 6 },
                
                // Social Media
                new SiteSetting { Key = "facebook_url", DisplayName = "Facebook URL", Value = "", Type = SettingType.Url, Category = "Social Media", SortOrder = 1 },
                new SiteSetting { Key = "instagram_url", DisplayName = "Instagram URL", Value = "", Type = SettingType.Url, Category = "Social Media", SortOrder = 2 },
                new SiteSetting { Key = "twitter_url", DisplayName = "Twitter URL", Value = "", Type = SettingType.Url, Category = "Social Media", SortOrder = 3 },
                new SiteSetting { Key = "youtube_url", DisplayName = "YouTube URL", Value = "", Type = SettingType.Url, Category = "Social Media", SortOrder = 4 },
                
                // Homepage Content
                new SiteSetting { Key = "hero_title", DisplayName = "Homepage Hero Title", Value = "Premium Powersports Vehicles & Gear", Type = SettingType.Text, Category = "Homepage", SortOrder = 1 },
                new SiteSetting { Key = "hero_subtitle", DisplayName = "Homepage Hero Subtitle", Value = "Discover our collection of ATVs, dirt bikes, UTVs, snowmobiles, and premium gear for all your outdoor adventures.", Type = SettingType.TextArea, Category = "Homepage", SortOrder = 2 },
                
                // Products Page
                new SiteSetting { Key = "products_title", DisplayName = "Products Page Title", Value = "Our Products", Type = SettingType.Text, Category = "Products Page", SortOrder = 1 },
                new SiteSetting { Key = "products_subtitle", DisplayName = "Products Page Subtitle", Value = "Explore our complete collection of powersports vehicles and gear", Type = SettingType.TextArea, Category = "Products Page", SortOrder = 2 },
                
                // About Page
                new SiteSetting { Key = "about_title", DisplayName = "About Page Title", Value = "About Us", Type = SettingType.Text, Category = "About Page", SortOrder = 1 },
                new SiteSetting { Key = "about_subtitle", DisplayName = "About Page Subtitle", Value = "Your trusted partner for powersports adventures", Type = SettingType.TextArea, Category = "About Page", SortOrder = 2 },
                
                // Contact Page
                new SiteSetting { Key = "contact_title", DisplayName = "Contact Page Title", Value = "Contact Us", Type = SettingType.Text, Category = "Contact Page", SortOrder = 1 },
                new SiteSetting { Key = "contact_subtitle", DisplayName = "Contact Page Subtitle", Value = "Get in touch with our team for questions, support, or to schedule a visit", Type = SettingType.TextArea, Category = "Contact Page", SortOrder = 2 },
                
                // FAQ Page
                new SiteSetting { Key = "faq_title", DisplayName = "FAQ Page Title", Value = "FAQ", Type = SettingType.Text, Category = "FAQ Page", SortOrder = 1 },
                new SiteSetting { Key = "faq_subtitle", DisplayName = "FAQ Page Subtitle", Value = "Frequently Asked Questions", Type = SettingType.TextArea, Category = "FAQ Page", SortOrder = 2 },
                new SiteSetting { Key = "faq_content", DisplayName = "FAQ Page Content", Value = "<h2>General Questions</h2><p>Default FAQ content goes here.</p>", Type = SettingType.Html, Category = "FAQ Page", SortOrder = 3 },
                
                // Shipping & Returns Page
                new SiteSetting { Key = "shipping_title", DisplayName = "Shipping & Returns Page Title", Value = "Shipping & Returns", Type = SettingType.Text, Category = "Shipping Page", SortOrder = 1 },
                new SiteSetting { Key = "shipping_subtitle", DisplayName = "Shipping & Returns Page Subtitle", Value = "Our shipping and return policies", Type = SettingType.TextArea, Category = "Shipping Page", SortOrder = 2 },
                new SiteSetting { Key = "shipping_content", DisplayName = "Shipping & Returns Page Content", Value = "<h2>Shipping Policy</h2><p>Default shipping policy content goes here.</p>", Type = SettingType.Html, Category = "Shipping Page", SortOrder = 3 },
                
                // Privacy Policy Page
                new SiteSetting { Key = "privacy_title", DisplayName = "Privacy Policy Page Title", Value = "Privacy Policy", Type = SettingType.Text, Category = "Privacy Page", SortOrder = 1 },
                new SiteSetting { Key = "privacy_subtitle", DisplayName = "Privacy Policy Page Subtitle", Value = "How we handle your personal information", Type = SettingType.TextArea, Category = "Privacy Page", SortOrder = 2 },
                new SiteSetting { Key = "privacy_content", DisplayName = "Privacy Policy Page Content", Value = "<h2>Privacy Policy</h2><p>Default privacy policy content goes here.</p>", Type = SettingType.Html, Category = "Privacy Page", SortOrder = 3 },
                
                // Terms of Service Page
                new SiteSetting { Key = "terms_title", DisplayName = "Terms of Service Page Title", Value = "Terms of Service", Type = SettingType.Text, Category = "Terms Page", SortOrder = 1 },
                new SiteSetting { Key = "terms_subtitle", DisplayName = "Terms of Service Page Subtitle", Value = "Terms and conditions for using our services", Type = SettingType.TextArea, Category = "Terms Page", SortOrder = 2 },
                new SiteSetting { Key = "terms_content", DisplayName = "Terms of Service Page Content", Value = "<h2>Terms of Service</h2><p>Default terms of service content goes here.</p>", Type = SettingType.Html, Category = "Terms Page", SortOrder = 3 },
                
                // Advanced - Security & Access
                new SiteSetting { Key = "session_timeout", DisplayName = "Session Timeout (minutes)", Value = "10", Type = SettingType.Number, Category = "Advanced", SortOrder = 1 },
                new SiteSetting { Key = "max_login_attempts", DisplayName = "Max Login Attempts", Value = "5", Type = SettingType.Number, Category = "Advanced", SortOrder = 2 },
                new SiteSetting { Key = "allow_user_registration", DisplayName = "Allow User Registration", Value = "true", Type = SettingType.Boolean, Category = "Advanced", SortOrder = 3 },
                new SiteSetting { Key = "require_email_verification", DisplayName = "Require Email Verification", Value = "false", Type = SettingType.Boolean, Category = "Advanced", SortOrder = 4 },
                new SiteSetting { Key = "enable_two_factor_auth", DisplayName = "Enable Two-Factor Authentication", Value = "false", Type = SettingType.Boolean, Category = "Advanced", SortOrder = 5 },
                new SiteSetting { Key = "allow_guest_checkout", DisplayName = "Allow Guest Checkout", Value = "false", Type = SettingType.Boolean, Category = "Advanced", SortOrder = 6 },
                
                // Advanced - Performance & Caching
                new SiteSetting { Key = "image_quality", DisplayName = "Image Quality (%)", Value = "85", Type = SettingType.Number, Category = "Advanced", SortOrder = 7 },
                new SiteSetting { Key = "cache_duration", DisplayName = "Cache Duration (hours)", Value = "24", Type = SettingType.Number, Category = "Advanced", SortOrder = 8 },
                new SiteSetting { Key = "max_image_width", DisplayName = "Max Image Width (px)", Value = "1920", Type = SettingType.Number, Category = "Advanced", SortOrder = 9 },
                new SiteSetting { Key = "max_image_height", DisplayName = "Max Image Height (px)", Value = "1080", Type = SettingType.Number, Category = "Advanced", SortOrder = 10 },
                new SiteSetting { Key = "enable_image_optimization", DisplayName = "Enable Image Optimization", Value = "true", Type = SettingType.Boolean, Category = "Advanced", SortOrder = 11 },
                new SiteSetting { Key = "enable_cdn", DisplayName = "Enable CDN", Value = "false", Type = SettingType.Boolean, Category = "Advanced", SortOrder = 12 },
                new SiteSetting { Key = "enable_compression", DisplayName = "Enable Compression", Value = "true", Type = SettingType.Boolean, Category = "Advanced", SortOrder = 13 },
                
                // Advanced - System Settings
                new SiteSetting { Key = "enable_maintenance_mode", DisplayName = "Enable Maintenance Mode", Value = "false", Type = SettingType.Boolean, Category = "Advanced", SortOrder = 14 },

                // Email (SMTP)
                new SiteSetting { Key = "smtp_enabled", DisplayName = "Enable Email (SMTP)", Value = "false", Type = SettingType.Boolean, Category = "Email", SortOrder = 1 },
                new SiteSetting { Key = "smtp_host", DisplayName = "SMTP Host", Value = "", Type = SettingType.Text, Category = "Email", SortOrder = 2 },
                new SiteSetting { Key = "smtp_port", DisplayName = "SMTP Port", Value = "587", Type = SettingType.Number, Category = "Email", SortOrder = 3 },
                new SiteSetting { Key = "smtp_username", DisplayName = "SMTP Username", Value = "", Type = SettingType.Text, Category = "Email", SortOrder = 4 },
                new SiteSetting { Key = "smtp_password", DisplayName = "SMTP Password", Value = "", Type = SettingType.Text, Category = "Email", SortOrder = 5 },
                new SiteSetting { Key = "smtp_from_email", DisplayName = "From Email Address", Value = "", Type = SettingType.Email, Category = "Email", SortOrder = 6 },
                new SiteSetting { Key = "smtp_from_name", DisplayName = "From Name", Value = "701 Performance Power", Type = SettingType.Text, Category = "Email", SortOrder = 7 },
                new SiteSetting { Key = "smtp_use_ssl", DisplayName = "Use SSL/TLS", Value = "true", Type = SettingType.Boolean, Category = "Email", SortOrder = 8 },
                new SiteSetting { Key = "site_url", DisplayName = "Site URL", Value = "http://localhost:3000", Type = SettingType.Url, Category = "Email", SortOrder = 9 }
            };

            var superAdminId = existingSuperAdmin?.Id ?? 1;

            var existingKeys = await context.SiteSettings.Select(s => s.Key).ToListAsync();
            var missingSettings = defaultSettings.Where(s => !existingKeys.Contains(s.Key)).ToList();

            if (missingSettings.Any())
            {
                Console.WriteLine($"Adding {missingSettings.Count} missing settings...");
                
                foreach (var setting in missingSettings)
                {
                    setting.LastModifiedBy = superAdminId;
                    setting.UpdatedAt = DateTime.UtcNow;
                    setting.CreatedAt = DateTime.UtcNow;
                    Console.WriteLine($"  + {setting.Key}");
                }

                context.SiteSettings.AddRange(missingSettings);
                await context.SaveChangesAsync();

                Console.WriteLine($"✅ Added {missingSettings.Count} new settings successfully!");
            }
            else if (settingsCount == 0)
            {
                Console.WriteLine("Seeding all settings for first time...");
                
                foreach (var setting in defaultSettings)
                {
                    setting.LastModifiedBy = superAdminId;
                    setting.UpdatedAt = DateTime.UtcNow;
                    setting.CreatedAt = DateTime.UtcNow;
                }

                context.SiteSettings.AddRange(defaultSettings);
                await context.SaveChangesAsync();

                Console.WriteLine($"✅ Created {defaultSettings.Length} default site settings successfully!");
            }
            else
            {
                Console.WriteLine("✅ All settings are up to date!");
            }
    }

}
