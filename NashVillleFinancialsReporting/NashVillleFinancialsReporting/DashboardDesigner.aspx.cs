using DevExpress.DashboardCommon;
using DevExpress.DashboardWeb;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class DashboardDesigner : System.Web.UI.Page
{
    private static string ConnectionStringName = "LeadGenerationConnection";
    protected void Page_Load(object sender, EventArgs e)
    {
        //ASPxDashboard1.SetConnectionStringsProvider(new DevExpress.DataAccess.Web.ConfigFileConnectionStringsProvider());
        //throw new Exception("Disabled in code for Security Reasons");
        //return;
        DataSourceInMemoryStorage dataSourceStorage = new DataSourceInMemoryStorage();

        // Registers an Object data source.            
        DashboardObjectDataSource objDataSource = new DashboardObjectDataSource("Object Data Source");
        
        dataSourceStorage.RegisterDataSource("objectDataSource", objDataSource.SaveToXml());
        ASPxDashboard1.SetDataSourceStorage(dataSourceStorage);
        ASPxDashboard1.SetDashboardStorage(new CustomDashBoardStorage(ConnectionStringName));
        

    }



    protected void ASPxDashboard1_ConfigureItemDataCalculation1(object sender, ConfigureItemDataCalculationWebEventArgs e)
    {
        e.CalculateAllTotals = true;
    }

    protected void ASPxDashboard1_DataLoading(object sender, DataLoadingWebEventArgs e)
    {
        
        var watch = new Stopwatch();
        watch.Start();
        var fromDateParam = e.Parameters.Where(x => x.Name == "FromDate").FirstOrDefault();
        var toDateParam = e.Parameters.Where(x => x.Name == "ToDate").FirstOrDefault();
        if (fromDateParam == null || toDateParam == null)
        {
            return;
        }
        var fromDate = Convert.ToDateTime(fromDateParam.Value);
        var toDate = Convert.ToDateTime(toDateParam.Value);

        using (SqlConnection cn = new SqlConnection(GetConnectionString()))
        {
            string query = "select State,County,CampaignNumber,ClosingDate,ISNULL(CallInDateTime,'" + fromDate+ "') as CallInDateTime,LastAction,AgentEmail from vwLeadRecordsReport with (nolock)  where((ClosingDate >= '" + fromDate + "') and(ClosingDate <= '" + toDate + "')) ";
            
            using (SqlCommand cmd = new SqlCommand(query, cn))
            {
                cmd.CommandTimeout = 0;
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {

                    var dt = new DataTable();

                    da.Fill(dt);
                    watch.Stop();
               
                    e.Data = dt;
                    

                }
            }
        }
    }

    private string GetConnectionString()
    {
        var cs = ConfigurationManager.ConnectionStrings["LeadGenerationConnection"].ConnectionString;
        cs = cs.Substring(cs.IndexOf("Server="));
        return cs;
    }
}