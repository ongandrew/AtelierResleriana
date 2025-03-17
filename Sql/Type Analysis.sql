-- Declare JSON input variable and read file
DECLARE @jsonData NVARCHAR(MAX);
SELECT @jsonData = BulkColumn
FROM OPENROWSET(BULK 'C:\Program Files (x86)\Steam\steamapps\common\AtelierResleriana\BepInEx\plugins\Types 2.json', SINGLE_CLOB) AS j;

-- Drop existing tables if they exist
IF OBJECT_ID('TypeMethodParameter', 'U') IS NOT NULL DROP TABLE TypeMethodParameter;
IF OBJECT_ID('TypeMethod', 'U') IS NOT NULL DROP TABLE TypeMethod;
IF OBJECT_ID('TypeProperty', 'U') IS NOT NULL DROP TABLE TypeProperty;
IF OBJECT_ID('TypeField', 'U') IS NOT NULL DROP TABLE TypeField;
IF OBJECT_ID('TypeConstructorParameter', 'U') IS NOT NULL DROP TABLE TypeConstructorParameter;
IF OBJECT_ID('TypeConstructor', 'U') IS NOT NULL DROP TABLE TypeConstructor;
IF OBJECT_ID('Type', 'U') IS NOT NULL DROP TABLE Type;

-- Create the main Type table
CREATE TABLE [Type]
(
    [Id] INT NOT NULL IDENTITY(1, 1) PRIMARY KEY,
    [Assembly] NVARCHAR(255) NOT NULL,
    [Name] NVARCHAR(255) NOT NULL,
    [BaseType_Assembly] NVARCHAR(MAX) NULL,
    [BaseType_Name] NVARCHAR(MAX) NULL,
    CONSTRAINT [AK_Type] UNIQUE ([Assembly], [Name])
);

-- Create the TypeConstructor table
CREATE TABLE [TypeConstructor]
(
    [Id] INT IDENTITY(1, 1) PRIMARY KEY,
    [TypeId] INT NOT NULL,
    CONSTRAINT [FK_TypeConstructor_Type] FOREIGN KEY ([TypeId]) REFERENCES [Type]([Id])
);

-- Create the TypeConstructorParameter table
CREATE TABLE [TypeConstructorParameter]
(
    [Id] INT IDENTITY(1, 1) PRIMARY KEY,
    [TypeConstructorId] INT NOT NULL,
    [TypeReference_Assembly] NVARCHAR(MAX) NOT NULL,
    [TypeReference_Name] NVARCHAR(MAX) NULL,
    [Name] NVARCHAR(255) NOT NULL,
    [Index] INT NOT NULL,
    CONSTRAINT [FK_TypeConstructorParameter_TypeConstructor] FOREIGN KEY ([TypeConstructorId]) REFERENCES [TypeConstructor]([Id])
);

-- Create the TypeField table
CREATE TABLE [TypeField]
(
    [Id] INT IDENTITY(1, 1) PRIMARY KEY,
    [TypeId] INT NOT NULL,
    [IsPublic] BIT NOT NULL,
    [IsStatic] BIT NOT NULL,
    [TypeReference_Assembly] NVARCHAR(MAX) NOT NULL,
    [TypeReference_Name] NVARCHAR(MAX) NULL,
    [Name] NVARCHAR(255) NOT NULL,
    CONSTRAINT [FK_TypeField_Type] FOREIGN KEY ([TypeId]) REFERENCES [Type]([Id])
);

-- Create the TypeProperty table
CREATE TABLE [TypeProperty]
(
    [Id] INT IDENTITY(1, 1) PRIMARY KEY,
    [TypeId] INT NOT NULL,
    [IsPublic] BIT NOT NULL,
    [IsStatic] BIT NOT NULL,
    [TypeReference_Assembly] NVARCHAR(MAX) NOT NULL,
    [TypeReference_Name] NVARCHAR(MAX) NULL,
    [Name] NVARCHAR(255) NOT NULL,
    CONSTRAINT [FK_TypeProperty_Type] FOREIGN KEY ([TypeId]) REFERENCES [Type]([Id])
);

-- Create the TypeMethod table
CREATE TABLE [TypeMethod]
(
    [Id] INT IDENTITY(1, 1) PRIMARY KEY,
    [TypeId] INT NOT NULL,
    [IsPublic] BIT NOT NULL,
    [IsStatic] BIT NOT NULL,
    [ReturnTypeReference_Assembly] NVARCHAR(MAX) NOT NULL,
    [ReturnTypeReference_Name] NVARCHAR(MAX) NULL,
    [Name] NVARCHAR(255) NOT NULL,
    CONSTRAINT [FK_TypeMethod_Type] FOREIGN KEY ([TypeId]) REFERENCES [Type]([Id])
);

-- Create the TypeMethodParameter table
CREATE TABLE [TypeMethodParameter]
(
    [Id] INT IDENTITY(1, 1) PRIMARY KEY,
    [TypeMethodId] INT NOT NULL,
    [TypeReference_Assembly] NVARCHAR(MAX) NOT NULL,
    [TypeReference_Name] NVARCHAR(MAX) NULL,
    [Name] NVARCHAR(255) NOT NULL,
    [Index] INT NOT NULL,
    CONSTRAINT [FK_TypeMethodParameter_TypeMethod] FOREIGN KEY ([TypeMethodId]) REFERENCES [TypeMethod]([Id])
);

-- Optional indexes for performance
CREATE INDEX [IX_TypeConstructor_TypeId] ON [TypeConstructor]([TypeId]);
CREATE INDEX [IX_TypeConstructorParameter_TypeConstructorId] ON [TypeConstructorParameter]([TypeConstructorId]);
CREATE INDEX [IX_TypeField_TypeId] ON [TypeField]([TypeId]);
CREATE INDEX [IX_TypeProperty_TypeId] ON [TypeProperty]([TypeId]);
CREATE INDEX [IX_TypeMethod_TypeId] ON [TypeMethod]([TypeId]);
CREATE INDEX [IX_TypeMethodParameter_TypeMethodId] ON [TypeMethodParameter]([TypeMethodId]);

-- Insert into Type table
INSERT INTO [Type] (Assembly, Name, BaseType_Assembly, BaseType_Name)
SELECT 
    JSON_VALUE(value, '$.assembly') as Assembly,
    JSON_VALUE(value, '$.name') as Name,
    JSON_VALUE(value, '$.baseType.assembly') as BaseType_Assembly,
    JSON_VALUE(value, '$.baseType.name') as BaseType_Name
FROM OPENJSON(@jsonData);

-- Insert into TypeConstructor and TypeConstructorParameter tables
WITH TypeConstructors AS (
    SELECT 
        t.Id as TypeId,
        c.[key] as ConstructorIndex,
        c.value as ConstructorJson
    FROM [Type] t
    CROSS APPLY OPENJSON(@jsonData) j
    CROSS APPLY OPENJSON(j.value, '$.constructors') c
    WHERE JSON_VALUE(j.value, '$.assembly') = t.Assembly
    AND JSON_VALUE(j.value, '$.name') = t.Name
)
INSERT INTO TypeConstructor (TypeId)
SELECT DISTINCT TypeId 
FROM TypeConstructors;

-- Insert constructor parameters would go here but they're not shown in the sample data

-- Insert TypeField
INSERT INTO TypeField (TypeId, IsPublic, IsStatic, TypeReference_Assembly, TypeReference_Name, Name)
SELECT 
    t.Id,
    CAST(JSON_VALUE(f.value, '$.isPublic') as bit),
    CAST(JSON_VALUE(f.value, '$.isStatic') as bit),
    JSON_VALUE(f.value, '$.type.assembly'),
    JSON_VALUE(f.value, '$.type.name'),
    JSON_VALUE(f.value, '$.name')
FROM [Type] t
CROSS APPLY OPENJSON(@jsonData) j
CROSS APPLY OPENJSON(j.value, '$.fields') f
WHERE JSON_VALUE(j.value, '$.assembly') = t.Assembly
AND JSON_VALUE(j.value, '$.name') = t.Name;

-- Insert TypeProperty
INSERT INTO TypeProperty (TypeId, IsPublic, IsStatic, TypeReference_Assembly, TypeReference_Name, Name)
SELECT 
    t.Id,
    CAST(JSON_VALUE(p.value, '$.isPublic') as bit),
    CAST(JSON_VALUE(p.value, '$.isStatic') as bit),
    JSON_VALUE(p.value, '$.type.assembly'),
    JSON_VALUE(p.value, '$.type.name'),
    JSON_VALUE(p.value, '$.name')
FROM [Type] t
CROSS APPLY OPENJSON(@jsonData) j
CROSS APPLY OPENJSON(j.value, '$.properties') p
WHERE JSON_VALUE(j.value, '$.assembly') = t.Assembly
AND JSON_VALUE(j.value, '$.name') = t.Name;

-- Insert TypeMethod
INSERT INTO TypeMethod (TypeId, IsPublic, IsStatic, ReturnTypeReference_Assembly, ReturnTypeReference_Name, Name)
SELECT 
    t.Id,
    CAST(JSON_VALUE(m.value, '$.isPublic') as bit),
    CAST(JSON_VALUE(m.value, '$.isStatic') as bit),
    JSON_VALUE(m.value, '$.returnType.assembly'),
    JSON_VALUE(m.value, '$.returnType.name'),
    JSON_VALUE(m.value, '$.name')
FROM [Type] t
CROSS APPLY OPENJSON(@jsonData) j
CROSS APPLY OPENJSON(j.value, '$.methods') m
WHERE JSON_VALUE(j.value, '$.assembly') = t.Assembly
AND JSON_VALUE(j.value, '$.name') = t.Name;

-- Insert TypeMethodParameter
WITH MethodParams AS (
    SELECT 
        tm.Id as TypeMethodId,
        p.[key] as ParamIndex,
        p.value as ParamJson
    FROM [Type] t
    JOIN TypeMethod tm ON t.Id = tm.TypeId
    CROSS APPLY OPENJSON(@jsonData) j
    CROSS APPLY OPENJSON(j.value, '$.methods') m
    CROSS APPLY OPENJSON(m.value, '$.parameters') p
    WHERE JSON_VALUE(j.value, '$.assembly') = t.Assembly
    AND JSON_VALUE(j.value, '$.name') = t.Name
    AND JSON_VALUE(m.value, '$.name') = tm.Name
)
INSERT INTO TypeMethodParameter (TypeMethodId, TypeReference_Assembly, TypeReference_Name, Name, [Index])
SELECT 
    TypeMethodId,
    JSON_VALUE(ParamJson, '$.type.assembly'),
    JSON_VALUE(ParamJson, '$.type.name'),
    JSON_VALUE(ParamJson, '$.name'),
    CAST(ParamIndex as int)
FROM MethodParams;