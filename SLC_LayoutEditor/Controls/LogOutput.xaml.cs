using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tasty.Logging;

namespace SLC_LayoutEditor.Controls
{
    /// <summary>
    /// Interaction logic for LogOutput.xaml
    /// </summary>
    public partial class LogOutput : Border, IConsole
    {
        public event EventHandler<EventArgs> ClosingRequested;

        public bool VerboseLogging() => App.IsDebugMode;

        private const double TIME_GROUPING_THRESHOLD_MS = 2000;

        private DateTime? lastUpdate;

        public LogOutput()
        {
            InitializeComponent();
#if DEBUG
            WriteString("Info!\n", LogType.INFO);
            WriteString("This is a warning\n", LogType.WARNING);
            WriteString("You done goofed up\n", LogType.ERROR);
            WriteString("Psssst, this is secret\n", LogType.CONSOLE);
            WriteString("Beep boop, beep boop, beep boop!\n", LogType.DEBUG);
            WriteString("01001000 01100101 01101100 01101100 01101111 00100000 01110111 01101111 01110010 01101100 01100100\n", LogType.VERBOSE);
#endif
        }

        public void WriteString(string msg, LogType logType)
        {
            Brush brush;
            switch (logType)
            {
                case LogType.INFO:
                    brush = FixedValues.DEFAULT_BRUSH;
                    break;

                case LogType.WARNING:
                    brush = FixedValues.YELLOW_BRUSH;
                    break;

                case LogType.ERROR:
                case LogType.FATAL:
                    brush = FixedValues.RED_BRUSH;
                    break;

                case LogType.CONSOLE:
                    brush = FixedValues.PATCH_CHANGED_BRUSH;
                    break;

                case LogType.DEBUG:
                    brush = FixedValues.LOG_DEBUG_BRUSH;
                    break;

                case LogType.VERBOSE:
                    brush = FixedValues.LOG_VERBOSE_BRUSH;
                    break;

                default:
                    brush = FixedValues.DEFAULT_SECONDARY_BRUSH;
                    break;
            }

            bool isNewBlock = false;
            if (lastUpdate.HasValue)
            {
                if ((DateTime.Now - lastUpdate.Value).TotalMilliseconds > TIME_GROUPING_THRESHOLD_MS)
                {
                    isNewBlock = true;
                }
            }

            SetText(msg, logType, brush, isNewBlock);

            lastUpdate = DateTime.Now;
        }

        void SetText(string msg, LogType logType, Brush brush, bool isNewBlock)
        {
            Dispatcher.Invoke(() =>
            {
                TextRange header = new TextRange(rtb_sessionLog.Document.ContentEnd, rtb_sessionLog.Document.ContentEnd)
                {
                    Text = (isNewBlock ? "\n" : "") + logType.ToPrefix() + " "
                };
                header.ApplyPropertyValue(TextElement.ForegroundProperty, brush);
                header.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);

                TextRange message = new TextRange(rtb_sessionLog.Document.ContentEnd, rtb_sessionLog.Document.ContentEnd)
                {
                    Text = msg
                };
                message.ApplyPropertyValue(TextElement.ForegroundProperty, brush);
                message.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
            });
        }

        private void rtb_sessionLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            rtb_sessionLog.ScrollToEnd();
        }

        private void CloseConsole_Click(object sender, RoutedEventArgs e)
        {
            OnClosingRequested(e);
        }

        protected virtual void OnClosingRequested(EventArgs e)
        {
            ClosingRequested?.Invoke(this, e);
        }

        private void ClearOutput_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                TextRange tr = new TextRange(rtb_sessionLog.Document.ContentStart, rtb_sessionLog.Document.ContentEnd)
                {
                    Text = ""
                };
            });
        }
    }
}
