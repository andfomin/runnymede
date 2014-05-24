CREATE TABLE [dbo].[resCoreInventory] (
    [FullCode]         NCHAR (5)      CONSTRAINT [DF_resCoreInventory_FullCode] DEFAULT (N'(right((''00000''+isnull([FeatureCode],''))+case when isnumeric([FeatureCode])=(1) then ''_'' else '' end,(5)))') NOT NULL,
    [ReferenceLevel]   NCHAR (2)      NOT NULL,
    [FeatureGroup]     NVARCHAR (100) NOT NULL,
    [Feature]          NVARCHAR (300) NOT NULL,
    [FeatureGroupCode] NVARCHAR (10)  NULL,
    [FeatureCode]      NVARCHAR (10)  NULL,
    [Exponents]        NVARCHAR (MAX) NULL,
    [Position]         AS             (right(('00000'+isnull([FeatureCode],''))+case when isnumeric([FeatureCode])=(1) then '_' else '' end,(5))),
    [FullFeature]      AS             (([FeatureGroup]+'. ')+[Feature]),
    CONSTRAINT [PK_resCoreInventory] PRIMARY KEY CLUSTERED ([FullCode] ASC, [ReferenceLevel] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_FeatureGroup_Feature_ReferenceLevel]
    ON [dbo].[resCoreInventory]([FeatureGroup] ASC, [Feature] ASC, [ReferenceLevel] ASC);


GO
GRANT SELECT
    ON OBJECT::[dbo].[resCoreInventory] TO [websiterole]
    AS [dbo];


GO
GRANT INSERT
    ON OBJECT::[dbo].[resCoreInventory] TO [websiterole]
    AS [dbo];

