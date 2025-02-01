using Microsoft.AspNetCore.Identity;
using WashDelivery.Application.Interfaces;
using WashDelivery.Domain.Constants;
using WashDelivery.Domain.Entities;
using WashDelivery.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace WashDelivery.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedTestData(
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        IRepository<Laundry> laundryRepository)
    {
        // Create roles if they don't exist
        var roles = new[] { Roles.Admin, Roles.Customer, Roles.Courier, Roles.LaundryWorker, Roles.LaundryManager };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Common password for all test users
        const string password = "Test123!";

        // Create test users if they don't exist
        var testUsers = new[]
        {
            new { Email = "admin@test.com", Role = Roles.Admin, FirstName = "Adam", LastName = "Admin" },
            new { Email = "customer@test.com", Role = Roles.Customer, FirstName = "Karol", LastName = "Klient" },
            new { Email = "courier@test.com", Role = Roles.Courier, FirstName = "Krzysztof", LastName = "Kurier" },
            new { Email = "worker@test.com", Role = Roles.LaundryWorker, FirstName = "Wojciech", LastName = "Worker" },
            new { Email = "manager@test.com", Role = Roles.LaundryManager, FirstName = "Mariusz", LastName = "Manager" }
        };

        // Standard laundry services that will be available in all laundries
        var standardServices = new[]
        {
            // Base services (urgency)
            new LaundryService(
                name: "Standardowe pranie",
                description: "Realizacja do 24h od odbioru. Zamówienia złożone po 18:00 realizowane są następnego dnia.",
                price: 15.99m,
                unit: "kg",
                isExtraService: false),
                
            new LaundryService(
                name: "Ekspresowe pranie",
                description: "Realizacja ~4h od odbioru. Dostępne tylko dla zamówień złożonych do 14:00.",
                price: 24.99m,
                unit: "kg",
                isExtraService: false),
                
            // Extra services
            new LaundryService(
                name: "Prasowanie",
                description: "Usługa prasowania dla Twoich ubrań.",
                price: 12.99m,
                unit: "szt",
                isExtraService: true),
                
            new LaundryService(
                name: "Pranie chemiczne",
                description: "Profesjonalne czyszczenie chemiczne dla delikatnych tkanin.",
                price: 29.99m,
                unit: "szt",
                isExtraService: true),
                
            new LaundryService(
                name: "Pościel i duże rzeczy",
                description: "Pranie pościeli, koców, zasłon i innych dużych elementów.",
                price: 19.99m,
                unit: "kg",
                isExtraService: true)
        };

        // Create test laundries if they don't exist
        var testLaundries = await laundryRepository.GetAllAsync();
        Laundry? testLaundry = null;
        
        if (!testLaundries.Any())
        {
            testLaundry = new Laundry(
                name: "Pralnia Testowa",
                contactEmail: "test@pralnia.pl",
                contactPhone: "123456789",
                address: new LocationAddress(
                    street: "ul. Testowa 1",
                    city: "Warszawa",
                    postalCode: "00-001",
                    latitude: 52.237049,
                    longitude: 21.017532
                )
            );

            // Add standard services to the laundry
            foreach (var service in standardServices)
            {
                testLaundry.AddService(service);
            }

            testLaundry.Activate(); // Make sure the laundry is active
            await laundryRepository.AddAsync(testLaundry);
        }
        else
        {
            testLaundry = testLaundries.First();
        }

        foreach (var testUser in testUsers)
        {
            var existingUser = await userManager.FindByEmailAsync(testUser.Email);
            if (existingUser == null)
            {
                User user = testUser.Role switch
                {
                    Roles.Customer => new Customer(
                        email: testUser.Email,
                        phoneNumber: "123456789",
                        firstName: testUser.FirstName,
                        lastName: testUser.LastName
                    ),
                    Roles.Courier => new Courier(
                        email: testUser.Email,
                        phoneNumber: "123456789",
                        firstName: testUser.FirstName,
                        lastName: testUser.LastName
                    ),
                    Roles.LaundryWorker => new LaundryWorker(
                        email: testUser.Email,
                        phoneNumber: "123456789",
                        firstName: testUser.FirstName,
                        lastName: testUser.LastName,
                        laundry: testLaundry
                    ),
                    Roles.LaundryManager => new LaundryManager(
                        email: testUser.Email,
                        phoneNumber: "123456789",
                        firstName: testUser.FirstName,
                        lastName: testUser.LastName,
                        laundry: testLaundry
                    ),
                    _ => new User(
                        email: testUser.Email,
                        phoneNumber: "123456789",
                        firstName: testUser.FirstName,
                        lastName: testUser.LastName
                    )
                };

                // Ensure all seeded users are active
                user.Activate();

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, testUser.Role);
                    
                    // Assign laundry worker to the test laundry
                    if ((testUser.Role == Roles.LaundryWorker || testUser.Role == Roles.LaundryManager) && testLaundry != null)
                    {
                        user.AssignToLaundry(testLaundry);
                        await userManager.UpdateAsync(user);
                    }
                }
            }
            else if (testUser.Role == Roles.LaundryWorker && testLaundry != null && string.IsNullOrEmpty(existingUser.LaundryId))
            {
                existingUser.AssignToLaundry(testLaundry);
                await userManager.UpdateAsync(existingUser);
            }
        }
    }

    private static async Task<bool> AnyLaundriesExistAsync(IRepository<Laundry> laundryRepository)
    {
        var laundries = await laundryRepository.GetAllAsync();
        return laundries.Any();
    }
}
