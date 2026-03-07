-- Migrates category hierarchy to dedicated ParentCategories table.
-- Safe to rerun.

IF OBJECT_ID('ParentCategories', 'U') IS NULL
BEGIN
    CREATE TABLE ParentCategories (
        Id               BIGINT        IDENTITY(1,1) PRIMARY KEY,
        Name             NVARCHAR(150) NOT NULL UNIQUE,
        SortOrder        INT           NOT NULL DEFAULT 0,
        IsActive         BIT           NOT NULL DEFAULT 1,
        CreatedDateTime  DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy        BIGINT        NOT NULL DEFAULT 0,
        UpdatedDateTime  DATETIME2     NULL,
        UpdatedBy        BIGINT        NULL
    );
END;
GO

IF EXISTS (
    SELECT 1
    FROM sys.foreign_keys fk
    WHERE fk.parent_object_id = OBJECT_ID('Categories')
      AND fk.referenced_object_id = OBJECT_ID('Categories')
)
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM tempdb.sys.tables
        WHERE name LIKE '#CategoryParentMap%'
    )
    BEGIN
        CREATE TABLE #CategoryParentMap (
            OldCategoryId BIGINT PRIMARY KEY,
            NewParentCategoryId BIGINT NOT NULL
        );
    END

    INSERT INTO ParentCategories (Name, SortOrder, IsActive, CreatedDateTime, CreatedBy, UpdatedDateTime, UpdatedBy)
    SELECT c.Name, c.SortOrder, c.IsActive, c.CreatedDateTime, c.CreatedBy, c.UpdatedDateTime, c.UpdatedBy
    FROM Categories c
    WHERE c.ParentCategoryId IS NULL
      AND NOT EXISTS (SELECT 1 FROM ParentCategories p WHERE p.Name = c.Name);

    INSERT INTO #CategoryParentMap (OldCategoryId, NewParentCategoryId)
    SELECT c.Id, p.Id
    FROM Categories c
    INNER JOIN ParentCategories p ON p.Name = c.Name
    WHERE c.ParentCategoryId IS NULL
      AND NOT EXISTS (SELECT 1 FROM #CategoryParentMap m WHERE m.OldCategoryId = c.Id);

    UPDATE c
    SET c.ParentCategoryId = m.NewParentCategoryId
    FROM Categories c
    INNER JOIN #CategoryParentMap m ON c.ParentCategoryId = m.OldCategoryId;

    DELETE c
    FROM Categories c
    INNER JOIN #CategoryParentMap m ON c.Id = m.OldCategoryId;
END;
GO

DECLARE @fkName SYSNAME;
SELECT TOP 1 @fkName = fk.name
FROM sys.foreign_keys fk
WHERE fk.parent_object_id = OBJECT_ID('Categories')
  AND fk.referenced_object_id = OBJECT_ID('Categories');

IF @fkName IS NOT NULL
BEGIN
    EXEC('ALTER TABLE Categories DROP CONSTRAINT ' + QUOTENAME(@fkName));
END;
GO

IF EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE name = 'CK_Categories_NoSelfParent'
      AND parent_object_id = OBJECT_ID('Categories')
)
BEGIN
    ALTER TABLE Categories DROP CONSTRAINT CK_Categories_NoSelfParent;
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys fk
    WHERE fk.name = 'FK_Categories_ParentCategories'
      AND fk.parent_object_id = OBJECT_ID('Categories')
)
BEGIN
    ALTER TABLE Categories
        WITH CHECK ADD CONSTRAINT FK_Categories_ParentCategories
        FOREIGN KEY (ParentCategoryId) REFERENCES ParentCategories(Id);
END;
GO

IF EXISTS (SELECT 1 FROM Categories WHERE ParentCategoryId IS NULL)
BEGIN
    THROW 50001, 'Migration stopped: Categories with NULL ParentCategoryId remain.', 1;
END;
GO

IF EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('Categories')
      AND name = 'ParentCategoryId'
      AND is_nullable = 1
)
BEGIN
    ALTER TABLE Categories ALTER COLUMN ParentCategoryId BIGINT NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_Categories_Parent_Name'
      AND object_id = OBJECT_ID('Categories')
)
BEGIN
    CREATE UNIQUE INDEX UX_Categories_Parent_Name
        ON Categories(ParentCategoryId, Name);
END;
GO
