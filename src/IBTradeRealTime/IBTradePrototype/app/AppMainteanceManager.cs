using IBApi;
using IBTradeRealTime.UI;
using IBTradeRealTime.util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace IBTradeRealTime.app
{
    public class AppMainteanceManager : IAppMainteanceManager
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        IAppStrategyManager stgManager;
        public BlockingCollection<AppEvent> storeEventQueue { get; set; }
        private static System.Timers.Timer timer;

        private DateTime lastHBEmailTime = AppConstant.INVALID_TIME;
        //private Boolean lunchTimeRTBReset = false;
        //private Boolean morningTimeRTBReset = false;
        //private Boolean dailyAllFlagsInit = false;

        [System.ComponentModel.DefaultValue(false)]
        public Boolean lunchTimeRTBReset { get; set; }

        [System.ComponentModel.DefaultValue(false)]
        public Boolean morningTimeRTBReset { get; set; }

        [System.ComponentModel.DefaultValue(false)]
        public Boolean dailyAllFlagsInit { get; set; }

        [System.ComponentModel.DefaultValue(false)]
        public Boolean dailyDayEndExport { get; set; }

        //private DateTime lastProcessDate = AppConstant.INVALID_TIME;
        public Dictionary<DateTime, DateTime> CompleteDailyReset { get; set; }

        public AppMainteanceManager(IAppStrategyManager stgManager)
        {
            this.stgManager = stgManager;
            storeEventQueue = new BlockingCollection<AppEvent>();
            CompleteDailyReset = new Dictionary<DateTime, DateTime>();
            //startManager();
        }

        public void resetDailyAllFlag(DateTime time)
        {
            //lastProcessDate = new DateTime(time.Day, time.Month, time.Day, 0, 0, 0, DateTimeKind.Local);
            //lastProcessDate = getResetTime(time);
            DateTime adjustTime = getResetTime(time);
            dailyAllFlagsInit = true;
            lunchTimeRTBReset = false;
            morningTimeRTBReset = false;
            dailyDayEndExport = false;
            CompleteDailyReset.Add(adjustTime, adjustTime);
        }

        public void startManager(){
            Action<object> action = run;
            Task runTask = new Task(run, "managerThread");
            runTask.Start();
        }

        public void run(Object argObj)
        {   
            while (true)
            {
                AppEvent appEvent = storeEventQueue.Take();
                if (appEvent.Type.Equals(AppEventType.TimeEvent))
                {
                    AppTimeEvent timeEvent = (AppTimeEvent)appEvent;
                    handleDailyAllFlagsReset(timeEvent);
                    handleResetRTBRequest(timeEvent);
                    handleDailyDayEndExport(timeEvent);
                    handleHBEmail(timeEvent);
                }
               if (appEvent.Type.Equals(AppEventType.OrderExecuted))
                {
                   AppOrderExecutedEvent exeEvent = (AppOrderExecutedEvent)appEvent;
                   handOrderExeEvent(exeEvent);
               }
            }
        }

        public static DateTime getResetTime(DateTime time)
        {
            DateTime resetTime = new DateTime(time.Year, time.Month, time.Day, 0, 10, 0, DateTimeKind.Local);
            if (time >= resetTime)
                return resetTime;
            else
                return resetTime.AddDays(-1); 
            //return new DateTime(time.Year, time.Month, time.Day, 0, 0, 0, DateTimeKind.Local);
        }

        public void handleDailyAllFlagsReset(AppTimeEvent timeEvent)
        {
            //DateTime _date = new DateTime(timeEvent.eventTime.Day, timeEvent.eventTime.Month, timeEvent.eventTime.Day, 0, 20,0, DateTimeKind.Local);
            DateTime _date = getResetTime(timeEvent.eventTime);
            if (!dailyAllFlagsInit)
            {
                log.Info("Init Maintenance Flags Daily Reset.");
                resetDailyAllFlag(timeEvent.eventTime);
                log.Info("[handleDailyAllFlagsReset]: Test 1");
                return;
            }
            else if (!CompleteDailyReset.ContainsKey(_date))
            {
                log.Info("Maintenance Flags Daily Reset.");
                resetDailyAllFlag(timeEvent.eventTime);
                handleDailyReset(timeEvent);
                return;
            }
        }

        public void handleDailyReset(AppTimeEvent timeEvent)
        {
            ConcurrentDictionary<String, String> stgNamesMap = stgManager.getActiveStgNamesMap();
            stgNamesMap.Keys.ToArray();
            IAppEventManager eventManager = stgManager.getAppEventManager();
            foreach (String name in stgNamesMap.Keys)
            {
                eventManager.storeEventQueue[name].Add(new AppDailyResetEvent());
            }
            stgManager.dailyReset();
        }

        public void handleDailyDayEndExport(AppTimeEvent timeEvent)
        {
            TickerInfo tickerInfo = stgManager.ParentUI.getTickerInfo();
            String strEndTime = String.Format("{0:yyyyMMdd}", timeEvent.eventTime) + "  " + tickerInfo.endTime;
            //String strEndTime = String.Format("{0:yyyyMMdd}", timeEvent.eventTime) + "  " + "16:15:00";
            DateTime endTime = DateTime.ParseExact(strEndTime, "yyyyMMdd  HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);
            DateTime endTime_from = endTime.AddMinutes(+2);
            DateTime endTime_to = endTime.AddMinutes(+3);

            if (timeEvent.eventTime > endTime_from && timeEvent.eventTime < endTime_to && !dailyDayEndExport)
            {
                log.Info("Daily Dayend Export Report Start.");
                //
                stgManager.dailyDayEndExport();
                dailyDayEndExport = true;
                log.Info("Daily Dayend Export Report End.");
                return;
            }
        }
        public void handleResetRTBRequest(AppTimeEvent timeEvent)
        {
            //Contract contract = stgManager.CurrentContract;
            Contract contract = stgManager.ParentUI.GetMDContract();
            IRealTimeBarsManagerBridge RTBManager = stgManager.ParentUI.getRealTimeBarsManager();
            if (contract == null)
                return;
            if(RTBManager==null)
                 return;

            TickerInfo tickerInfo = stgManager.ParentUI.getTickerInfo();
            String strStartTime = String.Format("{0:yyyyMMdd}", timeEvent.eventTime) + "  " + tickerInfo.startTime;
            DateTime startTime = DateTime.ParseExact(strStartTime, "yyyyMMdd  HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);
            DateTime startTime_from = startTime.AddMinutes(-3);
            DateTime startTime_to = startTime.AddMinutes(-2);
            if (timeEvent.eventTime > startTime_from && timeEvent.eventTime < startTime_to && !morningTimeRTBReset)
            {
                log.Info("Morning Reset RTBar Start.");
                resetRTBRequest(RTBManager);
                morningTimeRTBReset = true;
                log.Info("Morning Reset RTBar End.");
                return;
            }

            String strLunchTime = String.Format("{0:yyyyMMdd}", timeEvent.eventTime) + "  " + tickerInfo.lunchEndTime;
            DateTime lunchTime = DateTime.ParseExact(strLunchTime, "yyyyMMdd  HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);
            DateTime lunchTime_from = lunchTime.AddMinutes(-3);
            DateTime lunchTime_to = lunchTime.AddMinutes(-2);
            if (timeEvent.eventTime > lunchTime_from && timeEvent.eventTime < lunchTime_to && !lunchTimeRTBReset)
            {
                log.Info("Lunch Reset RTBar Start.");
                resetRTBRequest(RTBManager);
                lunchTimeRTBReset = true;
                log.Info("Lunch Reset RTBar End.");
                return;
            }

        }

        public void handOrderExeEvent(AppOrderExecutedEvent exeEvent)
        {
            UserPref pref = stgManager.ParentUI.getUserPref();
            Boolean isSendEmail = pref.sendEmail;
            Boolean isPlaySound = pref.playSound;
            if (!isSendEmail)
                return;
            ISendMailManager manager = SendMailManager.getManager();
            String emailBody = "";
            String template = "Ticker Name:[0], BQty=[1], SQty=[2], Price=[3], Time=[4], Status=[5], SNo=[6]\n";
            emailBody += template.Replace("[0]", exeEvent.TickerName).Replace("[1]", exeEvent.BQty).Replace("[2]", exeEvent.SQty).Replace("[3]", exeEvent.Price)
                         .Replace("[4]", exeEvent.Time).Replace("[5]", exeEvent.Status).Replace("[6]", exeEvent.SNo);
            manager.SendEmail("<AlgoEdge Alert> ***Execution*** : from " + System.Environment.MachineName + " " + exeEvent.Time, emailBody);
            if (isPlaySound)
                playTradeAlertSound();
        }

        public void resetRTBRequest(IRealTimeBarsManagerBridge RTBManager)
        {
            if (AppConstant.USE_OUT_REGULAR_DATA)
                RTBManager.ResetRequest(stgManager.ParentUI.GetMDContract(), stgManager.ParentUI.GetWTS(), false);
            else
                RTBManager.ResetRequest(stgManager.ParentUI.GetMDContract(), stgManager.ParentUI.GetWTS(), true);

        }

        public void handleHBEmail(AppTimeEvent timeEvent)
        {
            if (lastHBEmailTime.Equals(AppConstant.INVALID_TIME) || lastHBEmailTime.AddMinutes(AppConstant.HB_EMAIL_ELPASED_TIME) <= timeEvent.eventTime)
            {
                UserPref pref = stgManager.ParentUI.getUserPref();
                Boolean isSendEmail = pref.sendEmail;
                Boolean isPlaySound = pref.playSound;
                if (!isSendEmail)
                    return;
                lastHBEmailTime = Utils.convertToNearestTime(timeEvent.eventTime, lastHBEmailTime, AppConstant.HB_EMAIL_ELPASED_TIME);
                ISendMailManager manager = SendMailManager.getManager();

                List<String[]> summary = stgManager.getPositionSummary();
                String emailBody = "";
                foreach(String[] row in summary){
                    String template = "Name:[0], Position=[1], Unrealized PnL=[2], Total PnL=[3]\n";
                    emailBody += template.Replace("[0]", row[0]).Replace("[1]", row[1]).Replace("[2]", row[2]).Replace("[3]", row[3]);
                }

                if (emailBody.Equals(""))
                    emailBody = "[No Content]";

                manager.SendEmail("<AlgoEdge Alert> [HeartBeat] : from " + System.Environment.MachineName + " " + lastHBEmailTime, emailBody);
            } 
        }

        private void playTradeAlertSound()
        {
            SoundPlayer audio = new SoundPlayer(IBTradeRealTime.Properties.Resources.STRIKER_OUT); // here WindowsFormsApplication1 is the namespace and Connect is the audio file name
            audio.Play();
        }
    }
}
