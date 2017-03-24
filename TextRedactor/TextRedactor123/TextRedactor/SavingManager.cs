using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextRedactor
{
    public class SavingManager
    {
        private Project currentProject;

        public SavingManager(Project project)
        {
            currentProject = project;
            currentProject.OnChangingText += currentProject_OnChangingText;
        }

        void currentProject_OnChangingText()
        {
        }
    }
}
