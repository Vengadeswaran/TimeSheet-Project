Declare @stdate as datetime, @enddate as datetime
set @stdate = '2/1/2011'
set @enddate = '2/28/2011'
SELECT     t_res.ResourceUID AS ResUID, t_proj.ProjectUID AS ProjUID, t_res.ResourceName AS ResName, 
                      CASE WHEN t_class.Type <> 0 THEN t_class.ClassNAME ELSE t_proj.ProjectName END AS ProjName, t_act.TimeByDay AS t_date, 
                      t_act.TimeByDay_DayOfWeek AS t_wkday, t_class.Type AS t_type, CASE WHEN ((t_act.TimeByDay_DayOfWeek = 1 OR
                      t_act.TimeByDay_DayOfWeek = 7) AND (t_class.Type = 1)) THEN '0' WHEN (t_class.Type = 1 AND 
                      (t_act.ActualWorkBillable + t_act.ActualWorkNonBillable + t_act.ActualOvertimeWorkBillable + t_act.ActualOvertimeWorkNonBillable <= 4)) 
                      THEN '4' WHEN (t_class.Type = 1 AND 
                      (t_act.ActualWorkBillable + t_act.ActualWorkNonBillable + t_act.ActualOvertimeWorkBillable + t_act.ActualOvertimeWorkNonBillable > 4)) 
                      THEN '8' ELSE t_act.ActualWorkBillable + t_act.ActualWorkNonBillable + t_act.ActualOvertimeWorkBillable + t_act.ActualOvertimeWorkNonBillable END
                       AS totaltime
INTO            [#t1]
FROM         MSP_TimesheetActual AS t_act INNER JOIN
                      MSP_TimesheetResource AS t_res ON t_act.LastChangedResourceNameUID = t_res.ResourceNameUID INNER JOIN
                      MSP_TimesheetLine AS t_line ON t_act.TimesheetLineUID = t_line.TimesheetLineUID INNER JOIN
                      MSP_TimesheetClass AS t_class ON t_line.ClassUID = t_class.ClassUID INNER JOIN
                      MSP_TimesheetProject AS t_proj ON t_line.ProjectNameUID = t_proj.ProjectNameUID
WHERE     (t_act.TimeByDay BETWEEN @stdate AND @enddate)
ORDER BY ResUID, t_date, t_type

SELECT #t1.ResUID, #t1.ProjUID, #t1.ResName, #t1.ProjName, #t1.t_date, #t1.t_wkday, #t1.t_type, #t1.totaltime,
		ISNULL(dummyt1_nkwking.nonwktotal,0) AS nwktotal, ISNULL(dummyt1.daytotal,0) AS daytotal, 
		CASE WHEN #t1.t_type <> 1 AND ((dummyt1_nkwking.nonwktotal = 4 AND dummyt1.daytotal >4)or (ISNULL(dummyt1_nkwking.nonwktotal,0) =8) or
					(ISNULL(dummyt1_nkwking.nonwktotal,0) = 0 AND dummyt1.daytotal > 8)) 
			THEN (8-ISNULL(dummyt1_nkwking.nonwktotal,0))*#t1.totaltime/ISNULL(dummyt1.daytotal,1)
		ELSE #t1.totaltime END AS normalizedtime into #t2
FROM #t1 LEFT OUTER JOIN(
SELECT     dummyt1_nwking.ResUID, dummyt1_nwking.t_date, SUM(dummyt1_nwking.totaltime) AS nonwktotal
FROM         [#t1] AS dummyt1_nwking
WHERE     (dummyt1_nwking.t_type = 1)
GROUP BY dummyt1_nwking.ResUID, dummyt1_nwking.t_date
--ORDER BY dummyt1_nwking.ResUID, dummyt1_nwking.t_date
) AS dummyt1_nkwking ON #t1.ResUID = dummyt1_nkwking.ResUID AND #t1.t_date = dummyt1_nkwking.t_date LEFT OUTER JOIN
(
SELECT     dummyt1.ResUID, dummyt1.t_date, SUM(dummyt1.totaltime) AS daytotal
FROM         [#t1] AS dummyt1
WHERE     (dummyt1.t_type <> 1)
GROUP BY dummyt1.ResUID, dummyt1.t_date
--ORDER BY dummyt1.ResUID, dummyt1.t_date
) AS dummyt1 ON #t1.ResUID = dummyt1.ResUID AND #t1.t_date = dummyt1.t_date
ORDER BY #t1.ResUID, #t1.t_date, #t1.t_type
drop table #t1

SELECT distinct     TimeByDay AS missing_date,TimeDayOfTheWeek AS missing_wkday, t_Resources.ResUID AS missing_ResUID, t_Resources.ResName INTO #t3
FROM         MSP_TimeByDay CROSS JOIN (SELECT distinct [#t2].ResUID, #t2.ResName FROM [#t2]) AS t_Resources
WHERE     (TimeByDay BETWEEN @stdate AND @enddate) AND
(NOT EXISTS
(SELECT     *
FROM         [#t2] WHERE MSP_TimeByDay.TimeByDay = [#t2].t_date AND t_Resources.ResUID = [#t2].ResUID))

INSERT INTO #t2
SELECT #t3.missing_ResUID, 'E38038FA-F8CA-47D1-BFD4-6B45B8462972',#t3.ResName,'NotClocked',#t3.missing_date, #t3.missing_wkday, 1, 8, 8,0,8
FROM #t3 WHERE #t3.missing_wkday <> '1' AND #t3.missing_wkday <> '7'

drop table #t3

SELECT #t2.ResUID, SUM(#t2.normalizedtime) AS total_paid_leave INTO #t4
FROM #t2
WHERE #t2.t_type = 1 AND #t2.ProjName = 'Paid Leave-Public Holiday'
GROUP By #t2.ResUID
SELECT #t2.ResUID,#t2.ProjUID, 
SUM((#t2.normalizedtime /(CASE WHEN nortotal.total_Proj = 0 THEN 1 ELSE nortotal.total_Proj END))*#t4.total_paid_leave + #t2.normalizedtime) AS normalized_proj_time
INTO #t5
FROM #t2 INNER JOIN
(select #t2.ResUID, SUM(#t2.normalizedtime) AS total_Proj FROM #t2 WHERE t_type = 0 GROUP BY #t2.ResUID) AS nortotal ON #t2.ResUID = nortotal.ResUID
INNER JOIN #t4 ON #t2.ResUID = #t4.ResUID
WHERE #t2.t_type = 0
GROUP BY #t2.ResUID, #t2.ProjUID, #t2.ProjName, #t2.t_type

--select * from #t2
drop table #t4

SELECT #t2.ResUID, #t2.ProjUID, #t2.ProjName, #t2.t_type, SUM (#t2.normalizedtime) AS nor_time
INTO #t6
FROM #t2
GROUP By #t2.ResUID, #t2.ProjUID, #t2.ProjName,#t2.t_type

drop table #t2

select #t6.ResUID, #t6.ProjUID, #t6.ProjName,
CASE WHEN #t6.t_type = 0 THEN #t5.normalized_proj_time ELSE #t6.nor_time END AS total_time,
CASE WHEN #t6.t_type = 0 THEN ROUND((#t5.normalized_proj_time/res_total.res_total_time),3)*100 ELSE ROUND((#t6.nor_time/res_total.res_total_time),3)*100 END 
AS pct_total_time INTO #t7
from #t6 LEFT OUTER JOIN #t5 on #t6.ResUID = #t5.ResUID and #t6.ProjUID = #t5.ProjUID LEFT OUTER JOIN
(select #t6.ResUID, SUM (#t6.nor_time) AS res_total_time FROM #t6 GROUP BY #t6.ResUID) AS res_total ON #t6.ResUID = res_total.ResUID
where #t6.ProjName <> 'Paid Leave-Public Holiday'
order by #t6.ResUID
drop table #t5
drop table #t6
select #t7.ResUID, #t7.ProjUID, #t7.ProjName, #t7.total_time, #t7.pct_total_time,uv_res.ResourceName,
ISNULL (uv_proj.Project_RC_Code, #t7.ProjName) AS costcode, ISNULL(uv_res.ResourceEmailAddress,'NoEmail') AS res_email
FROM #t7 left outer join dbo.MSP_EpmResource_UserView as uv_res on #t7.ResUID = uv_res.ResourceUID
left outer join dbo.MSP_EpmProject_UserView as uv_proj on #t7.ProjUID = uv_proj.ProjectUID
order by #t7.resuid
--drop table #t1, #t2, #t3, #t4, #t5, #t6, #t7
