using DevExpress.DashboardCommon;
using DevExpress.DashboardWeb;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Xml.Linq;

public class CustomDashBoardStorage : IEditableDashboardStorage
{
    private string ConnectionName { get; set; }
    public CustomDashBoardStorage(string connectionName)
    {
        this.ConnectionName = connectionName;
    }
    private string GetConnectionString()
    {
        var cs = ConfigurationManager.ConnectionStrings[ConnectionName].ConnectionString;
        cs = cs.Substring(cs.IndexOf("Server="));
        return cs;
    }
    public string AddDashboard(XDocument dashboard, string dashboardName)
    {
        using (SqlConnection cn = new SqlConnection(GetConnectionString()))
        {
            cn.Open();
            string query = @"INSERT INTO DashboardInfo(Name,XMLData) OUTPUT INSERTED.ID VALUES(@Name, @XMLData)";
            using (SqlCommand cmd = new SqlCommand(query,cn))
            {
                cmd.Parameters.AddWithValue("@Name", dashboardName);
                cmd.Parameters.AddWithValue("@XMLData", dashboard.ToString());
                return cmd.ExecuteScalar().ToString();
            }
        }
    }

    public IEnumerable<DashboardInfo> GetAvailableDashboardsInfo()
    {
      
        
        using (SqlConnection cn = new SqlConnection(GetConnectionString()))
        {
            string query = "select * from DashboardInfo";
            using (SqlDataAdapter da = new SqlDataAdapter(query,cn))
            {
                var dt = new DataTable();
                da.Fill(dt);
                foreach (DataRow row in dt.Rows)
                {
                    yield return new DashboardInfo() { ID = Convert.ToString(row["ID"]), Name = Convert.ToString(row["Name"]) };
                }
            }
         
        }
    }

    public XDocument LoadDashboard(string dashboardID)
    {
        using (SqlConnection cn = new SqlConnection(GetConnectionString()))
        {
            cn.Open();
            string query = "select XMLData from DashboardInfo where ID = @ID";
            using (SqlCommand cmd = new SqlCommand(query, cn))
            {
                cmd.Parameters.AddWithValue("@ID", dashboardID);
                Dashboard dashboard = new Dashboard();
                dashboard.LoadFromXDocument(XDocument.Parse(cmd.ExecuteScalar().ToString()));
                foreach (var dataSource in dashboard.DataSources)
                {
                    if (dataSource is DashboardSqlDataSource)
                        dataSource.DataProcessingMode = DevExpress.DashboardCommon.DataProcessingMode.Client;
                }
                return dashboard.SaveToXDocument();
                 
            }

        }
    }

    public void SaveDashboard(string dashboardID, XDocument dashboard)
    {
        using (SqlConnection cn = new SqlConnection(GetConnectionString()))
        {
            cn.Open();
            string query = @"update DashboardInfo set XMLData =  @XMLData where ID = @ID";
            using (SqlCommand cmd = new SqlCommand(query, cn))
            {
                cmd.Parameters.AddWithValue("@ID", dashboardID);
                cmd.Parameters.AddWithValue("@XMLData", dashboard.ToString());
                cmd.ExecuteNonQuery();
            }
        }
    }
}