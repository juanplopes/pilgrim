using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TemplateWizard;
using Pilgrim.Template.Dialog;
using System.Windows.Forms;

namespace Pilgrim.Template
{
    public class ProjectWizard : IWizard
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
            var dialog = new DataConnectionDialog();
            DataSource.AddStandardDataSources(dialog);
            if (DataConnectionDialog.Show(dialog) == DialogResult.OK)
            {
                replacementsDictionary["$connectionString$"] = dialog.ConnectionString;
                replacementsDictionary["$connectionProvider$"] = dialog.SelectedDataProvider.Name;
            }
            else
            {
                replacementsDictionary["$connectionString$"] = "";
                replacementsDictionary["$connectionProvider$"] = "";
            }
        }


        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }
    }
}
