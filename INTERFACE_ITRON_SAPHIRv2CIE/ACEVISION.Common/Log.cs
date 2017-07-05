using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACEVISION.Common
{
    public static class Log
    {


        public static ILog MonitoringLogger
        {
            get { return LogManager.GetLogger("MonitoringLogger"); }
    


        }

        public static ILog ExceptionLogger
        {
            get { return LogManager.GetLogger("ExceptionLogger"); }
            //get { return LogManager.GetLogger(Assembly.GetExecutingAssembly(), "ExceptionLogger"); }
        }

    }
}
