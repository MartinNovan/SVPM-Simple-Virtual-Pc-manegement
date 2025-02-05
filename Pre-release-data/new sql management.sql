-- noinspection SqlDialectInspectionForFile

DROP TABLE IF EXISTS CustomersVirtualPCs;
DROP TABLE IF EXISTS Accounts;
DROP TABLE IF EXISTS VirtualPCs;
DROP TABLE IF EXISTS Customers;

CREATE TABLE Customers (
                           CustomerID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                           FullName NVARCHAR(255) NOT NULL,
                           CustomerTag NVARCHAR(50) NOT NULL UNIQUE,
                           Email VARCHAR(255) NULL,
                           Phone VARCHAR(20) NULL,
                           CustomerNotes NVARCHAR(255) NULL
);

CREATE TABLE VirtualPCs (
                            VirtualPcID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                            VirtualPcName NVARCHAR(255) NOT NULL UNIQUE,
                            ServiceName NVARCHAR(255) NULL,
                            OperatingSystem NVARCHAR(255) NULL,
                            CPU_Cores INT NULL,
                            RAM_Size_GB INT NULL,
                            Disk_Size_GB INT NULL,
                            Backupping BIT NOT NULL,
                            Administration BIT NOT NULL,
                            IP_Address VARCHAR(45) NULL,
                            FQDN NVARCHAR(255) NULL,
                            VirtualPcNotes NVARCHAR(255) NULL
);

CREATE TABLE CustomersVirtualPCs (
                                     MappingID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                                     CustomerID UNIQUEIDENTIFIER NOT NULL,
                                     VirtualPcID UNIQUEIDENTIFIER NOT NULL,
                                     FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID),
                                     FOREIGN KEY (VirtualPcID) REFERENCES VirtualPCs(VirtualPcID)
);

CREATE TABLE Accounts (
                          AccountID UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                          VirtualPcID UNIQUEIDENTIFIER NOT NULL,
                          Username NVARCHAR(255) NOT NULL,
                          Password NVARCHAR(255) NULL,
                          IsAdmin BIT NOT NULL,
                          LastUpdated DATETIME NOT NULL DEFAULT GETDATE(),
                          OriginalPassword NVARCHAR(255) NULL,
                          FOREIGN KEY (VirtualPcID) REFERENCES VirtualPCs(VirtualPcID)
);
