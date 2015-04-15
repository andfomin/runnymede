


CREATE PROCEDURE [dbo].[sysInitializeTestUsers]
AS
BEGIN
/*
+http://rusanu.com/2010/11/22/try-catch-throw-exception-handling-in-t-sql/
+http://msdn.microsoft.com/en-us/library/ms188378.aspx
*/
SET NOCOUNT ON;

declare @ExternalTran int, @ProcName sysname, @XState int;
select @ExternalTran = @@trancount, @ProcName = object_name(@@procid);
--raiserror('%s: ', 16, 1, @ProcName);

begin try
	if @ExternalTran > 0
		save transaction ProcedureSave;

	if @ExternalTran = 0
		begin transaction;

		execute dbo.sysInitializeUser
			N'ss@ss.ss', 
			N'AGxkSsC4upPCdR5RLSnB6E+X8uMvu7cTA7dViMEDL6/JTxmaVGswCjegLe98N0qfWg==', --'123456' 
			N'5997e70f-2544-45e9-9960-05c82813c030',
			N'student ss',
			0;

		--exec dbo.accPostIncomingPayPalPayment @UserName = 'ss@ss.ss', @Amount = 222.22, @Fee = 11.11, @ExtId = 'Init0000000001', @Details = null

		execute dbo.sysInitializeUser
			N'sss@sss.sss', 
			N'ANg8podNmEv4lUVjRWSosWvb02RvfLgOoKTuWS8W2RkRO0fl4LOsud+4llzCmzLS0Q==', --'123456' 
			N'3f038fa7-069e-4927-a7b2-f3a83164c801',
			N'sss name',
			0;

		--exec dbo.accPostIncomingPayPalPayment @UserName = 'sss@sss.sss', @Amount = 333.33, @Fee = 33.33, @ExtId = 'Init0000000002', @Details = null

		execute dbo.sysInitializeUser
			N'tt@tt.tt', 
			N'AAh/iWBDbHQ7l0uwMoGXAjbolN+JQpWLYG/80VQMWRJpzKws4zMNoRFopn6Bh1gAbw==', --'123456' 
			N'b4085404-d1f2-4a15-b74a-07ce4aa86de2',
			N'teacher1',
			1;

		execute dbo.sysInitializeUser
			N'ttt@ttt.ttt', 
			N'AE/uY3z2/5EumI2DTR8BzfKJ558puxJntnLmVhPx5xwWe67ZfLpn1Vpv0BnP61bq4w==', --'123456'
			N'58338b33-01f3-46ea-a40e-4bccc7af1829',
			N'ttt name',
			1;

-- -- Insert teachers in batch
--declare @i int = 100;
--declare @id nvarchar(100);
--while @i > 0 begin
--	set @i = @i - 1;		
--	set @id = cast(newid() as nvarchar(100));
--	execute dbo.[InitializeUser] @id, null, null, @id, 1;
--end


	if @ExternalTran = 0
		commit;
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