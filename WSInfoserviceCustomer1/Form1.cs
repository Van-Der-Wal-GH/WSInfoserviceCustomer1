using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using WSInfoserviceCustomer1.WSInfoservice;
using System.ServiceModel.Channels;
using System.Net;

using System.IO;
using System.Data.SqlClient;


namespace WSInfoserviceCustomer1
{
    public partial class Form1 : Form
    {
        public string connectionstring;
        public static SqlConnection SQLconn, SQLconn2, SQLconn3;
        public static SqlDataReader SQL_x, SQL_x2, SQL_x3;
        public static SqlCommand SQLcmd, SQLcmd2, SQLcmd3;
        public static int database_open = 0;

        public Form1()
        {
            InitializeComponent();
        }

        /*
        Contract number : 604018
        Login : 604018WS62886543 
        Password : 63629877 

        Contract number : 604007 
        Login : 604007WS81282175 
        Password : 86493101 
        */


        //----------------------------------------------------------------------------------------------------------
        public static int atol(string ipstr)
        {
            string resstr = "", bstr = "01234567890-.,";
            int i;
            string cstr;
            for (i = 0; i < ipstr.Length; i++)
            {
                if (bstr.IndexOf(ipstr.Substring(i, 1)) >= 0)
                {
                    cstr = ipstr.Substring(i, 1);
                    if ((cstr == ".") || (cstr == ","))
                    {
                        i = 999;
                    }
                    else
                    {
                        resstr += cstr; //ipstr.Substring(i, 1);
                    }
                }
            }
            if (resstr.Length == 0) return (0);
            return (Convert.ToInt32(resstr));
        }
        //----------------------------------------------------------------------------------------------------------
        public static double atof(string ipstr)
        {
            string resstr = "", bstr = "01234567890-.,";
            int i;
            for (i = 0; i < ipstr.Length; i++)
            {
                if (bstr.IndexOf(ipstr.Substring(i, 1)) >= 0)
                {
                    resstr += ipstr.Substring(i, 1);
                }
            }
            if (resstr.Length == 0) return (0);
            double fl = Convert.ToDouble(resstr);
            return (Convert.ToDouble(resstr));
        }
        //----------------------------------------------------------------------------------------------------------
        public static void wait(int msec)
        {
            long t_start = (System.DateTime.Now.Second * 1000) + System.DateTime.Now.Millisecond;
            long t_end = t_start;
            while ((t_end - t_start + 60000) % 60000 < msec)
            {
                t_end = (System.DateTime.Now.Second * 1000) + System.DateTime.Now.Millisecond;
                Application.DoEvents();
            }
            Application.DoEvents();
        }
        //----------------------------------------------------------------------------------------------------------
        public static string short_amount(string ipstr)
        {
            double f = atof(ipstr);
            if (Math.Abs(f) < 1000.0)
                return (f.ToString("###"));
            f /= 1000.0;
            if (Math.Abs(f) < 1000.0)
                return (f.ToString("###K"));
            f /= 1000.0;
            return (f.ToString("###M"));
        }
        //----------------------------------------------------------------------------------------------------------
        public static int writelog(string text)
        {
            TextWriter streamwriter = new StreamWriter("cm_" + DateTime.Now.ToString("yyMM") + ".txt", true);
            streamwriter.WriteLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + ": " + text);
            streamwriter.Close();
            return (1);
        }
        //--------------------------------------------------------------------------
        public static string datestring(DateTime dt)
        {
            return dt.ToString("dd-MM-yyyy");
        }
        //--------------------------------------------------------------------------
        public static string tostring(object x)
        {
            if (x == null) return "";
            try
            {
                return x.ToString();
            }
            catch
            {
            }
            return "";
        }
        //--------------------------------------------------------------------------
        public static int toint(object x)
        {
            if (x == null) return 0;
            try
            {
                if (Convert.ToInt32(x) == 0) return 0;
            }
            catch { }
            try
            {
                if (Convert.ToBoolean(x) == false) return 0;
            }
            catch { }
            try
            {
                if (Convert.ToString(x).Trim().Length == 0) return 0;
            }
            catch { }
            return 1;
        }
        //--------------------------------------------------------------------------
        public static DateTime todt(string ipstr)
        {
            try
            {
                return Convert.ToDateTime(ipstr);
            }
            catch
            {
                return (Convert.ToDateTime("2000-01-01"));
            }
        }
        //----------------------------------------------------------------------------------------------------------
        public void check_databaseopen()
        {
            if (database_open == 0)
            {
                //    server=TMS-BCK;uid=sa;pwd=pltas;database=tmw_ploeger2000
                connectionstring = "server=" + formsetup._setup[0].str +
                                    ";uid=" + formsetup._setup[2].str +
                                    ";pwd=" + formsetup._setup[3].str +
                                    ";database=" + formsetup._setup[1].str;
                try
                {
                    SQLconn = new SqlConnection(connectionstring); SQLconn.Open();
                    SQLconn2 = new SqlConnection(connectionstring); SQLconn2.Open();
                    SQLconn3 = new SqlConnection(connectionstring); SQLconn3.Open();
                    database_open = 1;
                }
                catch (SqlException sq)
                {
                    MessageBox.Show("Kan geen connectie maken met de database!\r\n\r\n" /*+ connectionstring*/, "fout connecting CM database " + sq.Message);
                    // Application.Exit();
                }
            }
        }


        static int process_transaction(transaction trans)
        {
            try
            {
                //-------------
                DateTime dt = trans.entryTransactionDate;//trans.transactionValueDate;
                try
                {
                    double fl = trans.entryTransactionDate.ToOADate();
                    if (fl <= 0)
                    {
                        dt = trans.entryTransactionDate;
                    }
                }
                catch (Exception ex)
                {
                    dt = trans.entryTransactionDate;
                }
                //-------------
                DateTime dt2 = trans.exitTransactionDate;
                try
                {
                    double fl = trans.exitTransactionDate.ToOADate();
                    if (fl <= 0)
                    {
                        dt2 = trans.exitTransactionDate;
                    }
                }
                catch (Exception ex)
                {
                    dt2 = trans.exitTransactionDate;
                }
                //-------------
                DateTime dt3 = trans.VCExpiryDate;
                try
                {
                    double fl = dt3.ToOADate();
                    if (fl <= 0)
                    {
                        dt3 = todt("2000-01-01"); 
                    }
                }
                catch (Exception ex)
                {
                    dt3 = todt("2000-01-01");
                }

                if (dt.ToOADate()<=0)
                    dt=todt("2000-01-01");
                if (dt2.ToOADate()<=0)
                    dt2=todt("2000-01-01");


                SQLcmd = new SqlCommand("delete from AS24 where as2_nummertransactie = @as2_nummertransactie", SQLconn);
                SQLcmd.Parameters.AddWithValue("@as2_nummertransactie", tostring(trans.transactionNumber));
                SQLcmd.ExecuteNonQuery();
                SQLcmd = new SqlCommand(
                    "insert into as24 (as2_typeOfferte, as2_LeverancierTransactie, as2_nummerTransactie, as2_nummerContract, as2_nummerVoertuigKaart, as2_typeVoertuigkaart, as2_inschrijvingsnummer, as2_vervaldatum, as2_codeIngangPartner, as2_ISOcodeland_ingang, as2_NummerStation, as2_muntCode, as2_codeIngangsnelweg, as2_naamIngang, as2_nummerchauffeurkaart, as2_naamchauffeur, as2_datumtransactie, as2_code_tolpartner, as2_ISOcodeTolland, as2_nummerTolstation, as2_codeTolSnelweg, as2_naamTolstation, as2_muntCodeTol, as2_datum_tol, as2_typeTransactie, as2_productcode, as2_productnaam, as2_hoeveelheid, as2_kilometer, as2_brutoprijs_allintank, as2_nettoprijs_allintank, as2_eenheidskorting_allintank, as2_nettoExtank, as2_BTWvoet, as2_bedragExBTWtank, as2_BTWtank, as2_bedragAllintank, as2_muntcodeBetaling) " +
                    "     values     (@as2_typeOfferte,@as2_LeverancierTransactie,@as2_nummerTransactie,@as2_nummerContract,@as2_nummerVoertuigKaart,@as2_typeVoertuigkaart,@as2_inschrijvingsnummer,@as2_vervaldatum,@as2_codeIngangPartner,@as2_ISOcodeland_ingang,@as2_NummerStation,@as2_muntCode,@as2_codeIngangsnelweg,@as2_naamIngang,@as2_nummerchauffeurkaart,@as2_naamchauffeur,@as2_datumtransactie,@as2_code_tolpartner,@as2_ISOcodeTolland,@as2_nummerTolstation,@as2_codeTolSnelweg,@as2_naamTolstation,@as2_muntCodeTol,@as2_datum_tol,@as2_typeTransactie,@as2_productcode,@as2_productnaam,@as2_hoeveelheid,@as2_kilometer,@as2_brutoprijs_allintank,@as2_nettoprijs_allintank,@as2_eenheidskorting_allintank,@as2_nettoExtank,@as2_BTWvoet,@as2_bedragExBTWtank,@as2_BTWtank,@as2_bedragAllintank,@as2_muntcodeBetaling)", SQLconn);
                SQLcmd.Parameters.AddWithValue("as2_typeOfferte", tostring(trans.offerType));
                SQLcmd.Parameters.AddWithValue("as2_LeverancierTransactie", tostring (trans.transactionInformation));
                SQLcmd.Parameters.AddWithValue("as2_nummerTransactie", tostring(trans.transactionNumber));
                SQLcmd.Parameters.AddWithValue("as2_nummerContract", "");
                SQLcmd.Parameters.AddWithValue("as2_nummerVoertuigKaart", tostring(trans.VCNumber));
                SQLcmd.Parameters.AddWithValue("as2_typeVoertuigkaart", tostring(trans.VCType));
                SQLcmd.Parameters.AddWithValue("as2_inschrijvingsnummer", tostring(trans.VCRegistrationNumber));
                SQLcmd.Parameters.AddWithValue("as2_vervaldatum", dt3);
                SQLcmd.Parameters.AddWithValue("as2_codeIngangPartner", ""+tostring(trans.entryPartnerCode));
                SQLcmd.Parameters.AddWithValue("as2_ISOcodeland_ingang", tostring(trans.entryISOCode));
                SQLcmd.Parameters.AddWithValue("as2_NummerStation", tostring(trans.entryIdNumber));
                SQLcmd.Parameters.AddWithValue("as2_muntCode", tostring(trans.entryCurrencyCode));
                SQLcmd.Parameters.AddWithValue("as2_codeIngangsnelweg", tostring(trans.entryMotorwayCode));
                SQLcmd.Parameters.AddWithValue("as2_naamIngang", tostring(trans.entryPlaceName));
                SQLcmd.Parameters.AddWithValue("as2_nummerchauffeurkaart", tostring(trans.DCNumber));
                SQLcmd.Parameters.AddWithValue("as2_naamchauffeur", tostring(trans.DCDriverName));
                SQLcmd.Parameters.AddWithValue("as2_datumtransactie", dt);
                if (dt > todt("2015-01-01"))
                    wait(1);
                SQLcmd.Parameters.AddWithValue("as2_code_tolpartner", tostring(trans.CIM));
                SQLcmd.Parameters.AddWithValue("as2_ISOcodeTolland", tostring(trans.exitISOCode));
                SQLcmd.Parameters.AddWithValue("as2_nummerTolstation", tostring(trans.exitPartnerCode));
                SQLcmd.Parameters.AddWithValue("as2_codeTolSnelweg", tostring(trans.exitMotorwayCode));
                SQLcmd.Parameters.AddWithValue("as2_naamTolstation", tostring(trans.exitPlaceName));
                SQLcmd.Parameters.AddWithValue("as2_muntCodeTol", tostring(trans.exitCurrencyCode));
                SQLcmd.Parameters.AddWithValue("as2_datum_tol", dt2);
                SQLcmd.Parameters.AddWithValue("as2_typeTransactie", tostring(trans.transactionType));
                SQLcmd.Parameters.AddWithValue("as2_productcode", tostring(trans.productCode));
                SQLcmd.Parameters.AddWithValue("as2_productnaam", tostring(trans.productDesignation));
                SQLcmd.Parameters.AddWithValue("as2_hoeveelheid", trans.quantity);
                SQLcmd.Parameters.AddWithValue("as2_kilometer", trans.mileage);
                SQLcmd.Parameters.AddWithValue("as2_brutoprijs_allintank", trans.totalPriceVATIncl);
                SQLcmd.Parameters.AddWithValue("as2_nettoprijs_allintank", trans.totalPriceVATExcl);
                SQLcmd.Parameters.AddWithValue("as2_eenheidskorting_allintank", trans.unitDiscountVATIncl);
                SQLcmd.Parameters.AddWithValue("as2_nettoExtank", 0);
                SQLcmd.Parameters.AddWithValue("as2_BTWvoet", trans.unitVATRate);
                SQLcmd.Parameters.AddWithValue("as2_bedragExBTWtank", trans.paymentVATExcl);
                SQLcmd.Parameters.AddWithValue("as2_BTWtank", trans.priceListVATIncl);
                SQLcmd.Parameters.AddWithValue("as2_bedragAllintank", trans.paymentVATIncl);
                SQLcmd.Parameters.AddWithValue("as2_muntcodeBetaling", tostring(trans.paymentCurrencyCode));

                SQLcmd.ExecuteNonQuery();

            }
            catch (SqlException ex)
            {
                MessageBox.Show("Webservice error: " + ex.Message);
                writelog("Webservice error: " + ex.Message);
            }
            return (0);
        }


        private void button1_Click(object sender, EventArgs e)
        {
            getAS24data("604007WS81282175", "86493101");
//            getAS24data("604018WS62886543", "63629877");
        }

        public bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public void getAS24data(String username, String password) {
        
            check_databaseopen();

            InfoServiceClient isc = null;
            try
            {
                //Force TLS
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;

                ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;
                ServicePointManager.Expect100Continue = true;


                isc = new InfoServiceClient();

                isc.ClientCredentials.UserName.UserName = username;
                isc.ClientCredentials.UserName.Password = password;

                var elements = isc.Endpoint.Binding.CreateBindingElements();

                var securityBindingElement = elements.Find<SecurityBindingElement>();
                securityBindingElement.IncludeTimestamp = false;

                isc.Endpoint.Binding = new CustomBinding(elements);


                isc.Open();

                transaction[] lt;
                
                
                lt = isc.getPendingTransactions();
                foreach (transaction trans in lt)
                {
                    Console.WriteLine(trans.clientId + " " + trans.transactionNumber + " " + trans.totalPriceVATIncl);
                    textBox1.Text += trans.clientId + " " + trans.transactionNumber + " " + trans.totalPriceVATIncl + "\r\n";
                    textBox1.Select(textBox1.Text.Length, 0); textBox1.ScrollToCaret();
                    process_transaction(trans);
                    wait(1);
                }


                int month = DateTime.Now.Month;
                int day = DateTime.Now.Day;
                DateTime checkdate = DateTime.Now.AddDays(-1);
                int checkmonth = checkdate.Month;
                int checkday = checkdate.Day;
                bool stoppen = false;
                while (!stoppen)
                {
                    if (checkmonth != month) stoppen= true;
                    if (checkday == 15) stoppen = true;
                    if (!stoppen)
                    {
                        checkdate = checkdate.AddDays(-1);
                        checkmonth = checkdate.Month;
                        checkday = checkdate.Day;
                    }
                }
                writelog("get transactions for invoicedate " + checkdate.ToString());
                
                lt = isc.getInvoicingTransactions(Convert.ToDateTime(checkdate));
                foreach (transaction trans in lt)
                {
                    Console.WriteLine(trans.clientId + " " + trans.transactionNumber + " " + trans.totalPriceVATIncl);
                    textBox1.Text += trans.clientId + " " + trans.transactionNumber + " " + trans.totalPriceVATIncl + "\r\n";
                    textBox1.Select(textBox1.Text.Length, 0); textBox1.ScrollToCaret();
                    process_transaction(trans);
                    wait(1);
                }
/*!!!!!!!!!!!!!!!!!!!
                lt = isc.getInvoicingTransactions(Convert.ToDateTime("2015-03-15"));
                foreach (transaction trans in lt)
                {
                    Console.WriteLine(trans.clientId + " " + trans.transactionNumber + " " + trans.totalPriceVATIncl);
                    textBox1.Text += trans.clientId + " " + trans.transactionNumber + " " + trans.totalPriceVATIncl + "\r\n";
                    textBox1.Select(textBox1.Text.Length, 0); textBox1.ScrollToCaret();
                    process_transaction(trans);
                }
*/
                //DateTime dt = Convert.ToDateTime("2014-12-01 13:00");
                //lt = isc.getNewTransactions();
                //lt = isc.getInvoicingTransactions(dt);
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.ToString());
            }

            finally
            {
                if (isc != null) isc.Close();
            }
            return;// ret;

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            formsetup._setup[0].str = textBox2.Text;
            formsetup.writesetup();
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            formsetup._setup[1].str = textBox3.Text;
            formsetup.writesetup();
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            formsetup._setup[2].str = textBox4.Text;
            formsetup.writesetup();
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            formsetup._setup[3].str = textBox5.Text;
            formsetup.writesetup();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            formsetup.readsetup();
            textBox2.Text = formsetup._setup[0].str;
            textBox3.Text = formsetup._setup[1].str;
            textBox4.Text = formsetup._setup[2].str;
            textBox5.Text = formsetup._setup[3].str;
            timer1.Enabled = true;
        }

        int timercnt=10;
        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            if (checkBox1.Checked)
            {
                timercnt--;
                checkBox1.Text = timercnt.ToString() + " seconds to wait";
                if (timercnt == 0)
                {
                    button1_Click(sender, e);
                    wait(1000);
                    Close();
                }
            }
            timer1.Enabled = true;
        }
    }

    public partial class formsetup
    {
        /*-------------------------------------------------
         *      string                int             double
         * 0    database server
         * 1    database name
         * 2    database userid
         * 3    database passwd
         * 
         * 50   
         * ..
         * 59   
         * 
         * 
         *--------------------------------------------------*/

        private const int MAXSETUP = 100;
        private const string FILE_NAME = "AS24.dat";

        public struct setupstruct
        {
            public string str;
            public int integer;
            public double dbl;
        }
        public static setupstruct[] _setup = new setupstruct[MAXSETUP];

        public static void writesetup()
        {
            FileStream fs = new FileStream(FILE_NAME, FileMode.Create);
            BinaryWriter w = new BinaryWriter(fs);      // Create the writer for data.
            for (int i = 0; i < MAXSETUP; i++)
            {
                w.Write(_setup[i].str + "");
                w.Write(_setup[i].integer);
                w.Write(_setup[i].dbl);
            }
            w.Close();
            fs.Close();
        }
        public static void readsetup()
        {
            string currentfolder = Directory.GetCurrentDirectory();

            if (!File.Exists(FILE_NAME))
            {
                return;
            }
            FileStream fs = new FileStream(FILE_NAME, FileMode.Open, FileAccess.ReadWrite);
            BinaryReader w = new BinaryReader(fs);      // Create the reader for data.
            try
            {
                for (int i = 0; i < MAXSETUP; i++)                // read data 
                {
                    _setup[i].str = w.ReadString();
                    _setup[i].integer = w.ReadInt32();
                    _setup[i].dbl = w.ReadDouble();
                }
            }
            catch { }
            fs.Close();
            w.Close();
        }
    }

}
