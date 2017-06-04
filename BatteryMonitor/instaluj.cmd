@echo off

echo instalacja programu do monitorowania stanu baterii
echo zanim przejdziesz dalej, otworz plik BatteryMonitor.exe.cofig i uzupełnij wymagane dane
echo 1) klucz "warningEmailTo" - wpisz listę adresów email które mają otrzymywać maila z powiadomieniem. adresy oddziel przecinkiem
echo 2) klucz userName="twoj@email.gmail" - wpisz poprawny adres email z gmaila. mozesz założyć jakieś dowolne konto na googlu.
echo 3) klucz password="hasło" - wpisz swoje hasło do konta w google. 
echo 4) wejdz na tę stronę:  https://www.google.com/settings/security/lesssecureapps (po zalogowaniu się na powyższe konto) i ustaw opcję "zezwalaj na mniej bezpieczne aplikacje"


echo instalacja serwisu - wymaga uprawnień administratora - uruchom tę komendę jako administrator. jesli nie masz uprawnień, dostaniesz błąd na konsoli...
pause

BatteryMonitor.exe install
echo Jesli nie pojawił się błąd - serwis się zainstalował

echo Probuje uruchomić serwis
BatteryMonitor.exe start

echo all done!
pause