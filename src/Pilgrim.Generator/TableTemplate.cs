using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.NVelocity;
using Pilgrim.Generator.Metadata;
using Simple;

namespace Pilgrim.Generator
{
    public class TableTemplate : SimpleTemplate, ITableTemplate
    {
        public string FileTemplate { get; set; }
        public string Project { get; set; }
        public ITableTemplate Target(string project, string file)
        {
            Project = project;
            FileTemplate = file;
            return this;
        }

        public string FileType { get; set; }
        public ITableTemplate As(string type)
        {
            this.FileType = type;
            return this;
        }

        public ITableConventions Conventions { get; set; }
        public ITableTemplate WithConventions(ITableConventions conventions)
        {
            this.Conventions = conventions;
            return this;
        }

        public void Create(DbTable table)
        {
            using (var project = new ProjectFileWriter(Project))
            {
                var fileName = FileTemplate.AsFormatFor(Conventions.NameFor(table));
                project.AddNewFile(fileName, FileType, this.ToString());
            }
        }


        public TableTemplate(string template)
            : base(template)
        {
            FileType = ProjectFileWriter.Compile;
        }

    }
}
