using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BuildBriefing.wizardSteps
{
    interface IStep
    {
        ucTaskPane ParentPane
        {
            get;
        }
        event EventHandler Completed;
        void WorkComplete();
        void Start();
    }
}
