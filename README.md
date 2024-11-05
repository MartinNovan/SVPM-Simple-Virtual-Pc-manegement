# 🌟 SVPM - Starlit Virtual PC Management 🌟

## 🖥️ Přehled

**SVPM** je aplikace určená k správě virtuálních serverů a zákaznických účtů. Poskytuje intuitivní rozhraní pro administraci serverů, úpravu přihlašovacích údajů a zobrazení hardwarových specifikací jednotlivých serverů. Aplikace je navržena pro snadné použití v podnikových prostředích.

---

## 🚀 Hlavní funkce (Některé funkce v beta verzi chybí)

- **🔑 Správa zákazníků:** Možnost přidávat, upravovat a mazat zákazníky.
- **🖥️ Správa serverů:** Seznam připojených serverů, jejich hardwarové specifikace a přihlašovací údaje.
- **🔐 Účty na serverech:** Možnost spravovat uživatelské účty přiřazené k jednotlivým serverům.
- **🔗 Připojení k SQL databázi:** Připojení k databázi pro načítání a ukládání dat.
- **🔄 Synchronizace dat:** Automatická synchronizace přihlašovacích údajů s virtuálními servery.

---

## 🛠️ Instalace a spuštění

### Požadavky:

- **.NET 8.0 SDK** nebo novější
- **Windows 10 nebo vyšší** (architektura x64)
- **SQL Server** pro správu dat

### Postup instalace:

1. **Stažení aplikace:** Stáhni si poslední verzi aplikace [zde](https://drive.google.com/drive/folders/1m-GEgfXPfE_agB2caelzvXPlPX92VYDk?usp=sharing).
2. **Rozbalení:** Rozbal stažený soubor do zvoleného adresáře.
3. **Spuštění aplikace:** Dvojklikem na soubor `SVPM-Setup.exe` či `SVPM-Setup.msi` spusť instalaci.
   - **Také**: Můžete použít `SVPM (portable).zip` a extrahováním ze zipu aplikaci spustíte.

---

## ⚙️ Nastavení SQL připojení (Některé funkce v beta verzi 1.0 chybí)

Při prvním spuštění aplikace je nutné nastavit připojení k SQL databázi:

1. **Vyplň údaje**:
   - **Server**: Název nebo IP adresa serveru.
   - **Databáze**: Název databáze.
   - **Přihlašovací údaje**: Uživatelské jméno a heslo k databázi či použítí windows auth.
   
2. **Uložení připojení**: Po vyplnění klikni na **Zapamatovat připojení** a připojení se uloží do šablony.

---

## 📊 Hlavní obrazovka

Po úspěšném přihlášení se zobrazí hlavní obrazovka aplikace, která se skládá ze tří záložek:

1. **👥 Zákazníci**:
   - Zobrazení seznamu zákazníků.
   - Možnost upravovat osobních údajů a kontaktní informace.
   
2. **🌐 Servery**:
   - Výpis přiřazených serverů s podrobnými informacemi o CPU, RAM a dalších specifikacích.
   - Možnost upravit účty na jednotlivých serverech.

2. **🪪 Účty**:
   - Výpis jednotlivých účtů ze všech serverů s informacemi jako je heslo, pokud je účet administátorský a datum i čas poslední změny.
   - Možnost upravit data.

---

## 📚 Databázová struktura

Aplikace využívá tři hlavní tabulky pro správu dat:

1. **Customers**:
   - `CustomerID`, `FullName`, `Email`, `Phone`
   
2. **Servers**:
   - `ServerID`, `CustomerID`, `CPU_Cores`, `RAM_Size_GB`, `Description`
   
3. **Accounts**:
   - `AccountID`, `ServerID`, `Username`, `Password`, `IsAdmin`, `LastUpdated`

---

## 💡 Tipy pro používání (Některé funkce v beta verzi chybí)

- Pokud připojení k databázi selže, zkontroluj správnost údajů a dostupnost serveru.
- Změny v údajích uživatelů a serverů jsou automaticky synchronizovány.

---

## 🛠️ Údržba a podpora

Pokud narazíte na chyby nebo potřebujete pomoc, neváhejte se obrátit na [martinnovan01@gmail.com](mailto:martinnovan01@gmail.com).

Nebo vytvořte požadavek zde na gitu v [issues.](https://git.starlit.cz/Praktikanti/SVPM-Starlit-Virtual-Pc-manegement/issues)

---

### 📢 Kontaktní informace
- **Email:** [martinnovan01@gmail.com](mailto:martinnovan01@gmail.com)
- **Telefon:** +420 703 397 132

---

### 🌟 Poděkování

Děkujeme, že používáte **SVPM**! Vážíme si vaší podpory a doufáme, že vám aplikace usnadní správu vašich serverů.
