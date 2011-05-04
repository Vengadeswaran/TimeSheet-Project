SELECT     MemberFullValue
FROM         MSPLT_Project_RC_Code_UserView
WHERE     (ParentLookupMemberUID IS NOT NULL)
ORDER BY CAST(MemberFullValue AS varchar(500))