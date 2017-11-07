using System;
using System.Collections.Generic;
using System.Text;
using NAnt.Core;
using System.Collections;

namespace NAnt.ColorConsoleLogger
{
    /// <summary>
    /// This code is from Eric Liu.
    /// http://twericliu.blogspot.com/2008/03/colorized-nant-console-output.html
    /// </summary>
    public class ColorConsoleLogger : DefaultLogger
    {
        private readonly MessageLevelColorMap levelColorMap = new MessageLevelColorMap();

        public override void BuildStarted(object sender, BuildEventArgs e)
        {
            base.BuildStarted(sender, e);
            levelColorMap.ConfigureUsing(e.Project.Properties, "ConsoleColorLogger");
        }

        protected override void OutputMessageToConsole(string message, Level messageLevel)
        {
            if (message.Contains("BUILD SUCCEEDED"))
            {
                // Clearly a hack, but this is not intended for widespread general use.
                using (new ConsoleColorScope(ConsoleColor.Green))
                {
                    base.OutputMessageToConsole(message, messageLevel);
                }
            }
            else
            {
                using (new ConsoleColorScope(levelColorMap[messageLevel]))
                {
                    base.OutputMessageToConsole(message, messageLevel);
                }
            }
        }
    }

    public class MessageLevelColorMap
    {
        private readonly IDictionary map = new Hashtable();

        public MessageLevelColorMap()
        {
            map[Level.Error] = ConsoleColor.Red;
            map[Level.Warning] = ConsoleColor.Yellow;
            map[Level.Info] = Console.ForegroundColor;
            map[Level.Verbose] = ConsoleColor.DarkGray;
            map[Level.Debug] = ConsoleColor.Blue;
        }

        public ConsoleColor this[Level level]
        {
            get { return (ConsoleColor)map[level]; }
        }

        /// <param name="properties">keys and values should both be of type <see cref="string"/> </param>
        /// <param name="propertyKeyPrefix">property key prefix without the "dot" (example: "some.prefix")</param>
        public void ConfigureUsing(IDictionary properties, string propertyKeyPrefix)
        {
            ConfigureColorForLevel(Level.Error, "error", properties, propertyKeyPrefix);
            ConfigureColorForLevel(Level.Warning, "warning", properties, propertyKeyPrefix);
            ConfigureColorForLevel(Level.Info, "info", properties, propertyKeyPrefix);
            ConfigureColorForLevel(Level.Debug, "debug", properties, propertyKeyPrefix);
            ConfigureColorForLevel(Level.Verbose, "verbose", properties, propertyKeyPrefix);
        }

        private void ConfigureColorForLevel(Level level, string propertySuffix, IDictionary properties,
                                            string propertyKeyPrefix)
        {
            string propertyKey = propertyKeyPrefix + "." + propertySuffix;
            if (!properties.Contains(propertyKey)) return;

            string colorName = (string)properties[propertyKey];
            map[level] = ParseConsoleColor(colorName);
        }

        private static ConsoleColor ParseConsoleColor(string colorName)
        {
            if (colorName.Equals("default", StringComparison.CurrentCultureIgnoreCase))
            {
                return Console.ForegroundColor;
            }

            try
            {
                return (ConsoleColor)Enum.Parse(typeof(ConsoleColor), colorName, true);
            }
            catch (ArgumentException)
            {
                throw new BuildException(
                    string.Format("Unrecognized console color:<{0}>. Accepted colors are 'default' and those in this list:<{1}>", colorName,
                                  "http://msdn2.microsoft.com/en-us/library/system.consolecolor.aspx"));
            }
        }
    }

    public class ConsoleColorScope : IDisposable
    {
        private readonly ConsoleColor originalColor;

        public ConsoleColorScope(ConsoleColor newForegroundColor)
        {
            originalColor = Console.ForegroundColor;
            Console.ForegroundColor = newForegroundColor;
        }

        public void Dispose()
        {
            Console.ForegroundColor = originalColor;
        }
    }
}
