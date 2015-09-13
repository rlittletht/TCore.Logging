using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace TCore.Logging
{
    public class CustomListener : TextWriterTraceListener
    {
        public CustomListener(string sFilename) : base(sFilename)
        {
        }


    }

    public class CorrelationID // crid
    {
        private Guid m_guidCorrelation;
        private CorrelationID m_cridParent;
        private string m_sText;
        private Int16 m_wHash2;

        public string Text
        {
            get
            {
                if (m_sText == null)
                    m_sText = m_guidCorrelation.ToString();
                return m_sText;
            }
        }

        public Int16 Hash2
        {
            get
            {
                if (m_wHash2 == 0)
                    {
                    byte[] rgb = m_guidCorrelation.ToByteArray();

                    m_wHash2 = (Int16) (rgb[0] | rgb[1] << 8);
                    }
                return m_wHash2;
            }
        }

        Guid ID => m_guidCorrelation;

        public CorrelationID()
        {
            m_guidCorrelation = Guid.NewGuid();
        }

        public CorrelationID(CorrelationID cridParent)
        {
            m_guidCorrelation = Guid.NewGuid();
            m_cridParent = cridParent;
        }

        public static CorrelationID Create(CorrelationID cridParent = null)
        {
            CorrelationID crid = new CorrelationID();
            crid.m_guidCorrelation = Guid.NewGuid();
            crid.m_cridParent = cridParent;

            return crid;
        }
    }

    public class LogProvider
    {
        private string m_sFile;
        private TraceSource m_ts;

        public void test(string s, params object[] rgo)
        {
            string s2 = String.Format(s, rgo);
            string s3 = s2;

        }

        public LogProvider(string sFile)
        {
            string s = String.Format("foo{0}{1}", "1", 2);
            test("foo{0}{1}", "1", 2);
            test("foo");

            m_ts = new TraceSource(sFile);
            if (m_ts.Switch.ShouldTrace(TraceEventType.Information))
                m_ts.TraceInformation("test");
            if (m_ts.Switch.ShouldTrace(TraceEventType.Critical))
                m_ts.TraceInformation("test");
            if (m_ts.Switch.ShouldTrace(TraceEventType.Error))
                m_ts.TraceInformation("test");
            m_ts.TraceEvent(TraceEventType.Information, 12345, "test{0}", "foo");
            m_sFile = sFile;
        }

        private const string _sGuidZero = "00000000-0000-0000-0000-000000000000";

        private static void LogInternal(TraceSource ts, CorrelationID crid, int nTicks, DateTime dttm, string s, params object[] rgo)
        {
            if (ts.Switch.ShouldTrace(TraceEventType.Information))
                {
                string sFormatted = String.Format(s, rgo);
                ts.TraceEvent(TraceEventType.Information, crid?.Hash2 ?? 0, "{0}\t{1}\t{2:X8}\t{3}\t{4}", crid?.Text ?? _sGuidZero, System.Threading.Thread.CurrentThread.ManagedThreadId, nTicks, dttm, sFormatted);
                }
        }

        /* L O G  S Z  U N S A F E */
        /*----------------------------------------------------------------------------
        	%%Function: LogSzUnsafe
        	%%Qualified: TCore.Logging.LogProvider.LogSzUnsafe
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        static public void LogSzUnsafe(CorrelationID crid, string s, string sFile)
        {
            TraceSource ts = new TraceSource(sFile);
            LogInternal(ts, crid, Environment.TickCount, DateTime.Now, s);
            ts.Close();
        }

        /* L O G  S Z  U N S A F E */
        /*----------------------------------------------------------------------------
        	%%Function: LogSzUnsafe
        	%%Qualified: TCore.Logging.LogProvider.LogSzUnsafe
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        static public void LogSzUnsafe(DateTime dttm, int nTicks, string s, string sFile)
        {
            TraceSource ts = new TraceSource(sFile);
            LogInternal(ts, null, nTicks, dttm, s);
            ts.Close();
        }

        /* L O G  S Z */
        /*----------------------------------------------------------------------------
        	%%Function: LogSz
        	%%Qualified: tcrec.TcRec.LogSz
        	%%Contact: rlittle

        ----------------------------------------------------------------------------*/
        public void LogSz(CorrelationID crid, string s, params object[] rgo)
        {
            if (!m_ts.Switch.ShouldTrace(TraceEventType.Information))
                return;

            int nTicks = Environment.TickCount;
            DateTime dttm = DateTime.Now;
            LogInternal(m_ts, crid, nTicks, dttm, s, rgo);
        }

    }
    public class LogProviderFile
    {
        private string m_sFile;

        public LogProviderFile(string sFile)
        {
            m_sFile = sFile;
        }

        private Object m_oLogLock = new Object();

        /* L O G  S Z  U N S A F E */
        /*----------------------------------------------------------------------------
        	%%Function: LogSzUnsafe
        	%%Qualified: TCore.Logging.LogProvider.LogSzUnsafe
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        static public void LogSzUnsafe(string s, string sFile)
        {
            LogSzUnsafe(DateTime.Now, Environment.TickCount, s, sFile);
        }

        /* L O G  S Z  U N S A F E */
        /*----------------------------------------------------------------------------
        	%%Function: LogSzUnsafe
        	%%Qualified: TCore.Logging.LogProvider.LogSzUnsafe
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
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
        public void LogSz(CorrelationID crid, string s)
        {
            int l = Environment.TickCount;

            DateTime dttm = DateTime.Now;

            lock (m_oLogLock)
                {
                LogSzUnsafe(dttm, l, String.Format("{0}: {1}", crid?.Text, s), m_sFile);
                }
        }

    }
}
