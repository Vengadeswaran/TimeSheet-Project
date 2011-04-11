SELECT     uv_res.ResourceName, ISNULL(uv_proj.ProjectName, (CASE WHEN (t_class.Type = 1) THEN 'Non-Wokring' ELSE t_class.ClassName END)) 
                      AS ProjectName, DATEPART(yyyy, t_actual.TimeByDay) AS t_Year, DATEPART(mm, t_actual.TimeByDay) AS t_Month, t_class.Type, 
                      CASE WHEN (t_class.Type = 1) THEN 'Non-Wokring' ELSE t_class.ClassName END AS ClassName, 
                      SUM(t_actual.ActualWorkBillable + t_actual.ActualWorkNonBillable + t_actual.ActualOvertimeWorkBillable + t_actual.ActualOvertimeWorkNonBillable) 
                      AS [Clocked Time], 
                      ROUND(CASE WHEN (SUM(t_actual.ActualWorkBillable + t_actual.ActualWorkNonBillable + t_actual.ActualOvertimeWorkBillable + t_actual.ActualOvertimeWorkNonBillable))
                       = 0 THEN 0 ELSE (SUM(t_actual.ActualWorkBillable + t_actual.ActualWorkNonBillable + t_actual.ActualOvertimeWorkBillable + t_actual.ActualOvertimeWorkNonBillable)
                       / MAX(total_time.[Total Time]) * 100) END, 2) AS pct, t_res.ResourceUID, t_proj.ProjectName AS Expr1, uv_proj.Project_RC_Code, 
                      uv_res.ResourceNTAccount
FROM         MSP_TimesheetResource AS t_res INNER JOIN
                      MSP_TimesheetActual AS t_actual ON t_res.ResourceNameUID = t_actual.LastChangedResourceNameUID INNER JOIN
                      MSP_TimesheetLine AS t_line ON t_actual.TimesheetLineUID = t_line.TimesheetLineUID INNER JOIN
                      MSP_TimesheetProject AS t_proj ON t_line.ProjectNameUID = t_proj.ProjectNameUID INNER JOIN
                      MSP_EpmResource_UserView AS uv_res ON t_res.ResourceUID = uv_res.ResourceUID INNER JOIN
                          (SELECT     t_total_res.ResourceUID AS resUID, DATEPART(yyyy, t_total_actual.TimeByDay) AS total_year, DATEPART(mm, 
                                                   t_total_actual.TimeByDay) AS total_month, 
                                                   SUM(t_total_actual.ActualWorkBillable + t_total_actual.ActualWorkNonBillable + t_total_actual.ActualOvertimeWorkBillable + t_total_actual.ActualOvertimeWorkNonBillable)
                                                    AS [Total Time]
                            FROM          MSP_TimesheetActual AS t_total_actual INNER JOIN
                                                   MSP_TimesheetResource AS t_total_res ON t_total_actual.LastChangedResourceNameUID = t_total_res.ResourceNameUID INNER JOIN
                                                   MSP_EpmResource_UserView AS uv_total_res ON t_total_res.ResourceUID = uv_total_res.ResourceUID
                            GROUP BY DATEPART(yyyy, t_total_actual.TimeByDay), DATEPART(mm, t_total_actual.TimeByDay), t_total_res.ResourceUID) AS total_time ON 
                      t_res.ResourceUID = total_time.resUID AND DATEPART(yyyy, t_actual.TimeByDay) = total_time.total_year AND DATEPART(mm, t_actual.TimeByDay) 
                      = total_time.total_month LEFT OUTER JOIN
                      MSP_TimesheetClass AS t_class ON t_line.ClassUID = t_class.ClassUID LEFT OUTER JOIN
                      MSP_EpmProject_UserView AS uv_proj ON t_proj.ProjectUID = uv_proj.ProjectUID
GROUP BY uv_res.ResourceName, DATEPART(yyyy, t_actual.TimeByDay), DATEPART(mm, t_actual.TimeByDay), t_class.Type, ISNULL(uv_proj.ProjectName, 
                      (CASE WHEN (t_class.Type = 1) THEN 'Non-Wokring' ELSE t_class.ClassName END)), CASE WHEN (t_class.Type = 1) 
                      THEN 'Non-Wokring' ELSE t_class.ClassName END, t_res.ResourceUID, t_proj.ProjectName, uv_proj.Project_RC_Code, 
                      uv_res.ResourceNTAccount
ORDER BY uv_res.ResourceName, t_Year, t_Month