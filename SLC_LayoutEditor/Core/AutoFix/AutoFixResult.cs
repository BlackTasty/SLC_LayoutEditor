using SLC_LayoutEditor.Controls.Notifications;

namespace SLC_LayoutEditor.Core.AutoFix
{
    public class AutoFixResult
    {
        private string resultText;
        private string successText;
        private int successCount;

        private string failText;
        private int failCount;

        private bool wasAborted;

        private string notificationTitle;

        public bool AllSucceeded => failCount == 0;

        public int SuccessCount => successCount;

        public int FailCount => failCount;

        public int TotalCount => successCount + failCount;

        public bool WasAborted => wasAborted;

        public AutoFixResult(string resultText, string successText, string failText, 
            string notificationTitle)
        {
            //this.floor = floor;
            this.resultText = resultText;
            this.successText = successText;
            this.failText = failText;
            this.notificationTitle = notificationTitle;
        }

        public void Abort()
        {
            wasAborted = true;
        }

        public void CountSuccess()
        {
            successCount++;
        }

        public void CountSuccesses(int amount)
        {
            successCount += amount;
        }

        public void CountFail()
        {
            failCount++;
        }

        public void CountFails(int amount)
        {
            failCount += amount;
        }

        public void SendNotification(string message)
        {
            Notification.MakeTimedNotification(!wasAborted ? notificationTitle : "Auto-fix cancelled", message, 15000, FixedValues.ICON_AUTO_FIX);
        }

        public override string ToString()
        {
            return string.Format("{0}\n\n{1}: {2}\n{3}: {4}", resultText, successText, successCount, failText, failCount);
        }
    }
}
