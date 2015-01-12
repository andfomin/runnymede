


CREATE PROCEDURE [dbo].[libCreatePersonalResource]
	@UserId int,
	@Id int = 0,
	@Format char(6) = null,
	@NaturalKey nvarchar(2000),
	@Segment nvarchar(1000) = null,
	@Title nvarchar(200),
	@CategoryIds nvarchar(50) = null,
	@Tags nvarchar(100) = null,
	@SourceId nchar(4) = null,
	@HasExplanation bit = 0,
	@HasExample bit = 0,
	@HasExercise bit = 0,
	@HasText bit = 0,
	@HasPicture bit = 0,
	@HasAudio bit = 0,
	@HasVideo bit = 0,
	@Comment nvarchar(200) = null,
	@IsForCopycat bit = 0
AS
BEGIN
SET NOCOUNT ON;
/* This procedure should be idempotent.
	Returns @ResourceId.
 */

declare @ProcName sysname, @ExternalTran int, @XState int;
select  @ProcName = object_name(@@procid), @ExternalTran = @@trancount;

declare @ResourceChecksum bigint, @ResourceId int, @ResourceExists bit, 
	@DescriptionChecksum bigint, @DescriptionId int, 
	@TitleId bigint, @TitleExists bit;

declare @Categories table (
	Id nvarchar(10) primary key not null
)

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	/* We expect @SourceId is a string of space separated values. We store it as is in dbo.libDescriptions.CategoryIds.
	   We expect that @CategoryIds is sanitized upstream, we do not sanitize the input here.
	   Since we split on space, a preceding, trailing, or an extra space in a malformed input string will cause an empty string row. 
	   If @CategoryIds is null, there will be a single row with NULL returned.
	   We check against dbo.libCategories to enforce referencial integrity on values stored in dbo.libDescriptions.CategoryId, imitating a foreign key.
	*/
	insert @Categories (Id)
		select S
		from dbo.sysSplit(' ', @CategoryIds)
		where S is not null;
		--where len(S) > 0;

	if exists (
		select *
		from @Categories TC
			left join dbo.libCategories C on TC.Id = C.Id
		where C.Id is null
	)
		raiserror('%s,%d,%s::Wrong categories.', 16, 1, @ProcName, @UserId, @CategoryIds);

	/* -- This is the example of how to produce CategoryIds back from @Categories.
	join/apply (
		select stuff((
			select N' ' + Id
			from @Categories 
			for xml path('')
		), 1, 1, '') as CategoryIds
	) q
	*/

	-- Make a scalar for the where clause. Otherwise there is a guess that the execution plan will calculate checksum for each row and ignore the UX_Checksum index.
	--set @ResourceHash = convert(uniqueidentifier, hashbytes('MD5', coalesce(@Format, '') + @NaturalKey + coalesce(@Segment, '') ));
	set @ResourceChecksum =	convert(bigint, 4294967296) * binary_checksum(@Format, @NaturalKey, @Segment) + binary_checksum(reverse(@Segment), reverse(@NaturalKey),reverse(@Format));

	select @ResourceId = Id, @ResourceExists = 1
	from dbo.libResources
	where [Checksum] = @ResourceChecksum;

	-- If @Id has a real value, it means the resource is not a new one, but a common one. In this case the provided @Id must correspond to the @ResourceHash.
	if (nullif(@Id, 0) is not null) and (@Id != @ResourceId)
		raiserror('%s,%d,%d::The resource not found.', 16, 1, @ProcName, @UserId, @Id);

	if (@ResourceExists is null) begin
		-- Get a new random Id
		select @ResourceId = dbo.libGetNewResourceId();
	end

	-- dbo.libTitles.Id is a computed column. @TitleId must be calculated the same way.
	--set @TitleId = isnull(convert(uniqueidentifier, hashbytes('MD5', @Title)), convert(binary(16), 0));	
	set @TitleId = isnull(convert(bigint, 4294967296) * binary_checksum(@Title) + binary_checksum(reverse(@Title)), 0);

	select @TitleExists = 1
	from dbo.libTitles 
	where Id = @TitleId

	if (@TitleExists is not null) begin

		-- Rows in dbo.libDescriptions must be immutable. We do not update an existing version, we insert a new version only.
		-- The Has... columns have default values.
		--set @DescriptionHash = convert(uniqueidentifier, hashbytes('MD5',
		--	coalesce(convert(nchar(36), @TitleId), ' ') + coalesce(@CategoryIds, ' ') + coalesce(@Tags, ' ') + coalesce(@SourceId, ' ') 
		--	+ convert(nchar(1), @HasExplanation) + convert(nchar(1), @HasExample)+ convert(nchar(1), @HasExercise) + convert(nchar(1), @HasText)
		--	+ convert(nchar(1), @HasPicture) + convert(nchar(1), @HasAudio) + convert(nchar(1), @HasVideo)
		--	));

		set @DescriptionChecksum =
			convert(bigint, 4294967296)
			* 
			binary_checksum(@TitleId, @CategoryIds, @Tags, @SourceId, @HasExplanation, @HasExample, @HasExercise, @HasText, @HasPicture, @HasAudio, @HasVideo)
			+
			binary_checksum(reverse(@CategoryIds), reverse(@Tags), reverse(@SourceId));

		select @DescriptionId = Id 
		from dbo.libDescriptions
		where [Checksum] = @DescriptionChecksum;

	end

	if @ExternalTran = 0
		begin transaction;

		-- Insert the title if it is new. Id is a computed column, its value must be calculated in the same way as @TitleId.
		insert dbo.libTitles (Title)
			select @Title
			where @TitleExists is null;

		-- Insert a new description if it is new.
		if (@DescriptionId is null) begin

			insert dbo.libDescriptions (TitleId, CategoryIds, Tags, SourceId, 
					HasExplanation, HasExample, HasExercise, HasText, HasPicture, HasAudio, HasVideo)
				values (@TitleId, @CategoryIds, @Tags, @SourceId, 
					@HasExplanation, @HasExample, @HasExercise, @HasText, @HasPicture, @HasAudio, @HasVideo);

			select @DescriptionId = scope_identity() where @@rowcount != 0;

		end

		-- Insert the resource if it is new. Update ForCopycat if needed.
		if (@ResourceExists is null) begin

			insert dbo.libResources (Id, [Format], NaturalKey, Segment, DescriptionId, UserId, IsForCopycat)
				values (@ResourceId, @Format, @NaturalKey, @Segment, @DescriptionId, @UserId, @IsForCopycat);

		end
		else begin

			update dbo.libResources
				set DescriptionId = @DescriptionId,	IsForCopycat = @IsForCopycat
			where Id = @ResourceId
				and @IsForCopycat = 1;

		end

		merge dbo.libUserResources as Trg
		using (values(@UserId, @ResourceId)) as Src (UserId, ResourceId)
			on Trg.UserId = Src.UserId and Trg.ResourceId = Src.ResourceId
		when matched then
			update set 
				IsPersonal = 1,
				DescriptionId = @DescriptionId,
				Comment = coalesce(@Comment, Comment),
				CopycatPriority = iif(@IsForCopycat = 1, 5, CopycatPriority),
				ReindexSearch = 1
		when not matched then
			insert (UserId, ResourceId, IsPersonal, DescriptionId, Comment, CopycatPriority, ReindexSearch)
				values (@UserId, @ResourceId, 1, @DescriptionId, @Comment, iif(@IsForCopycat = 1, 5, 0), 1);

	if @ExternalTran = 0
		commit;

	select @ResourceId as Id, @Format as [Format], @NaturalKey as NaturalKey, @Segment as Segment, 
		@Title as Title, @CategoryIds as CategoryIds, @Tags as Tags, @SourceId as SourceId, 
		@HasExplanation as HasExplanation, @HasExample as HasExample, @HasExercise as HasExercise, 
		@HasText as HasText, @HasPicture as HasPicture, @HasAudio as HasAudio, @HasVideo as HasVideo,
		convert(bit, 1) as IsPersonal, convert(bit, 1) as Viewed; 

end try
begin catch
	set @XState = xact_state();
	if @XState = 1 and @ExternalTran > 0
		rollback transaction ProcedureSave;
	if @XState = 1 and @ExternalTran = 0
		rollback;
	if @XState = -1
		rollback;
	throw;
end catch

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[libCreatePersonalResource] TO [websiterole]
    AS [dbo];

