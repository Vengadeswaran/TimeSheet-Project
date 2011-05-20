DECLARE @_stdate AS DATETIME, @_enddate AS DATETIME
SET @_stdate = '02/01/2011'
SET @_enddate = '02/28/2011'

SELECT		res.ResourceUID, res.ResourceName, res.RBS, tperiod.PeriodUID, tperiod.PeriodStatusID, tperiod.StartDate, tperiod.EndDate, tperiod.PeriodName,
            tperiod.LCID, ISNULL(TM_Name.ResourceName, 'Not Assigned') AS TM_Name
INTO        [#t1]
FROM        MSP_EpmResource_UserView AS TM_Name RIGHT OUTER JOIN
            MSP_EpmResource_UserView AS res ON TM_Name.ResourceUID = res.ResourceTimesheetManagerUID CROSS JOIN
            MSP_TimesheetPeriod AS tperiod
WHERE		(tperiod.StartDate BETWEEN (
			(SELECT		CASE WHEN (TimeDayOfTheWeek = 2) THEN @_stdate WHEN (TimeDayOfTheWeek = 1) THEN DATEADD(d,1, @_stdate )
						ELSE DATEADD(d,(2-TimeDayofTheWeek), @_stdate ) END AS stdate
			FROM        MSP_TimeByDay
			WHERE		(TimeByDay = CONVERT(DATETIME, @_stdate , 102)))
			)
			AND @_enddate ) --AND (res.ResourceUID IN " + filterresource + @")
SELECT      [#t1].PeriodUID, [#t1].ResourceUID,[#t1].TM_Name, [#t1].RBS, [#t1].ResourceName, [#t1].PeriodName,
			ISNULl(tstatus.Description,'Not Created') AS [TimeSheet Status], [#t1].StartDate, [#t1].EndDate
INTO #t2
FROM        MSP_TimesheetStatus AS tstatus INNER JOIN
            MSP_Timesheet AS tsheet ON tstatus.TimesheetStatusID = tsheet.TimesheetStatusID INNER JOIN
            MSP_TimesheetResource AS tres ON tsheet.OwnerResourceNameUID = tres.ResourceNameUID RIGHT OUTER JOIN
            [#t1] ON [#t1].ResourceUID = tres.ResourceUID AND [#t1].PeriodUID = tsheet.PeriodUID
drop table	#t1
SELECT		PeriodName, TM_Name, ResourceName, COUNT(CASE WHEN ([TimeSheet Status] = 'In Progress') THEN [TimeSheet Status] END) 
            AS [In Progress], COUNT(CASE WHEN ([TimeSheet Status] = 'Not Created') THEN [TimeSheet Status] END) AS [Not Created], 
            COUNT(CASE WHEN ([TimeSheet Status] = 'Submitted') THEN [TimeSheet Status] END) AS Submitted
FROM        [#t2]
WHERE		([TimeSheet Status] <> 'Approved')
GROUP BY	PeriodName, TM_Name, ResourceName
ORDER BY	PeriodName, TM_Name, ResourceName

SELECT		PeriodName, COUNT(CASE WHEN ([TimeSheet Status] = 'In Progress') THEN [TimeSheet Status] END) 
            AS [In Progress], COUNT(CASE WHEN ([TimeSheet Status] = 'Not Created') THEN [TimeSheet Status] END) AS [Not Created], 
            COUNT(CASE WHEN ([TimeSheet Status] = 'Submitted') THEN [TimeSheet Status] END) AS Submitted
FROM        [#t2]
WHERE		([TimeSheet Status] <> 'Approved')
GROUP BY	PeriodName
ORDER BY	PeriodName
drop table	#t2