declare @stdate as datetime, @enddate as datetime
set @stdate = '2011/3/1'
set @enddate = '2011/3/31'

SELECT     MSP_EpmResource_UserView.ResourceUID, MSP_EpmResource_UserView.ResourceName, MSP_EpmResource_UserView.RBS, 
                      MSP_TimesheetPeriod.PeriodUID, MSP_TimesheetPeriod.PeriodStatusID, MSP_TimesheetPeriod.StartDate, MSP_TimesheetPeriod.EndDate, 
                      MSP_TimesheetPeriod.PeriodName, MSP_TimesheetPeriod.LCID
INTO		#t1
FROM         MSP_TimesheetPeriod CROSS JOIN
                      MSP_EpmResource_UserView
WHERE     (MSP_TimesheetPeriod.StartDate BETWEEN DATEADD(d, - 7, @stdate) AND @enddate)
ORDER BY MSP_EpmResource_UserView.RBS, MSP_EpmResource_UserView.ResourceUID, MSP_TimesheetPeriod.StartDate

--select * from #t1
--drop table #t1
SELECT     MSP_TimesheetStatus.Description, MSP_Timesheet.PeriodUID, MSP_TimesheetResource.ResourceUID,
			#t1.RBS, #t1.ResourceName, #t1.StartDate, #t1.EndDate, #t1.PeriodName
			
FROM         MSP_TimesheetStatus INNER JOIN
                      MSP_Timesheet ON MSP_TimesheetStatus.TimesheetStatusID = MSP_Timesheet.TimesheetStatusID INNER JOIN
                      MSP_TimesheetResource ON MSP_Timesheet.OwnerResourceNameUID = MSP_TimesheetResource.ResourceNameUID RIGHT OUTER JOIN
				#t1 ON #t1.ResourceUID =  MSP_TimesheetResource.ResourceUID AND #t1.PeriodUID = MSP_Timesheet.PeriodUID