using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VEDriversLite;

namespace VENFTApp_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NeblioAccount account;
        public MainWindow()
        {
            InitializeComponent();
            account = new NeblioAccount();
        }

        private void btnCreateNewAddress_Click(object sender, RoutedEventArgs e)
        {
            account.CreateNewAccount("mypass");
        }

        private void btnSendTokenTx_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(account.Address))
                account.LoadAccount("mypass");

            var receiver = "NPWBL3i8ZQ8tmhDtrixXwYd93nofmunvhA";
            // create token metadata
            var metadata = new Dictionary<string, string>();
            metadata.Add("Data", "My first metadata in token with VEDriversLite");

            // fill input data for sending tx
            var dto = new SendTokenTxData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Amount = 1,
                Id = "La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8", // id of token
                Symbol = "VENFT", // symbol of token
                Metadata = metadata,
                Password = "mypass",
                SenderAddress = account.Address,
                ReceiverAddress = receiver
            };

            var txid = string.Empty;
            try
            {
                // send tx
                txid = NeblioTransactionHelpers.SendNTP1TokenAPI(dto, account);
                if (txid != null)
                    sentTxId.Content = txid;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot send neblio transaction: " + ex.Message);
                MessageBox.Show("Cannot sent neblio transaction: " + ex.Message);
            }
        }

        private void btnLoadAddress_Click(object sender, RoutedEventArgs e)
        {
            account.LoadAccount("mypass");
        }
    }
}
