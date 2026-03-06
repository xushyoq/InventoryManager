using Bogus;
using InventoryManager.Models;

namespace InventoryManager.Data;

public static class DataSeeder
{
    private static readonly string[] TagNames =
    [
        "электроника", "книги", "мебель", "одежда", "инструменты",
        "игрушки", "спорт", "кухня", "офис", "сад", "музыка",
        "искусство", "коллекция", "антиквариат", "техника", "канцелярия",
        "косметика", "продукты", "медицина", "путешествия"
    ];

    private static readonly string[] Categories = ["Other", "Books", "Electronics", "Clothing"];
    public static async Task SeedAsync(AppDbContext context)
    {
        var faker = new Faker("ru");

        // Users
        var users = new List<User>();
        for (int i = 0; i < 10; i++)
        {
            users.Add(new User
            {
                Email = faker.Internet.Email(),
                Name = faker.Name.FullName(),
                ProfileImageUrl = faker.Internet.Avatar(),
                Provider = "Seed",
                ProviderUserId = $"seed-{i + 1}"
            });
        }

        context.Users.AddRange(users);
        await context.SaveChangesAsync();

        // Tags
        var tagNamesToUse = faker.PickRandom(TagNames, 20).Distinct().ToList();
        var tags = tagNamesToUse.Select(name => new Tag { Name = name.ToLowerInvariant() }).ToList();
        context.Tags.AddRange(tags);
        await context.SaveChangesAsync();

        // Inventories
        var inventoryFaker = new Faker<Inventory>()
            .RuleFor(i => i.Name, f => f.Commerce.ProductName())
            .RuleFor(i => i.Description, f => f.Lorem.Paragraph())
            .RuleFor(i => i.Category, f => f.PickRandom(Categories))
            .RuleFor(i => i.IsPublic, f => f.Random.Bool(0.4f))
            .RuleFor(i => i.CreatedById, f => f.PickRandom(users).Id)
            .RuleFor(i => i.CreatedAt, f => DateTime.SpecifyKind(f.Date.Past(2), DateTimeKind.Utc))
            .RuleFor(i => i.UpdatedAt, (f, i) => i.CreatedAt);

        var inventories = inventoryFaker.Generate(30);

        foreach (var inv in inventories)
        {
            ApplyRandomCustomFields(inv, faker);
        }

        context.Inventories.AddRange(inventories);
        await context.SaveChangesAsync();

        // InventoryTags
        foreach (var inventory in inventories)
        {
            var inventoryTags = faker.PickRandom(tags, faker.Random.Int(1, Math.Min(5, tags.Count())))
                .Distinct()
                .Select(tag => new InventoryTag { InventoryId = inventory.Id, TagId = tag.Id });
            context.InventoryTags.AddRange(inventoryTags);
        }

        await context.SaveChangesAsync();

        // Items
        var items = new List<Item>();
        foreach (var inventory in inventories)
        {
            var owner = users.First(u => u.Id == inventory.CreatedById);
            for (int i = 0; i < 10; i++)
            {
                var item = new Item
                {
                    InventoryId = inventory.Id,
                    CreatedById = owner.Id,
                    CustomId = faker.Random.AlphaNumeric(8).ToUpper(),
                    CreatedAt = DateTime.SpecifyKind(faker.Date.Past(1), DateTimeKind.Utc)
                };

                ApplyRandomCustomFields(item, faker);
                items.Add(item);
            }
        }

        context.Items.AddRange(items);
        await context.SaveChangesAsync();

    }

    private static void ApplyRandomCustomFields(Inventory inventory, Faker faker)
    {
        var fieldsActions = new List<Action>()
        {
            () => { inventory.CustomString1State = faker.Random.Bool(0.5f); inventory.CustomString1Name = "Название"; },
            () => { inventory.CustomString2State = faker.Random.Bool(0.5f); inventory.CustomString2Name = "Модель"; },
            () => { inventory.CustomString3State = faker.Random.Bool(0.5f); inventory.CustomString3Name = "Серия"; },
            () => { inventory.CustomText1State = faker.Random.Bool(0.4f); inventory.CustomText1Name = "Описание"; },
            () => { inventory.CustomText2State = faker.Random.Bool(0.4f); inventory.CustomText2Name = "Заметки"; },
            () => { inventory.CustomInt1State = faker.Random.Bool(0.5f); inventory.CustomInt1Name = "Количество"; },
            () => { inventory.CustomInt2State = faker.Random.Bool(0.5f); inventory.CustomInt2Name = "Год"; },
            () => { inventory.CustomInt3State = faker.Random.Bool(0.5f); inventory.CustomInt3Name = "Цена"; },
            () => { inventory.CustomBool1State = faker.Random.Bool(0.4f); inventory.CustomBool1Name = "В наличии"; },
            () => { inventory.CustomBool2State = faker.Random.Bool(0.4f); inventory.CustomBool2Name = "Новое"; },
            () => { inventory.CustomLink1State = faker.Random.Bool(0.3f); inventory.CustomLink1Name = "Ссылка"; },
        };

        faker.PickRandom(fieldsActions, faker.Random.Int(3, 8)).ToList().ForEach(a => a());
    }

    private static void ApplyRandomCustomFields(Item item, Faker faker)
    {
        if (faker.Random.Bool(0.6f)) item.CustomString1 = faker.Commerce.ProductName();
        if (faker.Random.Bool(0.6f)) item.CustomString2 = faker.Commerce.ProductAdjective();
        if (faker.Random.Bool(0.5f)) item.CustomText1 = faker.Lorem.Sentence();
        if (faker.Random.Bool(0.5f)) item.CustomInt1 = faker.Random.Int(1, 100);
        if (faker.Random.Bool(0.5f)) item.CustomInt2 = faker.Random.Int(1990, 2025);
        if (faker.Random.Bool(0.4f)) item.CustomBool1 = faker.Random.Bool();
        if (faker.Random.Bool(0.3f)) item.CustomLink1 = faker.Internet.Url();
    }

}