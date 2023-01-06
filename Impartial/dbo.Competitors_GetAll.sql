CREATE PROCEDURE [dbo].[Competitors_GetAll]
AS
begin
	select *
	from dbo.[Competitors];
end