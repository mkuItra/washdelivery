# WashDelivery - System obsługi usług prania na wynos

## Autorzy projektu:
- Jakub Chimiak
- Maksymilian Jarosz

## Opis działania projektu:
„WashDelivery” to aplikacja webowa przeznaczona do zarządzania usługą prania na wynos. Aplikacja pozwala na bieżąco śledzić status zamówień, dzięki czemu użytkownicy mogą monitorować postęp realizacji swoich usług. „WashDelivery” usprawnia procesy zarządzania pralniami, zapewnia przejrzystość operacji oraz ułatwia obsługę zamówień. Jest responsywna i dostosowana do pracy na urządzeniach mobilnych i desktopowych.

## Specyfikacja Technologii:

**Backend:**
- C# .NET 8.0
- ASP.NET Core MVC
- SignalR (Real-time notifications) - działa tylko dla widoku zamówień pralni, nowe zamówienia automatycznie wskakują na stronę.

**Baza danych:**
- SQLite

**Frontend:**
- HTML
- Tailwind
- JavaScript
- SignalR (Real-time notifications)

## Konta użytkowników:

- **Admin**: admin@test.com / Test123!
- **Courier**: courier@test.com / Test123!
- **Manager**: manager@test.com / Test123!
- **Worker**: worker@test.com / Test123!
- **Customer**: customer@test.com / Test123!

## Instrukcja pierwszego uruchomienia:

Baza danych jest już w repozytorium, wystarczy uruchomić serwer z katalogu src/Washdelivery.Web poleceniem:

```bash
dotnet watch
