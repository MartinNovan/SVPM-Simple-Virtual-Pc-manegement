INSERT INTO Customers (FullName, CustomerTag, Email, Phone, CustomerNotes)
VALUES
('Jan Novák', 'JN01', 'jan.novak@example.com', '+420123456789', 'Důležitý zákazník'),
('Petr Svoboda', 'PS02', 'petr.svoboda@example.com', '+420987654321', 'Dlouhodobý klient'),
('Marie Dvořáková', 'MD03', 'marie.dvorakova@example.com', '+420112233445', 'Potenciálně nový zákazník'),
('Tomáš Novotný', 'TN04', 'tomas.novotny@example.com', NULL, 'Hledá nové řešení'),
('Eva Veselá', 'EV05', 'eva.vesela@example.com', '+420998877665', NULL),
('Lukáš Zelený', 'LZ06', 'lukas.zeleny@example.com', '+420556677889', 'VIP zákazník'),
('Anna Černá', 'AC07', 'anna.cerna@example.com', NULL, 'Zvažuje upgrade'),
('Marek Modrý', 'MM08', 'marek.modry@example.com', '+420667788990', NULL),
('Klára Bílá', 'KB09', 'klara.bila@example.com', '+420778899001', 'Snaží se přejít na nový systém'),
('David Hnědý', 'DH10', 'david.hnedy@example.com', NULL, 'Má problémy s aktuálním řešením');

INSERT INTO VirtualPCs (VirtualPcName, ServiceName, OperatingSystem, CPU_Cores, RAM_Size_GB, Disk_Size_GB, Backupping, Administration, IP_Address, FQDN, VirtualPcNotes)
VALUES
('VirtPC-01', 'Web Hosting', 'Windows Server 2019', 4, 16, 200, 1, 1, '192.168.1.10', 'virtpc01.example.com', 'Hlavní webový server'),
('VirtPC-02', 'Database Server', 'Ubuntu 20.04', 8, 32, 500, 1, 1, '192.168.1.11', 'virtpc02.example.com', 'MySQL databáze pro zákazníky'),
('VirtPC-03', 'Application Server', 'Windows Server 2016', 6, 24, 300, 0, 1, '192.168.1.12', 'virtpc03.example.com', 'Server pro aplikace'),
('VirtPC-04', 'Backup Server', 'CentOS 7', 4, 16, 1000, 1, 0, '192.168.1.13', 'virtpc04.example.com', 'Zálohovací server'),
('VirtPC-05', 'File Server', 'Debian 11', 2, 8, 100, 0, 1, '192.168.1.14', 'virtpc05.example.com', 'Server pro sdílení souborů'),
('VirtPC-06', 'Test Server', 'Windows 10 Pro', 4, 16, 256, 0, 0, '192.168.1.15', 'virtpc06.example.com', 'Testovací prostředí'),
('VirtPC-07', 'DNS Server', 'Ubuntu 22.04', 2, 4, 50, 1, 1, '192.168.1.16', 'virtpc07.example.com', 'DNS server'),
('VirtPC-08', 'Web Proxy', 'Windows Server 2022', 4, 16, 150, 0, 1, '192.168.1.17', 'virtpc08.example.com', 'Proxy server pro webový přístup'),
('VirtPC-09', 'Mail Server', 'CentOS 8', 6, 32, 300, 1, 1, '192.168.1.18', 'virtpc09.example.com', 'Mail server pro firemní klientelu'),
('VirtPC-10', 'Development Server', 'Windows 11', 8, 64, 500, 0, 0, '192.168.1.19', 'virtpc10.example.com', 'Vývojový server pro tým');

INSERT INTO CustomersVirtualPCs (CustomerID, VirtualPcID)
SELECT c.CustomerID, v.VirtualPcID
FROM Customers c
JOIN VirtualPCs v ON c.CustomerTag LIKE 'J%' OR c.CustomerTag LIKE 'P%'
WHERE v.VirtualPcName IN ('VirtPC-01', 'VirtPC-02', 'VirtPC-03');

INSERT INTO CustomersVirtualPCs (CustomerID, VirtualPcID)
SELECT c.CustomerID, v.VirtualPcID
FROM Customers c
JOIN VirtualPCs v ON c.CustomerTag LIKE 'M%' OR c.CustomerTag LIKE 'E%'
WHERE v.VirtualPcName IN ('VirtPC-04', 'VirtPC-05', 'VirtPC-06');

INSERT INTO CustomersVirtualPCs (CustomerID, VirtualPcID)
SELECT c.CustomerID, v.VirtualPcID
FROM Customers c
JOIN VirtualPCs v ON c.CustomerTag LIKE 'L%' OR c.CustomerTag LIKE 'A%'
WHERE v.VirtualPcName IN ('VirtPC-07', 'VirtPC-08', 'VirtPC-09');

INSERT INTO Accounts (VirtualPcID, Username, Password, IsAdmin)
VALUES
((SELECT VirtualPcID FROM VirtualPCs WHERE VirtualPcName = 'VirtPC-01'), 'admin01', 'pass01', 1),
((SELECT VirtualPcID FROM VirtualPCs WHERE VirtualPcName = 'VirtPC-02'), 'dbadmin', 'dbpass', 1),
((SELECT VirtualPcID FROM VirtualPCs WHERE VirtualPcName = 'VirtPC-03'), 'appuser', 'apppass', 0),
((SELECT VirtualPcID FROM VirtualPCs WHERE VirtualPcName = 'VirtPC-04'), 'backupuser', 'backuppass', 0),
((SELECT VirtualPcID FROM VirtualPCs WHERE VirtualPcName = 'VirtPC-05'), 'fileadmin', 'filepass', 1),
((SELECT VirtualPcID FROM VirtualPCs WHERE VirtualPcName = 'VirtPC-06'), 'testuser', 'testpass', 0),
((SELECT VirtualPcID FROM VirtualPCs WHERE VirtualPcName = 'VirtPC-07'), 'dnsadmin', 'dnspass', 1),
((SELECT VirtualPcID FROM VirtualPCs WHERE VirtualPcName = 'VirtPC-08'), 'proxyuser', 'proxypass', 0),
((SELECT VirtualPcID FROM VirtualPCs WHERE VirtualPcName = 'VirtPC-09'), 'mailadmin', 'mailpass', 1),
((SELECT VirtualPcID FROM VirtualPCs WHERE VirtualPcName = 'VirtPC-10'), 'devuser', 'devpass', 0);

