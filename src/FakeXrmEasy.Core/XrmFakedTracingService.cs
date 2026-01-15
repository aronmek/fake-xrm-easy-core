using System;
using System.Text;
using FakeXrmEasy.Abstractions;

namespace FakeXrmEasy
{
    /// <summary>
    /// A fake tracing service that stores all traces In-Memory and can then dump the entire trace log
    /// </summary>
    public class XrmFakedTracingService : IXrmFakedTracingService
    {
        /// <summary>
        /// 
        /// </summary>
        protected StringBuilder _trace { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private object _lock = new object();

        /// <summary>
        /// 
        /// </summary>
        public XrmFakedTracingService()
        {
            _trace = new StringBuilder();
        }

        /// <summary>
        /// Maximum size of the trace buffer before dumping. 0 means unlimited.
        /// </summary>
        public int MaxBufferSize { get; set; }

        /// <summary>
        /// Action to call when the trace buffer is dumped
        /// </summary>
        public Action<string> DumpAction { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void Trace(string format, params object[] args)
        {
            lock(_lock)
            {
                if (args == null || args.Length == 0)
                {
                   _trace.AppendLine(format);
                }
                else
                { 
                   _trace.AppendLine(string.Format(format, args));
                }
            }
        }

        /// <summary>
        /// Dumps the current trace to the DumpAction and clears the buffer
        /// </summary>
        public void Flush()
        {
            lock(_lock)
            {
                if (_trace.Length > 0 && DumpAction != null)
                {
                    DumpAction.Invoke(_trace.ToString());
                    _trace.Clear();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string DumpTrace()
        {
            return _trace.ToString();
        }
    }
}