﻿using DevExpress.DashboardCommon;
using DevExpress.DashboardWeb;
using DevExpress.DataAccess.Web;
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
    private static string CONFIG_CS = "LeadGenerationConnection";
    private static string ConnectionString = "LeadGenerationConnection";
    protected void Page_Load(object sender, EventArgs e)
    {
        DashboardData.ConfigureDashboard(ASPxDashboard1, CONFIG_CS);
    }

    protected void ASPxDashboard1_ConfigureItemDataCalculation1(object sender, ConfigureItemDataCalculationWebEventArgs e)
    {
        e.CalculateAllTotals = true;
    }

    protected void ASPxDashboard1_DataLoading(object sender, DataLoadingWebEventArgs e)
    {
        DashboardData.LoadData(e, CONFIG_CS, ConnectionString);
    }




}