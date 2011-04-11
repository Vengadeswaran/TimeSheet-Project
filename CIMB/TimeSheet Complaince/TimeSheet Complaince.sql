declare @stdate as datetime, @enddate as datetime
set @stdate = '2011/3/1'
set @enddate = '2011/3/31'

SELECT     res.ResourceUID, res.ResourceName, res.RBS, tperiod.PeriodUID, tperiod.PeriodStatusID, tperiod.StartDate, tperiod.EndDate, tperiod.PeriodName, 
                      tperiod.LCID
INTO            [#t1]
FROM         MSP_TimesheetPeriod AS tperiod CROSS JOIN
                      MSP_EpmResource_UserView AS res
WHERE     (tperiod.StartDate BETWEEN DATEADD(d, - 7, @stdate) AND @enddate)

--select * from #t1
SELECT      [#t1].PeriodUID, [#t1].ResourceUID, [#t1].RBS, [#t1].ResourceName, [#t1].PeriodName, 
			ISNULl(tstatus.Description,'Not Created') AS [TimeSheet Status], [#t1].StartDate, [#t1].EndDate
INTO #t2
FROM        MSP_TimesheetStatus AS tstatus INNER JOIN
            MSP_Timesheet AS tsheet ON tstatus.TimesheetStatusID = tsheet.TimesheetStatusID INNER JOIN
            MSP_TimesheetResource AS tres ON tsheet.OwnerResourceNameUID = tres.ResourceNameUID RIGHT OUTER JOIN
            [#t1] ON [#t1].ResourceUID = tres.ResourceUID AND [#t1].PeriodUID = tsheet.PeriodUID
drop table #t1

SELECT ResourceName, PeriodName, [TimeSheet Status] FROM #t2 WHERE [TimeSheet Status] <> 'Approved'

drop table #t2