using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Runtime.CompilerServices;
#if UNITY_EDITOR
using FuzzySearch = Sirenix.Utilities.Editor.FuzzySearch;
#endif

namespace VaporActorLogging
{
    [Serializable]
    public class RichStringLog : ISearchFilterable
    {
        public int Type;
        public string Content;
        public string StackTrace;
        public DateTimeOffset TimeStamp;

        public bool IsMatch(string searchString)
        {
#if UNITY_EDITOR
            return FuzzySearch.Contains(searchString, Content);
#else
            return false;
#endif
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, Content, TimeStamp);
        }
    }

    public enum LogLevel
    {
        Debug,
        Info,
        Warn,
        Error,
        Fatal
    }
    
    [Serializable]
    public class ActorLogger
    {        
        [SerializeField]
        private List<RichStringLog> _logs = new(1000);
        [SerializeField]
        private int _infoCount;
        [SerializeField]
        private int _warningCount;
        [SerializeField]
        private int _errorCount;        
        public List<RichStringLog> Logs = new(1000);

        private readonly StringBuilder _sb = new();
        private readonly int _infoStraceCount = 5;
        private readonly int _warningStraceCount = 10;
        private readonly int _errorStraceCount = 20;
        private readonly bool _autoClear;

        public ActorLogger()
        {
            _autoClear = false;
        }

        public ActorLogger(bool autoClear)
        {
            _autoClear = autoClear;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Log(LogLevel logLevel, string log, int traceSkipFrames = 1)
        {
            if (_autoClear)
            {
                if (_logs.Count > 100) { _logs.Clear(); Logs.Clear(); }
            }

            ToLogger(logLevel, log, traceSkipFrames);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogWithConsole(LogLevel logLevel, string log, int traceSkipFrames = 1)
        {
            if (_autoClear)
            {
                if (_logs.Count > 100) { _logs.Clear(); Logs.Clear(); }
            }

            ToLogger(logLevel, log, traceSkipFrames);

            switch (logLevel)
            {
                case LogLevel.Debug:
                        Debug.Log(log);
                    break;
                case LogLevel.Info:
                        Debug.Log(log);
                    break;
                case LogLevel.Warn:
                        Debug.LogWarning(log);
                    break;
                case LogLevel.Error:
                        Debug.LogError(log);
                    break;
                case LogLevel.Fatal:
                        Debug.LogError(log);
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ToLogger(LogLevel logLevel, string log, int traceSkipFrames)
        {
            switch (logLevel)
            {
                case LogLevel.Debug:
                    CreateStackTrace(_infoStraceCount, traceSkipFrames);
                    _logs.Add(new RichStringLog()
                    {
                        Type = 0,
                        Content = log,
                        StackTrace = _sb.ToString(),
                        TimeStamp = DateTimeOffset.UtcNow,
                    });
                    _infoCount++;
                    break;
                case LogLevel.Info:
                    CreateStackTrace(_infoStraceCount, traceSkipFrames);
                    _logs.Add(new RichStringLog()
                    {
                        Type = 0,
                        Content = log,
                        StackTrace = _sb.ToString(),
                        TimeStamp = DateTimeOffset.UtcNow,
                    });
                    _infoCount++;
                    break;
                case LogLevel.Warn:
                    CreateStackTrace(_warningStraceCount, traceSkipFrames);
                    _logs.Add(new RichStringLog()
                    {
                        Type = 1,
                        Content = log,
                        StackTrace = _sb.ToString(),
                        TimeStamp = DateTimeOffset.UtcNow,
                    });
                    _warningCount++;
                    break;
                case LogLevel.Error:
                    CreateStackTrace(_errorStraceCount, traceSkipFrames);
                    _logs.Add(new RichStringLog()
                    {
                        Type = 2,
                        Content = log,
                        StackTrace = _sb.ToString(),
                        TimeStamp = DateTimeOffset.UtcNow,
                    });
                    _errorCount++;
                    break;
                case LogLevel.Fatal:
                    CreateStackTrace(_errorStraceCount, traceSkipFrames);
                    _logs.Add(new RichStringLog()
                    {
                        Type = 2,
                        Content = log,
                        StackTrace = _sb.ToString(),
                        TimeStamp = DateTimeOffset.UtcNow,
                    });
                    _errorCount++;
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CreateStackTrace(int traceCount, int traceSkipFrames)
        {
            System.Diagnostics.StackTrace t = new(traceSkipFrames, true);
            int count = Mathf.Min(traceCount, t.FrameCount);
            _sb.Clear();
            for (int i = 0; i < count; i++)
            {
                var frame = t.GetFrame(i);
                if (frame.GetFileLineNumber() > 0)
                {
                    string fileName = System.IO.Path.GetFileName(frame.GetFileName());
                    _sb.AppendLine($"<b>{frame.GetMethod().Name}</b> | {fileName} <a cs=\"{frame.GetFileName()}\" ln=\"{frame.GetFileLineNumber()}\" cn=\"{frame.GetFileColumnNumber()}\"><b>[{frame.GetFileLineNumber()}]</b></a>");
                }
            }
        }
    }
}
