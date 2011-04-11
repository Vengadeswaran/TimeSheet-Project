SELECT     t_res.ResourceUID AS resUID, DATEPART(yyyy, t_actual.TimeByDay) AS t_year, DATEPART(mm, t_actual.TimeByDay) AS t_month, 
                      SUM(t_actual.ActualWorkBillable + t_actual.ActualWorkNonBillable + t_actual.ActualOvertimeWorkBillable + t_actual.ActualOvertimeWorkNonBillable) 
                      AS non_working
FROM         MSP_TimesheetActual AS t_actual INNER JOIN
                      MSP_TimesheetLine AS t_line ON t_actual.TimesheetLineUID = t_line.TimesheetLineUID INNER JOIN
                      MSP_TimesheetClass AS t_class ON t_line.ClassUID = t_class.ClassUID INNER JOIN
                      MSP_TimesheetProject AS t_proj ON t_line.ProjectNameUID = t_proj.ProjectNameUID INNER JOIN
                      MSP_TimesheetResource AS t_res ON t_actual.LastChangedResourceNameUID = t_res.ResourceNameUID
WHERE     (t_class.Type = 1)
GROUP BY t_res.ResourceUID, DATEPART(yyyy, t_actual.TimeByDay), DATEPART(mm, t_actual.TimeByDay), t_class.Type, CASE WHEN (t_class.Type = 1) 
                      THEN 'Non-Wokring' ELSE t_class.ClassName END
ORDER BY resUID, t_year, t_month