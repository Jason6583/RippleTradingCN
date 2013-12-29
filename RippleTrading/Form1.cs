using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Jayrock.Json;
using Jayrock.JsonRpc;
using RippleUtility;
using Newtonsoft;
using Newtonsoft.Json.Linq;
using System.Configuration;
using WebSocket4Net;
using SuperSocket.ClientEngine;

namespace RippleTrading
{
    public partial class Form1 : Form
    {

        private WebSocket wsClient;

        private double rippleXRPBal = 0;

        private OrderBook[] books;
        private OfferBook[] offers;

        double cnXRPBid;
        double cnXRPBidQty;
        double cnXRPAsk;
        double cnXRPAskQty;

        double bsXRPBid;
        double bsXRPBidQty;
        double bsXRPAsk;
        double bsXRPAskQty;

        double bsUSDBid;
        double bsUSDBidQty;
        double bsUSDAsk;
        double bsUSDAskQty;

        bool isAutoRefresh = false;

        delegate void msgHandler(string ID, JToken msg);

        int pageLimit = 8;


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            // Initialize Websocket client
            wsClient = new WebSocket(RConfig.WSServer1);
            wsClient.Opened += new EventHandler(websocket_Opened);
            wsClient.Error += new EventHandler<ErrorEventArgs>(websocket_Error);
            wsClient.Closed += new EventHandler(websocket_Closed);
            wsClient.MessageReceived += new EventHandler<MessageReceivedEventArgs>(websocket_MessageReceived);
            wsClient.Open();

            // Initialize other UI
            labelAddress.Text = ConfigurationManager.AppSettings["rippleAddress"];
            dataGridView1.AutoGenerateColumns = false;
            dataGridView2.AutoGenerateColumns = false;
            dataGridView3.AutoGenerateColumns = false;
            dataGridView4.AutoGenerateColumns = false;
            dataGridView5.AutoGenerateColumns = false;
            dataGridView6.AutoGenerateColumns = false;

            int x;
            if(int.TryParse(textBox1.Text.Trim(),out x)){
                pageLimit = x;
            }

            // Initalize the books
            books = new OrderBook[3];
            books[0] = new OrderBook(wsClient, 0, "XRP", "CNY", "", RConfig.gwRippleCN);
            books[1] = new OrderBook(wsClient, 1, "XRP", "USD", "", RConfig.gwBitstamp);
            books[2] = new OrderBook(wsClient, 2, "USD", "CNY", RConfig.gwBitstamp, RConfig.gwRippleCN);

            offers = new OfferBook[3];
            offers[0] = new OfferBook(wsClient, 0, "XRP", "CNY", "", RConfig.gwRippleCN);
            offers[1] = new OfferBook(wsClient, 1, "XRP", "USD", "", RConfig.gwBitstamp);
            offers[2] = new OfferBook(wsClient, 2, "USD", "CNY", RConfig.gwBitstamp, RConfig.gwRippleCN);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //string oAccountTX = JsonRequest.getResponse("account_tx", jClient, JsonRequest.getAccountTXJsonRequest(rippleAddress));
            //JObject jAccountTX = JObject.Parse(oAccountTX);
            //JObject[] arrAccountTX = jAccountTX["transactions"].ToObject<JObject[]>();

            fetchBook();

            fetchOffer();
        }

        public void fetchBook()
        {
            int x;
            if (int.TryParse(textBox1.Text.Trim(), out x))
            {
                pageLimit = x;
            }

            foreach (OrderBook b in books)
            {
                b.fetchBook(pageLimit);
            }
        }

        public void fetchBookHandler(string ID, JToken msg)
        {
            dataGridView1.DataSource = books[0].dtBid;
            dataGridView2.DataSource = books[0].dtAsk;
            dataGridView4.DataSource = books[1].dtBid;
            dataGridView3.DataSource = books[1].dtAsk;
            dataGridView6.DataSource = books[2].dtBid;
            dataGridView5.DataSource = books[2].dtAsk;

            // refresh rates
            cnXRPBid = books[0].bestBid;
            cnXRPBidQty = books[0].bestBidQty;
            cnXRPAsk = books[0].bestAsk;
            cnXRPAskQty = books[0].bestAskQty;

            bsXRPBid = books[1].bestBid;
            bsXRPBidQty = books[1].bestBidQty;
            bsXRPAsk = books[1].bestAsk;
            bsXRPAskQty = books[1].bestAskQty;

            bsUSDBid = books[2].bestBid;
            bsUSDBidQty = books[2].bestBidQty;
            bsUSDAsk = books[2].bestAsk;
            bsUSDAskQty = books[2].bestAskQty;


            // Display P&L
            refreshPL();

            labelLastUpdate.Text = "Last update : " + DateTime.Now.ToString("hh:mm:ss");

        }


        public void fetchOffer()
        {
            string req = WSRequest.getRequest("account_offers", "", JsonRequest.getAccountOffersJsonRequest(RConfig.rippleAddress));
            wsClient.Send(req);
        }

        private void refreshOfferHandler(String ID, JToken msg)
        {
            dataGridView8.DataSource = offers[0].dtBid;
            dataGridView9.DataSource = offers[0].dtAsk;
            dataGridView11.DataSource = offers[1].dtBid;
            dataGridView10.DataSource = offers[1].dtAsk;
            dataGridView13.DataSource = offers[2].dtBid;
            dataGridView12.DataSource = offers[2].dtAsk;
        }


        private void refreshbalance()
        {
            string req = WSRequest.getRequest("account_info", "Balance", JsonRequest.getAccountInfoJsonRequest(RConfig.rippleAddress));
            wsClient.Send(req);

            req = WSRequest.getRequest("account_lines", "Balance", JsonRequest.getAccountLinesJsonRequest(RConfig.rippleAddress));
            wsClient.Send(req);
        }

        private void refreshBalanceHandler(String ID, JToken msg)
        {
            if (ID == "account_infoBalance")
            {
                //XRP 
                rippleXRPBal = msg["account_data"]["Balance"].ToObject<double>();
                dgvBalance.Rows.Add(new object[] { "XRP", rippleXRPBal / RConfig.MILLION });

            }
            else if (ID == "account_linesBalance")
            {
                //IOU
                JObject[] arrLines = msg["lines"].ToObject<JObject[]>();

                foreach (JObject jo in arrLines)
                {
                    dgvBalance.Rows.Add(new object[] { jo["currency"].ToString(), jo["balance"].ToString() });
                }
            }

            btnGetBal.Enabled = true;
        }

        private void refreshPL()
        {
            double totalValue = 0;

            foreach (DataGridViewRow dgvr in dgvBalance.Rows)
            {
                if (dgvr.Cells[0].Value.ToString() == "XRP")
                {
                    totalValue += double.Parse(dgvr.Cells[1].Value.ToString()) * (cnXRPBid + cnXRPAsk) / 2;

                }else if  (dgvr.Cells[0].Value.ToString() == "CNY")
                {
                    totalValue += double.Parse(dgvr.Cells[1].Value.ToString());
                }
                else if (dgvr.Cells[0].Value.ToString() == "USD")
                {
                    totalValue += double.Parse(dgvr.Cells[1].Value.ToString()) * (bsUSDBid + bsUSDAsk) / 2;
                }
            }

            label16.Text = string.Format("{0:N2}", totalValue );
            if (totalValue > 0)
            {
                label16.ForeColor = Color.Green;
            }
            else
            {
                label16.ForeColor = Color.Red;
            }
        }


        #region  Websocket events

        private void websocket_Opened(object sender, EventArgs e)
        {
           
            this.Invoke(
                (MethodInvoker)delegate
                {
                    toolStripStatusLabel1.Text = ("Connection Opened");

                    btnGetBook.Enabled = true;

                    //Get initial balance
                    refreshbalance();
                },
                new object[] { });
        }

        private void websocket_Error(object sender, EventArgs e)
        {
            this.Invoke(
              (MethodInvoker)delegate
              {
                  toolStripStatusLabel1.Text = ("Error : " + e.ToString());
              },
              new object[] { });

        }

        private void websocket_Closed(object sender, EventArgs e)
        {

            this.Invoke(
          (MethodInvoker)delegate
          {
              toolStripStatusLabel1.Text = "Connection Closed";
          },
          new object[] { });


        }

        private void websocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {

            JObject jo = JObject.Parse(e.Message);
            string status = jo["status"].ToString();

            if (status == "error")
            {
                toolStripStatusLabel1.Text = (jo["error_code"].ToString() + ":" + jo["error_message"].ToString());
                return;
            }

            string ID = jo["id"].ToString();
            JToken msg = jo["result"];

            if (ID == "account_infoBalance")
            {
                this.Invoke(new msgHandler(refreshBalanceHandler), new object[] { ID, msg });
            }
            else if (ID == "account_linesBalance")
            {
                this.Invoke(new msgHandler(refreshBalanceHandler), new object[] { ID, msg });
            }
            else if (ID == "account_offers")
            {
                foreach (OfferBook ob in offers)
                {
                    ob.fetchBook(msg);
                }

                this.Invoke(new msgHandler(refreshOfferHandler), new object[] { ID, msg });
            }
            else if (ID.StartsWith("book_offers"))
            {
                string side = ID.Substring(11, 3);
                int bookID = int.Parse(ID.Substring(14));

                if (side == "bid")
                {
                    books[bookID].fetchBookBid(msg);
                }
                else if (side == "ask")
                {
                    books[bookID].fetchBookAsk(msg);
                }

                // Check if all books are ready to use
                bool isBooksReady = true;
                foreach (OrderBook b in books)
                {
                    if (!b.isReady())
                    {
                        isBooksReady = false;
                        break;
                    }
                }

                if (isBooksReady == true)
                {
                    this.Invoke(new msgHandler(fetchBookHandler), new object[] { ID, msg });
                }
            }
            else if (ID.StartsWith("submit"))
            {
                toolStripStatusLabel1.Text = "Order submitted";
            }
        }

        #endregion


        #region Form events

        private void btnGetBal_Click(object sender, EventArgs e)
        {
            dgvBalance.Rows.Clear();
            refreshbalance();
        }

        #endregion


        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

            double val = (double)dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            if (e.ColumnIndex == 0)
            {
                textBox5.Text = val.ToString();
            }
            else
            {
                textBox3.Text = val.ToString();
            }
        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            double val = (double)dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            if (e.ColumnIndex == 0)
            {
                textBox3.Text = val.ToString();
            }
            else
            {
                textBox5.Text = val.ToString();
            }
        }

        private void dataGridView4_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            double val = (double)dataGridView4.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            if (e.ColumnIndex == 0)
            {
                textBox4.Text = val.ToString();
            }
            else
            {
                textBox6.Text = val.ToString();
            }

        }

        private void dataGridView3_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            double val = (double)dataGridView3.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            if (e.ColumnIndex == 0)
            {
                textBox6.Text = val.ToString();
            }
            else
            {
                textBox4.Text = val.ToString();
            }

        }


        private void dataGridView6_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            double val = (double)dataGridView6.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            if (e.ColumnIndex == 0)
            {
                textBox7.Text = val.ToString();
            }
            else
            {
                textBox8.Text = val.ToString();
            }
        }

        private void dataGridView5_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            double val = (double)dataGridView5.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            if (e.ColumnIndex == 0)
            {
                textBox8.Text = val.ToString();
            }
            else
            {
                textBox7.Text = val.ToString();
            }

        }

        private void button6_Click(object sender, EventArgs e)
        {
            OrderRequest.submitOrder(wsClient,
             "XRP", "", (int)(RConfig.MILLION * double.Parse(textBox5.Text)),
             "CNY", RConfig.gwRippleCN, double.Parse(textBox3.Text) * double.Parse(textBox5.Text));
        }

        private void button5_Click(object sender, EventArgs e)
        {
            OrderRequest.submitOrder(wsClient,
             "CNY", RConfig.gwRippleCN, double.Parse(textBox3.Text) * double.Parse(textBox5.Text),
             "XRP", "", (int)(RConfig.MILLION * double.Parse(textBox5.Text))
           );
        }

        private void button7_Click(object sender, EventArgs e)
        {
            OrderRequest.submitOrder(wsClient,
                "XRP", "", (int)(RConfig.MILLION * double.Parse(textBox4.Text)),
                "USD", RConfig.gwBitstamp, double.Parse(textBox4.Text) * double.Parse(textBox6.Text));
        }

        private void button8_Click(object sender, EventArgs e)
        {
            OrderRequest.submitOrder(wsClient,
                "USD", RConfig.gwBitstamp, double.Parse(textBox4.Text) * double.Parse(textBox6.Text),
                "XRP", "", (int)(RConfig.MILLION * double.Parse(textBox4.Text)));
        }

        private void button9_Click(object sender, EventArgs e)
        {
            OrderRequest.submitOrder(wsClient,
                "USD", RConfig.gwBitstamp, double.Parse(textBox7.Text),
                "CNY", RConfig.gwRippleCN, double.Parse(textBox7.Text) * double.Parse(textBox8.Text));
        }

        private void button10_Click(object sender, EventArgs e)
        {
            OrderRequest.submitOrder(wsClient,
                "CNY", RConfig.gwRippleCN, double.Parse(textBox7.Text) * double.Parse(textBox8.Text),
                "USD", RConfig.gwBitstamp, double.Parse(textBox7.Text));
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (wsClient.State == WebSocketState.Open)
            {
                wsClient.Close();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            fetchBook();
            fetchOffer();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (isAutoRefresh)
            {
                isAutoRefresh = false;
                button11.Text = "Start";
                textBox9.Enabled = true;
                this.timer1.Stop();
            }
            else
            {
                string frequency = textBox9.Text.Trim();
                int intFreq;
                if (int.TryParse(frequency, out intFreq))
                {

                    if (intFreq <= 10)
                    {
                        this.toolStripStatusLabel1.Text = "Interval is too small";
                        return;
                    }

                    isAutoRefresh = true;
                    button11.Text = "Stop";
                    textBox9.Enabled = false;
                    this.timer1.Interval = intFreq * 1000;
                    this.timer1.Start();
                }
                else
                {
                    this.toolStripStatusLabel1.Text = "Invalid number of seconds";
                }

            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            AboutBox1 a = new AboutBox1();
            a.ShowDialog();
        }

    }
}
