using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using IBTradeRealTime.backend;
using IBTradeRealTime.message;

using IBApi;
using IBTradeRealTime.util;
using IBTradeRealTime.app;
using System.Collections.Concurrent;
using IBTradeRealTime.MarketData;
using IBTradeRealTime.AppOrders;
using System.Reflection;

namespace IBTradeRealTime.UI
{
    public partial class IBTradeApp : Form, IIBTradeAppBridge
    {
        delegate void MessageHandlerDelegate(IBMessage message);
        delegate void StrategOnOffDelegate(StrategyOnOff onOff);
        delegate void DailyResetDelegate();
        //delegate List<String[]> GetPosSummaryDelegate();

        private IBClient ibClient;

        private AccountDetailManager accountManager;
        private MarketDataManager marketDataManager;
        RealTimeBarsManager realTimeBarManager;
        private HistoricalDataManager historicalDataManager;
        private OrderManager orderManager;
        public AppStrategyManager appStrategyManager;

        private InMemoryNumIDFactory numFactory;
        private int orderId;
        private bool isConnected = false;

        public IBTradeApp()
        {
            initProperties();
            appStrategyManager = new AppStrategyManager(this);           
            InitializeComponent();
            ibClient = new IBClient(this);       
            readOrderFromFiles();
            orderManager = new OrderManager(ibClient, grid_open_order, grid_order_summary, this);
            appStrategyManager.setOrderManager(orderManager);
            appStrategyManager.ibClient = ibClient;
            accountManager = new AccountDetailManager(ibClient,  grid_account_details, grid_pos_summary, this);
            marketDataManager = new MarketDataManager(ibClient, grid_market_price, appStrategyManager);
            realTimeBarManager = new RealTimeBarsManager(ibClient, null, grid_rt_bar, appStrategyManager.getAppMDManager(), this);            
            historicalDataManager = new HistoricalDataManager(ibClient, null, grid_hist_bar, this);            
            tabMain.SelectedTab = tabOrderStatus;
            initTickerInfo();
        }

        public void dailyReset()
        {
            if (this.InvokeRequired)
            {
                DailyResetDelegate callback = new DailyResetDelegate(dailyReset);
                this.Invoke(callback);
            }
            else
            {
                dailyResetImpl();
            }
        }

        public void dailyResetImpl()
        {
            grid_open_order.Rows.Clear();
            grid_order_summary.Rows.Clear();
            grid_hist_bar.Rows.Clear();
            //grid_market_price.Columns.Clear();
            grid_rt_bar.Rows.Clear();
        }
        /*
        public List<String[]> getPositionSummary()
        {
            if (this.InvokeRequired)
            {
                GetPosSummaryDelegate callback = new GetPosSummaryDelegate(getPositionSummary);
                return this.Invoke(callback);
            }
            else
            {
                return getPositionSummaryImpl();
            }
        }


        public List<String[]> getPositionSummaryImpl()
        {
            List<String[]> summary = new List<String[]>();
            foreach (DataGridViewRow dr in grid_pos_summary.Rows)
            {
                string col1 = dr.Cells[0].Value.ToString();
                string col2 = dr.Cells[1].Value.ToString();
                string col3 = dr.Cells[2].Value.ToString();
                string col4 = dr.Cells[2].Value.ToString();
                String[] sumRow = new String[4];
                sumRow[0] = col1;
                sumRow[1] = col2;
                sumRow[2] = col3;
                sumRow[3] = col4;
                summary.Add(sumRow);
            }
            return summary;
        }*/

        public List<String[]> getPositionSummary()
        {
            List<String[]> summary = new List<String[]>();
            foreach (DataGridViewRow dr in grid_pos_summary.Rows)
            {
                string col1 = dr.Cells[0].Value.ToString();
                string col2 = dr.Cells[1].Value.ToString();
                string col3 = dr.Cells[2].Value.ToString();
                string col4 = dr.Cells[2].Value.ToString();
                String[] sumRow = new String[4];
                sumRow[0] = col1;
                sumRow[1] = col2;
                sumRow[2] = col3;
                sumRow[3] = col4;
                summary.Add(sumRow);
            }
            return summary;
        }

        public void initProperties()
        {
            String file = @"..\config\setup.txt";
            foreach (String line in System.IO.File.ReadAllLines(file))
            {
                if ((!String.IsNullOrEmpty(line)) &&
                    (!line.StartsWith(";")) &&
                    (!line.StartsWith("#")) &&
                    (!line.StartsWith("'")) &&
                    (line.Contains('=')))
                {
                    int index = line.IndexOf('=');
                    String key = line.Substring(0, index).Trim();
                    String value = line.Substring(index + 1).Trim();

                    if("FILE_ORDER_REP".Equals(key)){
                        AppConstant.FILE_ORDER_REP = value;
                    }
                    if("FILE_POSITION_RESULT_PREFIX".Equals(key)){
                        AppConstant.FILE_POSITION_RESULT_PREFIX = value;
                    }
                        /*
                    if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                        (value.StartsWith("'") && value.EndsWith("'")))
                    {
                        value = value.Substring(1, value.Length - 2);
                    }

                    try
                    {
                        //ignore dublicates
                        list.Add(key, value);
                    }
                    catch { }*/
                }
            }

        }

        //2015-10-27
        private Boolean isHistDataRequested(IBMessage message)
        {
            IAppMDManager appMDMManager = appStrategyManager.getAppMDManager();
            switch (message.Type)
            {
                case MessageType.HistoricalData:
                    HistoricalDataMessage hMessage = (HistoricalDataMessage)message;
                    if (appMDMManager.isHistDataAdHocRequested(hMessage.RequestId))
                        return true;
                    break;
                case MessageType.HistoricalDataEnd:
                    return false;
                    break; 
            }
            return false;
        }

        private void initTickerInfo()
        {
            cmb_ticker_profile.Items.Add(AppConstant.TICK1_NAME);
            cmb_ticker_profile.Items.Add(AppConstant.TICK2_NAME);
            cmb_ticker_profile.SelectedItem = AppConstant.TICK1_NAME;
            TickerInfo info = genTickerInfo(1);
            setTickerInfo(info);
        }

        private TickerInfo genTickerInfo(int i){
            TickerInfo info = new TickerInfo();
            if (i == 1)
            {
                info.tickerID = AppConstant.TICK1_ID;
                info.contractID = AppConstant.TICK1_CONTRACT_ID;
                info.symbol = AppConstant.TICK1_SYMBOL;
                info.type = AppConstant.TICK1_TYPE;
                info.exchange = AppConstant.TICK1_EXCHANGE;
                info.pExchange = AppConstant.TICK1_P_EXCHANGE;
                info.currency = AppConstant.TICK1_CURRENCY;
                info.lSymbol = AppConstant.TICK1_L_SYMBOL;
                info.whatToShow = AppConstant.TICK1_WTS;
                info.startTime = AppConstant.TICK1_START_TIME;
                info.lunchEndTime = AppConstant.TICK1_LUNCH_TIME_END;
                info.endTime = AppConstant.TICK1_END_TIME;
            }
            if (i == 2)
            {
                info.tickerID = AppConstant.TICK2_ID;
                info.contractID = AppConstant.TICK2_CONTRACT_ID;
                info.symbol = AppConstant.TICK2_SYMBOL;
                info.type = AppConstant.TICK2_TYPE;
                info.exchange = AppConstant.TICK2_EXCHANGE;
                info.pExchange = AppConstant.TICK2_P_EXCHANGE;
                info.currency = AppConstant.TICK2_CURRENCY;
                info.lSymbol = AppConstant.TICK2_L_SYMBOL;
                info.whatToShow = AppConstant.TICK2_WTS;
                info.startTime = AppConstant.TICK2_START_TIME;
                info.lunchEndTime = AppConstant.TICK2_LUNCH_TIME_END;
                info.endTime = AppConstant.TICK2_END_TIME;
            }
            return info;
        }

        public IRealTimeBarsManagerBridge getRealTimeBarsManager(){
            return realTimeBarManager;
        }
        public String GetWTS()
        {
            return txt_ticker_wts.Text;
        }

        public void setTickerInfo(TickerInfo info)
        {
            txt_ticker_id.Text = info.tickerID;
            txt_ticker_contract_id.Text = info.contractID;
            txt_ticker_symbol.Text = info.symbol;
            txt_ticker_type.Text = info.type;
            txt_ticker_exch.Text = info.exchange;
            txt_ticker_p_exch.Text = info.pExchange;
            txt_ticker_curr.Text = info.currency;
            txt_ticker_local_symb.Text = info.lSymbol;
            txt_ticker_wts.Text = info.whatToShow;
            txt_ticker_startTime.Text = info.startTime;
            txt_ticker_lunchEndTime.Text = info.lunchEndTime;
            txt_ticker_endTime.Text = info.endTime;
        }

        private void readOrderFromFiles()
        {
            IAppOrderManager appOrderManager = appStrategyManager.getAppOrderManager();
            List<OrderRecord> orders = OrderFileReader.getOrderRecorderFromFile(AppConstant.FILE_ORDER_REP);
            foreach(OrderRecord record in orders){
                appOrderManager.OrderRepositry.AddOrUpdate(record.orderId.ToString(), record, (kye, oldValue) => oldValue);
            }
        }
        public void setIDFactory(int seedID)
        {
            numFactory = new InMemoryNumIDFactory(seedID);

        }

        public bool IsConnected
        {
            get { return isConnected; }
            set { isConnected = value; }
        }

        public void showMessage(String message)
        {
            this.txt_console.Text = message;
        }

        public void HandleMessage(IBMessage message)
        {
            if (this.InvokeRequired)
            {
                MessageHandlerDelegate callback = new MessageHandlerDelegate(HandleMessage);
                this.Invoke(callback, new object[] { message });
            }
            else
            {
                UpdateUI(message);
            }
        }

        public void HandleStrategyOnOff(StrategyOnOff onOff)
        {
            if (this.InvokeRequired)
            {
                StrategOnOffDelegate callback = new StrategOnOffDelegate(HandleStrategyOnOff);
                this.Invoke(callback, new object[] { onOff });
            }
            else
            {
                Button onOffButton = this.Controls.Find("btn_stg" + (onOff.stgIndex + 1) + "_onoff", true).FirstOrDefault() as Button;
                if (onOff.isOn)
                {
                    onOffButton.BackColor = Color.LightGreen;
                    onOffButton.Text = "ON";
                }
                else
                {
                    onOffButton.BackColor = Color.Red;
                    onOffButton.Text = "OFF";
                }
            }
        }


        private void UpdateUI(IBMessage message)
        {
            switch (message.Type)
            {
                case MessageType.ConnectionStatus:
                    {
                        ConnectionStatusMessage statusMessage = (ConnectionStatusMessage)message;
                        if (statusMessage.IsConnected)
                        {
                            btn_connect_light.BackColor = Color.LightGreen;
                            btn_connect_light.Text = "ON";
                            accountManager.RequestAccountSummary();
                            
                            grid_order_summary.Rows.Clear();
                            ibClient.ClientSocket.reqExecutions(1, new ExecutionFilter());
                            ibClient.ClientSocket.reqCurrentTime();
                            //grid_open_order.Rows.Clear();
                            //ibClient.ClientSocket.reqAllOpenOrders();
                        }
                        else
                        {
                            btn_connect_light.BackColor = Color.Red;
                            btn_connect_light.Text = "OFF";

                        }
                        break;
                    }

                case MessageType.ManagedAccounts:
                    {
                        accountManager.ManagedAccounts = ((ManagedAccountsMessage)message).ManagedAccounts;
                        accountManager.SubscribeAccountUpdates();
                        break;
                    }

                case MessageType.AccountValue:
                    {
                        accountManager.UpdateUI(message);
                        break;
                    }

                case MessageType.TickOptionComputation:
                case MessageType.TickPrice:
                case MessageType.TickSize:
                    {
                        HandleTickMessage((MarketDataMessage)message);
                        break;
                    }

                case MessageType.RealTimeBars:
                    {
                        realTimeBarManager.UpdateUI(message);
                        break;
                    }

                case MessageType.HistoricalData:
                case MessageType.HistoricalDataEnd:
                    {
                        IAppMDManager appMDMManager = appStrategyManager.getAppMDManager();
                        //2015-10-27
                        if (isHistDataRequested(message))
                        {
                            appMDMManager.updateHistDataAdHoc((HistoricalDataMessage)message);
                        }
                        else
                        {
                            historicalDataManager.UpdateUI(message);
                        }

                        //historicalDataManager.UpdateUI(message);
                        break;
                    }

                case MessageType.PortfolioValue:
                    {
                        accountManager.UpdateUI(message);
                        break;
                    }

                case MessageType.OpenOrder:
                case MessageType.OpenOrderEnd:
                case MessageType.OrderStatus:
                case MessageType.ExecutionData:
                case MessageType.CommissionsReport:
                    {
                        if (message.Type == MessageType.ExecutionData)
                        {
                            orderManager.processExecution((ExecutionMessage)message);
                        }
                    //HandleExecutionMessage((ExecutionMessage)message);

                        orderManager.UpdateUI(message);
                        break;
                    }

                case MessageType.ServerTime:
                    {
                        ServerTimeMessage tm =(ServerTimeMessage) message;
                        appStrategyManager.calculateTimeDiffServer(tm.Time);
                        //strategyManager.calculateTimeDiffServer(tm.Time);
                        break;
                    }

                case MessageType.Error:
                    {
                        ErrorMessage error = (ErrorMessage)message;
                        ShowMessageOnPanel("Request " + error.RequestId + ", Code: " + error.ErrorCode + " - " + error.Message + "\r\n");
                        HandleErrorMessage(error);
                        break;
                    }
            }
        }


        private void HandleTickMessage(MarketDataMessage tickMessage)
        {
                marketDataManager.UpdateUI(tickMessage);
        }

        private void HandleErrorMessage(ErrorMessage message)
        {
            /*
            if (message.RequestId > MarketDataManager.TICK_ID_BASE && message.RequestId < DeepBookManager.TICK_ID_BASE)
                marketDataManager.NotifyError(message.RequestId);
            else if (message.RequestId > DeepBookManager.TICK_ID_BASE && message.RequestId < HistoricalDataManager.HISTORICAL_ID_BASE)
                deepBookManager.NotifyError(message.RequestId);
            else if (message.RequestId == ContractManager.CONTRACT_DETAILS_ID)
            {
                contractManager.HandleRequestError(message.RequestId);
                searchContractDetails.Enabled = true;
            }
            else if (message.RequestId == ContractManager.FUNDAMENTALS_ID)
            {
                contractManager.HandleRequestError(message.RequestId);
                fundamentalsQueryButton.Enabled = true;
            }
            else if (message.RequestId == OptionsManager.OPTIONS_ID_BASE)
            {
                optionsManager.Clear();
                queryOptionChain.Enabled = true;
            }
            else if (message.RequestId > OptionsManager.OPTIONS_ID_BASE)
            {
                queryOptionChain.Enabled = true;
            }
            if (message.ErrorCode == 202)
            {
            }
             */
        }

        private void ShowMessageOnPanel(string message)
        {
            String oldText = rtxt_error_msg.Text;
            if (oldText.Length> 200)
                rtxt_error_msg.Text = "";
            this.rtxt_error_msg.Text += (message);
            rtxt_error_msg.Select(rtxt_error_msg.Text.Length, 0);
            rtxt_error_msg.ScrollToCaret();
        }

        private void btn_connect_Click(object sender, EventArgs e)
        {
            if (!IsConnected)
            {
                int port;
                string host = this.txt_host.Text;

                if (host == null || host.Equals(""))
                    host = "127.0.0.1";
                try
                {
                    port = Int32.Parse(this.txt_port.Text);
                    ibClient.ClientId = Int32.Parse(this.txt_client_id.Text);
                    ibClient.ClientSocket.eConnect(host, port, ibClient.ClientId);
                }
                catch (Exception)
                {
                   // HandleMessage(new ErrorMessage(-1, -1, "Please check your connection attributes."));
                    rtxt_error_msg.Text = "Please check your connection attributes.";
                }
            }
            else
            {
                IsConnected = false;
                ibClient.ClientSocket.eDisconnect();
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            ibClient.ClientSocket.eDisconnect();
        }

        private void grid_market_price_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void btn_req_mkt_data_Click(object sender, EventArgs e)
        {
            if (isConnected)
            {
                Contract contract = GetMDContract();
                TickerInfo tickerInfo = getTickerInfo();
                IAppMDManager appMDManager = appStrategyManager.getAppMDManager();
                appMDManager.tickerInfo = tickerInfo;
                string genericTickList = "";
                marketDataManager.AddRequest(contract, genericTickList);
                string whatToShow = txt_ticker_wts.Text;
                if (AppConstant.USE_OUT_REGULAR_DATA)
                    realTimeBarManager.AddRequest(contract, whatToShow, false);
                else
                    realTimeBarManager.AddRequest(contract, whatToShow, true);
            } 
        }

        public void addHistDataRequest(Contract contract, String strEndTime, String duration, String barSize, String whatToShow, int outsideRTH)
        {
            historicalDataManager.AddRequest(contract, strEndTime, duration, barSize, whatToShow, outsideRTH, 1);
        }

        //2015-10-27
        public int addHistDataRequestWithID(Contract contract, String strEndTime, String duration, String barSize, String whatToShow, int outsideRTH)
        {
            int reqID = historicalDataManager.AddRequestWithID(contract, strEndTime, duration, barSize, whatToShow, outsideRTH, 1);
            return reqID;
        }

        public Contract GetMDContract()
        {
            Contract contract = new Contract();
            contract.SecType = this.txt_ticker_type.Text;
            contract.Symbol = this.txt_ticker_symbol.Text;
            contract.Exchange = this.txt_ticker_exch.Text;
            contract.Currency = this.txt_ticker_curr.Text;
            contract.PrimaryExch = this.txt_ticker_p_exch.Text;
            contract.LocalSymbol = this.txt_ticker_local_symb.Text;
            return contract;
        }



        private Order GetOrder(String action, String orderType)
        {
            Order order = new Order();
            //orderId = numFactory.getNext();
            if (orderId != 0)
                order.OrderId = orderId;
            order.Action = action;
            order.OrderType = orderType;

            //***testing
            if (!txt_test_buyprice.Text.Equals(""))
                order.LmtPrice = Double.Parse(txt_test_buyprice.Text);
            //if (!quantity.Text.Equals(""))
            //    order.TotalQuantity = Int32.Parse(quantity.Text);
            order.TotalQuantity = 1;
            //order.Account = account.Text;
            order.Account = appStrategyManager.UserAccount;
            //order.Tif = timeInForce.Text;
            order.Tif = "DAY";
            //if (!auxPrice.Text.Equals(""))
            //    order.AuxPrice = Double.Parse(auxPrice.Text);
            //if (!displaySize.Text.Equals(""))
            //    order.DisplaySize = Int32.Parse(displaySize.Text);
            order.TriggerMethod = 0;
            order.Transmit = true;
            return order;
        }

        private void btn_ref_execution_Click(object sender, EventArgs e)
        {
            grid_order_summary.Rows.Clear();
            ibClient.ClientSocket.reqExecutions(1, new ExecutionFilter());
        }

        private void btn_open_order_Click(object sender, EventArgs e)
        {
            grid_open_order.Rows.Clear();
            ibClient.ClientSocket.reqAllOpenOrders();
        }

        private void btn_test_buy_Click(object sender, EventArgs e)
        {
            IAppOrderManager appOrderManager = appStrategyManager.getAppOrderManager();
            Contract contract = GetMDContract();
            Order order = GetOrder("BUY", "LMT");
            //order.OrderId = ibClient.NextOrderId;
            //ibClient.NextOrderId++;
            order.OrderId = ibClient.IncreaseOrderId();
            orderManager.PlaceOrder(contract, order);
            txt_console.Text = order.OrderId.ToString();
            OrderRecord orderR = new OrderRecord();
            orderR.orderId = order.OrderId;
            orderR.sno = txt_sno.Text;
            orderR.orderTime = DateTime.Now;
            //orderRepositry.Add(order.OrderId.ToString(), orderR);
            appOrderManager.OrderRepositry.AddOrUpdate(order.OrderId.ToString(), orderR, (key, oldValue) => oldValue);
            List<OrderRecord> orderRecords = new List<OrderRecord>();
            orderRecords.Add(orderR);
            appStrategyManager.getAppOrderManager().OrderPersister.SaveLastRow(orderRecords);
            //if (orderId != 0)
            //    orderId = 0;
        }

        private void btn_test_sell_Click(object sender, EventArgs e)
        {
            IAppOrderManager appOrderManager = appStrategyManager.getAppOrderManager();
            Contract contract = GetMDContract();
            Order order = GetOrder("SELL", "LMT");

            order.OrderId = ibClient.NextOrderId;
            ibClient.NextOrderId++;
            orderManager.PlaceOrder(contract, order);
            //if (orderId != 0)
            //    orderId = 0;
            txt_console.Text = order.OrderId.ToString();
            OrderRecord orderR = new OrderRecord();
            orderR.orderId = order.OrderId;
            orderR.sno = txt_sno.Text;
            orderR.orderTime = DateTime.Now;
            //orderRepositry.Add(order.OrderId.ToString(), orderR);
            appOrderManager.OrderRepositry.AddOrUpdate(order.OrderId.ToString(), orderR, (KeyDown, oldValue) => oldValue);
            List<OrderRecord> orderRecords = new List<OrderRecord>();
            orderRecords.Add(orderR);
            appStrategyManager.getAppOrderManager().OrderPersister.SaveLastRow(orderRecords);
        }

        /*
        private Dictionary<String, String> getStg1Paras(){
            Dictionary<String, String> paras = new Dictionary<string,string>();
            paras.Add("PARA1", txt_stg1_para1.Text);
            paras.Add("PARA2", txt_stg1_para2.Text);
            paras.Add("PARA3", txt_stg1_para3.Text);
            paras.Add("PARA4", txt_stg1_para4.Text);
            return paras;
        }

        private Dictionary<String, String> getStg2Paras()
        {
            Dictionary<String, String> paras = new Dictionary<string, string>();
            paras.Add("PARA1", txt_stg3_para1.Text);
            paras.Add("PARA2", txt_stg3_para2.Text);
            return paras;
        }*/

        private Dictionary<String, String> getStgParas(int i)
        {        
            Dictionary<String, String> paras = new Dictionary<string, string>();
            int para_count = AppConstant.STG_NUM_PARAS[i];
            for(int k=1; k <= para_count; k++){
                TextBox box = this.Controls.Find("txt_stg" +(i+1) +"_para" + k, true).FirstOrDefault() as TextBox;
                paras.Add("PARA"+k, box.Text);
                String text = "PARA"+k+"="+box.Text;
            }
            return paras;
        }

        public TickerInfo getTickerInfo()
        {
            TickerInfo info = new TickerInfo();
            info.tickerID = txt_ticker_id.Text;
            info.contractID = txt_ticker_contract_id.Text;
            info.symbol = txt_ticker_symbol.Text;
            info.type = txt_ticker_type.Text;
            info.exchange = txt_ticker_exch.Text;
            info.pExchange = txt_ticker_p_exch.Text;
            info.currency = txt_ticker_curr.Text;
            info.lSymbol = txt_ticker_local_symb.Text;
            info.whatToShow = txt_ticker_wts.Text;
            info.startTime = txt_ticker_startTime.Text;
            info.lunchEndTime = txt_ticker_lunchEndTime.Text;
            info.endTime = txt_ticker_endTime.Text;
            return info;
        }

        public UserPref getUserPref()
        {
            UserPref pref =new UserPref();
            pref.sendEmail = chk_Send_AEmail.Checked;
            pref.playSound = chk_play_sound.Checked;
            return pref;
        }

        private void cmb_ticker_profile_SelectedIndexChanged(object sender, EventArgs e)
        {
            String selectedProfile = cmb_ticker_profile.GetItemText(cmb_ticker_profile.SelectedItem);
            int profileIndex = 0;
            if (AppConstant.TICK1_NAME.Equals(selectedProfile))
            {
                profileIndex = 1;
            }
            if (AppConstant.TICK2_NAME.Equals(selectedProfile))
            {
                profileIndex = 2;
            }
            TickerInfo info = genTickerInfo(profileIndex);
            setTickerInfo(info);
        }

        //***Start Button***
        private void onSubmitClick(int stgIndex)
        {
            Dictionary<String, String> paras = getStgParas(stgIndex);
            String[] stgNames = appStrategyManager.getStgNames();
            appStrategyManager.startStrategy(stgNames[stgIndex], stgIndex, paras, appStrategyManager.UserAccount, GetMDContract(), getTickerInfo());
            StrategyOnOff onOff = new StrategyOnOff();
            onOff.stgIndex = stgIndex;
            onOff.isOn = true;
            HandleStrategyOnOff(onOff);
            MessageBox.Show("Strategy Start!");
        }

        private void btn_stg1_submit_Click(object sender, EventArgs e)
        {
            int stgIndex = 0;
            onSubmitClick(stgIndex);
        }

        private void btn_stg2_start_Click(object sender, EventArgs e)
        {

        }
        private void btn_stg2_submit_Click(object sender, EventArgs e)
        {
            int stgIndex = 1;
            onSubmitClick(stgIndex);

        }

        private void btn_stg3_submit_Click(object sender, EventArgs e)
        {
            int stgIndex = 2;
            onSubmitClick(stgIndex);
        }

        private void btn_stg4_submit_Click(object sender, EventArgs e)
        {
            int stgIndex = 3;
            onSubmitClick(stgIndex);
        }

        private void btn_stg5_submit_Click(object sender, EventArgs e)
        {
            int stgIndex = 4;
            onSubmitClick(stgIndex);
        }

        private void btn_stg6_submit_Click(object sender, EventArgs e)
        {
            int stgIndex = 5;
            onSubmitClick(stgIndex);
        }

        private void btn_stg7_submit_Click(object sender, EventArgs e)
        {
            int stgIndex = 6;
            onSubmitClick(stgIndex);
        }

        //***Stop Button***

        private void btn_stg1_stop_Click(object sender, EventArgs e)
        {
            appStrategyManager.stopStrategy(0);
            MessageBox.Show("Strategy Stop Submitted!");
        }

        private void btn_stg2_stop_Click(object sender, EventArgs e)
        {
            appStrategyManager.stopStrategy(1);
            MessageBox.Show("Strategy Stop Submitted!");
        }

        private void btn_stg3_stop_Click(object sender, EventArgs e)
        {
            appStrategyManager.stopStrategy(2);
            MessageBox.Show("Strategy Stop Submitted!");
        }


        private void btn_stg4_stop_Click(object sender, EventArgs e)
        {
            IAppOrderManager appOrderManager = appStrategyManager.getAppOrderManager();
            String[] stgNames = appStrategyManager.getStgNames();
            appStrategyManager.stopStrategy(3);
            MessageBox.Show("Strategy Stop Submitted!");
        }

        private void btn_stg5_stop_Click(object sender, EventArgs e)
        {
            appStrategyManager.stopStrategy(4);
            MessageBox.Show("Strategy Stop Submitted!");

        }

        private void btn_stg6_stop_Click(object sender, EventArgs e)
        {
            appStrategyManager.stopStrategy(5);
            MessageBox.Show("Strategy Stop Submitted!");
        }

        private void btn_stg7_stop_Click(object sender, EventArgs e)
        {
            appStrategyManager.stopStrategy(6);
            MessageBox.Show("Strategy Stop Submitted!");
        }
        //***Export Button***

        private void onExportClick(int stgIndex)
        {
            /*IAppOrderManager appOrderManager = appStrategyManager.getAppOrderManager();
            String[] stgNames = appStrategyManager.getStgNames();
            PositionPersistHelper helper = new PositionPersistHelper(AppConstant.FILE_POSITION_RESULT_PREFIX);
            String stgName = stgNames[stgIndex];
            helper.SaveRows(stgName, appOrderManager.StoreStgClosedOrders[stgName]);
             */
            appStrategyManager.exportReport(stgIndex);
            MessageBox.Show("Positions Exported.");
        }

        private void btn_stg1_export_Click(object sender, EventArgs e)
        {
            onExportClick(0);
        }

        private void btn_stg2_export_Click(object sender, EventArgs e)
        {
            onExportClick(1);
        }

        private void btn_stg3_export_Click(object sender, EventArgs e)
        {
            onExportClick(2);
        }

        private void btn_stg4_export_Click(object sender, EventArgs e)
        {
            onExportClick(3);
        }

        private void btn_stg5_export_Click(object sender, EventArgs e)
        {
            onExportClick(4);
        }

        private void btn_stg6_export_Click(object sender, EventArgs e)
        {
            onExportClick(5);
        }
        private void btn_stg7_export_Click(object sender, EventArgs e)
        {
            onExportClick(6);
        }

        private void label40_Click(object sender, EventArgs e)
        {
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void tabPage7_Click(object sender, EventArgs e)
        {

        }

        private void txt_sno_TextChanged(object sender, EventArgs e)
        {

        }

        private void txt_test_buyprice_TextChanged(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {

        }


    }
}
