CREATE DATABASE ReliableCDMSDB;
GO

USE ReliableCDMSDB;
GO

-- Users table
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY,
    Username NVARCHAR(50) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) NOT NULL, -- Admin, Manager, Employee
    Department NVARCHAR(50),
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE()
);

-- Documents table
CREATE TABLE Documents (
    DocumentId INT PRIMARY KEY IDENTITY,
    FileName NVARCHAR(255) NOT NULL,
    Category NVARCHAR(50),
    UploadedBy INT FOREIGN KEY REFERENCES Users(UserId),
    UploadDate DATETIME DEFAULT GETDATE(),
    CurrentVersion INT DEFAULT 1,
    FilePath NVARCHAR(500),
    FileSize BIGINT,
    IsDeleted BIT DEFAULT 0
);

-- Version history
CREATE TABLE DocumentVersions (
    VersionId INT PRIMARY KEY IDENTITY,
    DocumentId INT FOREIGN KEY REFERENCES Documents(DocumentId),
    VersionNumber INT NOT NULL,
    FilePath NVARCHAR(500),
    UploadedBy INT FOREIGN KEY REFERENCES Users(UserId),
    UploadDate DATETIME DEFAULT GETDATE(),
    Comments NVARCHAR(500)
);

-- Audit log
CREATE TABLE AuditLog (
    LogId INT PRIMARY KEY IDENTITY,
    UserId INT,
    Action NVARCHAR(100),
    Details NVARCHAR(500),
    IPAddress NVARCHAR(50),
    LogDate DATETIME DEFAULT GETDATE()
);

-- Insert sample users (password is 'Pass@123' for all)
INSERT INTO Users (Username, PasswordHash, Role, Department, IsActive)
VALUES 
('admin', 'B6BC7B58510319A151D168BA3D5AECB3AC0A9708D06DD930F37FBC89B6CDC697', 'Admin', 'IT', 1),
('manager1', 'B6BC7B58510319A151D168BA3D5AECB3AC0A9708D06DD930F37FBC89B6CDC697', 'Manager', 'Sales', 1),
('employee1', 'B6BC7B58510319A151D168BA3D5AECB3AC0A9708D06DD930F37FBC89B6CDC697', 'Employee', 'HR', 1);
