-- Odstran�n� p�vodn�ch tabulek, pokud existuj�
DROP TABLE IF EXISTS Accounts;
DROP TABLE IF EXISTS VirtualPCs;
DROP TABLE IF EXISTS Customers;

-- Vytvo�en� tabulky Customers s GUID jako prim�rn�m kl��em
CREATE TABLE Customers (
    CustomerID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(), -- GUID generovan� automaticky
    FullName VARCHAR(255) NOT NULL, -- Povinn� jm�no
    Email VARCHAR(255) NOT NULL UNIQUE, -- Unik�tn� e-mail
    Phone VARCHAR(20) NULL
);

-- Vytvo�en� tabulky VirtualPCs s GUID jako ciz�m kl��em na Customers
CREATE TABLE VirtualPCs (
    VirtualPcID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CustomerID UNIQUEIDENTIFIER NOT NULL, -- GUID z tabulky Customers
    CPU_Cores INT NOT NULL,
    RAM_Size_GB INT NOT NULL,
    Disk_Size_GB INT NOT NULL,
    VirtualPcName VARCHAR(255) NULL,
    FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID)
);

-- Vytvo�en� tabulky Accounts s GUID jako ciz�m kl��em na VirtualPCs
CREATE TABLE Accounts (
    AccountID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    VirtualPcID UNIQUEIDENTIFIER NOT NULL, -- GUID z tabulky VirtualPCs
    Username VARCHAR(255) NOT NULL,
    Password VARCHAR(255) NOT NULL, -- Hesla mohou b�t �ifrov�na
    IsAdmin BIT NOT NULL,
    LastUpdated DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (VirtualPcID) REFERENCES VirtualPCs(VirtualPcID)
);

-- Vlo�en� testovac�ch dat do tabulky Customers
INSERT INTO Customers (FullName, Email, Phone)
VALUES 
('Jan Nov�k', 'jan.novak@email.com', '123456789'),
('Petr Svoboda', 'petr.svoboda@email.com', '987654321'),
('Eva Mal�', 'eva.mala@email.com', '456123789');

-- Vlo�en� testovac�ch dat do tabulky VirtualPCs
-- Pro Jana Nov�ka
INSERT INTO VirtualPCs (CustomerID, CPU_Cores, RAM_Size_GB, Disk_Size_GB, VirtualPcName)
SELECT CustomerID, 4, 16, 500, 'VirtualniPcNovak' 
FROM Customers WHERE FullName = 'Jan Nov�k';

-- Pro Petra Svobodu
INSERT INTO VirtualPCs (CustomerID, CPU_Cores, RAM_Size_GB, Disk_Size_GB, VirtualPcName)
SELECT CustomerID, 8, 32, 1000, 'VirtualniPcSvoboda' 
FROM Customers WHERE FullName = 'Petr Svoboda';

-- Pro Evu Malou
INSERT INTO VirtualPCs (CustomerID, CPU_Cores, RAM_Size_GB, Disk_Size_GB, VirtualPcName)
SELECT CustomerID, 2, 8, 250, 'VirtualniPcMala' 
FROM Customers WHERE FullName = 'Eva Mal�';

-- Vlo�en� testovac�ch dat do tabulky Accounts
-- Admin a oby�ejn� ��ty pro Jana Nov�ka
INSERT INTO Accounts (VirtualPcID, Username, Password, IsAdmin)
SELECT VirtualPcID, 'novakadmin', 'hesloAdmin123', 1 
FROM VirtualPCs WHERE VirtualPcName = 'VirtualniPcNovak';

INSERT INTO Accounts (VirtualPcID, Username, Password, IsAdmin)
SELECT VirtualPcID, 'jannovak', 'hesloUzivatel123', 0 
FROM VirtualPCs WHERE VirtualPcName = 'VirtualniPcNovak';

-- Admin a oby�ejn� ��ty pro Petra Svobodu
INSERT INTO Accounts (VirtualPcID, Username, Password, IsAdmin)
SELECT VirtualPcID, 'svobodaadmin', 'hesloAdmin456', 1 
FROM VirtualPCs WHERE VirtualPcName = 'VirtualniPcSvoboda';

INSERT INTO Accounts (VirtualPcID, Username, Password, IsAdmin)
SELECT VirtualPcID, 'petrsvoboda', 'hesloUzivatel456', 0 
FROM VirtualPCs WHERE VirtualPcName = 'VirtualniPcSvoboda';

-- Admin a oby�ejn� ��ty pro Evu Malou
INSERT INTO Accounts (VirtualPcID, Username, Password, IsAdmin)
SELECT VirtualPcID, 'malaadmin', 'hesloAdmin789', 1 
FROM VirtualPCs WHERE VirtualPcName = 'VirtualniPcMala';

INSERT INTO Accounts (VirtualPcID, Username, Password, IsAdmin)
SELECT VirtualPcID, 'evamala', 'hesloUzivatel789', 0 
FROM VirtualPCs WHERE VirtualPcName = 'VirtualniPcMala';
