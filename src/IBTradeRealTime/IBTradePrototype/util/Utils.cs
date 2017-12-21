using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBApi;

namespace IBTradeRealTime.util
{
    public class Utils
    {
        public static string ContractToString(Contract contract)
        {
            if (contract.PrimaryExch != null)
                return contract.Symbol + " " + contract.SecType + " " + contract.Currency + " @ " + contract.PrimaryExch;
            else
                return contract.Symbol + " " + contract.SecType + " " + contract.Currency;
        }

        public static bool ContractsAreEqual(Contract contractA, Contract contractB)
        {
            if (contractA.Symbol.Equals(contractB.Symbol) && contractA.SecType.Equals(contractB.SecType) && contractA.Currency.Equals(contractB.Currency))
            {
                if (contractA.Expiry != null && contractB.Expiry != null)
                {
                    if (contractA.Expiry.Equals(contractB.Expiry))
                    {
                        if (contractA.Multiplier != null && contractB.Multiplier != null)
                        {
                            return contractA.Multiplier.Equals(contractB.Multiplier);
                        }
                        else
                            return true;
                    }
                }
                else
                    return true;
            }

            return false;
        }

        public static DateTime convertToNearestTime(DateTime currentTime, DateTime lastTime, int minuteElapsed)
        {
            if(lastTime.Equals(AppConstant.INVALID_TIME)){
                int currentHour = currentTime.Hour;
                int currentMinute = currentTime.Minute;
                int convertMinute = (currentMinute / minuteElapsed) * minuteElapsed;
                return new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentHour, convertMinute, 0);
            }else {
                return lastTime.AddMinutes(minuteElapsed);
            }
        }
    }
}
