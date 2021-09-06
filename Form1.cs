using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Net.NetworkInformation;

namespace CAC_Branch_Location_Checker
{
    public partial class Form1 : Form
    {
        // Defining structure 
        public struct Feedback
        {

            // Declaring different data types 
            public string Company_name;
            public string RC_number;
            public string Originating_office;
            public string Date_submitted;
            public string Submitted_by;
            public string Application_status;
            public string Approving_officer_docupload;
            public string Approving_officer_crp;
            public string Auditing_officer;
            public string Last_date_of_query;
            public string Queried_by;
            public string Last_query_record;
            public string Query_status;
        }

        public static PhysicalAddress GetMacAddress()
        {
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                //Ethernet network interfaces
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet &&
                    nic.OperationalStatus == OperationalStatus.Up)
                {
                    return nic.GetPhysicalAddress();
                }
                //Wireless Network Interfaces
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 &&
                    nic.OperationalStatus == OperationalStatus.Up)
                {
                    return nic.GetPhysicalAddress();
                }
            }
            return null;
        }
        public Form1()
        {
            InitializeComponent();
        }

        private void searchBtn_Click(object sender, EventArgs e)
        {
            compName.Text = origState.Text = avCode.Text = regDate.Text = apprOfficer.Text = platform.Text = null;
            if (string.IsNullOrWhiteSpace(searchTxtBox.Text) ||
                searchTxtBox.Text == "Enter RC Number Here...")
            {
                MessageBox.Show("Empty entry detected in RC Number field.", "Entry Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else
            {
                if (classComboBox.Text == "Choose Company Type:")
                {
                    MessageBox.Show("Please select the type of Company from the Dropdown options listed.", "Entry Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                else
                {
                    SqlConnection conn_37 = new SqlConnection(Properties.Settings.Default.Conn_37);
                    try
                    {
                        conn_37.Open();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    finally
                    {
                        string Query = "SELECT TOP 1 C.APPROVED_NAME, C.AVAILABILITY_CODE, C.REGISTRATION_DATE, u.fullname, S.State  ";
                        Query += "FROM [CAC_38].[cac_prod].[dbo].[COMPANY] AS C INNER JOIN tblactivity as A ON ";
                        Query += "(C.AVAILABILITY_CODE COLLATE Latin1_General_CI_AS = A.avcode AND C.RC_NUMBER = @RC ";
                        Query += "AND (activity = 'Registration Approved') ) INNER JOIN tbldocument as D ON ";
                        Query += "(C.CLASSIFICATION_FK = @CLFC AND D.avcode = A.avcode AND (D.doctype ='Memorandum' OR D.doctype ='Identification' OR D.doctype = 'Registration Form' OR D.doctype ='Availability PrintOut' OR D.doctype ='Certificate' OR D.doctype ='Constitution' OR D.doctype = 'Stamp Duty' OR D.doctype = 'Declaration Forms' OR D.doctype = 'Consent')) ";
                        Query += "INNER JOIN tbldocuser as U ON (U.username = A.username) INNER JOIN tblStates AS S ON (D.scode = S.StateCode) ";
                        SqlCommand Command = new SqlCommand(Query, conn_37);
                        Command.Parameters.Add("@RC", SqlDbType.NVarChar);
                        Command.Parameters["@RC"].Value = searchTxtBox.Text;
                        int Classification = 0;
                        switch (classComboBox.Text)
                        {
                            case "Business Names":
                                {
                                    Classification = 1;
                                    break;
                                }
                            case "Limited Liability Company":
                                {
                                    Classification = 2;
                                    break;
                                }
                            case "Incorporated Trustees":
                                {
                                    Classification = 3;
                                    break;
                                }
                        }
                        Command.Parameters.Add("@CLFC", SqlDbType.BigInt);
                        Command.Parameters["@CLFC"].Value = Classification;
                        Command.CommandType = CommandType.Text;
                        SqlDataReader rd = Command.ExecuteReader();
                        if (rd.HasRows)//Meaning at least a row of data was found from the DB
                        {
                            while (rd.Read())
                            {
                                compName.Text = rd["APPROVED_NAME"].ToString();
                                regDate.Text = Convert.ToDateTime(rd["REGISTRATION_DATE"]).ToString();
                                avCode.Text = rd["AVAILABILITY_CODE"].ToString();
                                origState.Text = rd["State"].ToString();
                                apprOfficer.Text = rd["fullname"].ToString();
                                platform.Text = "Company Registration Portal (CRP)";
                            }
                            rd.Close();
                            conn_37.Close();
                            conn_37.Dispose();
                        }
                        else//Check Old Database For Details
                        {
                            rd.Close();
                            conn_37.Close();
                            conn_37.Dispose();
                            //Now Open 144 Server
                            SqlConnection conn_144 = new SqlConnection(Properties.Settings.Default.Conn_144);
                            try
                            {
                                conn_144.Open();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                            finally
                            {
                                string Query_ = "SELECT C.NAME, CRP.REGISTRATION_DATE, CRP.AVAILABILITY_CODE, AP.OFFICER, ";
                                Query_ += "AP.LOCATION FROM _CP_DOC_COMP_REG AS C INNER JOIN ";
                                Query_ += "[CAC_18].[cac_prod].[dbo].[COMPANY] AS CRP ";
                                Query_ += "ON((C.REG_NUMBER COLLATE Latin1_General_CI_AS = CRP.RC_NUMBER) ";
                                Query_ += "AND(C.REG_NUMBER = @RC AND C.TYPE_CODE = @CTYPE)  )";
                                Query_ += "INNER JOIN _CP_APPRV_STAFF AS AP ON (C.CHANGED_BY = AP.OFFICER)";
                                SqlCommand Command_ = new SqlCommand(Query_, conn_144);
                                Command_.Parameters.Add("@RC", SqlDbType.NVarChar);
                                Command_.Parameters["@RC"].Value = searchTxtBox.Text;
                                int Classification_ = 0;
                                switch (classComboBox.Text)
                                {
                                    case "Business Names":
                                        {
                                            Classification_ = 2;
                                            break;
                                        }
                                    case "Limited Liability Company":
                                        {
                                            Classification_ = 4;
                                            break;
                                        }
                                    case "Incorporated Trustees":
                                        {
                                            Classification_ = 3;
                                            break;
                                        }
                                }
                                Command_.Parameters.Add("@CTYPE", SqlDbType.Int);
                                Command_.Parameters["@CTYPE"].Value = Classification_;
                                Command_.CommandType = CommandType.Text;
                                Command_.CommandTimeout = 0;
                                SqlDataReader rd_ = Command_.ExecuteReader();
                                if (rd_.HasRows)//Meaning at least a row of data was found from the DB
                                {
                                    while (rd_.Read())
                                    {
                                        compName.Text = rd_["NAME"].ToString().Replace("      ", " ");
                                        regDate.Text = Convert.ToDateTime(rd_["REGISTRATION_DATE"]).ToString();
                                        avCode.Text = rd_["AVAILABILITY_CODE"].ToString();
                                        origState.Text = rd_["LOCATION"].ToString();
                                        apprOfficer.Text = rd_["OFFICER"].ToString();
                                        platform.Text = "Content Pinnacle";
                                    }
                                    rd.Close();
                                    conn_144.Close();
                                    conn_144.Dispose();
                                }
                                else//...............Search New Database with a Different Query........................
                                    //It means record wasn't on both Content Pinnacle & CRP Databases
                                {
                                    rd.Close();
                                    conn_144.Close();
                                    conn_144.Dispose();
                                    conn_37 = new SqlConnection(Properties.Settings.Default.Conn_37);
                                    try
                                    {
                                        conn_37.Open();
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show(ex.Message);
                                    }
                                    finally
                                    {
                                        Query = "SELECT DISTINCT C.APPROVED_NAME, C.REGISTRATION_DATE, C.AVAILABILITY_CODE, ";
                                        Query += "SR.userid, DU.FULLNAME, W.BRANCH_CODE ";
                                        Query += "FROM [CAC_38].[cac_prod].[dbo].[COMPANY]  AS C INNER JOIN [CAC_38].[cac_prod].[dbo].[PROCESS_PICK_UP] AS BL ON (C.CLASSIFICATION_FK = @CLFC ";
                                        Query += "AND C.RC_NUMBER = @RC AND C.ID = BL.COMPANY_FK AND ";
                                        Query += "BL.CAC_BRANCH_LOCATION_FK IS NOT NULL) INNER JOIN ";
                                        Query += "tbldocument as SR ";
                                        Query += "ON (SR.avcode COLLATE Latin1_General_CI_AS = C.AVAILABILITY_CODE) ";
                                        Query += "INNER JOIN [CAC_38].[cac_prod].[dbo].[CAC_BRANCH_LOCATION] AS W ON (W.ID = BL.CAC_BRANCH_LOCATION_FK) ";
                                        Query += "INNER JOIN tbldocUser as DU ";
                                        Query += " ON (DU.username = SR.userid)";
                                        Command = new SqlCommand(Query, conn_37);
                                        Command.Parameters.Add("@RC", SqlDbType.NVarChar);
                                        Command.Parameters["@RC"].Value = searchTxtBox.Text;
                                        Classification = 0;
                                        switch (classComboBox.Text)
                                        {
                                            case "Business Names":
                                                {
                                                    Classification = 1;
                                                    break;
                                                }
                                            case "Limited Liability Company":
                                                {
                                                    Classification = 2;
                                                    break;
                                                }
                                            case "Incorporated Trustees":
                                                {
                                                    Classification = 3;
                                                    break;
                                                }
                                        }
                                        Command.Parameters.Add("@CLFC", SqlDbType.BigInt);
                                        Command.Parameters["@CLFC"].Value = Classification;
                                        Command.CommandType = CommandType.Text;
                                        rd = Command.ExecuteReader();
                                        if (rd.HasRows)//Meaning at least a row of data was found from the DB
                                        {
                                            while (rd.Read())
                                            {
                                                compName.Text = rd["APPROVED_NAME"].ToString();
                                                regDate.Text = Convert.ToDateTime(rd["REGISTRATION_DATE"]).ToString();
                                                avCode.Text = rd["AVAILABILITY_CODE"].ToString();
                                                origState.Text = rd["BRANCH_CODE"].ToString();
                                                apprOfficer.Text = rd["fullname"].ToString();
                                                platform.Text = "Company Registration Portal (CRP)";

                                            }
                                            rd.Close();
                                            conn_37.Close();
                                            conn_37.Dispose();
                                        }
                                        else
                                        {   //Check without Approver FK
                                            rd.Close();
                                            conn_144.Close();
                                            conn_144.Dispose();
                                            conn_37 = new SqlConnection(Properties.Settings.Default.Conn_37);
                                            try
                                            {
                                                conn_37.Open();
                                            }
                                            catch (Exception ex)
                                            {
                                                MessageBox.Show(ex.Message);
                                            }
                                            finally
                                            {
                                                Query = "SELECT C.AVAILABILITY_CODE, BL.BRANCH_CODE, C.APPROVED_NAME, C.REGISTRATION_DATE ";
                                                Query += "FROM [CAC_38].[cac_prod].[dbo].[COMPANY] AS C INNER JOIN [CAC_38].[cac_prod].[dbo].[PROCESS_PICK_UP] AS PP ON (C.RC_NUMBER = @RC AND C.CLASSIFICATION_FK = @CLFC AND  ";
                                                Query += "(C.ID = PP.COMPANY_FK)) ";
                                                Query += "INNER JOIN [CAC_38].[cac_prod].[dbo].[CAC_BRANCH_LOCATION] AS BL ON (PP.CAC_BRANCH_LOCATION_FK = BL.ID) ";
                                                Command = new SqlCommand(Query, conn_37);
                                                Command.Parameters.Add("@RC", SqlDbType.NVarChar);
                                                Command.Parameters["@RC"].Value = searchTxtBox.Text;
                                                Classification = 0;
                                                switch (classComboBox.Text)
                                                {
                                                    case "Business Names":
                                                        {
                                                            Classification = 1;
                                                            break;
                                                        }
                                                    case "Limited Liability Company":
                                                        {
                                                            Classification = 2;
                                                            break;
                                                        }
                                                    case "Incorporated Trustees":
                                                        {
                                                            Classification = 3;
                                                            break;
                                                        }
                                                }
                                                Command.Parameters.Add("@CLFC", SqlDbType.BigInt);
                                                Command.Parameters["@CLFC"].Value = Classification;
                                                Command.CommandType = CommandType.Text;
                                                rd = Command.ExecuteReader();
                                                if (rd.HasRows)//Meaning at least a row of data was found from the DB
                                                {
                                                    while (rd.Read())
                                                    {
                                                        compName.Text = rd["APPROVED_NAME"].ToString();
                                                        regDate.Text = Convert.ToDateTime(rd["REGISTRATION_DATE"]).ToString();
                                                        avCode.Text = rd["AVAILABILITY_CODE"].ToString();
                                                        origState.Text = rd["BRANCH_CODE"].ToString();
                                                        apprOfficer.Text = "-";
                                                        platform.Text = "Company Registration Portal (CRP)";

                                                    }
                                                    rd.Close();
                                                    conn_37.Close();
                                                    conn_37.Dispose();
                                                }
                                                else
                                                {
                                                    MessageBox.Show("The application could not find the office of origin for the Company with RC Number " + searchTxtBox.Text + ".", "Result Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                                }
                                                rd.Close();
                                                conn_37.Close();
                                                conn_37.Dispose();
                                            }
                                        }
                                        rd.Close();
                                        conn_37.Close();
                                        conn_37.Dispose();
                                    }
                                }//.......................................
                            }
                        }
                    }
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.AcceptButton = preSearchBtn;
            compName.Text = rcnumber.Text = origState.Text = avCode.Text = regDate.Text = apprOfficer.Text = platform.Text = null;
            classComboBox.SelectedIndex = 0;

            preCompName.Text = preOrigOffice.Text = preDateSubmitted.Text = preApplicationStatus.Text = null;
            docuploadApprovingOfficer.Text = crpApprovingOfficer.Text = auditingOfficer.Text = lastDayQuery.Text = null;
            preSubmittedBy.Text = queryRecord.Text = preQueriedBy.Text = queryStatus.Text = null;


            st_ApplicationStatus.Text = null;
            st_ApprovingOfficer.Text = null;
            st_DateSubmitted.Text = null;
            st_OriginatingOffice.Text = null;


            receiptNo.Text = difResult.Text = difResult2.Text = null;
            auditCompName.Text = amount.Text = auditRC.Text = auditCompType.Text = usageStatus.Text = null;

            //For Post Search Box
            searchTxtBox.Text = "Enter RC Number Here...";
            searchTxtBox.Font = new Font(searchTxtBox.Font, FontStyle.Italic);
            searchTxtBox.ForeColor = Color.Silver;
            
            //For Pre Search Box
            preSearchTxtBox.Text = "Enter Availability Code Here...";
            preSearchTxtBox.Font = new Font(preSearchTxtBox.Font, FontStyle.Italic);
            preSearchTxtBox.ForeColor = Color.Silver;

            //For Status Search Box
            statusSearchTxtBox.Text = "Enter Availability Code Here...";
            statusSearchTxtBox.Font = new Font(statusSearchTxtBox.Font, FontStyle.Italic);
            statusSearchTxtBox.ForeColor = Color.Silver;

            //For Audit Box
            auditSearchBox.Text = "Enter RC or RRR Number Here...";
            auditSearchBox.Font = new Font(auditSearchBox.Font, FontStyle.Italic);
            auditSearchBox.ForeColor = Color.Silver;
            auditComboBox.SelectedIndex = 0;

            //Check if it is running for the first time
            const string REGISTRY_KEY = @"HKEY_CURRENT_USER\MyApplication";
            const string REGISTY_VALUE = "FirstRun";
            if (Convert.ToInt32(Microsoft.Win32.Registry.GetValue(REGISTRY_KEY, REGISTY_VALUE, 0)) == 0)
            {
                MessageBox.Show("This version comes with the following updates:\n\n1. Query update for Auditors' Module.", "What is new?", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //Change the value since the program has run once now
                Microsoft.Win32.Registry.SetValue(REGISTRY_KEY, REGISTY_VALUE, 1, Microsoft.Win32.RegistryValueKind.DWord);
            }
        }

        private void searchTxtBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) /*&& (e.KeyChar != '.')*/)
            {
                e.Handled = true;
            }
            searchTxtBox.ForeColor = Color.Black;
        }

        private void searchTxtBox_Enter(object sender, EventArgs e)
        {
            searchTxtBox.Font = new Font(searchTxtBox.Font, FontStyle.Regular);
            if (searchTxtBox.Text == "Enter RC Number Here...")
            {
                searchTxtBox.Text = null;
            }
        }

        private void searchTxtBox_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(searchTxtBox.Text))
            {
                searchTxtBox.Font = new Font(searchTxtBox.Font, FontStyle.Italic);
                searchTxtBox.Text = "Enter RC Number Here...";
                searchTxtBox.ForeColor = Color.Silver;
            }

        }

        private void preSearchTxtBox_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(preSearchTxtBox.Text))
            {
                preSearchTxtBox.Font = new Font(preSearchTxtBox.Font, FontStyle.Italic);
                preSearchTxtBox.Text = "Enter Availability Code Here...";
                preSearchTxtBox.ForeColor = Color.Silver;
            }
        }

        private void preSearchTxtBox_Enter(object sender, EventArgs e)
        {
            preSearchTxtBox.Font = new Font(preSearchTxtBox.Font, FontStyle.Regular);
            if (preSearchTxtBox.Text == "Enter Availability Code Here...")
            {
                preSearchTxtBox.Text = null;
            }
        }
        private void preSearchTxtBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) /*&& (e.KeyChar != '.')*/)
            {
                e.Handled = true;
            }
            preSearchTxtBox.ForeColor = Color.Black;
        }

        private void selectionTab_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (selectionTab.SelectedIndex)
            {
                case 0:
                    {
                        this.AcceptButton = preSearchBtn;
                        break;
                    }
                case 1:
                    {
                        this.AcceptButton = searchBtn;
                        break;
                    }
                case 2:
                    {
                        this.AcceptButton = statusBtn;
                        break;
                    }
                case 3:
                    {
                        this.AcceptButton = auditBtn;
                        string MAC = GetMacAddress().ToString();
                        if(MAC != "606DC706B9A11" && MAC != "9457A5D67F98" &&
                           MAC != "84A93EAB916E" && MAC != "002264234AC5" &&
                           MAC != "00215A71C487" && MAC != "0023541F1734" &&
                           MAC != "0021855828E2" && MAC != "C46516102AD2" && 
                           MAC != "E4E7494D7412" && MAC != "0A0027000005" &&
                           MAC != "B05CDADC87A0" && MAC != "5065F34E1E9E")
                        {
                            MessageBox.Show("You are not granted the privilege to use this section of the application. This is meant for Audit Department Staff only.", "Unauthorized Access Attempt", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                            auditComboBox.Enabled = auditSearchBox.Enabled = auditBtn.Enabled = auditPanel.Enabled = false;
                            withRcRadio.Enabled = false;
                            rrrRadio.Enabled = false;
                        }
                        break;
                    }
            }
        }

        private void preSearchBtn_Click(object sender, EventArgs e)
        {
            Feedback fb;
            fb.Company_name = "NIL";
            fb.RC_number = "NIL";
            fb.Originating_office = "NIL";
            fb.Date_submitted = "NIL";
            fb.Submitted_by = "NIL";
            fb.Application_status = "NIL";
            fb.Approving_officer_docupload = "NIL";
            fb.Approving_officer_crp = "NIL";
            fb.Auditing_officer = "NIL";
            fb.Last_date_of_query = "NIL";
            fb.Queried_by = "NIL";
            fb.Last_query_record = "NIL";
            fb.Query_status = "NIL";

        preCompName.Text = rcnumber.Text = preOrigOffice.Text = preDateSubmitted.Text = preApplicationStatus.Text = null;
            docuploadApprovingOfficer.Text = crpApprovingOfficer.Text = auditingOfficer.Text = lastDayQuery.Text = null;
            preSubmittedBy.Text = queryRecord.Text = preQueriedBy.Text = queryStatus.Text = null;
            if (string.IsNullOrWhiteSpace(preSearchTxtBox.Text) ||
                preSearchTxtBox.Text == "Enter Availability Code Here...")
            {
                MessageBox.Show("Empty entry detected in Availability Code field.", "Entry Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else
            {
                SqlConnection conn_37 = new SqlConnection(Properties.Settings.Default.Conn_37);
                try
                {
                    conn_37.Open();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    string Query = "SELECT C.APPROVED_NAME, C.RC_NUMBER, S.State, D.complete_date, C.REG_PORTAL_USER_FK, PU.FIRSTNAME, ";
                    Query += "PU.OTHER_NAME , PU.SURNAME , PU.USERNAME, D.status, D.userid, U.Fullname ";
                    Query += "FROM [CAC_38].[cac_prod].[dbo].[COMPANY] AS C INNER JOIN [Searcheslog].[dbo].tbldocument as D ON ";
                    Query += "(C.AVAILABILITY_CODE COLLATE Latin1_General_CI_AS = D.avcode AND C.AVAILABILITY_CODE = @AV ";
                    Query += "and (D.doctype ='Memorandum' OR D.doctype ='Identification' OR D.doctype = 'Registration Form' OR D.doctype ='Availability PrintOut' OR ";
                    Query += " D.doctype ='Certificate' OR D.doctype ='Constitution' OR D.doctype = 'Stamp Duty' OR D.doctype = 'Declaration Forms' OR D.doctype = 'Consent')) ";
                    Query += "INNER JOIN [CAC_38].[cac_prod].[dbo].[PORTAL_USER] AS PU ON ";
                    Query += "(C.REG_PORTAL_USER_FK = PU.ID AND PU.PORTAL_USER_TYPE_FK = 88)";
                    Query += " INNER JOIN [Searcheslog].[dbo].tbldocUser AS U ON (D.userid = U.username) ";
                    Query += "INNER JOIN [Searcheslog].[dbo].tblStates as S ON (U.state = S.StateCode) ";
                    SqlCommand Command = new SqlCommand(Query, conn_37);
                    Command.Parameters.Add("@AV", SqlDbType.NVarChar);
                    Command.Parameters["@AV"].Value = preSearchTxtBox.Text;
                    Command.CommandType = CommandType.Text;
                    SqlDataReader rd = Command.ExecuteReader();
                    if (rd.HasRows)//Meaning at least a row of data was found from the DB
                    {
                        while (rd.Read())
                        {
                            fb.Company_name = rd["APPROVED_NAME"].ToString();
                            fb.RC_number = rd["RC_NUMBER"].ToString();
                            fb.Originating_office = rd["State"].ToString();
                            fb.Date_submitted = Convert.ToDateTime(rd["complete_date"]).ToString();
                            fb.Submitted_by = rd["FIRSTNAME"].ToString() + " ";
                            if (string.IsNullOrWhiteSpace(rd["OTHER_NAME"].ToString()))
                                fb.Submitted_by += rd["OTHER_NAME"].ToString() + " ";
                            fb.Submitted_by += rd["SURNAME"].ToString() + " (";
                            fb.Submitted_by += rd["USERNAME"].ToString() + ")";
                            fb.Application_status = rd["status"].ToString();

                            //We need to check whether the application has been submitted, rejected, approved, or audited.
                            if (fb.Application_status != "AUDITED" && fb.Application_status != "APPROVED")
                            {
                                fb.Approving_officer_docupload = rd["Fullname"].ToString() + " (";
                                fb.Approving_officer_docupload += rd["userid"].ToString() + ")";
                                docuploadApprovingOfficer.Text = fb.Approving_officer_docupload;
                            }

                            //Now Display
                            preCompName.Text = fb.Company_name;
                            rcnumber.Text = fb.RC_number;
                            preOrigOffice.Text = fb.Originating_office;
                            preDateSubmitted.Text = fb.Date_submitted;
                            preSubmittedBy.Text = fb.Submitted_by;
                            preApplicationStatus.Text = fb.Application_status;
                        }
                        rd.Close();
                        conn_37.Close();
                        conn_37.Dispose();
                        //CRP Approver
                        Query = "SELECT PU.FIRSTNAME, PU.OTHER_NAME , PU.SURNAME , PU.USERNAME ";
                        Query += "FROM [CAC_38].[cac_prod].[dbo].[PORTAL_USER] AS PU INNER JOIN [CAC_38].[cac_prod].[dbo].[COMPANY] AS C ";
                        Query += "ON (C.AVAILABILITY_CODE = @AV AND C.APPROVER_FK = PU.ID)";
                        conn_37 = new SqlConnection(Properties.Settings.Default.Conn_37);
                        conn_37.Open();
                        Command = new SqlCommand(Query, conn_37);
                        Command.Parameters.Add("@AV", SqlDbType.NVarChar);
                        Command.Parameters["@AV"].Value = preSearchTxtBox.Text;
                        Command.CommandType = CommandType.Text;
                        rd = Command.ExecuteReader();
                        if (rd.HasRows)//Meaning at least a row of data was found from the DB
                        {
                            while (rd.Read())
                            {
                                fb.Approving_officer_crp = rd["FIRSTNAME"].ToString() + " ";
                                if (string.IsNullOrWhiteSpace(rd["OTHER_NAME"].ToString()))
                                    fb.Approving_officer_crp += rd["OTHER_NAME"].ToString() + " ";
                                fb.Approving_officer_crp += rd["SURNAME"].ToString() + " (";
                                fb.Approving_officer_crp += rd["USERNAME"].ToString() + ")";
                            }

                        }
                        crpApprovingOfficer.Text = fb.Approving_officer_crp;
                        rd.Close();
                        conn_37.Close();
                        conn_37.Dispose();

                        if (fb.Application_status == "AUDITED" || fb.Application_status == "APPROVED")
                        {
                            int temp = 0;
                            Query = "SELECT D.Fullname, D.username FROM [Searcheslog].[dbo].tbldocuser as D ";
                            Query += "INNER JOIN [Searcheslog].[dbo].tblActivity as A ";
                            Query += "ON (D.username = A.username AND (A.avcode = @AV AND ";
                            Query += "(A.activity = 'Registration Approved' OR A.activity = 'Registration Audited'))) ";
                            Query += "ORDER BY ddate";
                            conn_37 = new SqlConnection(Properties.Settings.Default.Conn_37);
                            conn_37.Open();
                            Command = new SqlCommand(Query, conn_37);
                            Command.Parameters.Add("@AV", SqlDbType.NVarChar);
                            Command.Parameters["@AV"].Value = preSearchTxtBox.Text;
                            Command.CommandType = CommandType.Text;
                            rd = Command.ExecuteReader();
                            if (rd.HasRows)//Meaning at least a row of data was found from the DB
                            {
                                while (rd.Read())
                                {
                                    if (temp == 0)
                                    {
                                        fb.Approving_officer_docupload = rd["Fullname"].ToString() + " (";
                                        fb.Approving_officer_docupload += rd["username"].ToString() + ")";
                                    }
                                    if (temp == 1)
                                    {
                                        fb.Auditing_officer = rd["Fullname"].ToString() + " (";
                                        fb.Auditing_officer += rd["username"].ToString() + ")";
                                    }
                                    temp++;
                                }
                            }
                            rd.Close();
                            conn_37.Close();
                            conn_37.Dispose();
                        }

                        //Finally, Get The Query Details
                        Query = "SELECT Q.DATE_OF_QUERY, PU.FIRSTNAME, PU.OTHER_NAME , PU.SURNAME , PU.USERNAME, Q.REASON_FOR_QUERY, Q.RESOLUTION_STATUS ";
                        Query += "FROM [CAC_38].[cac_prod].[dbo].[QUERY_HISTORY] AS Q INNER JOIN [CAC_38].[cac_prod].[dbo].[COMPANY] AS C ON ";
                        Query += "(C.AVAILABILITY_CODE = @AV AND C.QUERY_CODE = Q.ID) INNER JOIN ";
                        Query += "[CAC_38].[cac_prod].[dbo].[PORTAL_USER] AS PU ON (Q.QUERIED_BY_FK = PU.ID)";
                        conn_37 = new SqlConnection(Properties.Settings.Default.Conn_37);
                        conn_37.Open();
                        Command = new SqlCommand(Query, conn_37);
                        Command.Parameters.Add("@AV", SqlDbType.NVarChar);
                        Command.Parameters["@AV"].Value = preSearchTxtBox.Text;
                        Command.CommandType = CommandType.Text;
                        rd = Command.ExecuteReader();
                        if (rd.HasRows)//Meaning at least a row of data was found from the DB
                        {
                            while (rd.Read())
                            {
                                fb.Last_date_of_query = Convert.ToDateTime(rd["DATE_OF_QUERY"]).ToString();
                                fb.Queried_by = rd["FIRSTNAME"].ToString() + " ";
                                if (string.IsNullOrWhiteSpace(rd["OTHER_NAME"].ToString()))
                                    fb.Queried_by += rd["OTHER_NAME"].ToString() + " ";
                                fb.Queried_by += rd["SURNAME"].ToString() + " (";
                                fb.Queried_by += rd["USERNAME"].ToString() + ")";
                                fb.Query_status = rd["RESOLUTION_STATUS"].ToString();
                                fb.Last_query_record = rd["REASON_FOR_QUERY"].ToString();
                            }
                        }
                        rd.Close();
                        conn_37.Close();
                        conn_37.Dispose();
                        docuploadApprovingOfficer.Text = fb.Approving_officer_docupload;
                        auditingOfficer.Text = fb.Auditing_officer;
                        lastDayQuery.Text = fb.Last_date_of_query;
                        preQueriedBy.Text = fb.Queried_by;
                        queryStatus.Text = fb.Query_status;
                        queryRecord.Text = fb.Last_query_record;
                    }
                    else
                    {
                        MessageBox.Show("The application could not trace the status for Company with Availability Code '" + preSearchTxtBox.Text + "'.", "Result Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

            }
        }

        private void auditSearchBox_Enter(object sender, EventArgs e)
        {
            auditSearchBox.Font = new Font(auditSearchBox.Font, FontStyle.Regular);
            if (auditSearchBox.Text == "Enter RC or RRR Number Here...")
            {
                auditSearchBox.Text = null;
            }
        }

        private void auditSearchBox_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(auditSearchBox.Text))
            {
                auditSearchBox.Font = new Font(auditSearchBox.Font, FontStyle.Italic);
                auditSearchBox.Text = "Enter RC or RRR Number Here...";
                auditSearchBox.ForeColor = Color.Silver;
            }
        }
        private void statusSearchTxtBox_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(statusSearchTxtBox.Text))
            {
                statusSearchTxtBox.Font = new Font(statusSearchTxtBox.Font, FontStyle.Italic);
                statusSearchTxtBox.Text = "Enter Availability Code Here...";
                statusSearchTxtBox.ForeColor = Color.Silver;
            }
        }

        private void statusSearchTxtBox_Enter(object sender, EventArgs e)
        {
            statusSearchTxtBox.Font = new Font(statusSearchTxtBox.Font, FontStyle.Regular);
            if (statusSearchTxtBox.Text == "Enter Availability Code Here...")
            {
                statusSearchTxtBox.Text = null;
            }
        }

        private void statusSearchTxtBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) /*&& (e.KeyChar != '.')*/)
            {
                e.Handled = true;
            }
            statusSearchTxtBox.ForeColor = Color.Black;
        }

        private void statusBtn_Click(object sender, EventArgs e)
        {
            st_ApplicationStatus.Text = null;
            st_ApprovingOfficer.Text = null;
            st_DateSubmitted.Text = null;
            st_OriginatingOffice.Text = null;
            if (string.IsNullOrWhiteSpace(statusSearchTxtBox.Text) ||
                statusSearchTxtBox.Text == "Enter Availability Code Here...")
            {
                MessageBox.Show("Empty entry detected in Availability Code field.", "Entry Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else //Fetch Status Details
            {
                SqlConnection conn_37 = new SqlConnection(Properties.Settings.Default.Conn_37);
                try
                {
                    conn_37.Open();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    string Query = "SELECT S.State, D.complete_date, D.status, D.userid, U.Fullname ";
                    Query += "FROM [Searcheslog].[dbo].tbldocument as D INNER JOIN  [Searcheslog].[dbo].tbldocUser AS U ON ";
                    Query += "( D.avcode = @AV and (D.doctype ='Memorandum' OR D.doctype ='Identification' OR D.doctype = 'Registration Form' OR D.doctype = 'Payment Receipt' OR D.doctype ='Availability PrintOut' OR ";
                    Query += " D.doctype ='Certificate' OR D.doctype ='Constitution') and (D.userid = U.username)) ";
                    Query += "INNER JOIN [Searcheslog].[dbo].tblStates as S ON (U.state = S.StateCode) ";
                    SqlCommand Command = new SqlCommand(Query, conn_37);
                    Command.Parameters.Add("@AV", SqlDbType.NVarChar);
                    Command.Parameters["@AV"].Value = statusSearchTxtBox.Text;
                    Command.CommandType = CommandType.Text;
                    SqlDataReader rd = Command.ExecuteReader();
                    if (rd.HasRows)//Meaning at least a row of data was found from the DB
                    {
                        while (rd.Read())
                        {
                            st_OriginatingOffice.Text = rd["State"].ToString();
                            st_DateSubmitted.Text = Convert.ToDateTime(rd["complete_date"]).ToString();
                            st_ApprovingOfficer.Text = rd["Fullname"].ToString() + " (" + rd["userid"].ToString() + ")";
                            st_ApplicationStatus.Text = rd["status"].ToString();
                        }
                    }
                    else
                    {
                        MessageBox.Show("The application could not trace the status for Company with Availability Code '" + statusSearchTxtBox.Text + "'.", "Result Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            } //End of Fetch Status Details
        }

        private void audit_Click(object sender, EventArgs e)
        {
            
        }

        private void auditComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(auditComboBox.SelectedItem.ToString() == "Limited Liability Company")
            {
                difLabel.Text = "Share Capital Registered:";
                difLabel.Visible = true;
                auditCompName.Text = auditRC.Text = receiptNo.Text = amount.Text = difResult.Text = null;
            }
            if (auditComboBox.SelectedItem.ToString() == "Incorporated Trustees")
            {
                difLabel.Visible = false;
                difResult.Visible = false;
                auditCompName.Text = auditRC.Text = receiptNo.Text = amount.Text = difResult.Text = null;
            }
            if (auditComboBox.SelectedItem.ToString() == "Business Names")
            {
                difLabel.Text = "Branch Address";
                difLabel.Visible = true;
                auditCompName.Text = auditRC.Text = receiptNo.Text = amount.Text = difResult.Text = null;
            }
        }

        private void auditBtn_Click(object sender, EventArgs e)
        {
            difResult2.Visible = true;
            auditCompName.Text = auditRC.Text = difResult.Text = receiptNo.Text = amount.Text = auditCompType.Text = usageStatus.Text = null;
            if (string.IsNullOrWhiteSpace(auditSearchBox.Text) || auditSearchBox.Text == "Enter RC or RRR Number Here...")
            {
                MessageBox.Show("Empty entry detected in RC Number field.", "Entry Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else
            {
                if(withRcRadio.Checked) //Search Using RC
                {
                    if (auditComboBox.Text == "Choose Company Type:")
                    {
                        MessageBox.Show("Please select the type of Company from the Dropdown options listed.", "Entry Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    }
                    else
                    {
                        SqlConnection conn_38 = new SqlConnection(Properties.Settings.Default.Conn_38);
                        try
                        {
                            conn_38.Open();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        finally
                        {
                            string Query = "";
                            switch (auditComboBox.Text)
                            {
                                case "Business Names":
                                    {
                                        Query = "SELECT C.APPROVED_NAME, C.RC_NUMBER, C.BRANCH_ADDRESS, PH.USAGE_STATUS, PH.TELLER, PH.AMOUNT ";
                                        Query += "FROM COMPANY AS C INNER JOIN PAYMENT_HISTORY AS PH ON ";
                                        Query += "(C.RC_NUMBER = @RC AND C.CLASSIFICATION_FK = 1 AND ";
                                        Query += "C.ID = PH.RECORD_ID  AND (PH.PAYMENT_STATUS = 'APPROVED' AND (USAGE_STATUS = 'USED' OR USAGE_STATUS = 'NOT_USED')))";
                                        break;
                                    }
                                case "Limited Liability Company":
                                    {
                                        Query = "SELECT C.APPROVED_NAME, C.RC_NUMBER, SH.NOMINAL_SHARE_CAPITAL_IN_WORDS, SH.NOMINAL_SHARE_CAPITAL_IN_KOBO, PH.USAGE_STATUS, PH.AMOUNT,";
                                        Query += " PH.TELLER FROM COMPANY AS C INNER JOIN SHARE_DETAIL AS SH ON (C.RC_NUMBER = @RC AND C.CLASSIFICATION_FK = 2";
                                        Query += " AND(C.ID = SH.COMPANY_FK)) INNER JOIN PAYMENT_HISTORY AS PH ON ";
                                        Query += " (PH.RECORD_ID = C.ID AND (PH.PAYMENT_STATUS = 'APPROVED' AND (USAGE_STATUS = 'USED' OR USAGE_STATUS = 'NOT_USED')))";
                                        break;
                                    }
                                case "Incorporated Trustees":
                                    {
                                        Query = "SELECT C.APPROVED_NAME, C.RC_NUMBER, PH.TELLER, PH.USAGE_STATUS, PH.AMOUNT ";
                                        Query += "FROM COMPANY AS C INNER JOIN PAYMENT_HISTORY AS PH ON ";
                                        Query += "(C.RC_NUMBER = @RC AND C.CLASSIFICATION_FK = 3 AND ";
                                        Query += " C.ID = PH.RECORD_ID  AND (PH.PAYMENT_STATUS = 'APPROVED' AND (USAGE_STATUS = 'USED' OR USAGE_STATUS = 'NOT_USED')))";
                                        break;
                                    }
                            }
                            SqlCommand Command = new SqlCommand(Query, conn_38);
                            Command.Parameters.Add("@RC", SqlDbType.NVarChar);
                            Command.Parameters["@RC"].Value = auditSearchBox.Text;
                            SqlDataReader rd = Command.ExecuteReader();
                            int count = 0;
                            if (rd.HasRows)//Meaning at least a row of data was found from the DB
                            {
                                while (rd.Read())
                                {
                                    if(auditComboBox.Text == "Limited Liability Company")
                                    {
                                        if (count == 0) auditCompName.Text = rd["APPROVED_NAME"].ToString();
                                        if (count == 0) auditRC.Text = rd["RC_NUMBER"].ToString();
                                        if (count == 0)
                                            receiptNo.Text = rd["TELLER"].ToString();
                                        if(count > 0)
                                            receiptNo.Text += ", " + rd["TELLER"].ToString();
                                        if (count == 0)  amount.Text = rd["AMOUNT"].ToString();
                                        if (count > 0) amount.Text += ", " + rd["AMOUNT"].ToString();
                                        if (count == 0) usageStatus.Text = rd["USAGE_STATUS"].ToString();
                                        if (count > 0) usageStatus.Text += ", " + rd["USAGE_STATUS"].ToString();
                                        if (count == 0) auditCompType.Text = "Limited Liability Company";
                                        if (count == 0)  difResult.Text = rd["NOMINAL_SHARE_CAPITAL_IN_WORDS"].ToString().ToUpper() + " (" + rd["NOMINAL_SHARE_CAPITAL_IN_KOBO"].ToString() + ")";
                                        count++;
                                    }
                                    else if(auditComboBox.Text == "Business Names")
                                    {
                                        if (count == 0) auditCompName.Text = rd["APPROVED_NAME"].ToString();
                                        if (count == 0) auditRC.Text = rd["RC_NUMBER"].ToString();
                                        if (count == 0)
                                            receiptNo.Text = rd["TELLER"].ToString();
                                        if (count > 0)
                                            receiptNo.Text += ", " + rd["TELLER"].ToString();
                                        if (count == 0) amount.Text = rd["AMOUNT"].ToString();
                                        if (count > 0) amount.Text += ", " + rd["AMOUNT"].ToString();
                                        if (count == 0) usageStatus.Text = rd["USAGE_STATUS"].ToString();
                                        if (count > 0) usageStatus.Text += ", " + rd["USAGE_STATUS"].ToString();
                                        if (count == 0) auditCompType.Text = "Business Names";
                                        if (count == 0) difResult.Text = rd["BRANCH_ADDRESS"].ToString();
                                        count++;
                                    }
                                    else
                                    {
                                        if (count == 0) auditCompName.Text = rd["APPROVED_NAME"].ToString();
                                        if (count == 0) auditRC.Text = rd["RC_NUMBER"].ToString();
                                        if (count == 0)
                                            receiptNo.Text = rd["TELLER"].ToString();
                                        if (count > 0)
                                            receiptNo.Text += ", " + rd["TELLER"].ToString();
                                        if (count == 0) amount.Text = rd["AMOUNT"].ToString();
                                        if(count > 0) amount.Text += ", "+ rd["AMOUNT"].ToString();
                                        if (count == 0) usageStatus.Text = rd["USAGE_STATUS"].ToString();
                                        if (count > 0) usageStatus.Text += ", " + rd["USAGE_STATUS"].ToString();
                                        if (count == 0) auditCompType.Text = "Incorporated Trustees";
                                        count++;
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show("The RC or the receipt does not exist on the database. It may also be that you chose the wrong Company type.", "Result Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
                if(rrrRadio.Checked)
                {
                    SqlConnection conn_38 = new SqlConnection(Properties.Settings.Default.Conn_38);
                    try
                    {
                        conn_38.Open();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    finally
                    {
                        //Start with checking LLC Records
                        string Query = "SELECT C.APPROVED_NAME, C.RC_NUMBER, C.CLASSIFICATION_FK, SH.NOMINAL_SHARE_CAPITAL_IN_WORDS, SH.NOMINAL_SHARE_CAPITAL_IN_KOBO, PH.USAGE_STATUS, PH.AMOUNT ,";
                        Query += " PH.TELLER FROM PAYMENT_HISTORY AS PH INNER JOIN SHARE_DETAIL AS SH ON ";
                        Query += "((PH.TELLER = @teller  AND (PH.PAYMENT_STATUS = 'APPROVED' ";
                        Query += "AND (USAGE_STATUS = 'USED' OR USAGE_STATUS = 'NOT_USED'))) AND PH.RECORD_ID = SH.COMPANY_FK) ";
                        Query += "INNER JOIN  COMPANY AS C ON (SH.COMPANY_FK = C.ID)";
                        SqlCommand Command = new SqlCommand(Query, conn_38);
                        Command.Parameters.Add("@teller", SqlDbType.NVarChar);
                        Command.Parameters["@teller"].Value = auditSearchBox.Text;
                        SqlDataReader rd = Command.ExecuteReader();
                        if (rd.HasRows)
                        {
                            while (rd.Read())
                            {
                                auditCompName.Text = rd["APPROVED_NAME"].ToString();
                                auditRC.Text = rd["RC_NUMBER"].ToString();
                                receiptNo.Text = rd["TELLER"].ToString();
                                amount.Text = rd["AMOUNT"].ToString(); 
                                usageStatus.Text = rd["USAGE_STATUS"].ToString();
                                auditCompType.Text = "Limited Liability Company";
                                difResult.Text = rd["NOMINAL_SHARE_CAPITAL_IN_WORDS"].ToString().ToUpper() + " (" + rd["NOMINAL_SHARE_CAPITAL_IN_KOBO"].ToString() + ")";
                                difResult2.Text = "Not Applicable";
                            }
                        }
                        else
                        {
                            rd.Close();
                            conn_38.Close();
                            conn_38.Dispose();

                            //Now Check for Business Names
                            conn_38 = new SqlConnection(Properties.Settings.Default.Conn_38);
                            try
                            {
                                conn_38.Open();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                            finally
                            {
                                Query = "SELECT C.APPROVED_NAME, C.RC_NUMBER, C.BRANCH_ADDRESS, C.CLASSIFICATION_FK, PH.USAGE_STATUS, ";
                                Query += "PH.TELLER, PH.AMOUNT FROM COMPANY AS C INNER JOIN PAYMENT_HISTORY AS PH ";
                                Query += "ON ((PH.TELLER = @teller AND (PH.PAYMENT_STATUS = 'APPROVED' ";
                                Query += "AND (USAGE_STATUS = 'USED' OR USAGE_STATUS = 'NOT_USED'))) AND C.ID = PH.RECORD_ID  )";
                                Command = new SqlCommand(Query, conn_38);
                                Command.Parameters.Add("@teller", SqlDbType.NVarChar);
                                Command.Parameters["@teller"].Value = auditSearchBox.Text;
                                rd = Command.ExecuteReader();
                                if (rd.HasRows)
                                {
                                    while (rd.Read())
                                    {
                                        auditCompName.Text = rd["APPROVED_NAME"].ToString();
                                        auditRC.Text = rd["RC_NUMBER"].ToString();
                                        receiptNo.Text = rd["TELLER"].ToString();
                                        amount.Text = rd["AMOUNT"].ToString();
                                        usageStatus.Text = rd["USAGE_STATUS"].ToString();
                                        if (rd["CLASSIFICATION_FK"].ToString() == "1")
                                            auditCompType.Text = "Business Names";
                                        if (rd["CLASSIFICATION_FK"].ToString() == "3")
                                            auditCompType.Text = "Incorporated Trustees";
                                        difResult.Text = "Not Applicable";
                                        if (rd["CLASSIFICATION_FK"].ToString() == "1")
                                            difResult2.Text = rd["BRANCH_ADDRESS"].ToString();
                                        if (rd["CLASSIFICATION_FK"].ToString() == "3")
                                            difResult2.Text = "Not Applicable";
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("The RRR Number has not been captured on our database.", "Result Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void withRcRadio_CheckedChanged(object sender, EventArgs e)
        {
            auditBtn.Enabled = true;
            auditComboBox.Enabled = true;
            difLabel2.Visible = difResult2.Visible = false;
            auditComboBox.SelectedItem = "Choose Company Type:";
            receiptNo.Text = amount.Text = auditCompName.Text = auditRC.Text = null;
            auditCompType.Text = difResult.Text = difResult2.Text = null;
        }

        private void rrrRadio_CheckedChanged(object sender, EventArgs e)
        {
            auditBtn.Enabled = true;
            auditComboBox.Enabled = false;
            difLabel2.Visible = difResult2.Visible = true;
            difLabel.Visible = true;
            difLabel.Text = "Share Capital Registered:";
            difResult.Visible = true;
            receiptNo.Text = amount.Text = auditCompName.Text = auditRC.Text = null;
            auditCompType.Text = difResult.Text = difResult2.Text = usageStatus.Text = null;
        }

        private void auditSearchBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) /*&& (e.KeyChar != '.')*/)
            {
                e.Handled = true;
            }
            auditSearchBox.ForeColor = Color.Black;
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
