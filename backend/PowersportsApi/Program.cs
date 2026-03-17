
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO.Compression;
using System.Security.Claims;
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

        var connectionString = configuration.GetConnectionString("DefaultConnection")!;
        var dbProvider = configuration["DatabaseProvider"] ??
            (builder.Environment.IsProduction() ? "MySQL" : "SqlServer");

        builder.Services.AddDbContext<PowersportsDbContext>(options =>
        {
            if (dbProvider == "MySQL")
                options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0)));
            else
                options.UseSqlServer(connectionString);
        });

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ProductService>();
        builder.Services.AddScoped<AuthService>();
        builder.Services.AddScoped<FileService>();
        builder.Services.AddScoped<EmailService>();

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET")
            ?? jwtSettings["SecretKey"];
        if (string.IsNullOrWhiteSpace(secretKey))
            throw new InvalidOperationException("JWT secret key is not configured. Set the JWT_SECRET environment variable.");
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

        // Initialize system media sections
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<PowersportsDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            
            try
            {
                // Ensure database is created and migrations are applied
                await context.Database.MigrateAsync();
                
                // Create system sections if they don't exist
                var systemSections = new[]
                {
                    new { Name = "General", Description = "General purpose media files", DisplayOrder = 0 },
                    new { Name = "Team Members", Description = "Team member photos and profiles", DisplayOrder = 1 },
                    new { Name = "Hero Images", Description = "Hero section background images", DisplayOrder = 2 },
                    new { Name = "Banners", Description = "Promotional banners and graphics", DisplayOrder = 3 }
                };
                
                foreach (var section in systemSections)
                {
                    var exists = await context.MediaSections.AnyAsync(s => s.Name == section.Name);
                    if (!exists)
                    {
                        context.MediaSections.Add(new MediaSection
                        {
                            Name = section.Name,
                            Description = section.Description,
                            DisplayOrder = section.DisplayOrder,
                            IsSystem = true,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        });
                        logger.LogInformation("Created system media section: {SectionName}", section.Name);
                    }
                }
                
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error initializing media sections");
            }
        }

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
            var productsDto = products.Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                Category = p.Category?.Name ?? "",
                p.CategoryId,
                p.ImageUrl,
                p.IsFeatured,
                p.IsActive,
                p.Sku,
                p.StockQuantity,
                p.LowStockThreshold,
                p.CostPrice,
                p.Specifications,
                p.CreatedAt,
                p.UpdatedAt,
                ProductImages = p.ProductImages?.OrderBy(pi => pi.SortOrder).Select(pi => new
                {
                    pi.Id,
                    pi.ProductId,
                    pi.MediaFileId,
                    pi.IsMain,
                    pi.SortOrder,
                    Url = pi.MediaFile?.FilePath ?? "",
                    ThumbnailUrl = pi.MediaFile?.ThumbnailPath
                }).ToList()
            });
            return Results.Ok(productsDto);
        })
        .WithName("GetAllProducts")
        .WithSummary("Get all powersports products")
        .Produces<List<Product>>(200);

        productRoutes.MapGet("/{id:int}", async (ProductService productService, int id) =>
        {
            var product = await productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return Results.NotFound($"Product with ID {id} not found");
            }
            
            var productDto = new
            {
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                Category = product.Category?.Name ?? "",
                product.CategoryId,
                product.ImageUrl,
                product.IsFeatured,
                product.IsActive,
                product.Sku,
                product.StockQuantity,
                product.LowStockThreshold,
                product.CostPrice,
                product.CreatedAt,
                product.UpdatedAt,
                ProductImages = product.ProductImages?.OrderBy(pi => pi.SortOrder).Select(pi => new
                {
                    pi.Id,
                    pi.ProductId,
                    pi.MediaFileId,
                    pi.IsMain,
                    pi.SortOrder,
                    Url = pi.MediaFile?.FilePath ?? "",
                    ThumbnailUrl = pi.MediaFile?.ThumbnailPath
                }).ToList()
            };
            return Results.Ok(productDto);
        })
        .WithName("GetProductById")
        .WithSummary("Get a specific product by ID")
        .Produces<Product>(200)
        .Produces(404);

        productRoutes.MapGet("/category/{category}", async (ProductService productService, string category) =>
        {
            var products = await productService.GetProductsByCategoryAsync(category);
            var productsDto = products.Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                Category = p.Category?.Name ?? "",
                p.CategoryId,
                p.ImageUrl,
                p.IsFeatured,
                p.IsActive,
                p.Sku,
                p.StockQuantity,
                p.LowStockThreshold,
                p.CostPrice,
                p.CreatedAt,
                p.UpdatedAt,
                ProductImages = p.ProductImages?.OrderBy(pi => pi.SortOrder).Select(pi => new
                {
                    pi.Id,
                    pi.ProductId,
                    pi.MediaFileId,
                    pi.IsMain,
                    pi.SortOrder,
                    Url = pi.MediaFile?.FilePath ?? "",
                    ThumbnailUrl = pi.MediaFile?.ThumbnailPath
                }).ToList()
            });
            return Results.Ok(productsDto);
        })
        .WithName("GetProductsByCategory")
        .WithSummary("Get products by category (ATV, Dirtbike, UTV, Snowmobile, Gear)")
        .Produces<List<Product>>(200);

        productRoutes.MapGet("/featured", async (ProductService productService) =>
        {
            var products = await productService.GetFeaturedProductsAsync();
            var productsDto = products.Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.Specifications,
                Category = p.Category?.Name ?? "",
                p.CategoryId,
                p.ImageUrl,
                p.IsFeatured,
                p.IsActive,
                p.Sku,
                p.StockQuantity,
                p.LowStockThreshold,
                p.CostPrice,
                p.CreatedAt,
                p.UpdatedAt,
                ProductImages = p.ProductImages?.OrderBy(pi => pi.SortOrder).Select(pi => new
                {
                    pi.Id,
                    pi.ProductId,
                    pi.MediaFileId,
                    pi.IsMain,
                    pi.SortOrder,
                    Url = pi.MediaFile?.FilePath ?? "",
                    ThumbnailUrl = pi.MediaFile?.ThumbnailPath
                }).ToList()
            });
            return Results.Ok(productsDto);
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
                    Specifications = request.Specifications,
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
                existingProduct.Specifications = request.Specifications;
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

        authRoutes.MapGet("/me", async (HttpContext context, AuthService authService, PowersportsDbContext db) =>
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

                // Stamp last-seen — properly awaited so context isn't disposed first
                await db.Users
                    .Where(u => u.Id == userId)
                    .ExecuteUpdateAsync(s => s.SetProperty(u => u.LastSeenAt, DateTime.UtcNow));

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

        authRoutes.MapPost("/refresh-token", async (RefreshTokenRequest request, AuthService authService) =>
        {
            var result = await authService.RefreshTokenAsync(request);
            if (!result.IsSuccess)
                return Results.Unauthorized();
            return Results.Ok(result.Data);
        })
        .WithName("RefreshToken")
        .WithSummary("Refresh an expired access token using a refresh token")
        .Produces<AuthResponse>(200)
        .Produces(401)
        .AllowAnonymous();

        // Issue a new, independent refresh token without consuming any existing one.
        // Used by the admin on first load so it gets its own token, separate from the frontend's.
        authRoutes.MapPost("/issue-refresh-token", async (AuthService authService, HttpContext httpContext) =>
        {
            var userIdClaim = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            var newRefreshToken = await authService.IssueRefreshTokenAsync(userId);
            return Results.Ok(new { refreshToken = newRefreshToken });
        })
        .WithName("IssueRefreshToken")
        .WithSummary("Issue a new independent refresh token for a new client session (admin)")
        .Produces(200)
        .Produces(401)
        .RequireAuthorization();

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

        categoryRoutes.MapPost("/", async (CreateCategoryRequest request, PowersportsDbContext context, ILogger<Program> logger) =>
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
                
                // Auto-create a media section for this category
                var mediaSection = new MediaSection
                {
                    Name = category.Name,
                    Description = $"Media files for {category.Name} category",
                    CategoryId = category.Id,
                    IsSystem = false,
                    IsActive = true,
                    DisplayOrder = 100, // User categories start at 100
                    CreatedAt = DateTime.UtcNow
                };
                
                context.MediaSections.Add(mediaSection);
                await context.SaveChangesAsync();
                
                logger.LogInformation("Created category {CategoryId} and auto-created media section {SectionId}", category.Id, mediaSection.Id);
                
                return Results.Created($"/api/v1/categories/{category.Id}", category);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create category");
                return Results.BadRequest(new { message = $"Failed to create category: {ex.Message}" });
            }
        })
        .WithName("CreateCategory")
        .WithSummary("Create a new category (Admin+ only)")
        .Produces<Category>(201)
        .Produces(400)
        .RequireAuthorization("AdminOnly");

        categoryRoutes.MapPut("/{id:int}", async (int id, UpdateCategoryRequest request, PowersportsDbContext context, ILogger<Program> logger) =>
        {
            try
            {
                var existingCategory = await context.Categories.FindAsync(id);
                if (existingCategory == null)
                {
                    return Results.NotFound($"Category with ID {id} not found");
                }
                
                var oldName = existingCategory.Name;
                existingCategory.Name = request.Name;
                existingCategory.Description = request.Description;
                existingCategory.ImageUrl = request.ImageUrl;
                existingCategory.IsActive = request.IsActive;
                existingCategory.UpdatedAt = DateTime.UtcNow;
                
                // Update corresponding media section name if category name changed
                if (oldName != request.Name)
                {
                    var mediaSection = await context.MediaSections
                        .FirstOrDefaultAsync(s => s.CategoryId == id);
                    
                    if (mediaSection != null)
                    {
                        mediaSection.Name = request.Name;
                        mediaSection.Description = $"Media files for {request.Name} category";
                        mediaSection.UpdatedAt = DateTime.UtcNow;
                        logger.LogInformation("Updated media section {SectionId} name to match category {CategoryId}", mediaSection.Id, id);
                    }
                }
                
                await context.SaveChangesAsync();
                
                return Results.Ok(existingCategory);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update category {CategoryId}", id);
                return Results.BadRequest(new { message = $"Failed to update category: {ex.Message}" });
            }
        })
        .WithName("UpdateCategory")
        .WithSummary("Update an existing category (Admin+ only)")
        .Produces<Category>(200)
        .Produces(404)
        .Produces(400)
        .RequireAuthorization("AdminOnly");

        categoryRoutes.MapDelete("/{id:int}", async (
            int id, 
            PowersportsDbContext context,
            FileService fileService,
            ILogger<Program> logger) =>
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
                    // Delete associated media section, files, and folder
                    var (success, deletedFileCount, errorMessage) = await fileService.DeleteEntityMediaSectionAsync(id, "Category");
                    
                    if (!success)
                    {
                        logger.LogWarning("Failed to delete media section for category {CategoryId}: {Error}", id, errorMessage);
                        // Continue with category deletion even if media section deletion fails
                    }
                    
                    // Delete the category
                    context.Categories.Remove(category);
                    await context.SaveChangesAsync();
                    
                    var message = deletedFileCount > 0 
                        ? $"Category '{category.Name}' and {deletedFileCount} associated file(s) deleted successfully"
                        : $"Category '{category.Name}' deleted successfully";
                    
                    return Results.Ok(new { message });
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to delete category {CategoryId}", id);
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
                    u.LastSeenAt,
                    u.LastLoginIp,
                    u.IsActive,
                    u.SubscribeNewsletter,
                    u.IsEmailVerified
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
                    u.LastSeenAt,
                    u.LastLoginIp,
                    u.IsActive,
                    u.SubscribeNewsletter,
                    u.IsEmailVerified
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
                user.IsEmailVerified = request.IsEmailVerified;
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
        v1Routes.MapPost("/contact", async (ContactRequest request, EmailService emailService, PowersportsDbContext context) =>
        {
            // Save submission to database
            var submission = new ContactSubmission
            {
                Name = request.Name,
                Email = request.Email,
                Subject = request.Subject,
                Message = request.Message,
                Status = ContactStatus.New,
                CreatedAt = DateTime.UtcNow
            };
            
            context.ContactSubmissions.Add(submission);
            await context.SaveChangesAsync();
            
            // Send email notification
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
                    new SiteSetting { Key = "home_features", DisplayName = "Why Choose Us Features", Value = "[{\"icon\":\"🛍️\",\"title\":\"Wide Selection\",\"description\":\"From ATVs to snowmobiles, we have everything you need for your next adventure.\"},{\"icon\":\"⭐\",\"title\":\"Quality Brands\",\"description\":\"We partner with top manufacturers to bring you reliable, high-performance vehicles.\"},{\"icon\":\"🔧\",\"title\":\"Expert Support\",\"description\":\"Our knowledgeable team is here to help you find the perfect gear for your needs.\"}]", Type = SettingType.TextArea, Category = "Homepage", SortOrder = 3 },
                    new SiteSetting { Key = "partner_brands", DisplayName = "Partner Brands", Value = "[{\"name\":\"Vitacci\",\"logoUrl\":\"\",\"website\":\"\"},{\"name\":\"Apollo\",\"logoUrl\":\"\",\"website\":\"\"},{\"name\":\"Moto Morini\",\"logoUrl\":\"\",\"website\":\"\"},{\"name\":\"BLP Moto\",\"logoUrl\":\"\",\"website\":\"\"},{\"name\":\"Icebear\",\"logoUrl\":\"\",\"website\":\"\"}]", Type = SettingType.TextArea, Category = "Homepage", SortOrder = 4 },
                    new SiteSetting { Key = "brands_section_title", DisplayName = "Brands Section Title", Value = "Brands We Carry", Type = SettingType.Text, Category = "Homepage", SortOrder = 5 },
                    new SiteSetting { Key = "brands_section_subtitle", DisplayName = "Brands Section Subtitle", Value = "We partner with industry-leading manufacturers to bring you the best powersports vehicles", Type = SettingType.Text, Category = "Homepage", SortOrder = 6 },
                    
                    new SiteSetting { Key = "products_title", DisplayName = "Products Page Title", Value = "Our Products", Type = SettingType.Text, Category = "Products Page", SortOrder = 1 },
                    new SiteSetting { Key = "products_subtitle", DisplayName = "Products Page Subtitle", Value = "Explore our complete collection of powersports vehicles and gear", Type = SettingType.TextArea, Category = "Products Page", SortOrder = 2 },
                    
                    new SiteSetting { Key = "about_title", DisplayName = "About Page Title", Value = "About Us", Type = SettingType.Text, Category = "About Page", SortOrder = 1 },
                    new SiteSetting { Key = "about_subtitle", DisplayName = "About Page Subtitle", Value = "Your trusted partner for powersports adventures", Type = SettingType.TextArea, Category = "About Page", SortOrder = 2 },
                    new SiteSetting { Key = "about_story_paragraph1", DisplayName = "Our Story - Paragraph 1", Value = "Founded with a passion for adventure and the great outdoors, Powersports Gear & Vehicles has been serving the powersports community for over a decade. We specialize in high-quality ATVs, dirt bikes, UTVs, snowmobiles, and gear for adventure seekers who demand the best.", Type = SettingType.TextArea, Category = "About Page", SortOrder = 3 },
                    new SiteSetting { Key = "about_story_paragraph2", DisplayName = "Our Story - Paragraph 2", Value = "Our journey began when our founder, an avid off-road enthusiast, recognized the need for a reliable source of premium powersports equipment. Today, we've grown into a trusted name in the industry, serving customers across the country with top-tier products and exceptional service.", Type = SettingType.TextArea, Category = "About Page", SortOrder = 4 },
                    new SiteSetting { Key = "about_story_image", DisplayName = "Our Story - Image", Value = "https://images.unsplash.com/photo-1558618047-6c0c841469ed?w=600&h=400&fit=crop", Type = SettingType.Image, Category = "About Page", SortOrder = 5 },
                    new SiteSetting { Key = "about_mission_image", DisplayName = "Our Mission - Image", Value = "https://images.unsplash.com/photo-1591737622611-b6c5ae78542d?w=600&h=400&fit=crop", Type = SettingType.Image, Category = "About Page", SortOrder = 6 },
                    new SiteSetting { Key = "about_mission_text", DisplayName = "Our Mission - Main Text", Value = "To empower outdoor enthusiasts with the finest powersports vehicles and gear, ensuring every adventure is safe, thrilling, and unforgettable. We believe that the right equipment doesn't just enhance your experience—it transforms it.", Type = SettingType.TextArea, Category = "About Page", SortOrder = 7 },
                    new SiteSetting { Key = "about_mission_points", DisplayName = "Our Mission - Key Points", Value = "[{\"icon\":\"🎯\",\"title\":\"Quality First\",\"description\":\"We partner only with trusted manufacturers who share our commitment to excellence.\"},{\"icon\":\"🤝\",\"title\":\"Customer Focus\",\"description\":\"Your satisfaction drives everything we do, from product selection to after-sales support.\"},{\"icon\":\"🌟\",\"title\":\"Innovation\",\"description\":\"We stay ahead of industry trends to bring you the latest and greatest in powersports technology.\"}]", Type = SettingType.TextArea, Category = "About Page", SortOrder = 8 },
                    new SiteSetting { Key = "about_values", DisplayName = "Our Values", Value = "[{\"icon\":\"🛡️\",\"title\":\"Safety\",\"description\":\"Safety is paramount in everything we do. We provide only certified, tested equipment and comprehensive safety information.\"},{\"icon\":\"🌍\",\"title\":\"Sustainability\",\"description\":\"We're committed to responsible practices that preserve the natural environments we love to explore.\"},{\"icon\":\"🚀\",\"title\":\"Performance\",\"description\":\"We deliver products that perform when it matters most, built to handle any terrain and weather condition.\"}]", Type = SettingType.TextArea, Category = "About Page", SortOrder = 9 },
                    new SiteSetting { Key = "about_team_members", DisplayName = "Team Members", Value = "[{\"name\":\"Mike Johnson\",\"role\":\"Founder & CEO\",\"bio\":\"20+ years in powersports with a passion for bringing the best products to fellow enthusiasts.\",\"imageUrl\":\"https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=300&h=300&fit=crop&crop=face\"},{\"name\":\"Sarah Williams\",\"role\":\"Head of Sales\",\"bio\":\"Expert in matching customers with their perfect vehicle, with 15 years of industry experience.\",\"imageUrl\":\"https://images.unsplash.com/photo-1494790108755-2616b612b786?w=300&h=300&fit=crop&crop=face\"},{\"name\":\"Tom Rodriguez\",\"role\":\"Service Manager\",\"bio\":\"Certified technician ensuring every vehicle meets our rigorous quality and safety standards.\",\"imageUrl\":\"https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=300&h=300&fit=crop&crop=face\"}]", Type = SettingType.TextArea, Category = "About Page", SortOrder = 10 },
                    
                    new SiteSetting { Key = "contact_title", DisplayName = "Contact Page Title", Value = "Contact Us", Type = SettingType.Text, Category = "Contact Page", SortOrder = 1 },
                    new SiteSetting { Key = "contact_subtitle", DisplayName = "Contact Page Subtitle", Value = "Get in touch with our team for questions, support, or to schedule a visit", Type = SettingType.TextArea, Category = "Contact Page", SortOrder = 2 },
                    new SiteSetting { Key = "contact_section_title", DisplayName = "Contact Cards Section Title", Value = "Get in Touch", Type = SettingType.Text, Category = "Contact Page", SortOrder = 3 },
                    new SiteSetting { Key = "contact_address_title", DisplayName = "Address Card Title", Value = "Visit Our Showroom", Type = SettingType.Text, Category = "Contact Page", SortOrder = 4 },
                    new SiteSetting { Key = "contact_phone_title", DisplayName = "Phone Card Title", Value = "Call Us", Type = SettingType.Text, Category = "Contact Page", SortOrder = 5 },
                    new SiteSetting { Key = "contact_email_title", DisplayName = "Email Card Title", Value = "Email Us", Type = SettingType.Text, Category = "Contact Page", SortOrder = 6 },
                    new SiteSetting { Key = "contact_livechat_title", DisplayName = "Live Chat Card Title", Value = "Live Chat", Type = SettingType.Text, Category = "Contact Page", SortOrder = 7 },
                    new SiteSetting { Key = "contact_hours", DisplayName = "Business Hours", Value = "Mon-Fri: 9AM-6PM\nSat: 9AM-4PM", Type = SettingType.TextArea, Category = "Contact Page", SortOrder = 8 },
                    new SiteSetting { Key = "contact_address_note", DisplayName = "Address Note", Value = "Open for personal consultations", Type = SettingType.Text, Category = "Contact Page", SortOrder = 9 },
                    new SiteSetting { Key = "contact_email_note", DisplayName = "Email Response Note", Value = "We respond within 24 hours", Type = SettingType.Text, Category = "Contact Page", SortOrder = 10 },
                    new SiteSetting { Key = "contact_livechat_text", DisplayName = "Live Chat Availability", Value = "Available during business hours", Type = SettingType.Text, Category = "Contact Page", SortOrder = 11 },
                    new SiteSetting { Key = "contact_livechat_note", DisplayName = "Live Chat Note", Value = "Quick answers to your questions", Type = SettingType.Text, Category = "Contact Page", SortOrder = 12 },
                    new SiteSetting { Key = "contact_reasons", DisplayName = "Why Contact Us - Reasons", Value = "[{\"title\":\"Expert advice on product selection\"},{\"title\":\"Personalized recommendations\"},{\"title\":\"Financing options\"},{\"title\":\"Schedule a test ride\"},{\"title\":\"After-sales support\"}]", Type = SettingType.TextArea, Category = "Contact Page", SortOrder = 13 },
                    
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
                    
                    new SiteSetting { Key = "session_timeout", DisplayName = "Session Timeout (minutes)", Value = "480", Type = SettingType.Number, Category = "Advanced", SortOrder = 1 },
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

                    // Music Player
                    new SiteSetting { Key = "music_enabled", DisplayName = "Enable Music Player", Value = "false", Type = SettingType.Boolean, Category = "Music", SortOrder = 1 },
                    new SiteSetting { Key = "music_embed_code", DisplayName = "Music Embed Code", Value = "", Type = SettingType.TextArea, Category = "Music", SortOrder = 2 },

                    // Email (SMTP)
                    new SiteSetting { Key = "smtp_enabled", DisplayName = "Enable Email (SMTP)", Value = "false", Type = SettingType.Boolean, Category = "Email", SortOrder = 1 },
                    new SiteSetting { Key = "smtp_host", DisplayName = "SMTP Host", Value = "", Type = SettingType.Text, Category = "Email", SortOrder = 2 },
                    new SiteSetting { Key = "smtp_port", DisplayName = "SMTP Port", Value = "587", Type = SettingType.Number, Category = "Email", SortOrder = 3 },
                    new SiteSetting { Key = "smtp_username", DisplayName = "SMTP Username", Value = "", Type = SettingType.Text, Category = "Email", SortOrder = 4 },
                    new SiteSetting { Key = "smtp_password", DisplayName = "SMTP Password", Value = "", Type = SettingType.Text, Category = "Email", SortOrder = 5 },
                    new SiteSetting { Key = "smtp_from_email", DisplayName = "From Email Address", Value = "", Type = SettingType.Email, Category = "Email", SortOrder = 6 },
                    new SiteSetting { Key = "smtp_from_name", DisplayName = "From Name", Value = "701 Performance Power", Type = SettingType.Text, Category = "Email", SortOrder = 7 },
                    new SiteSetting { Key = "smtp_use_ssl", DisplayName = "Use SSL/TLS", Value = "true", Type = SettingType.Boolean, Category = "Email", SortOrder = 8 },
                    new SiteSetting { Key = "site_url", DisplayName = "Site URL", Value = "http://localhost:3000", Type = SettingType.Url, Category = "Email", SortOrder = 9 },
                    
                    // Theme - Preset Tracking
                    new SiteSetting { Key = "theme_preset_active", DisplayName = "Active Theme Preset", Value = "", Type = SettingType.Text, Category = "Theme", SortOrder = 99 },
                    
                    // Theme - Brand Colors
                    new SiteSetting { Key = "theme_primary_color", DisplayName = "Primary Color", Value = "#6366f1", Type = SettingType.Color, Category = "Theme", SortOrder = 100 },
                    new SiteSetting { Key = "theme_secondary_color", DisplayName = "Secondary Color", Value = "#ec4899", Type = SettingType.Color, Category = "Theme", SortOrder = 101 },
                    new SiteSetting { Key = "theme_accent_color", DisplayName = "Accent Color", Value = "#f59e0b", Type = SettingType.Color, Category = "Theme", SortOrder = 102 },
                    new SiteSetting { Key = "theme_success_color", DisplayName = "Success Color", Value = "#10b981", Type = SettingType.Color, Category = "Theme", SortOrder = 103 },
                    new SiteSetting { Key = "theme_warning_color", DisplayName = "Warning Color", Value = "#f59e0b", Type = SettingType.Color, Category = "Theme", SortOrder = 104 },
                    new SiteSetting { Key = "theme_danger_color", DisplayName = "Danger Color", Value = "#ef4444", Type = SettingType.Color, Category = "Theme", SortOrder = 105 },
                    
                    // Theme - Backgrounds
                    new SiteSetting { Key = "theme_bg_color", DisplayName = "Page Background", Value = "#ffffff", Type = SettingType.Color, Category = "Theme", SortOrder = 110 },
                    new SiteSetting { Key = "theme_bg_secondary", DisplayName = "Secondary Background", Value = "#f9fafb", Type = SettingType.Color, Category = "Theme", SortOrder = 111 },
                    new SiteSetting { Key = "theme_bg_muted", DisplayName = "Muted Background", Value = "#f3f4f6", Type = SettingType.Color, Category = "Theme", SortOrder = 112 },
                    
                    // Theme - Text
                    new SiteSetting { Key = "theme_text_primary", DisplayName = "Primary Text", Value = "#111827", Type = SettingType.Color, Category = "Theme", SortOrder = 120 },
                    new SiteSetting { Key = "theme_text_secondary", DisplayName = "Secondary Text", Value = "#6b7280", Type = SettingType.Color, Category = "Theme", SortOrder = 121 },
                    new SiteSetting { Key = "theme_text_muted", DisplayName = "Muted Text", Value = "#9ca3af", Type = SettingType.Color, Category = "Theme", SortOrder = 122 },
                    
                    // Theme - Borders
                    new SiteSetting { Key = "theme_border_color", DisplayName = "Default Border", Value = "#e5e7eb", Type = SettingType.Color, Category = "Theme", SortOrder = 130 },
                    new SiteSetting { Key = "theme_border_accent", DisplayName = "Accent Border", Value = "#d1d5db", Type = SettingType.Color, Category = "Theme", SortOrder = 131 },
                    
                    // Theme - Typography
                    new SiteSetting { Key = "theme_font_heading", DisplayName = "Heading Font", Value = "'Inter', system-ui, sans-serif", Type = SettingType.Text, Category = "Theme", SortOrder = 200 },
                    new SiteSetting { Key = "theme_font_body", DisplayName = "Body Font", Value = "'Inter', system-ui, sans-serif", Type = SettingType.Text, Category = "Theme", SortOrder = 201 },
                    new SiteSetting { Key = "theme_font_size_base", DisplayName = "Base Font Size", Value = "16", Type = SettingType.Number, Category = "Theme", SortOrder = 210 },
                    new SiteSetting { Key = "theme_font_size_h1", DisplayName = "H1 Size", Value = "2.5", Type = SettingType.Number, Category = "Theme", SortOrder = 211 },
                    new SiteSetting { Key = "theme_font_size_h2", DisplayName = "H2 Size", Value = "2", Type = SettingType.Number, Category = "Theme", SortOrder = 212 },
                    new SiteSetting { Key = "theme_font_size_h3", DisplayName = "H3 Size", Value = "1.5", Type = SettingType.Number, Category = "Theme", SortOrder = 213 },
                    new SiteSetting { Key = "theme_font_weight_heading", DisplayName = "Heading Weight", Value = "700", Type = SettingType.Number, Category = "Theme", SortOrder = 220 },
                    new SiteSetting { Key = "theme_font_weight_body", DisplayName = "Body Weight", Value = "400", Type = SettingType.Number, Category = "Theme", SortOrder = 221 },
                    new SiteSetting { Key = "theme_line_height_heading", DisplayName = "Heading Line Height", Value = "1.2", Type = SettingType.Number, Category = "Theme", SortOrder = 230 },
                    new SiteSetting { Key = "theme_line_height_body", DisplayName = "Body Line Height", Value = "1.6", Type = SettingType.Number, Category = "Theme", SortOrder = 231 },
                    
                    // Theme - Buttons
                    new SiteSetting { Key = "theme_button_radius", DisplayName = "Button Border Radius", Value = "6", Type = SettingType.Number, Category = "Theme", SortOrder = 300 },
                    new SiteSetting { Key = "theme_button_padding_y", DisplayName = "Button Padding Y", Value = "0.75", Type = SettingType.Number, Category = "Theme", SortOrder = 301 },
                    new SiteSetting { Key = "theme_button_padding_x", DisplayName = "Button Padding X", Value = "1.5", Type = SettingType.Number, Category = "Theme", SortOrder = 302 },
                    new SiteSetting { Key = "theme_button_font_weight", DisplayName = "Button Font Weight", Value = "600", Type = SettingType.Number, Category = "Theme", SortOrder = 303 },
                    new SiteSetting { Key = "theme_button_text_transform", DisplayName = "Button Text Transform", Value = "none", Type = SettingType.Text, Category = "Theme", SortOrder = 304 },
                    
                    // Theme - Cards
                    new SiteSetting { Key = "theme_card_radius", DisplayName = "Card Border Radius", Value = "12", Type = SettingType.Number, Category = "Theme", SortOrder = 310 },
                    new SiteSetting { Key = "theme_card_padding", DisplayName = "Card Padding", Value = "1.5", Type = SettingType.Number, Category = "Theme", SortOrder = 311 },
                    new SiteSetting { Key = "theme_card_shadow", DisplayName = "Card Shadow", Value = "0 1px 3px 0 rgb(0 0 0 / 0.1)", Type = SettingType.Text, Category = "Theme", SortOrder = 312 },
                    
                    // Theme - Inputs
                    new SiteSetting { Key = "theme_input_radius", DisplayName = "Input Border Radius", Value = "6", Type = SettingType.Number, Category = "Theme", SortOrder = 320 },
                    new SiteSetting { Key = "theme_input_border_width", DisplayName = "Input Border Width", Value = "1", Type = SettingType.Number, Category = "Theme", SortOrder = 321 },
                    new SiteSetting { Key = "theme_input_focus_color", DisplayName = "Input Focus Color", Value = "#6366f1", Type = SettingType.Color, Category = "Theme", SortOrder = 322 },
                    
                    // Theme - Layout
                    new SiteSetting { Key = "theme_container_max_width", DisplayName = "Max Container Width", Value = "1280", Type = SettingType.Number, Category = "Theme", SortOrder = 400 },
                    new SiteSetting { Key = "theme_container_padding", DisplayName = "Container Padding", Value = "20", Type = SettingType.Number, Category = "Theme", SortOrder = 401 },
                    new SiteSetting { Key = "theme_section_padding_top", DisplayName = "Section Padding Top", Value = "4", Type = SettingType.Number, Category = "Theme", SortOrder = 410 },
                    new SiteSetting { Key = "theme_section_padding_bottom", DisplayName = "Section Padding Bottom", Value = "4", Type = SettingType.Number, Category = "Theme", SortOrder = 411 },
                    new SiteSetting { Key = "theme_element_gap", DisplayName = "Element Gap", Value = "1.5", Type = SettingType.Number, Category = "Theme", SortOrder = 412 },
                    
                    // Theme - Effects
                    new SiteSetting { Key = "theme_transition_duration", DisplayName = "Transition Duration", Value = "200", Type = SettingType.Number, Category = "Theme", SortOrder = 500 },
                    new SiteSetting { Key = "theme_transition_timing", DisplayName = "Transition Timing", Value = "ease", Type = SettingType.Text, Category = "Theme", SortOrder = 501 },
                    new SiteSetting { Key = "theme_hover_lift_enabled", DisplayName = "Enable Hover Lift", Value = "true", Type = SettingType.Boolean, Category = "Theme", SortOrder = 510 },
                    new SiteSetting { Key = "theme_hover_lift_amount", DisplayName = "Hover Lift Amount", Value = "4", Type = SettingType.Number, Category = "Theme", SortOrder = 511 },
                    new SiteSetting { Key = "theme_hover_scale", DisplayName = "Hover Scale", Value = "1.02", Type = SettingType.Number, Category = "Theme", SortOrder = 512 },
                    new SiteSetting { Key = "theme_hover_shadow", DisplayName = "Hover Shadow", Value = "0 8px 20px 0 rgb(0 0 0 / 0.15)", Type = SettingType.Text, Category = "Theme", SortOrder = 513 },
                    
                    // Theme - Header
                    new SiteSetting { Key = "theme_header_bg", DisplayName = "Header Background", Value = "#ffffff", Type = SettingType.Color, Category = "Theme", SortOrder = 600 },
                    new SiteSetting { Key = "theme_header_text", DisplayName = "Header Text Color", Value = "#111827", Type = SettingType.Color, Category = "Theme", SortOrder = 601 },
                    new SiteSetting { Key = "theme_header_height", DisplayName = "Header Height", Value = "72", Type = SettingType.Number, Category = "Theme", SortOrder = 602 },
                    new SiteSetting { Key = "theme_header_sticky", DisplayName = "Header Sticky", Value = "true", Type = SettingType.Boolean, Category = "Theme", SortOrder = 603 },
                    new SiteSetting { Key = "theme_header_shadow", DisplayName = "Header Shadow", Value = "0 1px 3px 0 rgb(0 0 0 / 0.1)", Type = SettingType.Text, Category = "Theme", SortOrder = 604 },
                    
                    // Theme - Footer
                    new SiteSetting { Key = "theme_footer_bg", DisplayName = "Footer Background", Value = "#1f2937", Type = SettingType.Color, Category = "Theme", SortOrder = 700 },
                    new SiteSetting { Key = "theme_footer_text", DisplayName = "Footer Text Color", Value = "#9ca3af", Type = SettingType.Color, Category = "Theme", SortOrder = 701 },
                    new SiteSetting { Key = "theme_footer_padding", DisplayName = "Footer Padding", Value = "3", Type = SettingType.Number, Category = "Theme", SortOrder = 702 },
                    
                    // Theme - Advanced
                    new SiteSetting { Key = "theme_dark_mode_enabled", DisplayName = "Enable Dark Mode Toggle", Value = "false", Type = SettingType.Boolean, Category = "Theme", SortOrder = 800 },
                    new SiteSetting { Key = "theme_smooth_scroll", DisplayName = "Enable Smooth Scrolling", Value = "true", Type = SettingType.Boolean, Category = "Theme", SortOrder = 801 },
                    new SiteSetting { Key = "theme_parallax_enabled", DisplayName = "Enable Parallax Effects", Value = "false", Type = SettingType.Boolean, Category = "Theme", SortOrder = 802 },
                    new SiteSetting { Key = "theme_glass_morphism", DisplayName = "Glass Morphism", Value = "false", Type = SettingType.Boolean, Category = "Theme", SortOrder = 803 },
                    new SiteSetting { Key = "theme_animations_enabled", DisplayName = "Page Animations", Value = "true", Type = SettingType.Boolean, Category = "Theme", SortOrder = 804 },
                    new SiteSetting { Key = "theme_gradient_overlays", DisplayName = "Gradient Overlays", Value = "false", Type = SettingType.Boolean, Category = "Theme", SortOrder = 805 },
                    new SiteSetting { Key = "theme_custom_css", DisplayName = "Custom CSS", Value = "", Type = SettingType.Text, Category = "Theme", SortOrder = 900 },
                    
                    // Theme - Visual Effects
                    new SiteSetting { Key = "theme_button_style", DisplayName = "Button Style", Value = "solid", Type = SettingType.Text, Category = "Theme", SortOrder = 1000 },
                    new SiteSetting { Key = "theme_corner_style", DisplayName = "Corner Style", Value = "rounded", Type = SettingType.Text, Category = "Theme", SortOrder = 1001 },
                    new SiteSetting { Key = "theme_image_hover", DisplayName = "Image Hover Effect", Value = "zoom", Type = SettingType.Text, Category = "Theme", SortOrder = 1010 },
                    new SiteSetting { Key = "theme_image_border_style", DisplayName = "Image Border Style", Value = "shadow", Type = SettingType.Text, Category = "Theme", SortOrder = 1011 },
                    new SiteSetting { Key = "theme_bg_pattern", DisplayName = "Background Pattern", Value = "none", Type = SettingType.Text, Category = "Theme", SortOrder = 1020 },
                    new SiteSetting { Key = "theme_heading_shadow", DisplayName = "Heading Text Shadow", Value = "none", Type = SettingType.Text, Category = "Theme", SortOrder = 1030 },
                    new SiteSetting { Key = "theme_letter_spacing", DisplayName = "Letter Spacing", Value = "normal", Type = SettingType.Text, Category = "Theme", SortOrder = 1031 },
                    new SiteSetting { Key = "theme_loading_animation", DisplayName = "Loading Animation", Value = "spinner", Type = SettingType.Text, Category = "Theme", SortOrder = 1040 },
                    
                    // Theme - Gradients
                    new SiteSetting { Key = "theme_gradient_start", DisplayName = "Gradient Start Color", Value = "#4b5563", Type = SettingType.Color, Category = "Theme", SortOrder = 1100 },
                    new SiteSetting { Key = "theme_gradient_end", DisplayName = "Gradient End Color", Value = "#3b82f6", Type = SettingType.Color, Category = "Theme", SortOrder = 1101 },
                    new SiteSetting { Key = "theme_gradient_direction", DisplayName = "Gradient Direction", Value = "to right", Type = SettingType.Text, Category = "Theme", SortOrder = 1102 },
                    new SiteSetting { Key = "theme_gradient_opacity", DisplayName = "Gradient Opacity", Value = "70", Type = SettingType.Number, Category = "Theme", SortOrder = 1103 },
                    new SiteSetting { Key = "theme_backdrop_blur", DisplayName = "Backdrop Blur", Value = "10", Type = SettingType.Number, Category = "Theme", SortOrder = 1110 },
                    new SiteSetting { Key = "theme_modal_backdrop_opacity", DisplayName = "Modal Backdrop Opacity", Value = "75", Type = SettingType.Number, Category = "Theme", SortOrder = 1111 }
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

        // Backup & Restore Management
        var backupRoutes = v1Routes.MapGroup("/admin/backup").WithTags("Backup & Restore");

        // Helper method to ensure backup directory exists
        string GetBackupDirectory()
        {
            var backupDir = Path.Combine(Directory.GetCurrentDirectory(), "backups");
            if (!Directory.Exists(backupDir))
            {
                Directory.CreateDirectory(backupDir);
            }
            return backupDir;
        }

        // Helper method to get the protected (factory default) backup directory
        string GetProtectedBackupDirectory()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "protected-backups");
        }

        // Create Manual Backup
        backupRoutes.MapPost("/create", async (PowersportsDbContext context, ILogger<Program> logger, HttpRequest request) =>
        {
            try
            {
                var body = await request.ReadFromJsonAsync<Dictionary<string, string>>();
                var backupType = body?.GetValueOrDefault("type", "manual") ?? "manual";
                var rawName = body?.GetValueOrDefault("name", "") ?? "";

                // Sanitize name for use in filename (alphanumeric, spaces, hyphens only)
                var safeName = "";
                if (!string.IsNullOrWhiteSpace(rawName))
                {
                    safeName = "_" + System.Text.RegularExpressions.Regex.Replace(rawName.Trim(), @"[^\w\s\-]", "")
                        .Replace(" ", "_")
                        .Trim('_');
                    if (safeName.Length > 52) safeName = safeName[..52];
                }

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupFileName = $"backup_{backupType}{safeName}_{timestamp}.json";
                var backupDir = GetBackupDirectory();
                var backupPath = Path.Combine(backupDir, backupFileName);
                
                // Collect all database data
                var dbData = new
                {
                    Type = backupType,
                    Name = string.IsNullOrWhiteSpace(rawName) ? (string?)null : rawName.Trim(),
                    SiteSettings = await context.SiteSettings.ToListAsync(),
                    Users = await context.Users.Select(u => new { u.Id, u.FirstName, u.LastName, u.Email, u.PasswordHash, u.Role, u.CreatedAt }).ToListAsync(),
                    Categories = await context.Categories.ToListAsync(),
                    CategoryImages = await context.CategoryImages.ToListAsync(),
                    Products = await context.Products.ToListAsync(),
                    ProductImages = await context.ProductImages.ToListAsync(),
                    BackupDate = DateTime.UtcNow,
                    RecordCount = await context.SiteSettings.CountAsync() + 
                                 await context.Users.CountAsync() + 
                                 await context.Categories.CountAsync() + 
                                 await context.Products.CountAsync()
                };
                
                var jsonOptions = new JsonSerializerOptions 
                { 
                    WriteIndented = true
                };
                
                var jsonContent = JsonSerializer.Serialize(dbData, jsonOptions);
                
                // Save backup to file
                await File.WriteAllTextAsync(backupPath, jsonContent);
                
                var fileInfo = new FileInfo(backupPath);

                logger.LogInformation($"Backup created: {backupFileName} ({backupType})");

                return Results.Ok(new 
                { 
                    message = "Backup created successfully",
                    fileName = backupFileName,
                    size = fileInfo.Length,
                    createdAt = DateTime.UtcNow,
                    type = backupType,
                    recordCount = dbData.RecordCount
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create backup");
                return Results.BadRequest(new { message = $"Failed to create backup: {ex.Message}" });
            }
        })
        .WithName("CreateBackup")
        .WithSummary("Create a manual or automatic backup")
        .Produces(200)
        .Produces(400)
        .RequireAuthorization("SuperAdminOnly");

        // List All Backups
        backupRoutes.MapGet("/list", (ILogger<Program> logger) =>
        {
            try
            {
                var backupDir = GetBackupDirectory();
                var allEntries = new List<(string fileName, string? name, string type, bool isProtected, long size, DateTime createdAt, int recordCount)>();

                void CollectBackups(string dir, bool isProtected)
                {
                    if (!Directory.Exists(dir)) return;
                    foreach (var fp in Directory.GetFiles(dir, "backup_*.json"))
                    {
                        var fi = new FileInfo(fp);
                        var fn = fi.Name;
                        var tp = fn.Contains("_auto_") ? "auto" : "manual";
                        int rc = 0;
                        string? nm = null;
                        try
                        {
                            var txt = File.ReadAllText(fp);
                            var doc = JsonDocument.Parse(txt);
                            if (doc.RootElement.TryGetProperty("RecordCount", out var cProp)) rc = cProp.GetInt32();
                            if (doc.RootElement.TryGetProperty("Name", out var nProp)) nm = nProp.GetString();
                        }
                        catch { }
                        allEntries.Add((fn, nm, tp, isProtected, fi.Length, fi.CreationTime, rc));
                    }
                }

                CollectBackups(GetProtectedBackupDirectory(), true);
                CollectBackups(backupDir, false);

                var backupFiles = allEntries
                    .OrderByDescending(b => b.createdAt)
                    .Select(b => new { b.fileName, name = b.name, b.type, b.isProtected, b.size, b.createdAt, b.recordCount })
                    .ToList();

                // Calculate next auto backup time (1 AM tomorrow)
                var now = DateTime.Now;
                var nextAutoBackup = now.Date.AddDays(1).AddHours(1);
                if (now.Hour < 1)
                {
                    nextAutoBackup = now.Date.AddHours(1);
                }

                return Results.Ok(new 
                { 
                    backups = backupFiles,
                    nextAutoBackup
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to list backups");
                return Results.BadRequest(new { message = $"Failed to list backups: {ex.Message}" });
            }
        })
        .WithName("ListBackups")
        .WithSummary("List all available backups")
        .Produces(200)
        .RequireAuthorization("SuperAdminOnly");

        // Download Backup
        backupRoutes.MapGet("/download/{fileName}", (string fileName, ILogger<Program> logger) =>
        {
            try
            {
                // Validate filename to prevent directory traversal
                if (fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
                {
                    return Results.BadRequest(new { message = "Invalid filename" });
                }

                var backupDir = GetBackupDirectory();
                var backupPath = Path.Combine(backupDir, fileName);

                if (!File.Exists(backupPath))
                {
                    return Results.NotFound(new { message = "Backup file not found" });
                }

                var fileBytes = File.ReadAllBytes(backupPath);
                return Results.File(fileBytes, "application/json", fileName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to download backup: {fileName}");
                return Results.BadRequest(new { message = $"Failed to download backup: {ex.Message}" });
            }
        })
        .WithName("DownloadBackup")
        .WithSummary("Download a backup file")
        .Produces(200)
        .Produces(404)
        .RequireAuthorization("SuperAdminOnly");

        // Restore Backup
        backupRoutes.MapPost("/restore", async (PowersportsDbContext context, ILogger<Program> logger, HttpRequest request) =>
        {
            try
            {
                var body = await request.ReadFromJsonAsync<Dictionary<string, string>>();
                var fileName = body?.GetValueOrDefault("fileName", "");

                if (string.IsNullOrEmpty(fileName) || fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
                {
                    return Results.BadRequest(new { message = "Invalid filename" });
                }

                // Check protected directory first
                var protectedRestorePath = Path.Combine(GetProtectedBackupDirectory(), fileName);
                var backupDir = GetBackupDirectory();
                var backupPath = File.Exists(protectedRestorePath) ? protectedRestorePath : Path.Combine(backupDir, fileName);

                if (!File.Exists(backupPath))
                {
                    return Results.NotFound(new { message = "Backup file not found" });
                }

                var jsonContent = await File.ReadAllTextAsync(backupPath);
                var backupData = JsonSerializer.Deserialize<JsonElement>(jsonContent);

                // Begin transaction for restore
                using var transaction = await context.Database.BeginTransactionAsync();

                try
                {
                    // Clear existing data (except super admin user)
                    context.ProductImages.RemoveRange(context.ProductImages);
                    context.Products.RemoveRange(context.Products);
                    context.CategoryImages.RemoveRange(context.CategoryImages);
                    context.Categories.RemoveRange(context.Categories);
                    context.SiteSettings.RemoveRange(context.SiteSettings);
                    
                    // Keep at least one super admin
                    var usersToRemove = context.Users.Where(u => u.Role != UserRole.SuperAdmin);
                    context.Users.RemoveRange(usersToRemove);
                    
                    await context.SaveChangesAsync();

                    // Restore SiteSettings
                    if (backupData.TryGetProperty("SiteSettings", out var settingsJson))
                    {
                        var settings = JsonSerializer.Deserialize<List<SiteSetting>>(settingsJson.GetRawText());
                        if (settings != null)
                        {
                            context.SiteSettings.AddRange(settings);
                        }
                    }

                    // Restore Users (but don't overwrite existing super admin)
                    if (backupData.TryGetProperty("Users", out var usersJson))
                    {
                        var users = JsonSerializer.Deserialize<List<User>>(usersJson.GetRawText());
                        if (users != null)
                        {
                            var existingSuperAdminEmails = context.Users.Where(u => u.Role == UserRole.SuperAdmin).Select(u => u.Email).ToList();
                            var newUsers = users.Where(u => !existingSuperAdminEmails.Contains(u.Email)).ToList();
                            context.Users.AddRange(newUsers);
                        }
                    }

                    // Restore Categories
                    if (backupData.TryGetProperty("Categories", out var categoriesJson))
                    {
                        var categories = JsonSerializer.Deserialize<List<Category>>(categoriesJson.GetRawText());
                        if (categories != null)
                        {
                            context.Categories.AddRange(categories);
                        }
                    }

                    // Restore CategoryImages
                    if (backupData.TryGetProperty("CategoryImages", out var catImagesJson))
                    {
                        var categoryImages = JsonSerializer.Deserialize<List<CategoryImage>>(catImagesJson.GetRawText());
                        if (categoryImages != null)
                        {
                            context.CategoryImages.AddRange(categoryImages);
                        }
                    }

                    // Restore Products
                    if (backupData.TryGetProperty("Products", out var productsJson))
                    {
                        var products = JsonSerializer.Deserialize<List<Product>>(productsJson.GetRawText());
                        if (products != null)
                        {
                            context.Products.AddRange(products);
                        }
                    }

                    // Restore ProductImages
                    if (backupData.TryGetProperty("ProductImages", out var prodImagesJson))
                    {
                        var productImages = JsonSerializer.Deserialize<List<ProductImage>>(prodImagesJson.GetRawText());
                        if (productImages != null)
                        {
                            context.ProductImages.AddRange(productImages);
                        }
                    }

                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    logger.LogInformation($"Backup restored successfully: {fileName}");

                    return Results.Ok(new { message = "Backup restored successfully" });
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to restore backup");
                return Results.BadRequest(new { message = $"Failed to restore backup: {ex.Message}" });
            }
        })
        .WithName("RestoreBackup")
        .WithSummary("Restore database from a backup file")
        .Produces(200)
        .Produces(400)
        .Produces(404)
        .RequireAuthorization("SuperAdminOnly");

        // Delete Backup
        backupRoutes.MapDelete("/delete/{fileName}", (string fileName, ILogger<Program> logger) =>
        {
            try
            {
                if (fileName.Contains("..") || fileName.Contains("/") || fileName.Contains("\\"))
                {
                    return Results.BadRequest(new { message = "Invalid filename" });
                }

                // Block deletion of protected (factory default) backups
                var protectedPath = Path.Combine(GetProtectedBackupDirectory(), fileName);
                if (File.Exists(protectedPath))
                {
                    return Results.Json(new { message = "This is a protected factory default backup and cannot be deleted." }, statusCode: 403);
                }

                var backupDir = GetBackupDirectory();
                var backupPath = Path.Combine(backupDir, fileName);

                if (!File.Exists(backupPath))
                {
                    return Results.NotFound(new { message = "Backup file not found" });
                }

                File.Delete(backupPath);
                logger.LogInformation($"Backup deleted: {fileName}");

                return Results.Ok(new { message = "Backup deleted successfully" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to delete backup: {fileName}");
                return Results.BadRequest(new { message = $"Failed to delete backup: {ex.Message}" });
            }
        })
        .WithName("DeleteBackup")
        .WithSummary("Delete a backup file")
        .Produces(200)
        .Produces(404)
        .RequireAuthorization("SuperAdminOnly");

        // Get Auto Backup Settings
        backupRoutes.MapGet("/settings", async (PowersportsDbContext context) =>
        {
            var autoBackupSetting = await context.SiteSettings.FirstOrDefaultAsync(s => s.Key == "auto_backup_enabled");
            var retentionSetting = await context.SiteSettings.FirstOrDefaultAsync(s => s.Key == "auto_backup_retention");

            return Results.Ok(new
            {
                enabled = autoBackupSetting?.Value == "true",
                retention = int.TryParse(retentionSetting?.Value, out var retention) ? retention : 7
            });
        })
        .WithName("GetAutoBackupSettings")
        .WithSummary("Get auto backup configuration")
        .Produces(200)
        .RequireAuthorization("SuperAdminOnly");

        // Toggle Auto Backup
        backupRoutes.MapPost("/auto-backup/toggle", async (PowersportsDbContext context, HttpRequest request) =>
        {
            var body = await request.ReadFromJsonAsync<Dictionary<string, bool>>();
            var enabled = body?.GetValueOrDefault("enabled", false) ?? false;

            var setting = await context.SiteSettings.FirstOrDefaultAsync(s => s.Key == "auto_backup_enabled");
            if (setting == null)
            {
                setting = new SiteSetting
                {
                    Key = "auto_backup_enabled",
                    Value = enabled.ToString().ToLower(),
                    DisplayName = "Auto Backup Enabled",
                    Type = SettingType.Boolean,
                    Category = "System"
                };
                context.SiteSettings.Add(setting);
            }
            else
            {
                setting.Value = enabled.ToString().ToLower();
            }

            await context.SaveChangesAsync();

            return Results.Ok(new { message = $"Auto backup {(enabled ? "enabled" : "disabled")}" });
        })
        .WithName("ToggleAutoBackup")
        .WithSummary("Enable or disable automatic daily backups")
        .Produces(200)
        .RequireAuthorization("SuperAdminOnly");

        // Update Auto Backup Retention
        backupRoutes.MapPost("/auto-backup/retention", async (PowersportsDbContext context, HttpRequest request) =>
        {
            var body = await request.ReadFromJsonAsync<Dictionary<string, int>>();
            var retention = body?.GetValueOrDefault("retention", 7) ?? 7;

            var setting = await context.SiteSettings.FirstOrDefaultAsync(s => s.Key == "auto_backup_retention");
            if (setting == null)
            {
                setting = new SiteSetting
                {
                    Key = "auto_backup_retention",
                    Value = retention.ToString(),
                    DisplayName = "Auto Backup Retention Count",
                    Type = SettingType.Number,
                    Category = "System"
                };
                context.SiteSettings.Add(setting);
            }
            else
            {
                setting.Value = retention.ToString();
            }

            await context.SaveChangesAsync();

            return Results.Ok(new { message = "Retention setting updated" });
        })
        .WithName("SetAutoBackupRetention")
        .WithSummary("Set how many auto backups to keep")
        .Produces(200)
        .RequireAuthorization("SuperAdminOnly");

        // Contact Submissions Management (Phase 1)
        var contactSubmissionsRoutes = v1Routes.MapGroup("/admin/contact-submissions").WithTags("Contact Submissions");

        contactSubmissionsRoutes.MapGet("/", async (PowersportsDbContext context, ContactStatus? status, DateTime? fromDate, DateTime? toDate, int? assignedToUserId) =>
        {
            var query = context.ContactSubmissions
                .Include(c => c.AssignedToUser)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(c => c.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(c => c.CreatedAt <= toDate.Value);
            }

            if (assignedToUserId.HasValue)
            {
                query = query.Where(c => c.AssignedToUserId == assignedToUserId.Value);
            }

            var submissions = await query
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Email,
                    c.Subject,
                    c.Message,
                    c.Status,
                    c.AdminNotes,
                    c.CreatedAt,
                    c.ReadAt,
                    c.AssignedToUserId,
                    AssignedToUser = c.AssignedToUser != null ? new { c.AssignedToUser.Id, c.AssignedToUser.FullName } : null
                })
                .ToListAsync();

            return Results.Ok(submissions);
        })
        .WithName("GetContactSubmissions")
        .WithSummary("Get all contact submissions with optional filters (Admin+ only)")
        .Produces(200)
        .RequireAuthorization("AdminOnly");

        contactSubmissionsRoutes.MapGet("/{id:int}", async (int id, PowersportsDbContext context) =>
        {
            var submission = await context.ContactSubmissions
                .Include(c => c.AssignedToUser)
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Email,
                    c.Subject,
                    c.Message,
                    c.Status,
                    c.AdminNotes,
                    c.CreatedAt,
                    c.ReadAt,
                    c.AssignedToUserId,
                    AssignedToUser = c.AssignedToUser != null ? new { c.AssignedToUser.Id, c.AssignedToUser.FullName } : null
                })
                .FirstOrDefaultAsync();

            if (submission == null)
            {
                return Results.NotFound($"Contact submission with ID {id} not found");
            }

            return Results.Ok(submission);
        })
        .WithName("GetContactSubmissionById")
        .WithSummary("Get a specific contact submission by ID (Admin+ only)")
        .Produces(200)
        .Produces(404)
        .RequireAuthorization("AdminOnly");

        contactSubmissionsRoutes.MapPut("/{id:int}", async (int id, UpdateContactSubmissionRequest request, PowersportsDbContext context, HttpContext httpContext) =>
        {
            try
            {
                var submission = await context.ContactSubmissions.FindAsync(id);
                if (submission == null)
                {
                    return Results.NotFound($"Contact submission with ID {id} not found");
                }

                // Update status and mark as read if moving from New status
                if (submission.Status == ContactStatus.New && request.Status != ContactStatus.New)
                {
                    submission.ReadAt = DateTime.UtcNow;
                }

                submission.Status = request.Status;
                submission.AdminNotes = request.AdminNotes;
                submission.AssignedToUserId = request.AssignedToUserId;

                await context.SaveChangesAsync();

                return Results.Ok(new
                {
                    message = "Contact submission updated successfully",
                    submission = new
                    {
                        submission.Id,
                        submission.Status,
                        submission.AdminNotes,
                        submission.AssignedToUserId,
                        submission.ReadAt
                    }
                });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { message = $"Failed to update contact submission: {ex.Message}" });
            }
        })
        .WithName("UpdateContactSubmission")
        .WithSummary("Update a contact submission's status, notes, or assignment (Admin+ only)")
        .Produces(200)
        .Produces(404)
        .Produces(400)
        .RequireAuthorization("AdminOnly");

        contactSubmissionsRoutes.MapDelete("/{id:int}", async (int id, PowersportsDbContext context) =>
        {
            try
            {
                var submission = await context.ContactSubmissions.FindAsync(id);
                if (submission == null)
                {
                    return Results.NotFound($"Contact submission with ID {id} not found");
                }

                context.ContactSubmissions.Remove(submission);
                await context.SaveChangesAsync();

                return Results.Ok(new { message = "Contact submission deleted successfully" });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { message = $"Failed to delete contact submission: {ex.Message}" });
            }
        })
        .WithName("DeleteContactSubmission")
        .WithSummary("Delete a contact submission (Admin+ only)")
        .Produces(200)
        .Produces(404)
        .Produces(400)
        .RequireAuthorization("AdminOnly");

        contactSubmissionsRoutes.MapGet("/stats", async (PowersportsDbContext context) =>
        {
            var totalSubmissions = await context.ContactSubmissions.CountAsync();
            var newSubmissions = await context.ContactSubmissions.CountAsync(c => c.Status == ContactStatus.New);
            var inProgressSubmissions = await context.ContactSubmissions.CountAsync(c => c.Status == ContactStatus.InProgress);
            var resolvedSubmissions = await context.ContactSubmissions.CountAsync(c => c.Status == ContactStatus.Resolved);
            
            // Calculate average response time client-side to avoid database-specific function ambiguity
            var readSubmissions = await context.ContactSubmissions
                .Where(c => c.ReadAt.HasValue)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new { c.CreatedAt, c.ReadAt })
                .ToListAsync();
            
            var avgResponseTime = readSubmissions.Any() 
                ? readSubmissions.Average(c => (c.ReadAt!.Value - c.CreatedAt).TotalMinutes)
                : 0;

            return Results.Ok(new
            {
                totalSubmissions,
                newSubmissions,
                inProgressSubmissions,
                resolvedSubmissions,
                avgResponseTimeMinutes = avgResponseTime
            });
        })
        .WithName("GetContactSubmissionsStats")
        .WithSummary("Get contact submissions statistics (Admin+ only)")
        .Produces(200)
        .RequireAuthorization("AdminOnly");

        // ====================================================================================
        // Orders Management (Phase 2)
        // ====================================================================================
        var ordersRoutes = v1Routes.MapGroup("/admin/orders").WithTags("Orders");

        // GET /api/v1/admin/orders - List all orders with filters
        ordersRoutes.MapGet("/", async (
            PowersportsDbContext context,
            string? search,
            OrderStatus? orderStatus,
            PaymentStatus? paymentStatus,
            DateTime? startDate,
            DateTime? endDate,
            int page = 1,
            int pageSize = 50) =>
        {
            var query = context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Include(o => o.User)
                .AsQueryable();

            // Search by order number, customer name, email, or phone
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(o =>
                    o.OrderNumber.ToLower().Contains(searchLower) ||
                    o.CustomerName.ToLower().Contains(searchLower) ||
                    o.CustomerEmail.ToLower().Contains(searchLower) ||
                    o.CustomerPhone.Contains(searchLower));
            }

            // Filter by order status
            if (orderStatus.HasValue)
            {
                query = query.Where(o => o.OrderStatus == orderStatus.Value);
            }

            // Filter by payment status
            if (paymentStatus.HasValue)
            {
                query = query.Where(o => o.PaymentStatus == paymentStatus.Value);
            }

            // Filter by date range
            if (startDate.HasValue)
            {
                query = query.Where(o => o.CreatedAt >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(o => o.CreatedAt <= endOfDay);
            }

            var totalCount = await query.CountAsync();

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new
                {
                    o.Id,
                    o.OrderNumber,
                    o.CustomerName,
                    o.CustomerEmail,
                    o.CustomerPhone,
                    o.TotalAmount,
                    o.OrderStatus,
                    o.PaymentStatus,
                    o.PaymentMethod,
                    o.CreatedAt,
                    o.UpdatedAt,
                    o.ShippedDate,
                    o.TrackingNumber,
                    ItemCount = o.Items.Count,
                    UserId = o.UserId
                })
                .ToListAsync();

            return Results.Ok(new
            {
                orders,
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        })
        .WithName("GetOrders")
        .WithSummary("Get all orders with optional filters (Admin+ only)")
        .Produces(200)
        .RequireAuthorization("AdminOnly");

        // GET /api/v1/admin/orders/{id} - Get single order details
        ordersRoutes.MapGet("/{id:int}", async (int id, PowersportsDbContext context) =>
        {
            var order = await context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return Results.NotFound(new { message = "Order not found" });
            }

            return Results.Ok(new
            {
                order.Id,
                order.OrderNumber,
                order.UserId,
                UserName = order.User != null ? $"{order.User.FirstName} {order.User.LastName}" : null,
                order.CustomerName,
                order.CustomerEmail,
                order.CustomerPhone,
                order.ShippingAddress,
                order.ShippingCity,
                order.ShippingState,
                order.ShippingZipCode,
                order.ShippingCountry,
                Items = order.Items.Select(i => new
                {
                    i.Id,
                    i.ProductId,
                    i.ProductName,
                    i.ProductSku,
                    i.UnitPrice,
                    i.Quantity,
                    i.TotalPrice,
                    ProductImageUrl = i.Product.ImageUrl,
                    ProductIsActive = i.Product.IsActive
                }),
                order.Subtotal,
                order.TaxAmount,
                order.ShippingCost,
                order.TotalAmount,
                order.PaymentMethod,
                order.PaymentStatus,
                order.PaymentReceivedDate,
                order.PaymentNotes,
                order.OrderStatus,
                order.TrackingNumber,
                order.ShippingCarrier,
                order.ShippedDate,
                order.DeliveredDate,
                order.CustomerNotes,
                order.AdminNotes,
                order.CreatedAt,
                order.UpdatedAt
            });
        })
        .WithName("GetOrderById")
        .WithSummary("Get order by ID (Admin+ only)")
        .Produces(200)
        .Produces(404)
        .RequireAuthorization("AdminOnly");

        // POST /api/v1/admin/orders - Create a new order
        ordersRoutes.MapPost("/", async (
            CreateOrderRequest request,
            PowersportsDbContext context,
            ILogger<Program> logger) =>
        {
            try
            {
                // Validate that all products exist and get their prices
                var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
                var products = await context.Products
                    .Where(p => productIds.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id, p => p);

                if (products.Count != productIds.Count)
                {
                    var missingIds = productIds.Except(products.Keys).ToList();
                    return Results.BadRequest(new { message = $"Products not found: {string.Join(", ", missingIds)}" });
                }

                // Generate unique order number
                var today = DateTime.UtcNow;
                var orderNumberPrefix = $"ORD-{today:yyyyMMdd}";
                var lastOrderToday = await context.Orders
                    .Where(o => o.OrderNumber.StartsWith(orderNumberPrefix))
                    .OrderByDescending(o => o.OrderNumber)
                    .FirstOrDefaultAsync();

                int sequenceNumber = 1;
                if (lastOrderToday != null)
                {
                    var lastSequence = lastOrderToday.OrderNumber.Split('-').LastOrDefault();
                    if (int.TryParse(lastSequence, out int lastNum))
                    {
                        sequenceNumber = lastNum + 1;
                    }
                }

                var orderNumber = $"{orderNumberPrefix}-{sequenceNumber:D4}";

                // Calculate order totals
                decimal subtotal = 0;
                var orderItems = new List<OrderItem>();

                foreach (var item in request.Items)
                {
                    var product = products[item.ProductId];
                    var unitPrice = item.UnitPriceOverride ?? product.Price;
                    var totalPrice = unitPrice * item.Quantity;
                    subtotal += totalPrice;

                    orderItems.Add(new OrderItem
                    {
                        ProductId = item.ProductId,
                        ProductName = product.Name,
                        ProductSku = product.Sku,
                        UnitPrice = unitPrice,
                        Quantity = item.Quantity,
                        TotalPrice = totalPrice
                    });
                }

                var totalAmount = subtotal + request.TaxAmount + request.ShippingCost;

                // Create the order
                var order = new Order
                {
                    OrderNumber = orderNumber,
                    UserId = request.UserId,
                    CustomerName = request.CustomerName,
                    CustomerEmail = request.CustomerEmail,
                    CustomerPhone = request.CustomerPhone,
                    ShippingAddress = request.ShippingAddress,
                    ShippingCity = request.ShippingCity,
                    ShippingState = request.ShippingState,
                    ShippingZipCode = request.ShippingZipCode,
                    ShippingCountry = request.ShippingCountry ?? "USA",
                    Items = orderItems,
                    Subtotal = subtotal,
                    TaxAmount = request.TaxAmount,
                    ShippingCost = request.ShippingCost,
                    TotalAmount = totalAmount,
                    PaymentMethod = request.PaymentMethod,
                    PaymentStatus = request.PaymentStatus,
                    PaymentNotes = request.PaymentNotes,
                    OrderStatus = OrderStatus.Pending,
                    CustomerNotes = request.CustomerNotes,
                    AdminNotes = request.AdminNotes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                context.Orders.Add(order);
                await context.SaveChangesAsync();

                logger.LogInformation("Order {OrderNumber} created successfully", orderNumber);

                return Results.Created($"/api/v1/admin/orders/{order.Id}", new
                {
                    message = "Order created successfully",
                    orderId = order.Id,
                    orderNumber = order.OrderNumber
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating order");
                return Results.BadRequest(new { message = "Failed to create order", error = ex.Message });
            }
        })
        .WithName("CreateOrder")
        .WithSummary("Create a new manual order (Admin+ only)")
        .Produces(201)
        .Produces(400)
        .RequireAuthorization("AdminOnly");

        // PUT /api/v1/admin/orders/{id} - Update an order
        ordersRoutes.MapPut("/{id:int}", async (
            int id,
            UpdateOrderRequest request,
            PowersportsDbContext context,
            ILogger<Program> logger) =>
        {
            var order = await context.Orders.FindAsync(id);

            if (order == null)
            {
                return Results.NotFound(new { message = "Order not found" });
            }

            // Update fields if provided
            if (!string.IsNullOrWhiteSpace(request.CustomerName))
                order.CustomerName = request.CustomerName;
            if (!string.IsNullOrWhiteSpace(request.CustomerEmail))
                order.CustomerEmail = request.CustomerEmail;
            if (!string.IsNullOrWhiteSpace(request.CustomerPhone))
                order.CustomerPhone = request.CustomerPhone;
            if (!string.IsNullOrWhiteSpace(request.ShippingAddress))
                order.ShippingAddress = request.ShippingAddress;
            if (!string.IsNullOrWhiteSpace(request.ShippingCity))
                order.ShippingCity = request.ShippingCity;
            if (!string.IsNullOrWhiteSpace(request.ShippingState))
                order.ShippingState = request.ShippingState;
            if (!string.IsNullOrWhiteSpace(request.ShippingZipCode))
                order.ShippingZipCode = request.ShippingZipCode;
            if (request.ShippingCountry != null)
                order.ShippingCountry = request.ShippingCountry;

            if (request.OrderStatus.HasValue)
                order.OrderStatus = request.OrderStatus.Value;
            if (request.PaymentStatus.HasValue)
                order.PaymentStatus = request.PaymentStatus.Value;
            if (request.PaymentMethod.HasValue)
                order.PaymentMethod = request.PaymentMethod.Value;
            if (request.PaymentReceivedDate.HasValue)
                order.PaymentReceivedDate = request.PaymentReceivedDate;
            if (request.PaymentNotes != null)
                order.PaymentNotes = request.PaymentNotes;

            if (request.TrackingNumber != null)
                order.TrackingNumber = request.TrackingNumber;
            if (request.ShippingCarrier != null)
                order.ShippingCarrier = request.ShippingCarrier;
            if (request.ShippedDate.HasValue)
                order.ShippedDate = request.ShippedDate;
            if (request.DeliveredDate.HasValue)
                order.DeliveredDate = request.DeliveredDate;

            if (request.CustomerNotes != null)
                order.CustomerNotes = request.CustomerNotes;
            if (request.AdminNotes != null)
                order.AdminNotes = request.AdminNotes;

            order.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            logger.LogInformation("Order {OrderNumber} updated successfully", order.OrderNumber);

            return Results.Ok(new { message = "Order updated successfully", orderId = order.Id });
        })
        .WithName("UpdateOrder")
        .WithSummary("Update an existing order (Admin+ only)")
        .Produces(200)
        .Produces(404)
        .RequireAuthorization("AdminOnly");

        // DELETE /api/v1/admin/orders/{id} - Delete an order
        ordersRoutes.MapDelete("/{id:int}", async (int id, PowersportsDbContext context, ILogger<Program> logger) =>
        {
            var order = await context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return Results.NotFound(new { message = "Order not found" });
            }

            var orderNumber = order.OrderNumber;
            context.Orders.Remove(order);
            await context.SaveChangesAsync();

            logger.LogInformation("Order {OrderNumber} deleted successfully", orderNumber);

            return Results.Ok(new { message = "Order deleted successfully" });
        })
        .WithName("DeleteOrder")
        .WithSummary("Delete an order (Admin+ only)")
        .Produces(200)
        .Produces(404)
        .RequireAuthorization("AdminOnly");

        // GET /api/v1/admin/orders/stats - Get order statistics
        ordersRoutes.MapGet("/stats", async (PowersportsDbContext context) =>
        {
            var totalOrders = await context.Orders.CountAsync();
            var pendingOrders = await context.Orders.CountAsync(o => o.OrderStatus == OrderStatus.Pending);
            var processingOrders = await context.Orders.CountAsync(o => o.OrderStatus == OrderStatus.Processing);
            var shippedOrders = await context.Orders.CountAsync(o => o.OrderStatus == OrderStatus.Shipped);
            var deliveredOrders = await context.Orders.CountAsync(o => o.OrderStatus == OrderStatus.Delivered);
            
            var totalRevenue = await context.Orders
                .Where(o => o.PaymentStatus == PaymentStatus.Received)
                .SumAsync(o => o.TotalAmount);
            
            var pendingPayments = await context.Orders
                .Where(o => o.PaymentStatus == PaymentStatus.Pending)
                .SumAsync(o => o.TotalAmount);

            var recentOrders = await context.Orders
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .Select(o => new
                {
                    o.Id,
                    o.OrderNumber,
                    o.CustomerName,
                    o.TotalAmount,
                    o.OrderStatus,
                    o.CreatedAt
                })
                .ToListAsync();

            return Results.Ok(new
            {
                totalOrders,
                pendingOrders,
                processingOrders,
                shippedOrders,
                deliveredOrders,
                totalRevenue,
                pendingPayments,
                recentOrders
            });
        })
        .WithName("GetOrdersStats")
        .WithSummary("Get order statistics (Admin+ only)")
        .Produces(200)
        .RequireAuthorization("AdminOnly");

        // ====================================================================================
        // Media Library (Phase 3)
        // ====================================================================================
        var mediaRoutes = v1Routes.MapGroup("/admin/media").WithTags("Media Library");

        // POST /api/v1/admin/media/upload - Upload file to media library
        mediaRoutes.MapPost("/upload", async (
            HttpRequest request,
            HttpContext httpContext,
            FileService fileService,
            ILogger<Program> logger) =>
        {
            try
            {
                var form = await request.ReadFormAsync();
                var file = form.Files.FirstOrDefault();

                if (file == null || file.Length == 0)
                {
                    return Results.BadRequest(new { message = "No file provided" });
                }

                // Get metadata from form
                var altText = form["altText"].ToString();
                var caption = form["caption"].ToString();
                var tags = form["tags"].ToString();
                var sectionIdStr = form["sectionId"].ToString();
                
                // Parse sectionId (optional)
                int? sectionId = null;
                if (!string.IsNullOrEmpty(sectionIdStr) && int.TryParse(sectionIdStr, out var sid))
                {
                    sectionId = sid;
                }

                // Get current user ID
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userId = int.TryParse(userIdClaim, out var uid) ? uid : 1;

                var result = await fileService.UploadToMediaLibraryAsync(file, userId, altText, caption, tags, sectionId);

                if (!result.IsSuccess)
                {
                    logger.LogWarning("Failed to upload file to media library: {Error}", result.ErrorMessage);
                    return Results.BadRequest(new { message = result.ErrorMessage });
                }

                logger.LogInformation("File uploaded to media library: {FileName} by user {UserId}", file.FileName, userId);

                var data = result.Data!;
                return Results.Ok(new
                {
                    message = "File uploaded successfully",
                    mediaFile = new
                    {
                        data.Id,
                        data.FileName,
                        data.StoredFileName,
                        data.FilePath,
                        data.ThumbnailPath,
                        data.MimeType,
                        data.FileSize,
                        data.MediaType,
                        data.Width,
                        data.Height,
                        data.AltText,
                        data.Caption,
                        data.Tags,
                        data.UploadedAt
                    }
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error uploading file to media library");
                return Results.Problem("An error occurred while uploading the file");
            }
        })
        .WithName("UploadMediaFile")
        .WithSummary("Upload a file to the media library (Admin+ only)")
        .Produces(200)
        .Produces(400)
        .RequireAuthorization("AdminOnly")
        .DisableAntiforgery();

        // GET /api/v1/admin/media - List media files
        mediaRoutes.MapGet("/", async (
            FileService fileService,
            string? search,
            MediaType? mediaType,
            int? sectionId,
            int page = 1,
            int pageSize = 50) =>
        {
            var (files, totalCount) = await fileService.GetMediaFilesAsync(search, mediaType, sectionId, page, pageSize);

            return Results.Ok(new
            {
                files = files.Select(f => new
                {
                    f.Id,
                    f.FileName,
                    f.StoredFileName,
                    f.FilePath,
                    f.ThumbnailPath,
                    f.MimeType,
                    f.FileSize,
                    f.MediaType,
                    f.Width,
                    f.Height,
                    f.AltText,
                    f.Caption,
                    f.Tags,
                    f.UploadedAt,
                    f.UpdatedAt,
                    UploadedBy = f.UploadedByUser != null
                        ? $"{f.UploadedByUser.FirstName} {f.UploadedByUser.LastName}"
                        : null
                }),
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        })
        .WithName("GetMediaFiles")
        .WithSummary("Get media files with optional filtering (Admin+ only)")
        .Produces(200)
        .RequireAuthorization("AdminOnly");

        // GET /api/v1/admin/media/{id} - Get single media file
        mediaRoutes.MapGet("/{id:int}", async (int id, FileService fileService) =>
        {
            var mediaFile = await fileService.GetMediaFileByIdAsync(id);

            if (mediaFile == null)
            {
                return Results.NotFound(new { message = "Media file not found" });
            }

            return Results.Ok(new
            {
                mediaFile.Id,
                mediaFile.FileName,
                mediaFile.StoredFileName,
                mediaFile.FilePath,
                mediaFile.ThumbnailPath,
                mediaFile.MimeType,
                mediaFile.FileSize,
                mediaFile.MediaType,
                mediaFile.Width,
                mediaFile.Height,
                mediaFile.AltText,
                mediaFile.Caption,
                mediaFile.Tags,
                mediaFile.UploadedAt,
                mediaFile.UpdatedAt,
                UploadedBy = mediaFile.UploadedByUser != null
                    ? $"{mediaFile.UploadedByUser.FirstName} {mediaFile.UploadedByUser.LastName}"
                    : null,
                UploadedByEmail = mediaFile.UploadedByUser?.Email
            });
        })
        .WithName("GetMediaFileById")
        .WithSummary("Get media file by ID (Admin+ only)")
        .Produces(200)
        .Produces(404)
        .RequireAuthorization("AdminOnly");

        // PUT /api/v1/admin/media/{id} - Update media file metadata
        mediaRoutes.MapPut("/{id:int}", async (
            int id,
            UpdateMediaFileRequest request,
            FileService fileService,
            ILogger<Program> logger) =>
        {
            var result = await fileService.UpdateMediaFileAsync(id, request.AltText, request.Caption, request.Tags);

            if (!result.IsSuccess)
            {
                logger.LogWarning("Failed to update media file {Id}: {Error}", id, result.ErrorMessage);
                return Results.NotFound(new { message = result.ErrorMessage });
            }

            logger.LogInformation("Media file {Id} updated successfully", id);

            var data = result.Data!;
            return Results.Ok(new
            {
                message = "Media file updated successfully",
                mediaFile = new
                {
                    data.Id,
                    data.FileName,
                    data.FilePath,
                    data.ThumbnailPath,
                    data.AltText,
                    data.Caption,
                    data.Tags,
                    data.UpdatedAt
                }
            });
        })
        .WithName("UpdateMediaFile")
        .WithSummary("Update media file metadata (Admin+ only)")
        .Produces(200)
        .Produces(404)
        .RequireAuthorization("AdminOnly");

        // DELETE /api/v1/admin/media/{id} - Delete media file
        mediaRoutes.MapDelete("/{id:int}", async (
            int id,
            FileService fileService,
            ILogger<Program> logger) =>
        {
            var result = await fileService.DeleteMediaFileAsync(id);

            if (!result.IsSuccess)
            {
                logger.LogWarning("Failed to delete media file {Id}: {Error}", id, result.ErrorMessage);
                return Results.NotFound(new { message = result.ErrorMessage });
            }

            logger.LogInformation("Media file {Id} deleted successfully", id);

            return Results.Ok(new { message = "Media file deleted successfully" });
        })
        .WithName("DeleteMediaFile")
        .WithSummary("Delete media file (Admin+ only)")
        .Produces(200)
        .Produces(404)
        .RequireAuthorization("AdminOnly");

        // ====================================================================================
        // Media Sections (organization/folders)
        // ====================================================================================

        // GET /api/v1/admin/media/sections - List all sections
        mediaRoutes.MapGet("/sections", async (PowersportsDbContext db) =>
        {
            var sections = await db.MediaSections
                .Include(s => s.Category)
                .Where(s => s.IsActive)
                .OrderBy(s => s.DisplayOrder)
                .ThenBy(s => s.Name)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.Description,
                    s.DisplayOrder,
                    s.IsSystem,
                    s.CategoryId,
                    CategoryName = s.Category != null ? s.Category.Name : null,
                    MediaCount = s.MediaFiles.Count
                })
                .ToListAsync();

            return Results.Ok(sections);
        })
        .WithName("GetMediaSections")
        .WithSummary("List all active media sections")
        .Produces(200)
        .RequireAuthorization("AdminOnly");

        // POST /api/v1/admin/media/sections - Create new custom section
        mediaRoutes.MapPost("/sections", async (
            MediaSection section,
            PowersportsDbContext db,
            ILogger<Program> logger) =>
        {
            try
            {
                // Validate
                if (string.IsNullOrWhiteSpace(section.Name))
                {
                    return Results.BadRequest(new { message = "Section name is required" });
                }

                // Check for duplicate name
                var exists = await db.MediaSections.AnyAsync(s => s.Name == section.Name);
                if (exists)
                {
                    return Results.BadRequest(new { message = "A section with this name already exists" });
                }

                // Set defaults
                section.IsSystem = false; // User-created sections are never system sections
                section.IsActive = true;
                section.CreatedAt = DateTime.UtcNow;

                db.MediaSections.Add(section);
                await db.SaveChangesAsync();

                logger.LogInformation("Created new media section: {SectionName} (ID: {SectionId})", section.Name, section.Id);

                return Results.Created($"/api/v1/admin/media/sections/{section.Id}", new
                {
                    section.Id,
                    section.Name,
                    section.Description,
                    section.DisplayOrder,
                    section.IsSystem,
                    section.CategoryId
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating media section");
                return Results.BadRequest(new { message = "Failed to create section" });
            }
        })
        .WithName("CreateMediaSection")
        .WithSummary("Create a new custom media section")
        .Produces(201)
        .Produces(400)
        .RequireAuthorization("AdminOnly");

        // PUT /api/v1/admin/media/sections/{id} - Update section
        mediaRoutes.MapPut("/sections/{id:int}", async (
            int id,
            MediaSection updatedSection,
            PowersportsDbContext db,
            ILogger<Program> logger) =>
        {
            var section = await db.MediaSections.FindAsync(id);
            if (section == null)
            {
                return Results.NotFound(new { message = "Section not found" });
            }

            // Validate name
            if (string.IsNullOrWhiteSpace(updatedSection.Name))
            {
                return Results.BadRequest(new { message = "Section name is required" });
            }

            // Check for duplicate name (excluding current section)
            var exists = await db.MediaSections.AnyAsync(s => s.Name == updatedSection.Name && s.Id != id);
            if (exists)
            {
                return Results.BadRequest(new { message = "A section with this name already exists" });
            }

            // Update fields
            section.Name = updatedSection.Name;
            section.Description = updatedSection.Description;
            section.DisplayOrder = updatedSection.DisplayOrder;
            section.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            logger.LogInformation("Updated media section {SectionId}: {SectionName}", id, section.Name);

            return Results.Ok(new
            {
                section.Id,
                section.Name,
                section.Description,
                section.DisplayOrder,
                section.IsSystem,
                section.CategoryId
            });
        })
        .WithName("UpdateMediaSection")
        .WithSummary("Update a media section")
        .Produces(200)
        .Produces(400)
        .Produces(404)
        .RequireAuthorization("AdminOnly");

        // DELETE /api/v1/admin/media/sections/{id} - Delete section (custom only)
        mediaRoutes.MapDelete("/sections/{id:int}", async (
            int id,
            PowersportsDbContext db,
            IWebHostEnvironment environment,
            ILogger<Program> logger) =>
        {
            var section = await db.MediaSections
                .Include(s => s.MediaFiles)
                .FirstOrDefaultAsync(s => s.Id == id);
                
            if (section == null)
            {
                return Results.NotFound(new { message = "Section not found" });
            }

            // Prevent deletion of system sections
            if (section.IsSystem)
            {
                return Results.BadRequest(new { message = "System sections cannot be deleted" });
            }

            try
            {
                // Delete all media files in this section (physical files and database records)
                var basePath = environment.WebRootPath;
                var deletedCount = 0;
                
                foreach (var mediaFile in section.MediaFiles.ToList())
                {
                    // Delete physical main file
                    var mainFilePath = Path.Combine(basePath, mediaFile.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (File.Exists(mainFilePath))
                    {
                        File.Delete(mainFilePath);
                        logger.LogInformation("Deleted physical file: {FilePath}", mainFilePath);
                    }

                    // Delete physical thumbnail
                    if (!string.IsNullOrEmpty(mediaFile.ThumbnailPath))
                    {
                        var thumbnailPath = Path.Combine(basePath, mediaFile.ThumbnailPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                        if (File.Exists(thumbnailPath))
                        {
                            File.Delete(thumbnailPath);
                            logger.LogInformation("Deleted thumbnail: {ThumbnailPath}", thumbnailPath);
                        }
                    }

                    // Delete database record
                    db.MediaFiles.Remove(mediaFile);
                    deletedCount++;
                }

                // Delete the entire section folder
                var sectionFolderName = string.Concat(section.Name.Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == ' '))
                    .Trim()
                    .Replace(" ", "_");
                var sectionFolderPath = Path.Combine(basePath, "uploads", "media", sectionFolderName);
                
                if (Directory.Exists(sectionFolderPath))
                {
                    try
                    {
                        Directory.Delete(sectionFolderPath, recursive: true);
                        logger.LogInformation("Deleted section folder: {FolderPath}", sectionFolderPath);
                    }
                    catch (Exception folderEx)
                    {
                        logger.LogWarning(folderEx, "Could not delete section folder: {FolderPath}", sectionFolderPath);
                    }
                }

                // Delete the section itself
                db.MediaSections.Remove(section);
                await db.SaveChangesAsync();

                logger.LogInformation("Deleted media section {SectionId}: {SectionName} with {FileCount} files", id, section.Name, deletedCount);

                return Results.Ok(new 
                { 
                    message = $"Section '{section.Name}' and {deletedCount} file(s) deleted successfully",
                    deletedFiles = deletedCount
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting media section {SectionId}", id);
                return Results.Problem("An error occurred while deleting the section");
            }
        })
        .WithName("DeleteMediaSection")
        .WithSummary("Delete a custom media section and all its files (system sections cannot be deleted)")
        .Produces(200)
        .Produces(400)
        .Produces(404)
        .RequireAuthorization("AdminOnly");

        // ====================================================================================
        // Unified Photo Upload (for direct entity uploads via PhotoUploader component)
        // ====================================================================================
        var photoRoutes = v1Routes.MapGroup("/photos").WithTags("Photo Uploads");

        // POST /api/v1/photos/upload - Upload and link photo to entity in one step
        photoRoutes.MapPost("/upload", async (
            HttpRequest request,
            HttpContext httpContext,
            FileService fileService,
            PowersportsDbContext context,
            ILogger<Program> logger) =>
        {
            try
            {
                var form = await request.ReadFormAsync();
                var file = form.Files.FirstOrDefault();

                if (file == null || file.Length == 0)
                {
                    return Results.BadRequest(new { message = "No file provided" });
                }

                var entityType = form["entityType"].ToString();
                var entityIdStr = form["entityId"].ToString();
                var altText = form["altText"].ToString();
                var caption = form["caption"].ToString();

                if (string.IsNullOrEmpty(entityType) || string.IsNullOrEmpty(entityIdStr))
                {
                    return Results.BadRequest(new { message = "entityType and entityId are required" });
                }

                if (!int.TryParse(entityIdStr, out var entityId))
                {
                    return Results.BadRequest(new { message = "Invalid entityId" });
                }

                // Get current user ID
                var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userId = int.TryParse(userIdClaim, out var uid) ? uid : 1;

                // Step 1: Upload to Media Library
                var uploadResult = await fileService.UploadToMediaLibraryAsync(file, userId, altText, caption, null);

                if (!uploadResult.IsSuccess)
                {
                    logger.LogWarning("Failed to upload file: {Error}", uploadResult.ErrorMessage);
                    return Results.BadRequest(new { message = uploadResult.ErrorMessage });
                }

                var mediaFile = uploadResult.Data!;
                logger.LogInformation("File uploaded to media library: {FileName} (ID: {MediaFileId})", 
                    file.FileName, mediaFile.Id);

                // Step 2: Link to entity based on entityType
                switch (entityType.ToLower())
                {
                    case "product":
                        var product = await context.Products.FindAsync(entityId);
                        if (product == null)
                        {
                            return Results.NotFound(new { message = "Product not found" });
                        }

                        var maxSort = await context.ProductImages
                            .Where(pi => pi.ProductId == entityId)
                            .MaxAsync(pi => (int?)pi.SortOrder) ?? 0;

                        var isFirstProduct = !await context.ProductImages.AnyAsync(pi => pi.ProductId == entityId);

                        var productImage = new ProductImage
                        {
                            ProductId = entityId,
                            MediaFileId = mediaFile.Id,
                            IsMain = isFirstProduct,
                            SortOrder = maxSort + 1,
                            CreatedAt = DateTime.UtcNow
                        };

                        context.ProductImages.Add(productImage);
                        await context.SaveChangesAsync();

                        logger.LogInformation("Linked media file {MediaFileId} to product {ProductId}", 
                            mediaFile.Id, entityId);
                        break;

                    case "category":
                        var category = await context.Categories.FindAsync(entityId);
                        if (category == null)
                        {
                            return Results.NotFound(new { message = "Category not found" });
                        }

                        var existingCategoryImage = await context.CategoryImages
                            .FirstOrDefaultAsync(ci => ci.CategoryId == entityId);

                        if (existingCategoryImage != null)
                        {
                            existingCategoryImage.MediaFileId = mediaFile.Id;
                        }
                        else
                        {
                            var categoryImage = new CategoryImage
                            {
                                CategoryId = entityId,
                                MediaFileId = mediaFile.Id,
                                CreatedAt = DateTime.UtcNow
                            };
                            context.CategoryImages.Add(categoryImage);
                        }

                        await context.SaveChangesAsync();

                        logger.LogInformation("Linked media file {MediaFileId} to category {CategoryId}", 
                            mediaFile.Id, entityId);
                        break;

                    case "setting":
                    case "team":
                        // For settings and team, just upload to media library
                        // The frontend will handle linking via setting value updates
                        logger.LogInformation("File uploaded for {EntityType} {EntityId}", entityType, entityId);
                        break;

                    default:
                        return Results.BadRequest(new { message = $"Unsupported entityType: {entityType}" });
                }

                return Results.Ok(new
                {
                    message = "File uploaded successfully",
                    file = new
                    {
                        fileName = mediaFile.FileName,
                        fileSize = mediaFile.FileSize,
                        mediaFileId = mediaFile.Id,
                        filePath = mediaFile.FilePath,
                        thumbnailPath = mediaFile.ThumbnailPath
                    }
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in unified photo upload");
                return Results.Problem("An error occurred while uploading the file");
            }
        })
        .WithName("UnifiedPhotoUpload")
        .WithSummary("Upload and link photo to entity (products, categories, settings, team)")
        .Produces(200)
        .Produces(400)
        .Produces(404)
        .RequireAuthorization("AdminOnly");

        // GET /api/v1/photos/{entityType}/{entityId} - Get photos for entity
        photoRoutes.MapGet("/{entityType}/{entityId:int}", async (
            string entityType,
            int entityId,
            PowersportsDbContext context,
            ILogger<Program> logger) =>
        {
            try
            {
                switch (entityType.ToLower())
                {
                    case "product":
                        var productImages = await context.ProductImages
                            .Where(pi => pi.ProductId == entityId)
                            .Include(pi => pi.MediaFile)
                            .OrderBy(pi => pi.SortOrder)
                            .Select(pi => new
                            {
                                fileName = pi.MediaFile.FileName,
                                fileSize = pi.MediaFile.FileSize,
                                uploadDate = pi.CreatedAt.ToString("o"),
                                isDefault = pi.IsMain,
                                mediaFileId = pi.MediaFileId,
                                filePath = pi.MediaFile.FilePath,
                                thumbnailPath = pi.MediaFile.ThumbnailPath
                            })
                            .ToListAsync();

                        return Results.Ok(new { files = productImages });

                    case "category":
                        var categoryImage = await context.CategoryImages
                            .Where(ci => ci.CategoryId == entityId)
                            .Include(ci => ci.MediaFile)
                            .Select(ci => new
                            {
                                fileName = ci.MediaFile.FileName,
                                fileSize = ci.MediaFile.FileSize,
                                uploadDate = ci.CreatedAt.ToString("o"),
                                isDefault = true,
                                mediaFileId = ci.MediaFileId,
                                filePath = ci.MediaFile.FilePath,
                                thumbnailPath = ci.MediaFile.ThumbnailPath
                            })
                            .FirstOrDefaultAsync();

                        var files = categoryImage != null ? new[] { categoryImage } : Array.Empty<object>();
                        return Results.Ok(new { files });

                    default:
                        return Results.BadRequest(new { message = $"Unsupported entityType: {entityType}" });
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving photos for {EntityType} {EntityId}", entityType, entityId);
                return Results.Problem("An error occurred while retrieving photos");
            }
        })
        .WithName("GetEntityPhotos")
        .WithSummary("Get photos for an entity")
        .Produces(200);

        // DELETE /api/v1/photos/{entityType}/{entityId}/{fileName} - Delete photo
        photoRoutes.MapDelete("/{entityType}/{entityId:int}/{fileName}", async (
            string entityType,
            int entityId,
            string fileName,
            PowersportsDbContext context,
            ILogger<Program> logger) =>
        {
            try
            {
                switch (entityType.ToLower())
                {
                    case "product":
                        var productImage = await context.ProductImages
                            .Include(pi => pi.MediaFile)
                            .FirstOrDefaultAsync(pi => pi.ProductId == entityId && pi.MediaFile.FileName == fileName);

                        if (productImage == null)
                        {
                            return Results.NotFound(new { message = "Image not found" });
                        }

                        context.ProductImages.Remove(productImage);
                        await context.SaveChangesAsync();

                        logger.LogInformation("Removed product image {FileName} from product {ProductId}", 
                            fileName, entityId);
                        return Results.Ok(new { message = "Image removed successfully" });

                    case "category":
                        var categoryImage = await context.CategoryImages
                            .Include(ci => ci.MediaFile)
                            .FirstOrDefaultAsync(ci => ci.CategoryId == entityId && ci.MediaFile.FileName == fileName);

                        if (categoryImage == null)
                        {
                            return Results.NotFound(new { message = "Image not found" });
                        }

                        context.CategoryImages.Remove(categoryImage);
                        await context.SaveChangesAsync();

                        logger.LogInformation("Removed category image {FileName} from category {CategoryId}", 
                            fileName, entityId);
                        return Results.Ok(new { message = "Image removed successfully" });

                    default:
                        return Results.BadRequest(new { message = $"Unsupported entityType: {entityType}" });
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting photo for {EntityType} {EntityId}", entityType, entityId);
                return Results.Problem("An error occurred while deleting the photo");
            }
        })
        .WithName("DeleteEntityPhoto")
        .WithSummary("Delete photo from entity")
        .Produces(200)
        .Produces(404)
        .RequireAuthorization("AdminOnly");

        // PATCH /api/v1/photos/{entityType}/{entityId}/{fileName}/default - Set as main
        photoRoutes.MapPatch("/{entityType}/{entityId:int}/{fileName}/default", async (
            string entityType,
            int entityId,
            string fileName,
            PowersportsDbContext context,
            ILogger<Program> logger) =>
        {
            try
            {
                if (entityType.ToLower() == "product")
                {
                    var productImage = await context.ProductImages
                        .Include(pi => pi.MediaFile)
                        .FirstOrDefaultAsync(pi => pi.ProductId == entityId && pi.MediaFile.FileName == fileName);

                    if (productImage == null)
                    {
                        return Results.NotFound(new { message = "Image not found" });
                    }

                    // Unset other main images
                    var otherMains = await context.ProductImages
                        .Where(pi => pi.ProductId == entityId && pi.IsMain && pi.Id != productImage.Id)
                        .ToListAsync();
                    otherMains.ForEach(pi => pi.IsMain = false);

                    productImage.IsMain = true;
                    await context.SaveChangesAsync();

                    logger.LogInformation("Set product image {FileName} as main for product {ProductId}", 
                        fileName, entityId);
                    return Results.Ok(new { message = "Main image updated" });
                }

                return Results.BadRequest(new { message = "Setting main image only supported for products" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error setting main photo for {EntityType} {EntityId}", entityType, entityId);
                return Results.Problem("An error occurred while setting main photo");
            }
        })
        .WithName("SetEntityPhotoAsMain")
        .WithSummary("Set photo as main for entity")
        .Produces(200)
        .Produces(404)
        .RequireAuthorization("AdminOnly");

        // Product Gallery Management
        ConfigureProductGalleryEndpoints(v1Routes);

        // Category Image Management
        ConfigureCategoryImageEndpoints(v1Routes);

        // Appointment Management
        var appointmentRoutes = v1Routes.MapGroup("/appointments").WithTags("Appointments");

        // GET all appointments
        appointmentRoutes.MapGet("/", async (PowersportsDbContext context, DateTime? startDate, DateTime? endDate) =>
        {
            var query = context.Appointments
                .Include(a => a.User)
                .Include(a => a.CreatedBy)
                .AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(a => a.StartTime >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(a => a.EndTime <= endDate.Value);
            }

            var appointments = await query
                .OrderBy(a => a.StartTime)
                .Select(a => new
                {
                    a.Id,
                    a.StartTime,
                    a.EndTime,
                    a.CustomerName,
                    a.CustomerEmail,
                    a.CustomerPhone,
                    a.ServiceType,
                    a.Notes,
                    a.Status,
                    a.UserId,
                    User = a.User != null ? new { a.User.Id, a.User.FirstName, a.User.LastName, a.User.Email } : null,
                    a.CreatedAt,
                    a.UpdatedAt,
                    CreatedBy = a.CreatedBy != null ? new { a.CreatedBy.FirstName, a.CreatedBy.LastName } : null
                })
                .ToListAsync();

            return Results.Ok(appointments);
        })
        .WithName("GetAppointments")
        .WithSummary("Get all appointments with optional date filtering")
        .Produces(200)
        .RequireAuthorization("AdminOnly");

        // GET appointment by ID
        appointmentRoutes.MapGet("/{id:int}", async (int id, PowersportsDbContext context) =>
        {
            var appointment = await context.Appointments
                .Include(a => a.User)
                .Include(a => a.CreatedBy)
                .Where(a => a.Id == id)
                .Select(a => new
                {
                    a.Id,
                    a.StartTime,
                    a.EndTime,
                    a.CustomerName,
                    a.CustomerEmail,
                    a.CustomerPhone,
                    a.ServiceType,
                    a.Notes,
                    a.Status,
                    a.UserId,
                    User = a.User != null ? new { a.User.Id, a.User.FirstName, a.User.LastName, a.User.Email } : null,
                    a.CreatedAt,
                    a.UpdatedAt,
                    CreatedBy = a.CreatedBy != null ? new { a.CreatedBy.FirstName, a.CreatedBy.LastName } : null
                })
                .FirstOrDefaultAsync();

            return appointment != null ? Results.Ok(appointment) : Results.NotFound();
        })
        .WithName("GetAppointmentById")
        .WithSummary("Get appointment by ID")
        .Produces(200)
        .Produces(404)
        .RequireAuthorization("AdminOnly");

        // POST create appointment
        appointmentRoutes.MapPost("/", async (CreateAppointmentRequest request, PowersportsDbContext context, HttpContext httpContext) =>
        {
            try
            {
                // Validate time
                if (request.EndTime <= request.StartTime)
                {
                    return Results.BadRequest(new { message = "End time must be after start time" });
                }

                // Check for overlapping appointments
                var hasOverlap = await context.Appointments
                    .AnyAsync(a => 
                        a.Status != "Cancelled" &&
                        ((request.StartTime >= a.StartTime && request.StartTime < a.EndTime) ||
                         (request.EndTime > a.StartTime && request.EndTime <= a.EndTime) ||
                         (request.StartTime <= a.StartTime && request.EndTime >= a.EndTime)));

                if (hasOverlap)
                {
                    return Results.BadRequest(new { message = "This time slot overlaps with an existing appointment" });
                }

                string? currentUserIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim, out int currentUserId))
                {
                    return Results.Unauthorized();
                }

                var appointment = new Appointment
                {
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    CustomerName = request.CustomerName,
                    CustomerEmail = request.CustomerEmail,
                    CustomerPhone = request.CustomerPhone,
                    ServiceType = request.ServiceType,
                    Notes = request.Notes,
                    Status = "Scheduled",
                    UserId = request.UserId,
                    CreatedByUserId = currentUserId,
                    CreatedAt = DateTime.UtcNow
                };

                context.Appointments.Add(appointment);
                await context.SaveChangesAsync();

                return Results.Created($"/api/v1/appointments/{appointment.Id}", new { id = appointment.Id, message = "Appointment created successfully" });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { message = $"Failed to create appointment: {ex.Message}" });
            }
        })
        .WithName("CreateAppointment")
        .WithSummary("Create a new appointment")
        .Produces(201)
        .Produces(400)
        .RequireAuthorization("AdminOnly");

        // PUT update appointment
        appointmentRoutes.MapPut("/{id:int}", async (int id, UpdateAppointmentRequest request, PowersportsDbContext context) =>
        {
            try
            {
                var appointment = await context.Appointments.FindAsync(id);
                if (appointment == null)
                {
                    return Results.NotFound(new { message = "Appointment not found" });
                }

                // Validate time
                if (request.EndTime <= request.StartTime)
                {
                    return Results.BadRequest(new { message = "End time must be after start time" });
                }

                // Check for overlapping appointments (excluding current appointment)
                var hasOverlap = await context.Appointments
                    .AnyAsync(a => 
                        a.Id != id &&
                        a.Status != "Cancelled" &&
                        ((request.StartTime >= a.StartTime && request.StartTime < a.EndTime) ||
                         (request.EndTime > a.StartTime && request.EndTime <= a.EndTime) ||
                         (request.StartTime <= a.StartTime && request.EndTime >= a.EndTime)));

                if (hasOverlap)
                {
                    return Results.BadRequest(new { message = "This time slot overlaps with an existing appointment" });
                }

                appointment.StartTime = request.StartTime;
                appointment.EndTime = request.EndTime;
                appointment.CustomerName = request.CustomerName;
                appointment.CustomerEmail = request.CustomerEmail;
                appointment.CustomerPhone = request.CustomerPhone;
                appointment.ServiceType = request.ServiceType;
                appointment.Notes = request.Notes;
                appointment.Status = request.Status;
                appointment.UserId = request.UserId;
                appointment.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();

                return Results.Ok(new { message = "Appointment updated successfully" });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { message = $"Failed to update appointment: {ex.Message}" });
            }
        })
        .WithName("UpdateAppointment")
        .WithSummary("Update an existing appointment")
        .Produces(200)
        .Produces(400)
        .Produces(404)
        .RequireAuthorization("AdminOnly");

        // PATCH update appointment status
        appointmentRoutes.MapPatch("/{id:int}/status", async (int id, UpdateAppointmentStatusRequest request, PowersportsDbContext context) =>
        {
            try
            {
                var appointment = await context.Appointments.FindAsync(id);
                if (appointment == null)
                {
                    return Results.NotFound(new { message = "Appointment not found" });
                }

                appointment.Status = request.Status;
                appointment.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();

                return Results.Ok(new { message = "Appointment status updated successfully", status = appointment.Status });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { message = $"Failed to update appointment status: {ex.Message}" });
            }
        })
        .WithName("UpdateAppointmentStatus")
        .WithSummary("Update appointment status (Scheduled, Completed, Cancelled, NoShow)")
        .Produces(200)
        .Produces(400)
        .Produces(404)
        .RequireAuthorization("AdminOnly");

        // DELETE appointment
        appointmentRoutes.MapDelete("/{id:int}", async (int id, PowersportsDbContext context) =>
        {
            try
            {
                var appointment = await context.Appointments.FindAsync(id);
                if (appointment == null)
                {
                    return Results.NotFound(new { message = "Appointment not found" });
                }

                context.Appointments.Remove(appointment);
                await context.SaveChangesAsync();

                return Results.Ok(new { message = "Appointment deleted successfully" });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { message = $"Failed to delete appointment: {ex.Message}" });
            }
        })
        .WithName("DeleteAppointment")
        .WithSummary("Delete an appointment")
        .Produces(200)
        .Produces(404)
        .RequireAuthorization("AdminOnly");

        // GET list of registered users for appointment selection
        appointmentRoutes.MapGet("/users", async (PowersportsDbContext context) =>
        {
            var users = await context.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Select(u => new
                {
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.Phone,
                    FullName = u.FirstName + " " + u.LastName
                })
                .ToListAsync();

            return Results.Ok(users);
        })
        .WithName("GetUsersForAppointments")
        .WithSummary("Get list of active users for appointment selection")
        .Produces(200)
        .RequireAuthorization("AdminOnly");

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

    private static void ConfigureProductGalleryEndpoints(RouteGroupBuilder v1Routes)
    {
        var productImageRoutes = v1Routes.MapGroup("/admin/products/{productId:int}/images");

        // POST /api/v1/admin/products/{productId}/images - Link MediaFile to product
        productImageRoutes.MapPost("/", async (
            int productId,
            LinkImageRequest request,
            PowersportsDbContext context,
            ILogger<Program> logger) =>
        {
            try
            {
                var product = await context.Products.FindAsync(productId);
                if (product == null)
                {
                    logger.LogWarning("Product {ProductId} not found", productId);
                    return Results.NotFound(new { message = "Product not found" });
                }

                var mediaFile = await context.MediaFiles.FindAsync(request.MediaFileId);
                if (mediaFile == null)
                {
                    logger.LogWarning("Media file {MediaFileId} not found", request.MediaFileId);
                    return Results.NotFound(new { message = "Media file not found" });
                }

                // Get next sort order
                var maxSort = await context.ProductImages
                    .Where(pi => pi.ProductId == productId)
                    .MaxAsync(pi => (int?)pi.SortOrder) ?? 0;

                // Check if this is the first image
                var isFirst = !await context.ProductImages.AnyAsync(pi => pi.ProductId == productId);

                var productImage = new ProductImage
                {
                    ProductId = productId,
                    MediaFileId = request.MediaFileId,
                    IsMain = request.IsMain || isFirst, // Use requested IsMain value or auto-set for first image
                    SortOrder = maxSort + 1,
                    CreatedAt = DateTime.UtcNow
                };

                context.ProductImages.Add(productImage);
                await context.SaveChangesAsync();

                logger.LogInformation("Media file {MediaFileId} linked to product {ProductId}", request.MediaFileId, productId);

                return Results.Ok(new
                {
                    id = productImage.Id,
                    mediaFileId = productImage.MediaFileId,
                    isMain = productImage.IsMain,
                    sortOrder = productImage.SortOrder
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error linking media file to product {ProductId}", productId);
                return Results.Problem($"Failed to link media file: {ex.Message}");
            }
        })
        .WithName("LinkMediaToProduct")
        .WithSummary("Link a media file to a product (Admin+ only)")
        .Produces(200)
        .Produces(404)
        .RequireAuthorization("AdminOnly");

        // GET /api/v1/admin/products/{productId}/images - Get product gallery
        productImageRoutes.MapGet("/", async (
            int productId,
            PowersportsDbContext context,
            ILogger<Program> logger) =>
        {
            try
            {
                var images = await context.ProductImages
                    .Where(pi => pi.ProductId == productId)
                    .Include(pi => pi.MediaFile)
                    .OrderBy(pi => pi.SortOrder)
                    .Select(pi => new
                    {
                        id = pi.Id,
                        mediaFileId = pi.MediaFileId,
                        isMain = pi.IsMain,
                        sortOrder = pi.SortOrder,
                        mediaFile = new
                        {
                            id = pi.MediaFile.Id,
                            fileName = pi.MediaFile.FileName,
                            caption = pi.MediaFile.Caption,
                            altText = pi.MediaFile.AltText,
                            filePath = pi.MediaFile.FilePath,
                            thumbnailPath = pi.MediaFile.ThumbnailPath,
                            width = pi.MediaFile.Width,
                            height = pi.MediaFile.Height,
                            fileSize = pi.MediaFile.FileSize
                        }
                    })
                    .ToListAsync();

                logger.LogInformation("Retrieved {Count} images for product {ProductId}", images.Count, productId);

                return Results.Ok(images);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving product gallery for product {ProductId}", productId);
                return Results.Problem($"Failed to retrieve gallery: {ex.Message}");
            }
        })
        .WithName("GetProductGallery")
        .WithSummary("Get all images for a product")
        .Produces(200);

        // PUT /api/v1/admin/products/{productId}/images/{id} - Update IsMain/SortOrder
        productImageRoutes.MapPut("/{id:int}", async (
            int productId,
            int id,
            UpdateProductImageRequest request,
            PowersportsDbContext context,
            ILogger<Program> logger) =>
        {
            try
            {
                var productImage = await context.ProductImages
                    .FirstOrDefaultAsync(pi => pi.Id == id && pi.ProductId == productId);

                if (productImage == null)
                {
                    logger.LogWarning("Product image {Id} not found for product {ProductId}", id, productId);
                    return Results.NotFound(new { message = "Image not found" });
                }

                // If setting as main, unset other main images
                if (request.IsMain && !productImage.IsMain)
                {
                    var otherMains = await context.ProductImages
                        .Where(pi => pi.ProductId == productId && pi.IsMain && pi.Id != id)
                        .ToListAsync();
                    otherMains.ForEach(pi => pi.IsMain = false);
                }

                productImage.IsMain = request.IsMain;
                productImage.SortOrder = request.SortOrder;

                await context.SaveChangesAsync();

                logger.LogInformation("Updated product image {Id}: IsMain={IsMain}, SortOrder={SortOrder}", 
                    id, request.IsMain, request.SortOrder);

                return Results.Ok(new
                {
                    id = productImage.Id,
                    isMain = productImage.IsMain,
                    sortOrder = productImage.SortOrder
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating product image {Id}", id);
                return Results.Problem($"Failed to update image: {ex.Message}");
            }
        })
        .WithName("UpdateProductImage")
        .WithSummary("Update product image settings (Admin+ only)")
        .Produces(200)
        .Produces(404)
        .RequireAuthorization("AdminOnly");

        // DELETE /api/v1/admin/products/{productId}/images/{id} - Unlink ProductImage
        productImageRoutes.MapDelete("/{id:int}", async (
            int productId,
            int id,
            PowersportsDbContext context,
            ILogger<Program> logger) =>
        {
            try
            {
                var productImage = await context.ProductImages
                    .FirstOrDefaultAsync(pi => pi.Id == id && pi.ProductId == productId);

                if (productImage == null)
                {
                    logger.LogWarning("Product image {Id} not found for product {ProductId}", id, productId);
                    return Results.NotFound(new { message = "Image not found" });
                }

                var wasMain = productImage.IsMain;

                context.ProductImages.Remove(productImage);

                // If this was the main image, promote another one
                if (wasMain)
                {
                    var nextMain = await context.ProductImages
                        .Where(pi => pi.ProductId == productId)
                        .OrderBy(pi => pi.SortOrder)
                        .FirstOrDefaultAsync();

                    if (nextMain != null)
                    {
                        nextMain.IsMain = true;
                    }
                }

                await context.SaveChangesAsync();

                logger.LogInformation("Removed product image {Id} from product {ProductId}", id, productId);

                return Results.Ok(new { message = "Image removed from product" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error removing product image {Id}", id);
                return Results.Problem($"Failed to remove image: {ex.Message}");
            }
        })
        .WithName("UnlinkProductImage")
        .WithSummary("Remove image from product (Admin+ only)")
        .Produces(200)
        .Produces(404)
        .RequireAuthorization("AdminOnly");
    }

    private static void ConfigureCategoryImageEndpoints(RouteGroupBuilder v1Routes)
    {
        var categoryImageRoutes = v1Routes.MapGroup("/admin/categories/{categoryId:int}/image");

        // POST /api/v1/admin/categories/{categoryId}/image - Link MediaFile to category
        categoryImageRoutes.MapPost("/", async (
            int categoryId,
            LinkImageRequest request,
            PowersportsDbContext context,
            ILogger<Program> logger) =>
        {
            try
            {
                var category = await context.Categories.FindAsync(categoryId);
                if (category == null)
                {
                    logger.LogWarning("Category {CategoryId} not found", categoryId);
                    return Results.NotFound(new { message = "Category not found" });
                }

                var mediaFile = await context.MediaFiles.FindAsync(request.MediaFileId);
                if (mediaFile == null)
                {
                    logger.LogWarning("Media file {MediaFileId} not found", request.MediaFileId);
                    return Results.NotFound(new { message = "Media file not found" });
                }

                // Check if category already has an image
                var existingImage = await context.CategoryImages
                    .FirstOrDefaultAsync(ci => ci.CategoryId == categoryId);

                if (existingImage != null)
                {
                    // Update existing
                    existingImage.MediaFileId = request.MediaFileId;
                    logger.LogInformation("Updated category {CategoryId} image to media file {MediaFileId}", 
                        categoryId, request.MediaFileId);
                }
                else
                {
                    // Create new
                    var categoryImage = new CategoryImage
                    {
                        CategoryId = categoryId,
                        MediaFileId = request.MediaFileId,
                        CreatedAt = DateTime.UtcNow
                    };

                    context.CategoryImages.Add(categoryImage);
                    logger.LogInformation("Linked media file {MediaFileId} to category {CategoryId}", 
                        request.MediaFileId, categoryId);
                }

                await context.SaveChangesAsync();

                return Results.Ok(new { message = "Category image updated successfully" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error linking media file to category {CategoryId}", categoryId);
                return Results.Problem($"Failed to link media file: {ex.Message}");
            }
        })
        .WithName("LinkMediaToCategory")
        .WithSummary("Link or update a media file for a category (Admin+ only)")
        .Produces(200)
        .Produces(404)
        .RequireAuthorization("AdminOnly");

        // GET /api/v1/admin/categories/{categoryId}/image - Get category image
        categoryImageRoutes.MapGet("/", async (
            int categoryId,
            PowersportsDbContext context) =>
        {
            try
            {
                var categoryImage = await context.CategoryImages
                    .Where(ci => ci.CategoryId == categoryId)
                    .Include(ci => ci.MediaFile)
                    .Select(ci => new
                    {
                        id = ci.Id,
                        mediaFileId = ci.MediaFileId,
                        mediaFile = new
                        {
                            id = ci.MediaFile.Id,
                            fileName = ci.MediaFile.FileName,
                            caption = ci.MediaFile.Caption,
                            altText = ci.MediaFile.AltText,
                            filePath = ci.MediaFile.FilePath,
                            thumbnailPath = ci.MediaFile.ThumbnailPath,
                            width = ci.MediaFile.Width,
                            height = ci.MediaFile.Height,
                            fileSize = ci.MediaFile.FileSize
                        }
                    })
                    .FirstOrDefaultAsync();

                if (categoryImage == null)
                {
                    return Results.Ok(new { message = "No image found for this category" });
                }

                return Results.Ok(categoryImage);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Failed to retrieve category image: {ex.Message}");
            }
        })
        .WithName("GetCategoryImage")
        .WithSummary("Get image for a category")
        .Produces(200);

        // DELETE /api/v1/admin/categories/{categoryId}/image - Unlink CategoryImage
        categoryImageRoutes.MapDelete("/", async (
            int categoryId,
            PowersportsDbContext context,
            ILogger<Program> logger) =>
        {
            try
            {
                var categoryImage = await context.CategoryImages
                    .FirstOrDefaultAsync(ci => ci.CategoryId == categoryId);

                if (categoryImage == null)
                {
                    logger.LogWarning("No image found for category {CategoryId}", categoryId);
                    return Results.NotFound(new { message = "No image found for this category" });
                }

                context.CategoryImages.Remove(categoryImage);
                await context.SaveChangesAsync();

                logger.LogInformation("Removed image from category {CategoryId}", categoryId);

                return Results.Ok(new { message = "Category image removed successfully" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error removing category image for category {CategoryId}", categoryId);
                return Results.Problem($"Failed to remove image: {ex.Message}");
            }
        })
        .WithName("UnlinkCategoryImage")
        .WithSummary("Remove image from category (Admin+ only)")
        .Produces(200)
        .Produces(404)
        .RequireAuthorization("AdminOnly");
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
                Phone = "7018222605",
                Role = UserRole.SuperAdmin,
                IsEmailVerified = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.Users.Add(superAdmin);
            await context.SaveChangesAsync();

            Console.WriteLine("✅ Super Admin user created successfully!");
        }
        else
        {
            existingSuperAdmin.FirstName = "Patrick";
            existingSuperAdmin.LastName = "Farrell";
            existingSuperAdmin.Phone = "7018222605";
            existingSuperAdmin.Role = UserRole.SuperAdmin;
            existingSuperAdmin.IsEmailVerified = true;
            existingSuperAdmin.IsActive = true;
            existingSuperAdmin.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            Console.WriteLine("✅ Super Admin user updated successfully!");
        }

        Console.WriteLine("Checking for site settings...");
        var settingsCount = await context.SiteSettings.CountAsync();
        Console.WriteLine($"Found {settingsCount} existing settings in database");
        
        var defaultSettings = new[]
            {
                // General Settings
                new SiteSetting { Key = "site_name", DisplayName = "Site Name", Value = "701 Performance Power", Type = SettingType.Text, Category = "General", SortOrder = 1, IsRequired = true },
                new SiteSetting { Key = "site_tagline", DisplayName = "Site Tagline", Value = "", Type = SettingType.Text, Category = "General", SortOrder = 2 },
                new SiteSetting { Key = "logo_url", DisplayName = "Logo URL", Value = "", Type = SettingType.Image, Category = "General", SortOrder = 3 },
                
                // Contact Settings  
                new SiteSetting { Key = "contact_email", DisplayName = "Contact Email", Value = "", Type = SettingType.Email, Category = "General", SortOrder = 4, IsRequired = true },
                new SiteSetting { Key = "contact_phone", DisplayName = "Contact Phone", Value = "", Type = SettingType.Phone, Category = "General", SortOrder = 5 },
                new SiteSetting { Key = "contact_address", DisplayName = "Contact Address", Value = "123 Powersports Drive, Fargo, ND 58102", Type = SettingType.TextArea, Category = "General", SortOrder = 6 },
                
                // Social Media
                new SiteSetting { Key = "facebook_url", DisplayName = "Facebook URL", Value = "", Type = SettingType.Url, Category = "Social Media", SortOrder = 1 },
                new SiteSetting { Key = "instagram_url", DisplayName = "Instagram URL", Value = "", Type = SettingType.Url, Category = "Social Media", SortOrder = 2 },
                new SiteSetting { Key = "twitter_url", DisplayName = "Twitter URL", Value = "", Type = SettingType.Url, Category = "Social Media", SortOrder = 3 },
                new SiteSetting { Key = "youtube_url", DisplayName = "YouTube URL", Value = "", Type = SettingType.Url, Category = "Social Media", SortOrder = 4 },
                
                // Homepage Content
                new SiteSetting { Key = "hero_title", DisplayName = "Homepage Hero Title", Value = "", Type = SettingType.Text, Category = "Homepage", SortOrder = 1 },
                new SiteSetting { Key = "hero_subtitle", DisplayName = "Homepage Hero Subtitle", Value = "", Type = SettingType.TextArea, Category = "Homepage", SortOrder = 2 },
                new SiteSetting { Key = "home_features", DisplayName = "Why Choose Us Features", Value = "[]", Type = SettingType.TextArea, Category = "Homepage", SortOrder = 3 },
                new SiteSetting { Key = "partner_brands", DisplayName = "Partner Brands", Value = "[]", Type = SettingType.TextArea, Category = "Homepage", SortOrder = 4 },
                new SiteSetting { Key = "brands_section_title", DisplayName = "Brands Section Title", Value = "", Type = SettingType.Text, Category = "Homepage", SortOrder = 5 },
                new SiteSetting { Key = "brands_section_subtitle", DisplayName = "Brands Section Subtitle", Value = "", Type = SettingType.Text, Category = "Homepage", SortOrder = 6 },
                
                // Products Page
                new SiteSetting { Key = "products_title", DisplayName = "Products Page Title", Value = "Our Products", Type = SettingType.Text, Category = "Products Page", SortOrder = 1 },
                new SiteSetting { Key = "products_subtitle", DisplayName = "Products Page Subtitle", Value = "Explore our complete collection of powersports vehicles and gear", Type = SettingType.TextArea, Category = "Products Page", SortOrder = 2 },
                
                // About Page
                new SiteSetting { Key = "about_title", DisplayName = "About Page Title", Value = "About Us", Type = SettingType.Text, Category = "About Page", SortOrder = 1 },
                new SiteSetting { Key = "about_subtitle", DisplayName = "About Page Subtitle", Value = "", Type = SettingType.TextArea, Category = "About Page", SortOrder = 2 },
                new SiteSetting { Key = "about_story_paragraph1", DisplayName = "Our Story - Paragraph 1", Value = "", Type = SettingType.TextArea, Category = "About Page", SortOrder = 3 },
                new SiteSetting { Key = "about_story_paragraph2", DisplayName = "Our Story - Paragraph 2", Value = "", Type = SettingType.TextArea, Category = "About Page", SortOrder = 4 },
                new SiteSetting { Key = "about_story_image", DisplayName = "Our Story - Image", Value = "", Type = SettingType.Image, Category = "About Page", SortOrder = 5 },
                new SiteSetting { Key = "about_mission_image", DisplayName = "Our Mission - Image", Value = "", Type = SettingType.Image, Category = "About Page", SortOrder = 6 },
                new SiteSetting { Key = "about_mission_text", DisplayName = "Our Mission - Main Text", Value = "", Type = SettingType.TextArea, Category = "About Page", SortOrder = 7 },
                new SiteSetting { Key = "about_mission_points", DisplayName = "Our Mission - Key Points", Value = "[]", Type = SettingType.TextArea, Category = "About Page", SortOrder = 8 },
                new SiteSetting { Key = "about_values", DisplayName = "Our Values", Value = "[]", Type = SettingType.TextArea, Category = "About Page", SortOrder = 9 },
                new SiteSetting { Key = "about_team_members", DisplayName = "Team Members", Value = "[]", Type = SettingType.TextArea, Category = "About Page", SortOrder = 10 },
                
                // Contact Page
                new SiteSetting { Key = "contact_title", DisplayName = "Contact Page Title", Value = "Contact Us", Type = SettingType.Text, Category = "Contact Page", SortOrder = 1 },
                new SiteSetting { Key = "contact_subtitle", DisplayName = "Contact Page Subtitle", Value = "Get in touch with our team for questions, support, or to schedule a visit", Type = SettingType.TextArea, Category = "Contact Page", SortOrder = 2 },
                new SiteSetting { Key = "contact_section_title", DisplayName = "Contact Cards Section Title", Value = "Get in Touch", Type = SettingType.Text, Category = "Contact Page", SortOrder = 3 },
                new SiteSetting { Key = "contact_address_title", DisplayName = "Address Card Title", Value = "Visit Our Showroom", Type = SettingType.Text, Category = "Contact Page", SortOrder = 4 },
                new SiteSetting { Key = "contact_phone_title", DisplayName = "Phone Card Title", Value = "Call Us", Type = SettingType.Text, Category = "Contact Page", SortOrder = 5 },
                new SiteSetting { Key = "contact_email_title", DisplayName = "Email Card Title", Value = "Email Us", Type = SettingType.Text, Category = "Contact Page", SortOrder = 6 },
                new SiteSetting { Key = "contact_livechat_title", DisplayName = "Live Chat Card Title", Value = "Live Chat", Type = SettingType.Text, Category = "Contact Page", SortOrder = 7 },
                new SiteSetting { Key = "contact_hours", DisplayName = "Business Hours", Value = "Mon-Fri: 9AM-6PM\nSat: 9AM-4PM", Type = SettingType.TextArea, Category = "Contact Page", SortOrder = 8 },
                new SiteSetting { Key = "contact_address_note", DisplayName = "Address Note", Value = "Open for personal consultations", Type = SettingType.Text, Category = "Contact Page", SortOrder = 9 },
                new SiteSetting { Key = "contact_email_note", DisplayName = "Email Response Note", Value = "We respond within 24 hours", Type = SettingType.Text, Category = "Contact Page", SortOrder = 10 },
                new SiteSetting { Key = "contact_livechat_text", DisplayName = "Live Chat Availability", Value = "Available during business hours", Type = SettingType.Text, Category = "Contact Page", SortOrder = 11 },
                new SiteSetting { Key = "contact_livechat_note", DisplayName = "Live Chat Note", Value = "Quick answers to your questions", Type = SettingType.Text, Category = "Contact Page", SortOrder = 12 },
                new SiteSetting { Key = "contact_reasons", DisplayName = "Why Contact Us - Reasons", Value = "[]", Type = SettingType.TextArea, Category = "Contact Page", SortOrder = 13 },
                
                // FAQ Page
                new SiteSetting { Key = "faq_title", DisplayName = "FAQ Page Title", Value = "FAQ", Type = SettingType.Text, Category = "FAQ Page", SortOrder = 1 },
                new SiteSetting { Key = "faq_subtitle", DisplayName = "FAQ Page Subtitle", Value = "Frequently Asked Questions", Type = SettingType.TextArea, Category = "FAQ Page", SortOrder = 2 },
                new SiteSetting { Key = "faq_content", DisplayName = "FAQ Page Content", Value = "", Type = SettingType.Html, Category = "FAQ Page", SortOrder = 3 },
                
                // Shipping & Returns Page
                new SiteSetting { Key = "shipping_title", DisplayName = "Shipping & Returns Page Title", Value = "Shipping & Returns", Type = SettingType.Text, Category = "Shipping Page", SortOrder = 1 },
                new SiteSetting { Key = "shipping_subtitle", DisplayName = "Shipping & Returns Page Subtitle", Value = "", Type = SettingType.TextArea, Category = "Shipping Page", SortOrder = 2 },
                new SiteSetting { Key = "shipping_content", DisplayName = "Shipping & Returns Page Content", Value = "", Type = SettingType.Html, Category = "Shipping Page", SortOrder = 3 },
                
                // Privacy Policy Page
                new SiteSetting { Key = "privacy_title", DisplayName = "Privacy Policy Page Title", Value = "Privacy Policy", Type = SettingType.Text, Category = "Privacy Page", SortOrder = 1 },
                new SiteSetting { Key = "privacy_subtitle", DisplayName = "Privacy Policy Page Subtitle", Value = "", Type = SettingType.TextArea, Category = "Privacy Page", SortOrder = 2 },
                new SiteSetting { Key = "privacy_content", DisplayName = "Privacy Policy Page Content", Value = "", Type = SettingType.Html, Category = "Privacy Page", SortOrder = 3 },
                
                // Terms of Service Page
                new SiteSetting { Key = "terms_title", DisplayName = "Terms of Service Page Title", Value = "Terms of Service", Type = SettingType.Text, Category = "Terms Page", SortOrder = 1 },
                new SiteSetting { Key = "terms_subtitle", DisplayName = "Terms of Service Page Subtitle", Value = "", Type = SettingType.TextArea, Category = "Terms Page", SortOrder = 2 },
                new SiteSetting { Key = "terms_content", DisplayName = "Terms of Service Page Content", Value = "", Type = SettingType.Html, Category = "Terms Page", SortOrder = 3 },
                
                // Advanced - Security & Access
                new SiteSetting { Key = "session_timeout", DisplayName = "Session Timeout (minutes)", Value = "480", Type = SettingType.Number, Category = "Advanced", SortOrder = 1 },
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

                // Music Player
                new SiteSetting { Key = "music_enabled", DisplayName = "Enable Music Player", Value = "false", Type = SettingType.Boolean, Category = "Music", SortOrder = 1 },
                new SiteSetting { Key = "music_embed_code", DisplayName = "Music Embed Code", Value = "", Type = SettingType.TextArea, Category = "Music", SortOrder = 2 },

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

// Request DTOs
public record LinkImageRequest(int MediaFileId, bool IsMain = false);
public record UpdateProductImageRequest(bool IsMain, int SortOrder);
