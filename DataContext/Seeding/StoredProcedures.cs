/*
ALTER PROCEDURE[dbo].[GetAuctionSeries]
@IsActive int
AS
BEGIN

	SELECT
		dbo.Category.Name AS CategoryName,
		dbo.Series.Name AS SeriesName,
		dbo.Series.Id,
		dbo.Series.RegStartDateTime,
		dbo.Series.RegEndDateTime
	FROM
		dbo.Category
		INNER JOIN dbo.Series ON dbo.Series.CategoryId = dbo.Category.Id
	WHERE
		(Series.IsSoftDeleted IS NULL OR Series.IsSoftDeleted = 0)      AND
		Series.IsActive = @IsActive;
END



ALTER PROCEDURE [dbo].[GetAuctionSeriesDetail]
	@SeriesId int,
	@PageNumber int = 1,
	@PageSize int = 20
AS
BEGIN

	SELECT
		Category.Name [CategoryName],
		Series.Name [SeriesName]
	FROM
		Category
		INNER JOIN Series ON Series.CategoryId = Category.Id 
	WHERE
		(Series.IsSoftDeleted IS NULL OR Series.IsSoftDeleted = 0)
		AND Series.Id = @SeriesId;
		
	SELECT
		Id,
		SeriesId,
		SeriesNumber,
		ReservePrice 
	FROM
		SeriesDetail
	WHERE
		(IsSoftDeleted IS NULL OR IsSoftDeleted = 0)
		AND SeriesId = @SeriesId
	ORDER BY 1
	OFFSET (@PageNumber - 1) * @PageSize ROWS FETCH NEXT @PageSize ROWS ONLY;
			
END



ALTER PROCEDURE [dbo].[RegisterCustomer]
	@Name varchar(100),
	@FatherHusbandName varchar(100),
	@CNIC varchar(13),
	@Phone varchar(50),
	@Email varchar(50),
	@Address varchar(100),
	@NTN Varchar(50),
	@ChasisNumber varchar(50)
AS
BEGIN
	
	IF NOT EXISTS ( SELECT * FROM Customer WHERE CNIC = @CNIC OR NTN = @NTN OR ChasisNumber = @ChasisNumber ) 
				INSERT INTO Customer ( 
					CreatedOn, 
					CustomerTypeId, 
					Name, 
					FatherHusbandName, 
					CNIC, 
					Phone, 
					Email, 
					Address, 
					NTN, 
					ChasisNumber )
				VALUES (
					GETDATE(),
					1,	
					@Name,
					@FatherHusbandName,
					@CNIC,
					@Phone,
					@Email,
					@Address,
					@NTN,
					@ChasisNumber 
					);
	END



CREATE PROCEDURE[dbo].[SaveApplication]
	@SeriesNumberId INT,
	@CustomerId INT
AS
BEGIN

	INSERT INTO Application(
		ApplicationStatusId,
		AIN,
		CustomerId,
		SeriesCategoryId,
		SeriesId,
		SeriesNumber,
		CreatedOn,
		CreatedBy)
		
		SELECT
			1,
			'AIN',
			1,
			Series.SeriesCategoryId,
			SeriesNumber.SeriesId,
			SeriesNumber.AuctionNumber,
			GETDATE(),
			1	--SYSTEM
		FROM
			dbo.Series
			INNER JOIN dbo.SeriesNumber ON dbo.Series.Id = dbo.SeriesNumber.SeriesId
		WHERE
			SeriesNumber.Id = @SeriesNumberId;
	
END




ALTER PROCEDURE [dbo].[RegisterCustomerAndApply]
	@Name varchar(100),
	@FatherHusbandName varchar(100),
	@CNIC varchar(13),
	@Phone varchar(50),
	@Email varchar(50),
	@Address varchar(100),
	@NTN Varchar(50) = NULL,
	@ChasisNumber varchar(50) = NULL,
	@SeriesNumberId INT
AS
BEGIN
	
	EXEC dbo.RegisterCustomer
			@Name,
			@FatherHusbandName,
			@CNIC,
			@Phone,
			@Email,
			@Address,
			@NTN,
			@ChasisNumber;
	--GO
	
	DECLARE @V_CustomerId  INT;
	SET @V_CustomerId = 0;
	
	SELECT @V_CustomerId = Id FROM Customer WHERE CNIC = @CNIC OR NTN = @NTN OR ChasisNumber = @ChasisNumber;
	
	EXEC dbo.SaveApplication
			@SeriesNumberId,
			@V_CustomerId;
		
END


*/