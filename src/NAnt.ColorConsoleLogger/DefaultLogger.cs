using System;
using System.Collections.Generic;
using System.Text;
using NAnt.Core;
using System.IO;
using System.Collections;
using System.Globalization;

namespace NAnt.ColorConsoleLogger
{
    /// <summary>
    /// This code is pulled directly from NAnt 0.85 final NAnt.Core Log.cs.
    /// One method needed to be modified to call a virtual "write line" method instead of directly
    /// writing to the console output.
    /// </summary>
    [Serializable()]
    public class DefaultLogger : IBuildLogger
    {
        #region Public Instance Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultLogger" /> 
        /// class.
        /// </summary>
        public DefaultLogger()
        {
        }

        #endregion Public Instance Constructors

        #region Implementation of IBuildLogger

        /// <summary>
        /// Gets or sets the highest level of message this logger should respond 
        /// to.
        /// </summary>
        /// <value>
        /// The highest level of message this logger should respond to.
        /// </value>
        /// <remarks>
        /// Only messages with a message level higher than or equal to the given 
        /// level should be written to the log.
        /// </remarks>
        public virtual Level Threshold
        {
            get { return _threshold; }
            set { _threshold = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to produce emacs (and other
        /// editor) friendly output.
        /// </summary>
        /// <value>
        /// <see langword="true" /> if output is to be unadorned so that emacs 
        /// and other editors can parse files names, etc. The default is
        /// <see langword="false" />.
        /// </value>
        public virtual bool EmacsMode
        {
            get { return _emacsMode; }
            set { _emacsMode = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="TextWriter" /> to which the logger is 
        /// to send its output.
        /// </summary>
        /// <value>
        /// The <see cref="TextWriter" /> to which the logger sends its output.
        /// </value>
        public virtual TextWriter OutputWriter
        {
            get { return _outputWriter; }
            set { _outputWriter = value; }
        }

        /// <summary>
        /// Flushes buffered build events or messages to the underlying storage.
        /// </summary>
        public virtual void Flush()
        {
            if (OutputWriter != null)
            {
                OutputWriter.Flush();
            }
        }

        #endregion Implementation of IBuildLogger

        #region Implementation of IBuildListener

        /// <summary>
        /// Signals that a build has started.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="BuildEventArgs" /> object that contains the event data.</param>
        /// <remarks>
        /// This event is fired before any targets have started.
        /// </remarks>
        public virtual void BuildStarted(object sender, BuildEventArgs e)
        {
            _buildReports.Push(new BuildReport(DateTime.Now));
        }

        /// <summary>
        /// Signals that the last target has finished.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="BuildEventArgs" /> object that contains the event data.</param>
        /// <remarks>
        /// This event will still be fired if an error occurred during the build.
        /// </remarks>
        public virtual void BuildFinished(object sender, BuildEventArgs e)
        {
            Exception error = e.Exception;
            int indentationLevel = 0;

            if (e.Project != null)
            {
                indentationLevel = e.Project.IndentationLevel * e.Project.IndentationSize;
            }

            BuildReport report = (BuildReport)_buildReports.Pop();

            if (error == null)
            {
                OutputMessage(Level.Info, string.Empty, indentationLevel);
                if (report.Errors == 0 && report.Warnings == 0)
                {
                    OutputMessage(Level.Info, "BUILD SUCCEEDED", indentationLevel);
                }
                else
                {
                    OutputMessage(Level.Info, string.Format(CultureInfo.InvariantCulture,
                        "BUILD SUCCEEDED - {0} non-fatal error(s), {1} warning(s)",
                        report.Errors, report.Warnings), indentationLevel);
                }
                OutputMessage(Level.Info, string.Empty, indentationLevel);
            }
            else
            {
                OutputMessage(Level.Error, string.Empty, indentationLevel);
                if (report.Errors == 0 && report.Warnings == 0)
                {
                    OutputMessage(Level.Error, "BUILD FAILED", indentationLevel);
                }
                else
                {
                    OutputMessage(Level.Error, string.Format(CultureInfo.InvariantCulture,
                        "BUILD FAILED - {0} non-fatal error(s), {1} warning(s)",
                        report.Errors, report.Warnings), indentationLevel);
                }
                OutputMessage(Level.Error, string.Empty, indentationLevel);

                if (error is BuildException)
                {
                    if (Threshold <= Level.Verbose)
                    {
                        OutputMessage(Level.Error, error.ToString(), indentationLevel);
                    }
                    else
                    {
                        if (error.Message != null)
                        {
                            OutputMessage(Level.Error, error.Message, indentationLevel);
                        }

                        // output nested exceptions
                        Exception nestedException = error.InnerException;
                        int exceptionIndentationLevel = indentationLevel;
                        int indentShift = 4; //e.Project.IndentationSize;
                        while (nestedException != null && !string.IsNullOrEmpty(nestedException.Message))
                        {
                            exceptionIndentationLevel += indentShift;
                            OutputMessage(Level.Error, nestedException.Message, exceptionIndentationLevel);
                            nestedException = nestedException.InnerException;
                        }
                    }
                }
                else
                {
                    OutputMessage(Level.Error, "INTERNAL ERROR", indentationLevel);
                    OutputMessage(Level.Error, string.Empty, indentationLevel);
                    OutputMessage(Level.Error, error.ToString(), indentationLevel);
                    OutputMessage(Level.Error, string.Empty, indentationLevel);
                    OutputMessage(Level.Error, "Please send bug report to nant-developers@lists.sourceforge.net.", indentationLevel);
                }

                OutputMessage(Level.Error, string.Empty, indentationLevel);
            }

            // output total build time
            TimeSpan buildTime = DateTime.Now - report.StartTime;
            OutputMessage(Level.Info, string.Format(CultureInfo.InvariantCulture,
                "Total time: {0} seconds." + Environment.NewLine,
                Math.Round(buildTime.TotalSeconds, 1)), indentationLevel);

            // make sure all messages are written to the underlying storage
            Flush();
        }

        /// <summary>
        /// Signals that a target has started.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="BuildEventArgs" /> object that contains the event data.</param>
        public virtual void TargetStarted(object sender, BuildEventArgs e)
        {
            int indentationLevel = 0;

            if (e.Project != null)
            {
                indentationLevel = e.Project.IndentationLevel * e.Project.IndentationSize;
            }

            if (e.Target != null)
            {
                OutputMessage(Level.Info, string.Empty, indentationLevel);
                OutputMessage(
                    Level.Info,
                    string.Format(CultureInfo.InvariantCulture, "{0}:", e.Target.Name),
                    indentationLevel);
                OutputMessage(Level.Info, string.Empty, indentationLevel);
            }
        }

        /// <summary>
        /// Signals that a task has finished.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="BuildEventArgs" /> object that contains the event data.</param>
        /// <remarks>
        /// This event will still be fired if an error occurred during the build.
        /// </remarks>
        public virtual void TargetFinished(object sender, BuildEventArgs e)
        {
        }

        /// <summary>
        /// Signals that a task has started.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="BuildEventArgs" /> object that contains the event data.</param>
        public virtual void TaskStarted(object sender, BuildEventArgs e)
        {
        }

        /// <summary>
        /// Signals that a task has finished.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="BuildEventArgs" /> object that contains the event data.</param>
        /// <remarks>
        /// This event will still be fired if an error occurred during the build.
        /// </remarks>
        public virtual void TaskFinished(object sender, BuildEventArgs e)
        {
        }

        /// <summary>
        /// Signals that a message has been logged.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="BuildEventArgs" /> object that contains the event data.</param>
        /// <remarks>
        /// Only messages with a priority higher or equal to the threshold of 
        /// the logger will actually be output in the build log.
        /// </remarks>
        public virtual void MessageLogged(object sender, BuildEventArgs e)
        {
            if (_buildReports.Count > 0)
            {
                if (e.MessageLevel == Level.Error)
                {
                    BuildReport report = (BuildReport)_buildReports.Peek();
                    report.Errors++;
                }
                else if (e.MessageLevel == Level.Warning)
                {
                    BuildReport report = (BuildReport)_buildReports.Peek();
                    report.Warnings++;
                }
            }

            // output the message
            OutputMessage(e);
        }

        #endregion Implementation of IBuildListener

        #region Protected Instance Methods

        /// <summary>
        /// Empty implementation which allows derived classes to receive the
        /// output that is generated in this logger.
        /// </summary>
        /// <param name="message">The message being logged.</param>
        protected virtual void Log(string message)
        {
        }

        protected virtual void OutputMessageToConsole(string message, Level messageLevel)
        {
            Console.Out.WriteLine(message);
        }

        #endregion Protected Instance Methods

        #region Private Instance Methods

        /// <summary>
        /// Outputs an indented message to the build log if its priority is 
        /// greather than or equal to the <see cref="Threshold" /> of the 
        /// logger.
        /// </summary>
        /// <param name="messageLevel">The priority of the message to output.</param>
        /// <param name="message">The message to output.</param>
        /// <param name="indentationLength">The number of characters that the message should be indented.</param>
        private void OutputMessage(Level messageLevel, string message, int indentationLength)
        {
            OutputMessage(CreateBuildEvent(messageLevel, message), indentationLength);
        }

        /// <summary>
        /// Outputs an indented message to the build log if its priority is 
        /// greather than or equal to the <see cref="Threshold" /> of the 
        /// logger.
        /// </summary>
        /// <param name="e">The event to output.</param>
        private void OutputMessage(BuildEventArgs e)
        {
            int indentationLength = 0;

            if (e.Project != null)
            {
                indentationLength = e.Project.IndentationLevel * e.Project.IndentationSize;
            }

            OutputMessage(e, indentationLength);
        }

        /// <summary>
        /// Outputs an indented message to the build log if its priority is 
        /// greather than or equal to the <see cref="Threshold" /> of the 
        /// logger.
        /// </summary>
        /// <param name="e">The event to output.</param>
        /// <param name="indentationLength">TODO</param>
        private void OutputMessage(BuildEventArgs e, int indentationLength)
        {
            if (e.MessageLevel >= Threshold)
            {
                string txt = e.Message;

                // beautify the message a bit
                txt = txt.Replace("\t", " "); // replace tabs with spaces
                txt = txt.Replace("\r", ""); // get rid of carriage returns

                // split the message by lines - the separator is "\n" since we've eliminated
                // \r characters
                string[] lines = txt.Split('\n');
                string label = String.Empty;

                if (e.Task != null && !EmacsMode)
                {
                    label = "[" + e.Task.Name + "] ";
                    label = label.PadLeft(e.Project.IndentationSize);
                }

                if (indentationLength > 0)
                {
                    label = new String(' ', indentationLength) + label;
                }

                foreach (string line in lines)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(label);
                    sb.Append(line);

                    string indentedMessage = sb.ToString();

                    // output the message to the console
                    OutputMessageToConsole(indentedMessage, e.MessageLevel);

                    // if an OutputWriter was set, write the message to it
                    if (OutputWriter != null && !object.ReferenceEquals(OutputWriter, Console.Out))
                    {
                        OutputWriter.WriteLine(indentedMessage);
                    }

                    Log(indentedMessage);
                }
            }
        }

        #endregion Private Instance Methods

        #region Private Static Methods

        private static BuildEventArgs CreateBuildEvent(Level messageLevel, string message)
        {
            BuildEventArgs buildEvent = new BuildEventArgs();
            buildEvent.MessageLevel = messageLevel;
            buildEvent.Message = message;
            return buildEvent;
        }

        #endregion Private Static Methods

        #region Private Instance Fields

        private Level _threshold = Level.Info;
        private TextWriter _outputWriter;
        private bool _emacsMode;

        #endregion Private Instance Fields

        #region Private Static Fields

        /// <summary>
        /// Holds a stack of reports for all running builds.
        /// </summary>
        private Stack _buildReports = new Stack();

        #endregion Private Static Fields
    }
}
