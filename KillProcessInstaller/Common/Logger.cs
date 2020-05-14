﻿using System;
using NLog;
using GeraldServiceAutoOperate.Entity;

namespace GeraldServiceAutoOperate.Common
{
    public class Logger
    {
        NLog.Logger _logger;

        public Logger(NLog.Logger logger)
        {
            _logger = logger;
        }

        public Logger(string name) : this(LogManager.GetLogger(name))
        {

        }

        public Logger Default { get; private set; }
        public  Logger()
        {
            Default = new Logger(NLog.LogManager.GetCurrentClassLogger());
        }

        #region Debug
        public void Debug(string msg, params object[] args)
        {
            _logger.Debug(msg, args);
        }

        public void Debug(string msg, Exception err)
        {
            _logger.Debug(err, msg);
        }
        #endregion

        #region Info
        public void Info(string msg, params object[] args)
        {
            _logger.Info(msg, args);
        }

        public void Info(string msg, Exception err)
        {
            _logger.Info(err, msg);
        }
        #endregion

        #region Warn
        public void Warn(string msg, params object[] args)
        {
            _logger.Warn(msg, args);
        }

        public void Warn(string msg, Exception err)
        {
            _logger.Warn(err, msg);
        }
        #endregion

        #region Trace
        public void Trace(string msg, params object[] args)
        {
            _logger.Trace(msg, args);
        }

        public void Trace(string msg, Exception err)
        {
            _logger.Trace(err, msg);
        }
        #endregion

        #region Error
        public void Error(string msg, params object[] args)
        {
            _logger.Error(msg, args);
        }

        public void Error(string msg, Exception err)
        {
            _logger.Error(err, msg);
        }
        #endregion

        #region Fatal
        public void Fatal(string msg, params object[] args)
        {
            _logger.Fatal(msg, args);
        }

        public void Fatal(string msg, Exception err)
        {
            _logger.Fatal(err, msg);
        }
        #endregion

        #region Custom

        public void Process(Log log)
        {
            var level = LogLevel.Info;
            if (log.Level == "Trace")
                level = LogLevel.Trace;
            else if (log.Level == "Debug")
                level = LogLevel.Debug;
            else if (log.Level == "Info")
                level = LogLevel.Info;
            else if (log.Level == "Warn")
                level = LogLevel.Warn;
            else if (log.Level == "Error")
                level = LogLevel.Error;
            else if (log.Level == "Fatal")
                level = LogLevel.Fatal;

            var ei = new MyLogEventInfo(level, _logger.Name, log.Message);
            ei.TimeStamp = log.Timestamp;
            ei.Properties["Action"] = log.Action;
            ei.Properties["Amount"] = log.Amount;

            _logger.Log(level, ei);
        }

        #endregion

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="timeoutMilliseconds">Maximum time to allow for the flush. Any messages after that time will be discarded.</param>
        public void Flush(int? timeoutMilliseconds = null)
        {
            if (timeoutMilliseconds != null)
                NLog.LogManager.Flush(timeoutMilliseconds.Value);

            NLog.LogManager.Flush();
        }
    }

    public class MyLogEventInfo : LogEventInfo
    {
        public MyLogEventInfo() { }
        public MyLogEventInfo(LogLevel level, string loggerName, string message) : base(level, loggerName, message)
        { }

        public override string ToString()
        {
            //Message format
            //Log Event: Logger='XXX' Level=Info Message='XXX' SequenceID=5
            return FormattedMessage;
        }
    }
}
