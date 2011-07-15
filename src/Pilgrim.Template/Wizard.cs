using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TemplateWizard;

namespace Pilgrim.Template
{
    public class Wizard : IWizard
    {
        public void BeforeOpeningFile(EnvDTE.ProjectItem projectItem)
        {
        }

        public void ProjectFinishedGenerating(EnvDTE.Project project)
        {
        }

        public void ProjectItemFinishedGenerating(EnvDTE.ProjectItem projectItem)
        {
        }

        public void RunFinished()
        {
        }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            replacementsDictionary["$timestamp$"] = string.Format("{0:yyyyMMddHHmmss}", DateTime.Now);
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }
    }
}
