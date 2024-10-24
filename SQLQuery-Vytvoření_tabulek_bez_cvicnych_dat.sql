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
