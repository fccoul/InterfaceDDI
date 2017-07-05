using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACEVISION.Common
{
    public interface IServiceSynchro
    {
        bool EstServiceSynchronisation();
        bool TryConnectWCFEndPoint(string pEndPointAddress, string pConfigPath, ref string pErrMsg);
    }
}
