-- Odstran�n� p�vodn�ch tabulek, pokud existuj�
DROP TABLE IF EXISTS Accounts;
DROP TABLE IF EXISTS Servers;
DROP TABLE IF EXISTS Customers;

-- Vytvo�en� tabulky Customers
CREATE TABLE Customers (
    CustomerID INT PRIMARY KEY IDENTITY(1,1),
    FullName VARCHAR(255) NOT NULL, -- Povinn� jm�no
    Email VARCHAR(255) NOT NULL UNIQUE,
    Phone VARCHAR(20) NULL
);

-- Vytvo�en� tabulky Servers s ciz�m kl��em na Customers
CREATE TABLE Servers (
    ServerID INT PRIMARY KEY IDENTITY(1,1),
    CustomerID INT NOT NULL, -- Propojeno s tabulkou Customers
    CPU_Cores INT NOT NULL,
    RAM_Size_GB INT NOT NULL,
    Disk_Size_GB INT NOT NULL,
    VirtualPcName VARCHAR(255) NULL,
    FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID) -- Ciz� kl��
);

-- Vytvo�en� tabulky Accounts s ciz�m kl��em na Servers
CREATE TABLE Accounts (
    AccountID INT PRIMARY KEY IDENTITY(1,1),
    ServerID INT NOT NULL, -- Propojeno s tabulkou Servers
    Username VARCHAR(255) NOT NULL,
    Password VARCHAR(255) NOT NULL, -- Hesla mohou b�t �ifrov�na
    IsAdmin BIT NOT NULL,
    LastUpdated DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (ServerID) REFERENCES Servers(ServerID) -- Ciz� kl��
);
