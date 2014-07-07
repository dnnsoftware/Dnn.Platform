using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetNuke.Entities.Content.Workflow
{
    public interface IContentWorkflowAction
    {
        string GetAction(string[] parameters);
    }
}
