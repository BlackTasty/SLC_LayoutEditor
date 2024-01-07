using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SLC_LayoutEditor.Core.AutoFix
{
    public class AutoFixResult
    {
        private string resultText;
        private string successText;
        private int successCount;

        private string failText;
        private int failCount;

        public bool AllSucceeded => failCount == 0;

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

        public AutoFixResult(string resultText, string successText, string failText)
        {
            //this.floor = floor;
            this.resultText = resultText;
            this.successText = successText;
            this.failText = failText;
        }

        public override string ToString()
        {
            return string.Format("{0}\n\n{1}: {2}\n{3}: {4}", resultText, successText, successCount, failText, failCount);
        }
    }
}
