using Microsoft.AspNetCore.Identity;

if (args.Length != 1 || string.IsNullOrWhiteSpace(args[0]))
{
    Console.Error.WriteLine("Użycie: dotnet run --project tools/PoolNet.PasswordHasher -- <hasło>");
    return 1;
}

var hasher = new PasswordHasher<string>();
Console.WriteLine(hasher.HashPassword("admin", args[0]));
return 0;
