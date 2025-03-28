DROP PROCEDURE IF EXISTS GetCustomers;
DROP PROCEDURE IF EXISTS GetSpecificCustomer;
DROP PROCEDURE IF EXISTS AddCustomer;
DROP PROCEDURE IF EXISTS DeleteCustomer;
DROP PROCEDURE IF EXISTS UpdateCustomer;
DROP PROCEDURE IF EXISTS CheckForCustomerConflict;

DROP PROCEDURE IF EXISTS GetVirtualPCs;
DROP PROCEDURE IF EXISTS GetSpecificVirtualPC;
DROP PROCEDURE IF EXISTS AddVirtualPc;
DROP PROCEDURE IF EXISTS DeleteVirtualPc;
DROP PROCEDURE IF EXISTS UpdateVirtualPc;
DROP PROCEDURE IF EXISTS CheckForVirtualPcConflict;

DROP PROCEDURE IF EXISTS GetMappings;
DROP PROCEDURE IF EXISTS AddMapping;
DROP PROCEDURE IF EXISTS DeleteMapping;

DROP PROCEDURE IF EXISTS GetAccounts;
DROP PROCEDURE IF EXISTS GetSpecificAccount;
DROP PROCEDURE IF EXISTS AddAccount;
DROP PROCEDURE IF EXISTS DeleteAccount;
DROP PROCEDURE IF EXISTS UpdateAccount;
DROP PROCEDURE IF EXISTS CheckForAccountConflict;

DROP TABLE IF EXISTS CustomersVirtualPCs;
DROP TABLE IF EXISTS Accounts;
DROP TABLE IF EXISTS VirtualPCs;
DROP TABLE IF EXISTS Customers;

CREATE TABLE Customers (
                           CustomerId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                           FullName NVARCHAR(255) NOT NULL,
                           CustomerTag NVARCHAR(50) NOT NULL UNIQUE,
                           Email VARCHAR(255) NULL,
                           Phone VARCHAR(20) NULL,
                           Notes NVARCHAR(255) NULL,
                           Updated DATETIME NOT NULL DEFAULT GETDATE(),
                           VerifyHash NVARCHAR(255) NOT NULL
);

CREATE TABLE VirtualPCs (
                            VirtualPcId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                            VirtualPcName NVARCHAR(255) NOT NULL UNIQUE,
                            Service NVARCHAR(255) NULL,
                            OperatingSystem NVARCHAR(255) NULL,
                            CpuCores VARCHAR(20) NULL,
                            RamSize VARCHAR(20) NULL,
                            DiskSize VARCHAR(20) NULL,
                            Backupping BIT NOT NULL,
                            Administration BIT NOT NULL,
                            IpAddress VARCHAR(45) NULL,
                            Fqdn NVARCHAR(255) NULL,
                            Notes NVARCHAR(255) NULL,
                            Updated DATETIME NOT NULL DEFAULT GETDATE(),
                            VerifyHash NVARCHAR(255) NOT NULL
);

CREATE TABLE CustomersVirtualPCs (
                                     MappingId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                                     CustomerId UNIQUEIDENTIFIER NOT NULL,
                                     VirtualPcId UNIQUEIDENTIFIER NOT NULL,
                                     FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID) ON DELETE CASCADE,
                                     FOREIGN KEY (VirtualPcID) REFERENCES VirtualPCs(VirtualPcID) ON DELETE CASCADE
);

CREATE INDEX IX_CustomersVirtualPCs_CustomerID ON CustomersVirtualPCs (CustomerID);
CREATE INDEX IX_CustomersVirtualPCs_VirtualPcID ON CustomersVirtualPCs (VirtualPcID);

CREATE TABLE Accounts (
                          AccountId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
                          VirtualPcId UNIQUEIDENTIFIER NOT NULL,
                          Username NVARCHAR(255) NOT NULL,
                          Password NVARCHAR(255) NULL,
                          BackupPassword NVARCHAR(255) NULL,
                          Admin BIT NOT NULL,
                          Updated DATETIME NOT NULL DEFAULT GETDATE(),
                          VerifyHash NVARCHAR(255) NOT NULL,
                          FOREIGN KEY (VirtualPcID) REFERENCES VirtualPCs(VirtualPcID) ON DELETE CASCADE
);

CREATE INDEX IX_Accounts_VirtualPcID ON Accounts (VirtualPcID);

GO

CREATE PROCEDURE GetCustomers
AS
BEGIN
    SELECT * FROM Customers;
END;

GO

CREATE PROCEDURE GetSpecificCustomer
@CustomerId UNIQUEIDENTIFIER
AS
BEGIN
    IF @CustomerId IS NOT NULL
        BEGIN
            SELECT * FROM Customers WHERE CustomerId = @CustomerId;
        END
END;

GO

CREATE PROCEDURE AddCustomer
    @CustomerId UNIQUEIDENTIFIER,
    @FullName NVARCHAR(255),
    @CustomerTag NVARCHAR(50),
    @Email VARCHAR(255) = NULL,
    @Phone VARCHAR(20) = NULL,
    @Notes NVARCHAR(255) = NULL,
    @VerifyHash NVARCHAR(255)
AS
BEGIN
    IF EXISTS (SELECT 1 FROM Customers WHERE CustomerTag = @CustomerTag)
        BEGIN
            RAISERROR('Customer with this tag is already in database.', 16, 1);
        END
    BEGIN
        INSERT INTO Customers (CustomerId, FullName, CustomerTag, Email, Phone, Notes, VerifyHash, Updated)
        VALUES (@CustomerId, @FullName, @CustomerTag, @Email, @Phone, @Notes, @VerifyHash, GETDATE());
    END
END;

GO

CREATE PROCEDURE DeleteCustomer
@CustomerId UNIQUEIDENTIFIER
AS
BEGIN
    BEGIN TRANSACTION;

    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM Customers WHERE CustomerId = @CustomerId)
            BEGIN
                RAISERROR('Customer not found', 16, 1);
            END

        DELETE FROM CustomersVirtualPCs WHERE CustomerId = @CustomerId;
        DELETE FROM Customers WHERE CustomerId = @CustomerId;

        IF EXISTS (SELECT 1 FROM CustomersVirtualPCs WHERE CustomerId = @CustomerId)
            BEGIN
                RAISERROR('Some mappings weren''t properly deleted!', 16, 1);
            END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;


GO

CREATE PROCEDURE UpdateCustomer
    @CustomerId UNIQUEIDENTIFIER,
    @FullName NVARCHAR(255),
    @CustomerTag NVARCHAR(50),
    @Email VARCHAR(255),
    @Phone VARCHAR(20),
    @Notes NVARCHAR(255),
    @VerifyHash NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM Customers WHERE CustomerId = @CustomerId)
            BEGIN
                RAISERROR('Customer not found.', 16, 1);
            END

        IF EXISTS (SELECT 1 FROM Customers WHERE CustomerTag = @CustomerTag AND CustomerId != @CustomerId)
            BEGIN
                RAISERROR('Customer with this tag is already in database.', 16, 1);
            END

        UPDATE Customers
        SET
            FullName = @FullName,
            CustomerTag = @CustomerTag,
            Email = @Email,
            Phone = @Phone,
            Notes = @Notes,
            VerifyHash = @VerifyHash,
            Updated = GETDATE()
        WHERE CustomerId = @CustomerId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;

GO

CREATE PROCEDURE CheckForCustomerConflict
    @CustomerId UNIQUEIDENTIFIER,
    @OriginalVerifyHash NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Customers WHERE CustomerId = @CustomerId)
        BEGIN
            RAISERROR('Customer not found in the database.', 16, 1);
        END

    DECLARE @CurrentVerifyHash NVARCHAR(255);
    SELECT @CurrentVerifyHash = VerifyHash FROM Customers WHERE CustomerId = @CustomerId;

    IF @CurrentVerifyHash != @OriginalVerifyHash
        BEGIN
            SELECT * FROM Customers WHERE CustomerId = @CustomerId;
        END
END;

GO

CREATE PROCEDURE GetVirtualPCs
AS
BEGIN
    SELECT * FROM VirtualPCs;
END;

GO

CREATE PROCEDURE GetSpecificVirtualPC
@VirtualPcId UNIQUEIDENTIFIER
AS
BEGIN
    IF @VirtualPcId IS NOT NULL
        BEGIN
            SELECT * FROM VirtualPCs WHERE VirtualPcId = @VirtualPcId;
        END
END;

GO

CREATE PROCEDURE AddVirtualPc
    @VirtualPcId UNIQUEIDENTIFIER,
    @VirtualPcName NVARCHAR(255),
    @Service NVARCHAR(255) = NULL,
    @OperatingSystem VARCHAR(255) = NULL,
    @CpuCores VARCHAR(20) = NULL,
    @RamSize VARCHAR(20) = NULL,
    @DiskSize VARCHAR(20) = NULL,
    @Backupping BIT,
    @Administration BIT,
    @IpAddress VARCHAR(45) = NULL,
    @Fqdn NVARCHAR(255) = NULL,
    @Notes NVARCHAR(255) = NULL,
    @VerifyHash NVARCHAR(255)
AS
BEGIN
    IF EXISTS (SELECT 1 FROM VirtualPCs WHERE VirtualPcName = @VirtualPcName)
        BEGIN
            RAISERROR('VirtualPC with this name is already in database.', 16, 1);
        END
    BEGIN
        INSERT INTO VirtualPCs (VirtualPcId, VirtualPcName, Service, OperatingSystem, CpuCores, RamSize, DiskSize, Backupping, Administration, IpAddress, Fqdn, Notes, Updated, VerifyHash)
        VALUES (@VirtualPcId, @VirtualPcName, @Service, @OperatingSystem, @CpuCores, @RamSize, @DiskSize, @Backupping, @Administration, @IpAddress, @Fqdn, @Notes, GETDATE(), @VerifyHash);
    END
END;

GO

CREATE PROCEDURE DeleteVirtualPc
@VirtualPcId UNIQUEIDENTIFIER
AS
BEGIN
    BEGIN TRANSACTION;

    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM VirtualPCs WHERE VirtualPcId = @VirtualPcId)
            BEGIN
                RAISERROR('Virtual PC not found', 16, 1);
            END

        DELETE FROM CustomersVirtualPCs WHERE VirtualPcId = @VirtualPcId;
        DELETE FROM Accounts WHERE VirtualPcId = @VirtualPcId;
        DELETE FROM VirtualPCs WHERE VirtualPcId = @VirtualPcId;

        IF EXISTS (SELECT 1 FROM CustomersVirtualPCs WHERE VirtualPcId = @VirtualPcId)
            BEGIN
                RAISERROR('Some mappings weren''t properly deleted!', 16, 1);
            END
        IF EXISTS (SELECT 1 FROM Accounts WHERE VirtualPcId = @VirtualPcId)
            BEGIN
                RAISERROR('Some accounts weren''t properly deleted!', 16, 1);
            END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;

GO

CREATE PROCEDURE UpdateVirtualPc
    @VirtualPcId UNIQUEIDENTIFIER,
    @VirtualPcName NVARCHAR(255),
    @Service NVARCHAR(255) = NULL,
    @OperatingSystem VARCHAR(255) = NULL,
    @CpuCores VARCHAR(20) = NULL,
    @RamSize VARCHAR(20) = NULL,
    @DiskSize VARCHAR(20) = NULL,
    @Backupping BIT,
    @Administration BIT,
    @IpAddress VARCHAR(45) = NULL,
    @Fqdn NVARCHAR(255) = NULL,
    @Notes NVARCHAR(255) = NULL,
    @VerifyHash NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM VirtualPCs WHERE VirtualPcId = @VirtualPcId)
            BEGIN
                RAISERROR('Virtual PC not found', 16, 1);
            END

        IF EXISTS (SELECT 1 FROM VirtualPCs WHERE VirtualPcName = @VirtualPcName AND VirtualPcId != @VirtualPcId)
            BEGIN
                RAISERROR('VirtualPC with this name is already in database.', 16, 1);
            END

        UPDATE VirtualPCs
        SET VirtualPcName = @VirtualPcName,
            Service = @Service,
            OperatingSystem = @OperatingSystem,
            CpuCores = @CpuCores,
            RamSize = @RamSize,
            DiskSize = @DiskSize,
            Backupping = @Backupping,
            Administration = @Administration,
            IpAddress = @IpAddress,
            Fqdn = @Fqdn,
            Notes = @Notes,
            Updated = GETDATE(),
            VerifyHash = @VerifyHash
        WHERE VirtualPcId = @VirtualPcId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;

GO

CREATE PROCEDURE CheckForVirtualPcConflict
    @VirtualPcId UNIQUEIDENTIFIER,
    @OriginalVerifyHash NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM VirtualPCs WHERE VirtualPcId = @VirtualPcId)
        BEGIN
            RAISERROR('VirtualPc not found in the database.', 16, 1);
        END

    DECLARE @CurrentVerifyHash NVARCHAR(255);
    SELECT @CurrentVerifyHash = VerifyHash FROM VirtualPCs WHERE VirtualPcId = @VirtualPcId;

    IF @CurrentVerifyHash != @OriginalVerifyHash
        BEGIN
            SELECT * FROM VirtualPCs WHERE VirtualPcId = @VirtualPcId;
        END
END;

GO

CREATE PROCEDURE GetMappings
AS
BEGIN
    SELECT * FROM CustomersVirtualPCs;
END;

GO

CREATE PROCEDURE AddMapping
    @CustomerId UNIQUEIDENTIFIER,
    @VirtualPcId UNIQUEIDENTIFIER
AS
BEGIN
    INSERT INTO CustomersVirtualPCs (CustomerId, VirtualPcID) VALUES (@CustomerId, @VirtualPcID)
END;

GO

CREATE PROCEDURE DeleteMapping
    @MappingId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    IF EXISTS (SELECT 1 FROM CustomersVirtualPCs WHERE MappingId = @MappingId)
        BEGIN
            DELETE FROM CustomersVirtualPcs WHERE MappingId = @MappingId;
            COMMIT TRANSACTION;
        END
    ELSE
        BEGIN
            ROLLBACK TRANSACTION;
        END
END;

GO

CREATE PROCEDURE GetAccounts
AS
BEGIN
    SELECT * FROM Accounts;
END;

GO

CREATE PROCEDURE GetSpecificAccount
@AccountId UNIQUEIDENTIFIER
AS
BEGIN
    IF @AccountId IS NOT NULL
        BEGIN
            SELECT * FROM Accounts WHERE AccountId = @AccountId;
        END
END;

GO

CREATE PROCEDURE AddAccount
    @AccountId UNIQUEIDENTIFIER,
    @VirtualPcId UNIQUEIDENTIFIER,
    @Username NVARCHAR(255),
    @Password NVARCHAR(255),
    @BackupPassword NVARCHAR(255),
    @Admin BIT,
    @VerifyHash NVARCHAR(255)
AS
BEGIN
    INSERT INTO Accounts (AccountId, VirtualPcId, Username, Password, BackupPassword, Admin, VerifyHash, Updated)
    VALUES (@AccountId, @VirtualPcId, @Username, @Password, @BackupPassword, @Admin, @VerifyHash, GETDATE());
END;

GO

CREATE PROCEDURE DeleteAccount
@AccountId UNIQUEIDENTIFIER
AS
BEGIN
    BEGIN TRANSACTION;

    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM Accounts WHERE AccountId = @AccountId)
            BEGIN
                RAISERROR('Account not found', 16, 1);
            END

        DELETE FROM Accounts WHERE AccountId = @AccountId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;

GO

CREATE PROCEDURE UpdateAccount
    @AccountId UNIQUEIDENTIFIER,
    @VirtualPcId UNIQUEIDENTIFIER,
    @Username NVARCHAR(255),
    @Password NVARCHAR(255),
    @BackupPassword NVARCHAR(255),
    @Admin BIT,
    @VerifyHash NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;

    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM Accounts WHERE AccountId = @AccountId)
            BEGIN
                RAISERROR('Account not found', 16, 1);
            END

        UPDATE Accounts
        SET VirtualPcId = @VirtualPcId,
            Username = @Username,
            Password = @Password,
            BackupPassword = @BackupPassword,
            Admin = @Admin,
            VerifyHash = @VerifyHash,
            Updated = GETDATE()
        WHERE AccountId = @AccountId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;

GO

CREATE PROCEDURE CheckForAccountConflict
    @AccountId UNIQUEIDENTIFIER,
    @OriginalVerifyHash NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Accounts WHERE AccountId = @AccountId)
        BEGIN
            RAISERROR('Account not found in the database.', 16, 1);
        END

    DECLARE @CurrentVerifyHash NVARCHAR(255);
    SELECT @CurrentVerifyHash = VerifyHash FROM Accounts WHERE AccountId = @AccountId;

    IF @CurrentVerifyHash != @OriginalVerifyHash
        BEGIN
            SELECT * FROM Accounts WHERE AccountId = @AccountId;
        END
END;