-- Odstranìní pùvodních tabulek, pokud existují
DROP TABLE IF EXISTS Accounts;
DROP TABLE IF EXISTS VirtualPCs;
DROP TABLE IF EXISTS Customers;

-- Vytvoøení tabulky Customers s GUID jako primárním klíèem
CREATE TABLE Customers (
    CustomerID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(), -- GUID generovaný automaticky
    FullName VARCHAR(255) NOT NULL, -- Povinné jméno
    Email VARCHAR(255) NOT NULL UNIQUE, -- Unikátní e-mail
    Phone VARCHAR(20) NULL
);

-- Vytvoøení tabulky VirtualPCs s GUID jako cizím klíèem na Customers
CREATE TABLE VirtualPCs (
    VirtualPcID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CustomerID UNIQUEIDENTIFIER NOT NULL, -- GUID z tabulky Customers
    CPU_Cores INT NOT NULL,
    RAM_Size_GB INT NOT NULL,
    Disk_Size_GB INT NOT NULL,
    VirtualPcName VARCHAR(255) NULL,
    FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID)
);

-- Vytvoøení tabulky Accounts s GUID jako cizím klíèem na VirtualPCs
CREATE TABLE Accounts (
    AccountID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    VirtualPcID UNIQUEIDENTIFIER NOT NULL, -- GUID z tabulky VirtualPCs
    Username VARCHAR(255) NOT NULL,
    Password VARCHAR(255) NOT NULL, -- Hesla mohou být šifrována
    IsAdmin BIT NOT NULL,
    LastUpdated DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (VirtualPcID) REFERENCES VirtualPCs(VirtualPcID)
);

-- Vložení testovacích dat do tabulky Customers
INSERT INTO Customers (FullName, Email, Phone)
VALUES 
('Jan Novák', 'jan.novak@email.com', '123456789'),
('Petr Svoboda', 'petr.svoboda@email.com', '987654321'),
('Eva Malá', 'eva.mala@email.com', '456123789');

-- Vložení testovacích dat do tabulky VirtualPCs
-- Pro Jana Nováka
INSERT INTO VirtualPCs (CustomerID, CPU_Cores, RAM_Size_GB, Disk_Size_GB, VirtualPcName)
SELECT CustomerID, 4, 16, 500, 'VirtualniPcNovak' 
FROM Customers WHERE FullName = 'Jan Novák';

-- Pro Petra Svobodu
INSERT INTO VirtualPCs (CustomerID, CPU_Cores, RAM_Size_GB, Disk_Size_GB, VirtualPcName)
SELECT CustomerID, 8, 32, 1000, 'VirtualniPcSvoboda' 
FROM Customers WHERE FullName = 'Petr Svoboda';

-- Pro Evu Malou
INSERT INTO VirtualPCs (CustomerID, CPU_Cores, RAM_Size_GB, Disk_Size_GB, VirtualPcName)
SELECT CustomerID, 2, 8, 250, 'VirtualniPcMala' 
FROM Customers WHERE FullName = 'Eva Malá';

-- Vložení testovacích dat do tabulky Accounts
-- Admin a obyèejné úèty pro Jana Nováka
INSERT INTO Accounts (VirtualPcID, Username, Password, IsAdmin)
SELECT VirtualPcID, 'novakadmin', 'hesloAdmin123', 1 
FROM VirtualPCs WHERE VirtualPcName = 'VirtualniPcNovak';

INSERT INTO Accounts (VirtualPcID, Username, Password, IsAdmin)
SELECT VirtualPcID, 'jannovak', 'hesloUzivatel123', 0 
FROM VirtualPCs WHERE VirtualPcName = 'VirtualniPcNovak';

-- Admin a obyèejné úèty pro Petra Svobodu
INSERT INTO Accounts (VirtualPcID, Username, Password, IsAdmin)
SELECT VirtualPcID, 'svobodaadmin', 'hesloAdmin456', 1 
FROM VirtualPCs WHERE VirtualPcName = 'VirtualniPcSvoboda';

INSERT INTO Accounts (VirtualPcID, Username, Password, IsAdmin)
SELECT VirtualPcID, 'petrsvoboda', 'hesloUzivatel456', 0 
FROM VirtualPCs WHERE VirtualPcName = 'VirtualniPcSvoboda';

-- Admin a obyèejné úèty pro Evu Malou
INSERT INTO Accounts (VirtualPcID, Username, Password, IsAdmin)
SELECT VirtualPcID, 'malaadmin', 'hesloAdmin789', 1 
FROM VirtualPCs WHERE VirtualPcName = 'VirtualniPcMala';

INSERT INTO Accounts (VirtualPcID, Username, Password, IsAdmin)
SELECT VirtualPcID, 'evamala', 'hesloUzivatel789', 0 
FROM VirtualPCs WHERE VirtualPcName = 'VirtualniPcMala';
