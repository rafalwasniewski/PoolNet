# PoolNet

Aplikacja webowa ASP.NET Core MVC (.NET 10) do ewidencji meczów bilardowych (odmiany 8, 9 i 10), lig oraz turniejów pucharowych. Dane są przechowywane w SQLite (EF Core). Administrator zapisuje wyniki i tworzy rozgrywki; goście mają dostęp tylko do odczytu.

## Wymagania

- .NET SDK 10
- opcjonalnie Docker / Podman

## Uruchomienie lokalne

```bash
dotnet run --project src/PoolNet.Web
```

Aplikacja domyślnie nasłuchuje na `http://localhost:5092` (profil `http`).

W środowisku **Development** dostępne jest konto demonstracyjne:

- login: `admin`
- hasło: `admin`

Dane demonstracyjne (50 graczy, ligi i turnieje) są seedowane wyłącznie w Development i tylko wtedy, gdy baza jest pusta. Seed można wyłączyć ustawiając `SeedData__Enabled=false`.

## Testy

```bash
dotnet test
```

## Konfiguracja

| Klucz | Opis |
| --- | --- |
| `ConnectionStrings__DefaultConnection` | Ścieżka/connection string do SQLite |
| `SeedData__Enabled` | `true` włącza seed danych demo (tylko pusta baza) |
| `AdminCredentials__Username` | Login administratora |
| `AdminCredentials__PasswordHash` | Hash hasła (PBKDF2/ASP.NET `PasswordHasher`) |

Hasło administratora jest przechowywane wyłącznie jako hash. Do wygenerowania hasha służy narzędzie:

```bash
dotnet run --project tools/PoolNet.PasswordHasher -- "moje-hasło"
```

W środowisku produkcyjnym ustaw `AdminCredentials__PasswordHash` przez zmienną środowiskową, sekret lub user-secrets. Nie zapisuj prawdziwych danych dostępowych w `appsettings.json`. Logowanie jest wyłączone, jeśli hash nie jest skonfigurowany.

## Docker

```bash
export POOLNET_ADMIN_PASSWORD_HASH="$(dotnet run --project tools/PoolNet.PasswordHasher -- 'moje-hasło')"
docker compose up --build
```

Baza jest przechowywana w wolumenie `poolnet_data` (`/app/data/poolnet.db`). Aplikacja działa za reverse proxy — TLS jest terminowany na proxy, a nagłówki `X-Forwarded-*` są obsługiwane dzięki `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true`.

## Struktura

```
src/PoolNet.Domain/          encje, enumy, reguły domenowe
src/PoolNet.Application/     DTO, serwisy, generatory terminarzy i drabinek
src/PoolNet.Infrastructure/  EF Core, SQLite, seeder
src/PoolNet.Web/             ASP.NET Core MVC, widoki Razor, autoryzacja
tests/PoolNet.UnitTests/     testy jednostkowe (xUnit + FluentAssertions)
tools/PoolNet.PasswordHasher/ generator hashy haseł
```
