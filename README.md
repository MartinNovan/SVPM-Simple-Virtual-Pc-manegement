<h1> <p "font-size:200px;"><img align="left" src="https://github.com/MartinNovan/SVPM-Simple-Virtual-Pc-manegement/blob/main/SVPM/SVPM/Resources/AppIcon/app_icon.png" width="100">SVPM - Simple Virtual PC Management</p> </h1> 
Manage your VMs simply and cleanly!

## 🖥️ Overview

**SVPM** is an application designed to manage virtual computers, customer and accounts. It provides an intuitive interface to administer virtual PCs, edit login credentials and view hardware specifications of each virtual PC. The application is designed for ease of use in enterprise environments.

---

## 🚀 Main features

- **🔑 Customer Management:** Ability to add, edit and delete customers.
- **🖥️ Virtual PC Management:** List of virtual PCs , their hardware specifications and login details.
- **🔐 Virtual PC Accounts:** Ability to manage user accounts assigned to individual virtual PCs.
- **🔗 SQL database connection:** Connection to the database for retrieving and storing data.

---

## 🛠️ Installation and startup

### Requirements:

- **.NET 9.0 SDK** or later
- **Windows 10 or higher** (x64 architecture)
- **SQL Server** for data management

### Installation procedure:

1. **Download the application:** Download the [latest version](https://github.com/MartinNovan/SVPM-Simple-Virtual-Pc-manegement/releases/latest) of the application.
2. **Extract:** Unzip the downloaded file to the directory of your choice.
3. **Start the application:** Double click on the `SVPM-Setup.msi` file to start the installation.

### Database setup:
#### If you do not have a SQL Server:
1. **Install SQL Server**: Download and install SQL Server Express or any other version of SQL Server.
1. **Create a new database**: Open SQL Server Management Studio and create a new database named `SVPM`.
2. **Run the SQL script**: Execute the [`SVPM.sql`](/SVPM/SVPM/Resources/Scripts/SVPM.sql) script to create the necessary tables, procedures and triggers.
3. **Configure the connection**: After installation, you will need to set up the SQL connection in the application settings.
#### If you already have a SQL Server:
1. **Create a new database**: Open SQL Server Management Studio and create a new database named `SVPM`.
2. **Run the SQL script**: Execute the [`SVPM.sql`](/SVPM/SVPM/Resources/Scripts/SVPM.sql) script to create the necessary tables, procedures and triggers.
3. **Configure the connection**: After installation, you will need to set up the SQL connection in the application settings.
---

## ⚙️ SQL Connection Settings

When you run the application for the first time, you need to set up a connection to the SQL database:

1. **Fill data**:
   - **Server**: Server name or IP address.
   - **Database**: Database name.
   - **Login details**: Database username and password or use windows auth.
   
2. **Save Connection**.

---

## 📊 Main Screen

After successful login, the main screen of the application will be displayed, which consists of three tabs:

1. **👥 Customers**:
   - Display the list of customers.
   - Option to edit personal details and contact information.
   
![Customers Page](/Pictures/Customer-Page.png)
   
2. **🌐 Virtual PCs**:
   - List of assigned virtual PCs with detailed information about CPU, RAM and other specifications.
   - Ability to edit individual virtual PCs.
   
![Virtual PCs Page](/Pictures/VirtualPc-Page.png)

3. **🔑 Accounts**:
   - A listing of individual accounts from all virtual PCs with information such as password, if the account is an administrator account.
   - Option to edit the data.

![Accounts Page](/Pictures/Accounts-Page.png)

---

## 📊 Sub Screens

This pages appear when you click on customer or virtual PC.

1. **👥 Customers Info**:
   - Display detailed informations of the customer.
   - See his assigned virtual PCs.

![Customer Info Page](/Pictures/CustomerInfo-Page.png)
   
2. **🌐 Virtual PCs Info**:
   - Display detailed informations of the virtual PC.
   - See it's assigned accounts.

![Virtual PCs Info Page](/Pictures/VirtualPcInfo-Page.png)

---

## 📊 Config Screens

1. **🔗 Sql Connections**:
   - Display the list of saved SQL connections.
   - Option to add/edit/remove connections.

![SQL Connection Page](/Pictures/SQLConnection-Page.png)
   
2. **📜 Logs**:
   - List of detailed informtaions about changes that was made in database.

![Log Page](/Pictures/Log-Page.png)

## 💡 Usage tips

- If you need help, you can read manual for your version, or you can contact us [here](https://github.com/MartinNovan/SVPM-Simple-Virtual-Pc-manegement/issues).
- Black/White theme is automatically selected by your windows setting.
- If the database connection fails, check for correct data and server availability.

---

## 🛠️ Maintenance and Support

If you encounter bugs or need help, make a request here on git at [issues.](https://github.com/MartinNovan/SVPM-Simple-Virtual-Pc-manegement/issues)

---

### 🌟 Acknowledgements

Thank you for using **SVPM**! We appreciate your support and hope that the application will make it easier for you to manage your virtual PCs.
