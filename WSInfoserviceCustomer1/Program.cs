using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WSInfoserviceCustomer1.WSInfoservice;
using System.ServiceModel.Channels;
using System.Net;

using System.Windows.Forms;

/*
Contract number : 604018


Login : 604018WS62886543 

Password : 63629877 




Contract number : 604007 

Login : 604007WS81282175 

Password : 86493101 


*/
namespace WSInfoserviceCustomer1
{
    class Program
    {
        static int process_transaction(transaction trans)
        {
            return (0);
        }


        static int Main(string[] args)
        {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

            return (1);

            String username = "604018WS62886543";
            String password = "63629877";
            int ret = 0;

            InfoServiceClient isc = null;
            try
            {
                //Force TLS
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;

                isc = new InfoServiceClient();

                isc.ClientCredentials.UserName.UserName = username;
                isc.ClientCredentials.UserName.Password = password;

                var elements = isc.Endpoint.Binding.CreateBindingElements();

                var securityBindingElement = elements.Find<SecurityBindingElement>();
                securityBindingElement.IncludeTimestamp = false;

                isc.Endpoint.Binding = new CustomBinding(elements);


                isc.Open();

                transaction[] lt = isc.getPendingTransactions();
                foreach (transaction trans in lt)
                {
                    Console.WriteLine(trans.clientId + " " + trans.transactionNumber + " " + trans.totalPriceVATIncl);
                    ret = 1;
                    process_transaction(trans);
                    Application.DoEvents();
                }

                //DateTime dt = Convert.ToDateTime("2014-12-01 13:00");
                //lt = isc.getNewTransactions();
                //lt = isc.getInvoicingTransactions(dt);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            finally
            {
                if (isc != null) isc.Close();
            }
            return ret;
        }
    }
}
