
-- Creating function to convert AD integers to datetime
CREATE FUNCTION [dbo].[fnConvertADInteger8DateTime] (@valuepassed varchar(18))
RETURNS datetime
AS
BEGIN
  declare @mathValue bigint
  declare @datetimevalue datetime
  if(@valuepassed is null or @valuepassed='0')
    set @datetimevalue = null
  else
    select 
      @mathValue=CAST(left(@valuepassed,14) AS bigint)/60000,
      @mathValue=@mathValue-157258500, -- -[(min from 01/01/1601 to 01/01/1900) + (timezone offset)]  --change as necessary for your timezone --157258380
      @datetimevalue=dateadd(mi,@mathValue,0)
  RETURN (@datetimevalue)
END