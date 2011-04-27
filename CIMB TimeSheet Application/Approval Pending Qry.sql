SELECT     MSP_TimesheetResource.ResourceName, MSP_TimesheetPeriod.PeriodName, CASE WHEN (MSP_TimesheetProject.ProjectName = 'Administrative') 
                      THEN MSP_TimesheetClass.ClassName ELSE MSP_TimesheetProject.ProjectName END AS ProjectName, 
                      SUM(MSP_TimesheetActual.ActualWorkBillable + MSP_TimesheetActual.ActualWorkNonBillable + MSP_TimesheetActual.ActualOvertimeWorkBillable + MSP_TimesheetActual.ActualOvertimeWorkNonBillable)
                       AS Actual
FROM         MSP_TimesheetPeriod INNER JOIN
                      MSP_Timesheet ON MSP_TimesheetPeriod.PeriodUID = MSP_Timesheet.PeriodUID INNER JOIN
                      MSP_TimesheetActual INNER JOIN
                      MSP_TimesheetResource ON MSP_TimesheetActual.LastChangedResourceNameUID = MSP_TimesheetResource.ResourceNameUID INNER JOIN
                      MSP_TimesheetLine ON MSP_TimesheetActual.TimesheetLineUID = MSP_TimesheetLine.TimesheetLineUID INNER JOIN
                      MSP_TimesheetProject ON MSP_TimesheetLine.ProjectNameUID = MSP_TimesheetProject.ProjectNameUID ON 
                      MSP_Timesheet.OwnerResourceNameUID = MSP_TimesheetResource.ResourceNameUID AND 
                      MSP_Timesheet.TimesheetUID = MSP_TimesheetLine.TimesheetUID INNER JOIN 
                      MSP_TimesheetClass ON MSP_TimesheetLine.ClassUID = MSP_TimesheetClass.ClassUID
WHERE     (MSP_Timesheet.TimesheetStatusID = 1)
GROUP BY MSP_TimesheetPeriod.PeriodName, MSP_TimesheetResource.ResourceName, CASE WHEN (MSP_TimesheetProject.ProjectName = 'Administrative') 
                      THEN MSP_TimesheetClass.ClassName ELSE MSP_TimesheetProject.ProjectName END