using DentalLink.Application.Common.Interfaces;
using DentalLink.Domain.Constants;
using DentalLink.Domain.Entities;
using DentalLink.Domain.Entities.Lookup;
using DentalLink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DentalLink.API.Infrastructure;

public static class DatabaseInitializer
{
    public static async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DentalLinkDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DentalLinkDbContext>>();

        try
        {
            await context.Database.EnsureCreatedAsync();

            // Seed Cabinets if empty
            if (!await context.Cabinets.AnyAsync())
            {
                context.Cabinets.AddRange(
                    new Cabinet { Name = "Кабинет №1 (Терапия)", ChairNumber = 1, Description = "Оснащён дентальным микроскопом Carl Zeiss и установкой Sirona.", IsAvailable = true },
                    new Cabinet { Name = "Кабинет №2 (Хирургия и имплантация)", ChairNumber = 1, Description = "Стерильная операционная с физиодиспенсером.", IsAvailable = true },
                    new Cabinet { Name = "Кабинет №3 (Ортодонтия)", ChairNumber = 1, Description = "Кабинет функциональной диагностики с 3D-сканером iTero.", IsAvailable = true },
                    new Cabinet { Name = "Кабинет №4 (Детская стоматология)", ChairNumber = 1, Description = "Детский дизайн интерьера, монитор над креслом.", IsAvailable = true }
                );
                await context.SaveChangesAsync();
            }

            // Seed Service Categories & Services if empty
            if (!await context.ServiceCategories.AnyAsync())
            {
                var catTherapy = new ServiceCategory { Name = "Терапевтическая стоматология", Description = "Лечение кариеса, пульпита и художественная реставрация" };
                var catSurgery = new ServiceCategory { Name = "Хирургия и имплантация", Description = "Удаление зубов, костная пластика и имплантация под ключ" };
                var catOrtho = new ServiceCategory { Name = "Ортодонтия", Description = "Исправление прикуса брекетами и прозрачными элайнерами" };
                var catPediatric = new ServiceCategory { Name = "Детская стоматология", Description = "Бережное лечение молочных и постоянных зубов без боли" };
                var catHygiene = new ServiceCategory { Name = "Профгигиена и отбеливание", Description = "Ультразвуковая чистка Air-Flow и безопасное отбеливание" };

                context.ServiceCategories.AddRange(catTherapy, catSurgery, catOrtho, catPediatric, catHygiene);
                await context.SaveChangesAsync();

                context.DentalServices.AddRange(
                    new DentalService { CategoryId = catTherapy.Id, Name = "Первичная консультация и осмотр", Price = 50.00m, DurationMinutes = 30, IsActive = true, Description = "Детальный визуальный осмотр, фотопротокол и составление плана лечения" },
                    new DentalService { CategoryId = catTherapy.Id, Name = "Лечение глубокого кариеса световой пломбой", Price = 250.00m, DurationMinutes = 60, IsActive = true, Description = "Препарирование, лечебная прокладка и моделирование анатомической формы зуба" },
                    new DentalService { CategoryId = catTherapy.Id, Name = "Эндодонтическое лечение одноканального зуба", Price = 400.00m, DurationMinutes = 60, IsActive = true, Description = "Механическая и медикаментозная обработка корневого канала под микроскопом" },
                    new DentalService { CategoryId = catSurgery.Id, Name = "Простое удаление зуба", Price = 150.00m, DurationMinutes = 30, IsActive = true, Description = "Атравматичное удаление под инфильтрационной или проводниковой анестезией" },
                    new DentalService { CategoryId = catSurgery.Id, Name = "Установка дентального имплантата (Osstem/Straumann)", Price = 3500.00m, DurationMinutes = 90, IsActive = true, Description = "Хирургический этап дентальной имплантации с наложением швов" },
                    new DentalService { CategoryId = catOrtho.Id, Name = "Консультация ортодонта с 3D-сканированием iTero", Price = 120.00m, DurationMinutes = 30, IsActive = true, Description = "Снятие цифровых оттисков, компьютерный прогноз перемещения зубов" },
                    new DentalService { CategoryId = catOrtho.Id, Name = "Установка самолигирующей брекет-системы (1 челюсть)", Price = 4500.00m, DurationMinutes = 90, IsActive = true, Description = "Фиксация брекетов Damon с подбором дуги" },
                    new DentalService { CategoryId = catPediatric.Id, Name = "Лечение кариеса молочного зуба", Price = 180.00m, DurationMinutes = 30, IsActive = true, Description = "Безболезненное пломбирование биосовместимыми детскими материалами" },
                    new DentalService { CategoryId = catHygiene.Id, Name = "Комплексная гигиена полости рта (Air-Flow + УЗ)", Price = 300.00m, DurationMinutes = 60, IsActive = true, Description = "Снятие твёрдого зубного камня, полировка пастами и покрытие фторлаком" }
                );
                await context.SaveChangesAsync();
            }

            // Seed Users & Doctors if empty
            if (!await context.Users.AnyAsync())
            {
                var defaultPasswordHash = hasher.HashPassword("Password123!");

                var admin = new User
                {
                    FullName = "Саидов Фаррух Джамшедович",
                    Email = "admin@dentallink.tj",
                    PasswordHash = defaultPasswordHash,
                    Phone = "+992900000001",
                    RoleId = LookupIds.UserRole.Admin,
                    IsActive = true,
                };

                var receptionist = new User
                {
                    FullName = "Саидова Нигина Алишеровна",
                    Email = "reception@dentallink.tj",
                    PasswordHash = defaultPasswordHash,
                    Phone = "+992900000002",
                    RoleId = LookupIds.UserRole.Receptionist,
                    IsActive = true,
                };

                var doctorUser1 = new User
                {
                    FullName = "Рахимов Алишер Баходурович",
                    Email = "a.rahimov@dentallink.tj",
                    PasswordHash = defaultPasswordHash,
                    Phone = "+992927770001",
                    RoleId = LookupIds.UserRole.Doctor,
                    IsActive = true,
                };

                var doctorUser2 = new User
                {
                    FullName = "Умаров Джамшед Каримович",
                    Email = "j.umarov@dentallink.tj",
                    PasswordHash = defaultPasswordHash,
                    Phone = "+992927770002",
                    RoleId = LookupIds.UserRole.Doctor,
                    IsActive = true,
                };

                var doctorUser3 = new User
                {
                    FullName = "Турсунова Зарина Мурадовна",
                    Email = "z.tursunova@dentallink.tj",
                    PasswordHash = defaultPasswordHash,
                    Phone = "+992927770003",
                    RoleId = LookupIds.UserRole.Doctor,
                    IsActive = true,
                };

                context.Users.AddRange(admin, receptionist, doctorUser1, doctorUser2, doctorUser3);
                await context.SaveChangesAsync();

                var doctor1 = new Doctor
                {
                    UserId = doctorUser1.Id,
                    SpecializationId = LookupIds.DoctorSpecialization.Therapist,
                    Qualification = "Врач высшей квалификационной категории",
                    ExperienceYears = 12,
                    Bio = "Специализируется на эстетической реставрации зубов и микроскопической эндодонтии.",
                    AvatarUrl = "https://images.unsplash.com/photo-1622253692010-333f2da6031d?w=300",
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                var doctor2 = new Doctor
                {
                    UserId = doctorUser2.Id,
                    SpecializationId = LookupIds.DoctorSpecialization.Implantologist,
                    Qualification = "Кандидат медицинских наук, челюстно-лицевой хирург",
                    ExperienceYears = 15,
                    Bio = "Проводит сложные операции по костной пластике, синус-лифтингу и дентальной имплантации.",
                    AvatarUrl = "https://images.unsplash.com/photo-1537368910025-700350fe46c7?w=300",
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                var doctor3 = new Doctor
                {
                    UserId = doctorUser3.Id,
                    SpecializationId = LookupIds.DoctorSpecialization.Orthodontist,
                    Qualification = "Ведущий ортодонт, член Ассоциации ортодонтов",
                    ExperienceYears = 8,
                    Bio = "Эксперт по исправлению прикуса современными брекет-системами Damon и элайнерами.",
                    AvatarUrl = "https://images.unsplash.com/photo-1594824813583-9b8b73a7f849?w=300",
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                context.Doctors.AddRange(doctor1, doctor2, doctor3);
                await context.SaveChangesAsync();

                // Generate initial shifts & slots for next 14 days
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var cab1 = await context.Cabinets.FirstAsync(c => c.Name.Contains("Терапия"));
                var cab2 = await context.Cabinets.FirstAsync(c => c.Name.Contains("Хирургия"));
                var cab3 = await context.Cabinets.FirstAsync(c => c.Name.Contains("Ортодонтия"));

                for (int dayOffset = 0; dayOffset < 14; dayOffset++)
                {
                    var currentDate = today.AddDays(dayOffset);

                    // Skip Sundays if needed or keep daily
                    var schedule1 = new DoctorSchedule
                    {
                        DoctorId = doctor1.Id,
                        CabinetId = cab1.Id,
                        WorkDate = currentDate,
                        StartTime = new TimeOnly(9, 0),
                        EndTime = new TimeOnly(15, 0),
                        CreatedAt = DateTimeOffset.UtcNow
                    };

                    var schedule2 = new DoctorSchedule
                    {
                        DoctorId = doctor2.Id,
                        CabinetId = cab2.Id,
                        WorkDate = currentDate,
                        StartTime = new TimeOnly(12, 0),
                        EndTime = new TimeOnly(18, 0),
                        CreatedAt = DateTimeOffset.UtcNow
                    };

                    var schedule3 = new DoctorSchedule
                    {
                        DoctorId = doctor3.Id,
                        CabinetId = cab3.Id,
                        WorkDate = currentDate,
                        StartTime = new TimeOnly(14, 0),
                        EndTime = new TimeOnly(20, 0),
                        CreatedAt = DateTimeOffset.UtcNow
                    };

                    context.DoctorSchedules.AddRange(schedule1, schedule2, schedule3);
                    await context.SaveChangesAsync();

                    GenerateSlotsForSchedule(context, schedule1);
                    GenerateSlotsForSchedule(context, schedule2);
                    GenerateSlotsForSchedule(context, schedule3);
                }

                await context.SaveChangesAsync();
                logger.LogInformation("Database successfully seeded with initial DentalLink records.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during database initialization/seeding.");
        }
    }

    private static void GenerateSlotsForSchedule(DentalLinkDbContext context, DoctorSchedule schedule)
    {
        var current = schedule.StartTime;
        while (current.AddMinutes(30) <= schedule.EndTime)
        {
            var next = current.AddMinutes(30);
            context.TimeSlots.Add(new TimeSlot
            {
                ScheduleId = schedule.Id,
                DoctorId = schedule.DoctorId,
                CabinetId = schedule.CabinetId,
                SlotDate = schedule.WorkDate,
                StartTime = current,
                EndTime = next,
                StatusId = LookupIds.TimeSlotStatus.Free,
                ReservedUntil = null
            });
            current = next;
        }
    }
}
