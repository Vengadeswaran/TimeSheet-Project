SELECT     t_res.ResourceUID AS resUID, DATEPART(yyyy, t_actual.TimeByDay) AS t_year, DATEPART(mm, t_actual.TimeByDay) AS t_month, 
                      SUM(t_actual.ActualWorkBillable + t_actual.ActualWorkNonBillable + t_actual.ActualOvertimeWorkBillable + t_actual.ActualOvertimeWorkNonBillable) 
                      AS standardsum
INTO            [#t1]
FROM         MSP_TimesheetActual AS t_actual INNER JOIN
                      MSP_TimesheetLine AS t_line ON t_actual.TimesheetLineUID = t_line.TimesheetLineUID INNER JOIN
                      MSP_TimesheetClass AS t_class ON t_line.ClassUID = t_class.ClassUID INNER JOIN
                      MSP_TimesheetResource AS t_res ON t_actual.LastChangedResourceNameUID = t_res.ResourceNameUID
WHERE     (t_class.Type = 0)
GROUP BY t_res.ResourceUID,DATEPART(yyyy, t_actual.TimeByDay), DATEPART(mm, t_actual.TimeByDay) 

SELECT     t_res.ResourceUID AS resUID, t_proj.ProjectUID AS projUID, DATEPART(yyyy, t_actual.TimeByDay) AS t_year, DATEPART(mm, t_actual.TimeByDay) AS t_month, 
                      SUM(t_actual.ActualWorkBillable + t_actual.ActualWorkNonBillable + t_actual.ActualOvertimeWorkBillable + t_actual.ActualOvertimeWorkNonBillable) 
                      AS projsum INTO #t2
FROM         MSP_TimesheetActual AS t_actual INNER JOIN
                      MSP_TimesheetLine AS t_line ON t_actual.TimesheetLineUID = t_line.TimesheetLineUID INNER JOIN
                      MSP_TimesheetClass AS t_class ON t_line.ClassUID = t_class.ClassUID INNER JOIN
                      MSP_TimesheetProject AS t_proj ON t_line.ProjectNameUID = t_proj.ProjectNameUID INNER JOIN
                      MSP_TimesheetResource AS t_res ON t_actual.LastChangedResourceNameUID = t_res.ResourceNameUID
WHERE     (t_class.Type = 0)
GROUP BY t_res.ResourceUID, t_proj.ProjectUID, DATEPART(yyyy, t_actual.TimeByDay), DATEPART(mm, t_actual.TimeByDay)
ORDER BY t_res.ResourceUID, t_year, t_month,t_proj.ProjectUID

select #t2.resUID, #t2.projUID, #t2.t_year, #t2.t_month, #t2.projsum, #t1.standardsum, 
ROUND((CASE WHEN #t1.standardsum = 0 THEN 0 ELSE(#t2.projsum/#t1.standardsum) END),3)*100 AS projpct INTO #t3
from #t2 left outer join #t1 ON #t2.resUID = #t1.resUID and #t2.t_year = #t1.t_year and #t2.t_month = #t1.t_month

drop table #t1
drop table #t2

SELECT     t_res.ResourceUID AS resUID, DATEPART(yyyy, t_actual.TimeByDay) AS t_year, DATEPART(mm, t_actual.TimeByDay) AS t_month, 
                      SUM(t_actual.ActualWorkBillable + t_actual.ActualWorkNonBillable + t_actual.ActualOvertimeWorkBillable + t_actual.ActualOvertimeWorkNonBillable) 
                      AS non_working INTO #t4
FROM         MSP_TimesheetActual AS t_actual INNER JOIN
                      MSP_TimesheetLine AS t_line ON t_actual.TimesheetLineUID = t_line.TimesheetLineUID INNER JOIN
                      MSP_TimesheetClass AS t_class ON t_line.ClassUID = t_class.ClassUID INNER JOIN
                      MSP_TimesheetProject AS t_proj ON t_line.ProjectNameUID = t_proj.ProjectNameUID INNER JOIN
                      MSP_TimesheetResource AS t_res ON t_actual.LastChangedResourceNameUID = t_res.ResourceNameUID
WHERE     (t_class.Type = 1)
GROUP BY t_res.ResourceUID, DATEPART(yyyy, t_actual.TimeByDay), DATEPART(mm, t_actual.TimeByDay), t_class.Type, CASE WHEN (t_class.Type = 1) 
                      THEN 'Non-Wokring' ELSE t_class.ClassName END
ORDER BY resUID, t_year, t_month

SELECT	#t3.resUID, #t3.projUID, #t3.t_year, #t3.t_month, (#t3.projsum +
		CASE WHEN #t3.projpct = 100 THEN ISNULL(#t4.non_working,0) ELSE ROUND(ISNULL(#t4.non_working,0)*#t3.projpct/100,0)END) AS normalizedprojsum
		INTO #t5
FROM #t3 LEFT OUTER JOIN #t4 ON #t3.resUID = #t4.resUID AND #t3.t_year = #t4.t_year AND #t3.t_month= #t4.t_month

drop table #t3
drop table #t4

--'E38038FA-F8CA-47D1-BFD4-6B45B8462972' is projUID for all administrative work

INSERT INTO #t5
SELECT     t_res.ResourceUID AS resUID, 'E38038FA-F8CA-47D1-BFD4-6B45B8462972' AS projUID, DATEPART(yyyy, t_actual.TimeByDay) AS t_year, DATEPART(mm, t_actual.TimeByDay) 
                      AS t_month, 
                      SUM(t_actual.ActualWorkBillable + t_actual.ActualWorkNonBillable + t_actual.ActualOvertimeWorkBillable + t_actual.ActualOvertimeWorkNonBillable) 
                      AS projsum
FROM         MSP_TimesheetActual AS t_actual INNER JOIN
                      MSP_TimesheetLine AS t_line ON t_actual.TimesheetLineUID = t_line.TimesheetLineUID INNER JOIN
                      MSP_TimesheetClass AS t_class ON t_line.ClassUID = t_class.ClassUID INNER JOIN
                      MSP_TimesheetProject AS t_proj ON t_line.ProjectNameUID = t_proj.ProjectNameUID INNER JOIN
                      MSP_TimesheetResource AS t_res ON t_actual.LastChangedResourceNameUID = t_res.ResourceNameUID
WHERE (t_class.Type <>0 AND t_class.Type<>1)
GROUP BY t_res.ResourceUID, t_proj.ProjectUID, DATEPART(yyyy, t_actual.TimeByDay), DATEPART(mm, t_actual.TimeByDay)

SELECT #t5.resUID, #t5.projUID, #t5.t_year, #t5.t_month, #t5.normalizedprojsum, 
CASE WHEN tempt5.sumofactualtime = 0 THEN 0 ELSE ROUND(#t5.normalizedprojsum/tempt5.sumofactualtime*100,2) END AS pctnormalized
INTO #t6
FROM #t5 left outer join 
(SELECT #t5.resUID,#t5.t_year, #t5.t_month, sum(#t5.normalizedprojsum) AS sumofactualtime FROM #t5 GROUP BY resUID, t_year, t_month) AS tempt5 
ON #t5.resUID = tempt5.resUID AND #t5.t_year = tempt5.t_year AND #t5.t_month = tempt5.t_month
ORDER BY #t5.resUID, #t5.t_year, #t5.t_month, #t5.projUID

SELECT #t6.resUID, #t6.projUID, uv_res.ResourceName AS res_name, uv_res.ResourceNTAccount,
CASE WHEN #t6.projUID = 'E38038FA-F8CA-47D1-BFD4-6B45B8462972' THEN 'BAU and Others'
ELSE ISNULL(uv_proj.ProjectName,'Project Deleted') END AS proj_name, ISNULL(uv_proj.Project_RC_Code,'0000') AS proj_RC_Code, #t6.t_year, #t6.t_month, #t6.normalizedprojsum, #t6.pctnormalized 
FROM #t6 LEFT OUTER JOIN MSP_EpmProject_UserView AS uv_proj ON
#t6.projUID = uv_proj.ProjectUID LEFT OUTER JOIN MSP_EpmResource_UserView AS uv_res ON
#t6.resUID = uv_res.ResourceUID
ORDER BY #t6.resUID, #t6.t_year, #t6.t_month
DROP TABLE #t5
DROP TABLE #t6