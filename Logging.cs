using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if !WINDOWS_UWP
using System.Diagnostics;
#else
using Windows.Foundation.Diagnostics;
using Windows.Storage;
#endif

namespace TCore.Logging
{
#if !WINDOWS_UWP
    public class CustomListener : TextWriterTraceListener
    {
        public CustomListener(string sFilename) : base(sFilename)
        {
        }
    }
#endif

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

        public Guid Crids => m_guidCorrelation;

        public CorrelationID()
        {
            m_guidCorrelation = Guid.NewGuid();
        }

        public static implicit operator Guid(CorrelationID crid)
        {
            return crid.Crids;
        }

        public CorrelationID(CorrelationID cridParent)
        {
            m_guidCorrelation = Guid.NewGuid();
            m_cridParent = cridParent;
        }

        public static CorrelationID FromCrids(Guid crids)
        {
            CorrelationID crid = new CorrelationID();
            crid.m_guidCorrelation = crids;
            return crid;
        }

        public static CorrelationID Create(CorrelationID cridParent = null)
        {
            CorrelationID crid = new CorrelationID();
            crid.m_guidCorrelation = Guid.NewGuid();
            crid.m_cridParent = cridParent;

            return crid;
        }
    }

    public enum EventType
    {
#if WINDOWS_UWP
        Critical = LoggingLevel.Critical,
        Error = LoggingLevel.Error,
        Warning = LoggingLevel.Warning,
        Information = LoggingLevel.Information,
        Verbose = LoggingLevel.Verbose
#else
        Critical = TraceEventType.Critical,
        Error = TraceEventType.Error,
        Warning = TraceEventType.Warning,
        Information = TraceEventType.Information,
        Verbose = TraceEventType.Verbose
#endif
    };

    public class LogProvider
    {
        private string m_sFile;
#if WINDOWS_UWP
        private LoggingChannel m_ts;
        private LoggingSession m_ls;
#else
        private TraceSource m_ts;
#endif

        public void test(string s, params object[] rgo)
        {
            string s2 = String.Format(s, rgo);
            string s3 = s2;

        }

        public LogProvider(string sFile)
        {
#if WINDOWS_UWP
            m_ls = new LoggingSession(sFile);
            LoggingChannelOptions lco = new LoggingChannelOptions();
            
            m_ts = new LoggingChannel(sFile, lco);

            m_ls.AddLoggingChannel(m_ts);
#else
            m_ts = new TraceSource(sFile);
#if TESTPROVIDER
            string s = String.Format("foo{0}{1}", "1", 2);
            test("foo{0}{1}", "1", 2);
            test("foo");

            if (m_ts.Switch.ShouldTrace(TraceEventType.Information))
                m_ts.TraceInformation("test");
            if (m_ts.Switch.ShouldTrace(TraceEventType.Critical))
                m_ts.TraceInformation("test");
            if (m_ts.Switch.ShouldTrace(TraceEventType.Error))
                m_ts.TraceInformation("test");
            m_ts.TraceEvent(TraceEventType.Information, 12345, "test{0}", "foo");
#endif
//            System.Diagnostics.Trace.TraceInformation("ListenersCount: {0}", Trace.Listeners.Count);
            for (int i = 0; i < Trace.Listeners.Count; i++)
                {
                m_ts.Listeners.Add(System.Diagnostics.Trace.Listeners[i]);
                }
#endif
            m_sFile = sFile;
        }

        private const string _sGuidZero = "00000000-0000-0000-0000-000000000000";

#if WINDOWS_UWP
        private static void LogInternal(LoggingChannel ts, CorrelationID crid, EventType et, int nTicks, DateTime dttm, string s, params object[] rgo)
        {
            string sFormatted = String.Format(s, rgo);
            string sStringField = String.Format("{0}\t{1}\t{2:X8}\t{3}\t{4}",
                                                crid?.Text ?? _sGuidZero, -1, nTicks, dttm,
                                                sFormatted);

            LoggingFields lf = new LoggingFields();
            lf.AddString("sParm", sStringField);
            lf.AddInt16("Hash2", crid?.Hash2 ?? 0);
            string sType;

            if (et == EventType.Critical)
                sType = "Critical";
            else if (et == EventType.Error)
                sType = "Error";
            else if (et == EventType.Information)
                sType = "Information";
            else if (et == EventType.Warning)
                sType = "Warning";
            else
                sType = "Verbose";

            ts.LogEvent(sType, lf, (LoggingLevel) et);
        }
#else
        private static void LogInternal(TraceSource ts, CorrelationID crid, EventType et, int nTicks, DateTime dttm, string s, params object[] rgo)
        {
            if (ts.Switch.ShouldTrace((TraceEventType) et))
                {
                string sFormatted = String.Format(s, rgo);
                ts.TraceEvent((TraceEventType) et, crid?.Hash2 ?? 0, "{0}\t{1}\t{2:X8}\t{3}\t{4}",
                    crid?.Text ?? _sGuidZero, System.Threading.Thread.CurrentThread.ManagedThreadId, nTicks, dttm,
                    sFormatted);
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
            LogInternal(ts, crid, EventType.Information, Environment.TickCount, DateTime.Now, s);
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
            LogInternal(ts, null, EventType.Information, nTicks, dttm, s);
            ts.Close();
        }

        public bool FShouldLog(EventType et)
        {
            return m_ts.Switch.ShouldTrace((TraceEventType)et);
        }
#endif

#if WINDOWS_UWP
        public bool FShouldLog(EventType et)
        {
            return true;
        }
#endif

        /* L O G  E V E N T */
        /*----------------------------------------------------------------------------
        	%%Function: LogEvent
        	%%Qualified: TCore.Logging.LogProvider.LogEvent
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public void LogEvent(CorrelationID crid, EventType et, string s, params object[] rgo)
        {
            if (!FShouldLog(et))
                return;

            int nTicks = Environment.TickCount;
            DateTime dttm = DateTime.Now;
            LogInternal(m_ts, crid, et, nTicks, dttm, s, rgo);
        }

        /* L O G  S Z */
        /*----------------------------------------------------------------------------
        	%%Function: LogSz
        	%%Qualified: tcrec.TcRec.LogSz
        	%%Contact: rlittle

        ----------------------------------------------------------------------------*/
        public void LogSz(CorrelationID crid, string s, params object[] rgo)
        {
            LogEvent(crid, EventType.Information, s, rgo);
        }

        /* L O G  V E R B O S E  S Z */
        /*----------------------------------------------------------------------------
        	%%Function: LogVerboseSz
        	%%Qualified: TCore.Logging.LogProvider.LogVerboseSz
        	%%Contact: rlittle
        	
        ----------------------------------------------------------------------------*/
        public void LogVerboseSz(CorrelationID crid, string s, params object[] rgo)
        {
            LogEvent(crid, EventType.Verbose, s, rgo);
        }

#if WINDOWS_UWP
        public void Flush()
        {
            m_ls.SaveToFileAsync(ApplicationData.Current.LocalFolder, m_sFile).AsTask().Wait();
        }
#endif
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
#if WINDOWS_UWP
            using (Stream stm = new FileStream(sFile, FileMode.Append))
                using (StreamWriter sw = new StreamWriter(stm, System.Text.Encoding.UTF8))
                {
                sw.Write(String.Format("[{0:X8}:{1}]: {2}\n", nTicks, dttm, s));
                sw.Flush();
                }
#else
            StreamWriter sw = new StreamWriter(sFile, true /*fAppend*/, System.Text.Encoding.Default);

            sw.Write(String.Format("[{0:X8}:{1}]: {2}\n", nTicks, dttm, s));
            sw.Close();
#endif
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
