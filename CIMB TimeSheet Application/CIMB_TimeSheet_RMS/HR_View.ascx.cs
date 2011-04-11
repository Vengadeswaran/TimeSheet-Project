using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;

namespace CIMB_TimeSheet_RMS
{
    public partial class HR_View : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                EntiyList.DataSource = entityds();
                EntiyList.DataValueField = "ID";
                EntiyList.DataTextField = "Entity";
                EntiyList.DataBind();
                EntiyList.Items.Insert(0, "Select an Entity");
            }
        }

        protected void deptdatafilter()
        {
            if (EntiyList.SelectedValue != null && EntiyList.SelectedIndex != 0)
            {
                DeptList.Enabled = true;
                Edit_Entity_btn.Enabled = true;
                DeptList.DataSource = deptds(Convert.ToInt16(EntiyList.SelectedValue.ToString()));
                DeptList.DataValueField = "ID";
                DeptList.DataTextField = "Department Name";
                DeptList.DataBind();
                DeptList.Items.Insert(0, "Select the Department");
            }
            else
            {
                DeptList.Enabled = false;
                Edit_Entity_btn.Enabled = false;
            }
        }

        protected void resdetailsdatafilter()
        {
            if (DeptList.SelectedValue != null && DeptList.SelectedIndex != 0)
            {
                ResDetails.Enabled = true;
                ResDetails.DataSource = resdetailds(Convert.ToInt32(DeptList.SelectedValue.ToString()));
                ResDetails.DataBind();
            }
        }

        protected void secondmentdatafilter()
        {
            if (ResDetails.SelectedValue != null)
            {
                ResSecondment.Enabled = true;
                ResSecondment.DataSource = resSecondmentds(Convert.ToInt32(ResDetails.SelectedValue.ToString()));
                ResSecondment.DataBind();
            }
        }

        protected SPDataSource entityds()
        {
            using (SPSite site = new SPSite(MyConfiguration.GetSiteURL(SPContext.Current)))
            {
                SPWeb hrWeb = site.AllWebs["HRMV02"];
                SPDataSource entityds = new SPDataSource();
                entityds.List = hrWeb.Lists["Entity"];
                return entityds;
            }
        }

        protected SPDataSource deptds(int deptid)
        {
            using (SPSite site = new SPSite(MyConfiguration.GetSiteURL(SPContext.Current)))
            {
                SPWeb hrWeb = site.AllWebs["HRMV02"];
                SPDataSource deptds = new SPDataSource();
                deptds.SelectCommand = @"<Query>
                                            <Where>
                                                <Eq>
                                                    <FieldRef Name='Entity' LookupId='True'/>
                                                    <Value Type='Lookup'>" + deptid + @"</Value>
                                                </Eq>
                                            </Where>
                                        </Query>";
                deptds.List = hrWeb.Lists["Department"];
                return deptds;
            }
        }

        protected SPDataSource resdetailds(int resfilterid)
        {
            using (SPSite site = new SPSite(MyConfiguration.GetSiteURL(SPContext.Current)))
            {
                SPWeb hrWeb = site.AllWebs["HRMV02"];
                SPDataSource resdetds = new SPDataSource();
                resdetds.SelectCommand = @"<Query>
                                            <Where>
                                                <Eq>
                                                    <FieldRef Name='Department' LookupId='True' />
                                                    <Value Type='Lookup'>" + resfilterid + @"</Value>
                                                </Eq>
                                            </Where>
                                        </Query>";
                resdetds.List = hrWeb.Lists["Resource Details"];
                return resdetds;
            }
        }

        protected SPDataSource resSecondmentds(int secondmentfilterid)
        {
            using (SPSite site = new SPSite(MyConfiguration.GetSiteURL(SPContext.Current)))
            {
                SPWeb hrWeb = site.AllWebs["HRMV02"];
                SPDataSource resSecondmentds = new SPDataSource();
                resSecondmentds.SelectCommand = @"<Query>
                                            <Where>
                                                <Eq>
                                                    <FieldRef Name='Resource_x0020_Name' LookupId='True' />
                                                    <Value Type='Lookup'>" + secondmentfilterid + @"</Value>
                                                </Eq>
                                            </Where>
                                        </Query>";
                resSecondmentds.List = hrWeb.Lists["Resource Secondment"];
                return resSecondmentds;
            }
        }

        protected void filterDept(object sender, EventArgs e)
        {
            deptdatafilter();
        }

        protected void filterRes(object sender, EventArgs e)
        {
            resdetailsdatafilter();
        }

        protected void filterSecondment(object sender, EventArgs e)
        {
            secondmentdatafilter();
        }

        protected void Edit_Entity_btn_Click(object sender, EventArgs e)
        {
        }
    }
}

#region Un Used Code

//DataTable entity_Dt = new DataTable();
//entity_Dt.Columns.Add("ID");
//entity_Dt.Columns.Add("Title");
//foreach (SPListItem en_item in entity)
//{
//    DataRow row = entity_Dt.NewRow();
//    row["ID"] = en_item["ID"].ToString();
//    row["Title"] = en_item["Title"].ToString();
//    entity_Dt.Rows.Add(row);
//}
//EntiyList.DataSource = entity_Dt;
//EntiyList.DataTextField = entity_Dt.Columns[1].ToString();
//EntiyList.DataValueField = entity_Dt.Columns[0].ToString();
//EntiyList.DataBind();

#endregion Un Used Code