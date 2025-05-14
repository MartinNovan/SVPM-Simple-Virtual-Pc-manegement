-- Tabulka nesmí obsahovat stejné záznamy
-- Vložení 15 zákazníků
DECLARE @i INT = 1;
WHILE @i <= 15
    BEGIN
        DECLARE @CustomerId UNIQUEIDENTIFIER = NEWID();
        DECLARE @CustomerTag NVARCHAR(50) = 'CUST' + RIGHT('000' + CAST(@i AS NVARCHAR(3)), 3);
        DECLARE @FullName NVARCHAR(255) = 'Zákazník ' + CAST(@i AS NVARCHAR(3));
        DECLARE @Email VARCHAR(255) = 'zakaznik' + CAST(@i AS NVARCHAR(3)) + '@example.com';
        DECLARE @Phone VARCHAR(20) = '+420 123 456 ' + RIGHT('00' + CAST(@i AS NVARCHAR(2)), 2);
        DECLARE @Notes NVARCHAR(255) = 'Testovací zákazník č. ' + CAST(@i AS NVARCHAR(3));
        DECLARE @CustomerVerifyHash NVARCHAR(255) = CAST(NEWID() AS NVARCHAR(255));

        EXEC AddCustomer
             @CustomerId = @CustomerId,
             @FullName = @FullName,
             @CustomerTag = @CustomerTag,
             @Email = @Email,
             @Phone = @Phone,
             @Notes = @Notes,
             @VerifyHash = @CustomerVerifyHash;

        -- Pro každého zákazníka vytvoříme 2 virtuální PC
        DECLARE @j INT = 1;
        WHILE @j <= 2
            BEGIN
                DECLARE @VirtualPcId UNIQUEIDENTIFIER = NEWID();
                DECLARE @VirtualPcName NVARCHAR(255) = 'PC-' + @CustomerTag + '-' + CAST(@j AS NVARCHAR(2));
                DECLARE @Service NVARCHAR(255) = CASE WHEN @j = 1 THEN 'Web hosting' ELSE 'Databázový server' END;
                DECLARE @OS NVARCHAR(255) = CASE WHEN @j = 1 THEN 'Windows Server 2019' ELSE 'Ubuntu 20.04' END;
                DECLARE @Cores VARCHAR(20) = CASE WHEN @j = 1 THEN '4' ELSE '8' END;
                DECLARE @Ram VARCHAR(20) = CASE WHEN @j = 1 THEN '8 GB' ELSE '16 GB' END;
                DECLARE @Disk VARCHAR(20) = CASE WHEN @j = 1 THEN '100 GB' ELSE '500 GB' END;
                DECLARE @IpAddress VARCHAR(45) = '192.168.' + CAST(@i AS NVARCHAR(2)) + '.' + CAST(@j AS NVARCHAR(2));
                DECLARE @Fqdn NVARCHAR(255) = @VirtualPcName + '.example.com';
                DECLARE @PcNotes NVARCHAR(255) = 'Testovací virtuální PC pro ' + @CustomerTag;
                DECLARE @PcVerifyHash NVARCHAR(255) = CAST(NEWID() AS NVARCHAR(255));

                EXEC AddVirtualPc
                     @VirtualPcId = @VirtualPcId,
                     @VirtualPcName = @VirtualPcName,
                     @Service = @Service,
                     @OperatingSystem = @OS,
                     @CpuCores = @Cores,
                     @RamSize = @Ram,
                     @DiskSize = @Disk,
                     @Backupping = 1,
                     @Administration = 1,
                     @IpAddress = @IpAddress,
                     @Fqdn = @Fqdn,
                     @Notes = @PcNotes,
                     @VerifyHash = @PcVerifyHash;

                -- Propojení zákazníka s virtuálním PC
                EXEC AddMapping
                     @CustomerId = @CustomerId,
                     @VirtualPcId = @VirtualPcId;

                -- Pro každé virtuální PC vytvoříme 2 účty (1 admin a 1 basic)
                -- Admin účet
                DECLARE @AdminAccountId UNIQUEIDENTIFIER = NEWID();
                DECLARE @AdminUsername NVARCHAR(255) = 'admin_' + @CustomerTag + '_' + CAST(@j AS NVARCHAR(2));
                DECLARE @AdminVerifyHash NVARCHAR(255) = CAST(NEWID() AS NVARCHAR(255));

                EXEC AddAccount
                     @AccountId = @AdminAccountId,
                     @VirtualPcId = @VirtualPcId,
                     @Username = @AdminUsername,
                     @Password = 'Admin123!',
                     @BackupPassword = 'Backup123!',
                     @Admin = 1,
                     @VerifyHash = @AdminVerifyHash;

                -- Basic účet
                DECLARE @UserAccountId UNIQUEIDENTIFIER = NEWID();
                DECLARE @UserUsername NVARCHAR(255) = 'user_' + @CustomerTag + '_' + CAST(@j AS NVARCHAR(2));
                DECLARE @UserVerifyHash NVARCHAR(255) = CAST(NEWID() AS NVARCHAR(255));

                EXEC AddAccount
                     @AccountId = @UserAccountId,
                     @VirtualPcId = @VirtualPcId,
                     @Username = @UserUsername,
                     @Password = 'User123!',
                     @BackupPassword = 'Backup456!',
                     @Admin = 0,
                     @VerifyHash = @UserVerifyHash;

                SET @j = @j + 1;
            END

        SET @i = @i + 1;
    END
