using System;
using System.Collections.Generic;
using System.Text;
using NAnt.Core.Attributes;
using NAnt.Core;
using System.Reflection;

namespace BizTalk.NAnt.Tasks
{
    /// <summary>
    /// Extracts the display name and version number of a .NET assembly into NAnt properties.
    /// </summary>
    [TaskName("getAssemblyNameAndVersion")]
    public class GetAssemblyNameAndVersion : Task
    {
        private string _assemblyPath;
        private string _assemblyNameProperty;
        private string _assemblyVersionProperty;

        [TaskAttribute("assemblyPath")]
        public string AssemblyPath
        {
            get { return _assemblyPath; }
            set { _assemblyPath = value; }
        }

        [TaskAttribute("assemblyNameProperty")]
        public string AssemblyNameProperty
        {
            get { return _assemblyNameProperty; }
            set { _assemblyNameProperty = value; }
        }

        [TaskAttribute("assemblyVersionProperty")]
        public string AssemblyVersionProperty
        {
            get { return _assemblyVersionProperty; }
            set { _assemblyVersionProperty = value; }
        }
	
        protected override void ExecuteTask()
        {
            Assembly a = Assembly.ReflectionOnlyLoadFrom(_assemblyPath);
            this.Properties[_assemblyNameProperty] = a.FullName;
            this.Properties[_assemblyVersionProperty] = a.GetName().Version.ToString();
        }
    }
}
