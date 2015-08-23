using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCore.Logging
{
    public class LogProvider
    {
        private string m_sFile;

        public LogProvider(string sFile)
        {
            m_sFile = sFile;
        }

        private Object m_oLogLock = new Object();

        static public void LogSzUnsafe(string s, string sFile)
        {
            LogSzUnsafe(DateTime.Now, Environment.TickCount, s, sFile);
        }

        static public void LogSzUnsafe(DateTime dttm, int nTicks, string s, string sFile)
        {
            StreamWriter sw = new StreamWriter(sFile, true /*fAppend*/, System.Text.Encoding.Default);
            sw.Write(String.Format("[{0:X8}:{1}]: {2}\n", nTicks, dttm, s));
            sw.Close();
        }
        /* L O G  S Z */
        /*----------------------------------------------------------------------------
        	%%Function: LogSz
        	%%Qualified: tcrec.TcRec.LogSz
        	%%Contact: rlittle

        ----------------------------------------------------------------------------*/
        public void LogSz(string s)
        {
            int l = Environment.TickCount;

            DateTime dttm = DateTime.Now;

            lock (m_oLogLock)
                {
                LogSzUnsafe(dttm, l, s, m_sFile);
                }
        }

    }
}
