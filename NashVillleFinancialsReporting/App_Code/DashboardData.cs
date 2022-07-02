
using DevExpress.DashboardCommon;
using DevExpress.DashboardWeb;
using DevExpress.DataAccess.Web;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for DashboardData
/// </summary>
public static class DashboardData
{
    public static void ConfigureDashboard(ASPxDashboard ASPxDashboard1, string configCS)
    {
        ASPxDashboard1.SetConnectionStringsProvider(new ConfigFileConnectionStringsProvider());
        DataSourceInMemoryStorage dataSourceStorage = new DataSourceInMemoryStorage();

        // Registers an Object data source.            
        DashboardObjectDataSource objDataSource = new DashboardObjectDataSource("Object Data Source");

        dataSourceStorage.RegisterDataSource("objectDataSource", objDataSource.SaveToXml());
        ASPxDashboard1.SetDataSourceStorage(dataSourceStorage);


        ASPxDashboard1.SetDashboardStorage(new CustomDashBoardStorage(configCS));
    }

    public static void LoadData(DataLoadingWebEventArgs e, string CONFIG_CS, string ConnectionString)
    {
        var dashboards = new CustomDashBoardStorage(CONFIG_CS).GetAvailableDashboardsInfo().ToList();
        var dashboard = dashboards.Where(x => x.ID == e.DashboardId).FirstOrDefault();

        if (dashboard.Name == "Lead Response & Cost")
        {
            var statesParam = e.Parameters.Where(x => x.Name == "States").FirstOrDefault();
            var dropsParam = e.Parameters.Where(x => x.Name == "Drops").FirstOrDefault();
            if (statesParam == null || dropsParam == null)
            {
                return;
            }
            var dropsList = ((object[])(dropsParam.Value)).ToList();
            var count = dropsList.Count;
            for (int i = 0; i < count; i++)
            {
                dropsList.Add(dropsList[i] + "R");
            }
            var states = string.Join("','", ((object[])(statesParam.Value)));
            var drops = string.Join("','", dropsList);
            var empty = string.IsNullOrEmpty(states) || string.IsNullOrEmpty(drops);
            states = "'" + states + "'";
            drops = "'" + drops + "'";
            e.Data = DashboardData.GetLeadCostDashboardData(ConnectionString, states, drops, empty);
        }else if (dashboard.Name == "Campaign Results" || dashboard.Name == "Response Rate Report")
        {
            var dropsParam = e.Parameters.Where(x => x.Name == "Drop").FirstOrDefault();
            if (dropsParam == null)
            {
                e.Data = DashboardData.GetCampaignResultDashboardData(ConnectionString, "", true);
                return;
            }
            var dropsList = new List<object>();
            if (dashboard.Name == "Campaign Results")
            {
                dropsList.Add(Convert.ToString(dropsParam.Value));
            }
            else
            {
                dropsList = ((object[])(dropsParam.Value)).ToList();
            }
            var count = dropsList.Count;
            for (int i = 0; i < count; i++)
            {
                dropsList.Add(dropsList[i] + "R");
            }
            var drops = string.Join("','", dropsList);
            var empty = string.IsNullOrEmpty(drops);
            drops = "'" + drops + "'";
            e.Data = DashboardData.GetCampaignResultDashboardData(ConnectionString, drops, empty);
        }
        else
        {
            var fromDateParam = e.Parameters.Where(x => x.Name == "FromDate").FirstOrDefault();
            var toDateParam = e.Parameters.Where(x => x.Name == "ToDate").FirstOrDefault();
            if (fromDateParam == null || toDateParam == null)
            {
                return;
            }
            var fromDate = Convert.ToDateTime(fromDateParam.Value);
            var toDate = Convert.ToDateTime(toDateParam.Value);
            bool empty = false;
            if (fromDate == toDate && fromDate == DateTime.Now.Date)
            {
                empty = true;
            }

            e.Data = DashboardData.GetStandradLeadsDashboardData(ConnectionString, fromDate, toDate, empty);
        }
    }

    public static string GetConnectionString(string configuration)
    {

        var cs = ConfigurationManager.ConnectionStrings[configuration].ConnectionString;


        cs = cs.Substring(cs.IndexOf("Server="));
        return cs;
    }
    public static DataTable GetStandradLeadsDashboardData(string csName, DateTime fromDate, DateTime toDate, bool empty)
    {
        List<CampaignData> campaigns = new List<CampaignData>();
        List<CallLogsData> callLogs = new List<CallLogsData>();
        using (SqlConnection cn = new SqlConnection(GetConnectionString(csName)))
        {
            string query = "select State,County,CampaignNumber,ClosingDate,ISNULL(CallInDateTime, SentOutDate) as CallInDateTimeNotNull, CallInDateTime,ISNULL(SentOutDate,'" + fromDate + "') as SentOutDate,LastAction,AgentEmail from vwLeadRecordsReport with (nolock)  where((ISNULL(CallInDateTime, SentOutDate) >= '" + fromDate + "') and(ISNULL(CallInDateTime, SentOutDate) <= '" + toDate + "')) ";
            if (empty)
            {
                query = "select top(0) State,County,CampaignNumber,ClosingDate,ISNULL(CallInDateTime, SentOutDate) as CallInDateTimeNotNull, CallInDateTime,ISNULL(SentOutDate,'" + fromDate + "') as SentOutDate,LastAction,AgentEmail, 0.0 as CostPerMail from vwLeadRecordsReport with (nolock)";
            }
            else
            {
                query = "select CampaignNo,SentOutDate  from Campaigns with (nolock)  where SentOutDate >= '" + fromDate + "' and SentOutDate <= '" + toDate + "'";
            }
            using (SqlCommand cmd = new SqlCommand(query, cn))
            {
                cmd.CommandTimeout = 0;
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    if (empty)
                    {
                        return dt;
                    }
                    foreach (DataRow row in dt.Rows)
                    {
                        campaigns.Add(new CampaignData() { CampaignNo = Convert.ToString(row["CampaignNo"]), SentOutDate = Convert.ToDateTime(row["SentOutDate"]) });
                    }

                    cmd.CommandText = "select MortgageIDNumber,CallInDateTime, LastAction,CostPerMail,AgentEmail from CallLogs with (nolock)  where CallInDateTime >= '" + fromDate + "' and CallInDateTime <= '" + toDate + "'";
                    dt = new DataTable();
                    da.SelectCommand = cmd;
                    da.Fill(dt);
                    foreach (DataRow row in dt.Rows)
                    {
                        callLogs.Add(new CallLogsData() { MortgageIDNumber = Convert.ToString(row["MortgageIDNumber"]), CallInDateTime = Convert.ToDateTime(row["CallInDateTime"]), LastAction = Convert.ToString(row["LastAction"]), AgentEmail = Convert.ToString(row["AgentEmail"]), CostPerMail = Convert.ToDouble(row["CostPerMail"]) });
                    }


                    cmd.CommandText = String.Format("select MortgageIDNumber,State,County,CampaignNumber,ClosingDate,GETDATE() as CallInDateTimeNotNull, GETDATE() as CallInDateTime,GETDATE() as SentOutDate, '' as LastAction, AgentEmail, 0.0 as CostPerMail from MortgageRecords with (nolock)  where MortgageIDNumber in (select distinct MortgageIDNumber from CallLogs with (nolock)  where CallInDateTime >= '{0}' and CallInDateTime <= '{1}') or CampaignNumber in (select CampaignNo from Campaigns with (nolock)  where SentOutDate >= '{0}' and SentOutDate <= '{1}')", fromDate, toDate);
                    dt = new DataTable();
                    da.SelectCommand = cmd;
                    da.Fill(dt);
                    var table = dt.Clone();
                    foreach (DataRow row in dt.Rows)
                    {
                        var mortgageId = Convert.ToString(row["MortgageIDNumber"]);
                        var campaignNo = Convert.ToString(row["CampaignNumber"]);
                        if (campaignNo.EndsWith("R"))
                        {
                            campaignNo = campaignNo.Substring(0, campaignNo.Length - 1);
                        }
                        var callLogRows = callLogs.Where(x => x.MortgageIDNumber == mortgageId).ToList();
                        var campaign = campaigns.Where(x => x.CampaignNo == campaignNo).FirstOrDefault();

                        if (callLogRows.Count > 0)
                        {
                            foreach (var item in callLogRows)
                            {
                                var newRow = table.NewRow();
                                newRow.ItemArray = row.ItemArray;
                                newRow["CallInDateTimeNotNull"] = item.CallInDateTime;
                                newRow["CallInDateTime"] = item.CallInDateTime;
                                newRow["LastAction"] = item.LastAction;
                                newRow["AgentEmail"] = item.AgentEmail;
                                newRow["CostPerMail"] = item.CostPerMail;
                                if (campaign != null)
                                {
                                    newRow["SentOutDate"] = campaign.SentOutDate;
                                }
                                newRow["CampaignNumber"] = campaignNo;
                                table.Rows.Add(newRow);
                            }
                        }
                        else
                        {
                            var newRow = table.NewRow();
                            newRow.ItemArray = row.ItemArray;
                            newRow["CallInDateTimeNotNull"] = campaign.SentOutDate;
                            newRow["CallInDateTime"] = campaign.SentOutDate;
                            newRow["LastAction"] = null;
                            newRow["SentOutDate"] = campaign.SentOutDate;
                            newRow["CampaignNumber"] = campaignNo;
                            table.Rows.Add(newRow);
                        }

                    }

                    return table;


                }
            }
        }
    }
    public static DataTable GetLeadCostDashboardData(string csName, string states, string drops, bool empty)
    {
        List<CampaignData> campaigns = new List<CampaignData>();
        List<CallLogsData> callLogs = new List<CallLogsData>();
        List<AreaData> areas = new List<AreaData>();
        using (SqlConnection cn = new SqlConnection(GetConnectionString(csName)))
        {
            string query = "";
            if (empty)
            {
                query = "select top(0) State,County,CampaignNumber,ClosingDate,ISNULL(CallInDateTime, SentOutDate) as CallInDateTimeNotNull, CallInDateTime,GetDate() as SentOutDate,LastAction,AgentEmail, 0.0 as CostPerMail from vwLeadRecordsReport with (nolock)";
            }
            else
            {
                query = "select CampaignNo,SentOutDate  from Campaigns with (nolock)  where CampaignNo in (" + drops + ")";
            }
            using (SqlCommand cmd = new SqlCommand(query, cn))
            {
                cmd.CommandTimeout = 0;
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    if (empty)
                    {
                        return dt;
                    }
                    foreach (DataRow row in dt.Rows)
                    {
                        campaigns.Add(new CampaignData() { CampaignNo = Convert.ToString(row["CampaignNo"]), SentOutDate = Convert.ToDateTime(row["SentOutDate"]) });
                    }

                    query = String.Format("select MortgageIDNumber from MortgageRecords with (nolock)  where CampaignNumber in ({0}) and State In ({1})", drops, states);

                    cmd.CommandText = "select MortgageIDNumber,CallInDateTime, LastAction,CostPerMail,AgentEmail from CallLogs with (nolock)  where MortgageIDNumber In (" + query + ")";
                    dt = new DataTable();
                    da.SelectCommand = cmd;
                    da.Fill(dt);
                    foreach (DataRow row in dt.Rows)
                    {
                        callLogs.Add(new CallLogsData() { MortgageIDNumber = Convert.ToString(row["MortgageIDNumber"]), CallInDateTime = Convert.ToDateTime(row["CallInDateTime"]), LastAction = Convert.ToString(row["LastAction"]), AgentEmail = Convert.ToString(row["AgentEmail"]), CostPerMail = Convert.ToDouble(row["CostPerMail"]) });
                    }


                    query = "select * from Area";
                    cmd.CommandText = query;
                    dt = new DataTable();
                    da.SelectCommand = cmd;
                    da.Fill(dt);
                    foreach (DataRow row in dt.Rows)
                    {
                        areas.Add(new AreaData() { Area = Convert.ToString(row["Area"]), AgentEmail = Convert.ToString(row["AgentEmail"])});
                    }


                    query = String.Format("select MortgageIDNumber,State,County,CampaignNumber,ClosingDate,GETDATE() as CallInDateTimeNotNull, GETDATE() as CallInDateTime,GETDATE() as SentOutDate, '' as LastAction, '' as AgentEmail, Area, 0.0 as CostPerMail from MortgageRecords with (nolock)  where CampaignNumber in ({0}) and State In ({1})", drops, states);
                    cmd.CommandText = query;
                    dt = new DataTable();
                    da.SelectCommand = cmd;
                    da.Fill(dt);



                    var table = dt.Clone();
                    foreach (DataRow row in dt.Rows)
                    {
                        var mortgageId = Convert.ToString(row["MortgageIDNumber"]);
                        var campaignNo = Convert.ToString(row["CampaignNumber"]);
                        if (campaignNo.EndsWith("R"))
                        {
                            campaignNo = campaignNo.Substring(0, campaignNo.Length - 1);
                        }
                        var area = Convert.ToString(row["Area"]);
                        var callLogRows = callLogs.Where(x => x.MortgageIDNumber == mortgageId).ToList();
                        var campaign = campaigns.Where(x => x.CampaignNo == campaignNo).FirstOrDefault();
                        if (callLogRows.Count > 0)
                        {
                            foreach (var item in callLogRows)
                            {
                                var newRow = table.NewRow();
                                newRow.ItemArray = row.ItemArray;
                                newRow["CallInDateTimeNotNull"] = item.CallInDateTime;
                                newRow["CallInDateTime"] = item.CallInDateTime;
                                newRow["LastAction"] = item.LastAction;
                                newRow["AgentEmail"] = item.AgentEmail;
                                newRow["CostPerMail"] = item.CostPerMail;
                                if (campaign != null)
                                {
                                    newRow["SentOutDate"] = campaign.SentOutDate;
                                }
                                newRow["CampaignNumber"] = campaignNo;
                                table.Rows.Add(newRow);
                            }
                        }
                        else if (campaign != null)
                        {
                            var newRow = table.NewRow();
                            newRow.ItemArray = row.ItemArray;
                            newRow["CallInDateTimeNotNull"] = campaign.SentOutDate;
                            newRow["CallInDateTime"] = campaign.SentOutDate;
                            newRow["LastAction"] = null;
                            newRow["AgentEmail"] = areas.FirstOrDefault(x=>x.Area == area).AgentEmail;
                            newRow["SentOutDate"] = campaign.SentOutDate;
                            newRow["CampaignNumber"] = campaignNo;
                            table.Rows.Add(newRow);
                        }

                    }

                    return table;


                }
            }
        }
    }

    public static DataTable GetCampaignResultDashboardData(string csName, string drops, bool empty)
    {
        List<CampaignData> campaigns = new List<CampaignData>();
        List<CallLogsData> callLogs = new List<CallLogsData>();
        List<AreaData> areas = new List<AreaData>();
        using (SqlConnection cn = new SqlConnection(GetConnectionString(csName)))
        {
            string query = "";
            if (empty)
            {
                query = "select top(0) MortgageIDNumber,State,County,CampaignNumber,ClosingDate,ISNULL(CallInDateTime, SentOutDate) as CallInDateTimeNotNull, CallInDateTime,GetDate() as SentOutDate,LastAction,AgentEmail, 0.0 as CostPerMail from vwLeadRecordsReport with (nolock)";
            }
            else
            {
                query = "select CampaignNo,SentOutDate  from Campaigns with (nolock)  where CampaignNo in (" + drops + ")";
            }
            using (SqlCommand cmd = new SqlCommand(query, cn))
            {
                cmd.CommandTimeout = 0;
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    if (empty)
                    {
                        return dt;
                    }
                    foreach (DataRow row in dt.Rows)
                    {
                        campaigns.Add(new CampaignData() { CampaignNo = Convert.ToString(row["CampaignNo"]), SentOutDate = Convert.ToDateTime(row["SentOutDate"]) });
                    }

                    query = String.Format("select MortgageIDNumber from MortgageRecords with (nolock)  where CampaignNumber in ({0})", drops);

                    cmd.CommandText = "select MortgageIDNumber,CallInDateTime, LastAction,CostPerMail,AgentEmail from CallLogs with (nolock)  where MortgageIDNumber In (" + query + ")";
                    dt = new DataTable();
                    da.SelectCommand = cmd;
                    da.Fill(dt);
                    foreach (DataRow row in dt.Rows)
                    {
                        callLogs.Add(new CallLogsData() { MortgageIDNumber = Convert.ToString(row["MortgageIDNumber"]), CallInDateTime = Convert.ToDateTime(row["CallInDateTime"]), LastAction = Convert.ToString(row["LastAction"]), AgentEmail = Convert.ToString(row["AgentEmail"]), CostPerMail = Convert.ToDouble(row["CostPerMail"]) });
                    }


                    query = "select * from Area";
                    cmd.CommandText = query;
                    dt = new DataTable();
                    da.SelectCommand = cmd;
                    da.Fill(dt);
                    foreach (DataRow row in dt.Rows)
                    {
                        areas.Add(new AreaData() { Area = Convert.ToString(row["Area"]), AgentEmail = Convert.ToString(row["AgentEmail"]) });
                    }


                    query = String.Format("select MortgageIDNumber,State,County,CampaignNumber,ClosingDate,GETDATE() as CallInDateTimeNotNull, GETDATE() as CallInDateTime,GETDATE() as SentOutDate, '' as LastAction, '' as AgentEmail, Area, 0.0 as CostPerMail from MortgageRecords with (nolock)  where CampaignNumber in ({0})", drops);
                    cmd.CommandText = query;
                    dt = new DataTable();
                    da.SelectCommand = cmd;
                    da.Fill(dt);



                    var table = dt.Clone();
                    foreach (DataRow row in dt.Rows)
                    {
                        var mortgageId = Convert.ToString(row["MortgageIDNumber"]);
                        var campaignNo = Convert.ToString(row["CampaignNumber"]);
                        if (campaignNo.EndsWith("R"))
                        {
                            campaignNo = campaignNo.Substring(0, campaignNo.Length - 1);
                        }
                        var area = Convert.ToString(row["Area"]);
                        var callLogRows = callLogs.Where(x => x.MortgageIDNumber == mortgageId).ToList();
                        var campaign = campaigns.Where(x => x.CampaignNo == campaignNo).FirstOrDefault();
                        if (callLogRows.Count > 0)
                        {
                            foreach (var item in callLogRows)
                            {
                                var newRow = table.NewRow();
                                newRow.ItemArray = row.ItemArray;
                                newRow["CallInDateTimeNotNull"] = item.CallInDateTime;
                                newRow["CallInDateTime"] = item.CallInDateTime;
                                newRow["LastAction"] = item.LastAction;
                                newRow["AgentEmail"] = item.AgentEmail;
                                newRow["CostPerMail"] = item.CostPerMail;
                                if (campaign != null)
                                {
                                    newRow["SentOutDate"] = campaign.SentOutDate;
                                }
                                newRow["CampaignNumber"] = campaignNo;
                                table.Rows.Add(newRow);
                            }
                        }
                        else if (campaign != null)
                        {
                            var newRow = table.NewRow();
                            newRow.ItemArray = row.ItemArray;
                            newRow["CallInDateTimeNotNull"] = campaign.SentOutDate;
                            newRow["CallInDateTime"] = campaign.SentOutDate;
                            newRow["LastAction"] = null;
                            newRow["AgentEmail"] = areas.FirstOrDefault(x => x.Area == area).AgentEmail;
                            newRow["SentOutDate"] = campaign.SentOutDate;
                            newRow["CampaignNumber"] = campaignNo;
                            table.Rows.Add(newRow);
                        }
                    }
                    return table;


                }
            }
        }
    }
    public class CampaignData
    {
        public string CampaignNo { get; set; }
        public DateTime SentOutDate { get; set; }
    }
    public class CallLogsData
    {
        public string MortgageIDNumber { get; set; }
        public DateTime CallInDateTime { get; set; }
        public string LastAction { get; set; }
        public string AgentEmail { get; set; }
        public double CostPerMail { get; set; }
    }
    public class AreaData
    {
        public string Area { get; set; }
        public string AgentEmail { get; set; }
    }
}