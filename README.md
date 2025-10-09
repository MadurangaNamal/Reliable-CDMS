# ReliableCDMS - Corporate Document Management System

## Overview
ReliableCDMS is a secure document management system built with ASP.NET WebForms, REST API, SOAP API, and SQL Server.

## Technologies Used
- ASP.NET WebForms (.NET Framework 4.8)
- REST API (Web API 2)
- SOAP API (WCF Service)
- SQL Server Database
- Bootstrap 5 for UI

## Prerequisites
- Visual Studio 2019/2022
- SQL Server Express 2019+ or LocalDB
- .NET Framework 4.8
- IIS Express (included with Visual Studio)

## Installation Steps

### 1. Database Setup
1. Open SQL Server Management Studio
2. Connect to your SQL Server instance
3. Execute the SQL script from `Database/CreateDatabase.sql`
4. Verify tables are created

### 2. Configure Connection String
1. Open `Web.config`
2. Update the connection string:
```xml
   <add name="ReliableCDMSDB" 
        connectionString="Server=YOUR_SERVER;Database=ReliableCDMSDB;Integrated Security=True;" 
        providerName="System.Data.SqlClient"/>