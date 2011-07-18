using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple;
using log4net;

namespace Pilgrim.Generator.Console
{
    public abstract class ContextBase : MarshalByRefObject
    {
        CommandResolver resolver = null;
        protected abstract CommandResolver Configure();
        private ILog logger = null;

        protected bool OverrideLogConfig { get { return true; } }
        protected string ProjectText { get; set; }
        protected string DefaultEnvironment { get; set; }

        protected ContextBase(string projectText, string defaultEnv)
        {
            ProjectText = projectText;
            DefaultEnvironment = defaultEnv;
            Init();
        }

        protected void Init()
        {
            this.logger = LogManager.GetLogger(this.GetType());
            try
            {
                resolver = Configure();
            }
            catch (Exception e)
            {
                logger.Warn("Failed to configure: {0}".AsFormatFor(e.Message) , e);
            }
        }

      
        protected virtual void OnBeforeParse(string command, bool interactive) { }
        protected virtual void OnBeforeExecute(ICommand commandObject, string command, bool interactive)
        {
            if (interactive && commandObject is IUnsafeCommand)
                throw new ParserException("Command not allowed in interactive mode.");
        }
        protected virtual void OnAfterExecute(ICommand commandObject, string command, bool interactive) { }

        public virtual void Execute(string command, bool interactive)
        {
            try
            {
                OnBeforeParse(command, interactive);

                var commandObject = resolver.Resolve(command);

                OnBeforeExecute(commandObject, command, interactive);

                commandObject.Execute();

                OnAfterExecute(commandObject, command, interactive);
            }
            catch (ParserException e)
            {
                logger.Warn(e.Message);
                if (!interactive) Environment.Exit(1);
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                if (!interactive) Environment.Exit(1);
            }
        }
    }
}
