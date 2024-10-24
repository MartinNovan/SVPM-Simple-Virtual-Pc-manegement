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
