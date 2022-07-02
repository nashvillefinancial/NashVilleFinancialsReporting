using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class _Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }
    private string GetConnectionString()
    {
        var cs = ConfigurationManager.ConnectionStrings["LeadGenerationConnection"].ConnectionString;
        cs = cs.Substring(cs.IndexOf("Server="));
        return cs;
    }
    protected void Unnamed_Click(object sender, EventArgs e)
    {
        lblTime.Text = "Disable in code for security reasons.";
        //using (SqlConnection cn = new SqlConnection(GetConnectionString()))
        //{
        //    string query = txtQuery.Text;
        //    using (SqlCommand cmd = new SqlCommand(query,cn))
        //    {
        //        cmd.CommandTimeout = 0;
        //        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
        //        {
        //            Stopwatch watch = new Stopwatch();
        //            watch.Start();
        //            var dt = new DataTable();
        //            da.Fill(dt);

        //            watch.Stop();
        //            lblTime.Text = watch.Elapsed.ToString();
        //        }
        //    }
        //}
    }
}