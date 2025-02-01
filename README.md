WashDelivery - System obsługi usług prania na wynos

Autorzy projektu:

 Jakub Chimiak
Maksymilian Jarosz


Opis działania projektu:

„WashDelivery” to aplikacja webowa przeznaczona do zarządzania usługą prania na wynos.
Aplikacja pozwala na bieżące śledzenie statusu zamówień, dzięki czemu użytkownicy mogą monitorować postęp realizacji swoich usług. 
„WashDelivery” usprawnia procesy zarządzania pralniami, zapewnia przejrzystość operacji oraz ułatwia obsługę zamówień. Jest responsywna i dostosowana do pracy na urządzeniach mobilnych i desktopowych.
Specyfikacja Technologii:

Backend:
C# .NET 8.0
ASP.NET Core MVC
SignalR (Real-time notifications) - działa tylko dla w widoku zamówień pralni, nowe zamówienia automatycznie wskakują na stronę
Baza danych:
SQLite
Frontend: 
HTML
Tailwind
JavaScript
SignalR (Real-time notifications)

Konta użytkowników: 

Admin: 
admin@test.com
Test123!


Courier: 
courier@test.com
Test123!

Manager
manager@test.com
Test123!

Worker:
worker@test.com
Test123!

Customer:
customer@test.com
Test123!


Instrukcja pierwszego uruchomienia:

baza jest już w repozytorium, wystarczy uruchomić serwer poleceniem dotnet watch w folderze WashDelivery/src/WashDelivery.Web

Flow zamówienia
Użytkownik tworzy zamówienie
Manager pralni musi je zaakceptować, żeby pojawiło się w ogóle im to zamówienie muszą być w promieniu 10km, aktualnie jest jedna pralnia w Warszawie a konto customera ma już dodany warszawski adres
Zaakceptowane zamówienie pokazuję się kurierowi i może je wziąć na siebie, kurier jedzie do klienta, potwierdza odbiór
Pralnia potwierdza otrzymanie prania od kuriera, zamówienie jest w realizacji
Pralnia zaznacza kiedy pranie jest gotowe do odbioru
Kurierowi pojawia się nowe zamówienie
Kurier dostarcza pranie do klienta i potwierdza dostawę





