using ErogeHelper.Common.Extension;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using System;
using System.Collections.Concurrent;
using System.IO;

namespace ErogeHelper.Common.Helper
{
    class InMemorySink : ILogEventSink
    {
        private readonly ITextFormatter _textFormatter = new MessageTemplateTextFormatter(
                                                "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                                                null);

        public delegate void LogMessageUpdated(object sender);
        public static event LogMessageUpdated? LogMessageUpdatedEvent;
        
        // TODO: Delete this in the future
        public const int MaxSize = 64;
        public static ConcurrentCircularBuffer<string> Events { get; } = new(MaxSize);

        public void Emit(LogEvent? logEvent)
        {
            if (logEvent is null)
                throw new ArgumentNullException(nameof(logEvent));

            var renderSpace = new StringWriter();
            _textFormatter.Format(logEvent, renderSpace);
            Events.Enqueue(renderSpace.ToString());

            LogMessageUpdatedEvent?.Invoke(typeof(InMemorySink));
        }
    }
}
