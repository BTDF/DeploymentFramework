// Deployment Framework for BizTalk
// Copyright (C) 2008-14 Thomas F. Abraham, 2004-08 Scott Colestock
// This source file is subject to the Microsoft Public License (Ms-PL).
// See http://www.opensource.org/licenses/ms-pl.html.
// All other rights reserved.

using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace SetEnvUI
{
  
   public enum SetEnvUIValueType {Text,Password,FileSelect,Checkbox,RadioButtons};

   /// <summary>
   /// Summary description for SetEnvUIConfig.
   /// </summary>
   public class SetEnvUIConfig
   {
      /// <summary>
      /// The caption that should appear for the wizard
      /// </summary>
      public string DialogCaption;

      /// <summary>
      /// An array of SetEnvUIConfigItems
      /// </summary>
      [XmlElement(ElementName = "SetEnvUIConfigItem",Type = typeof(SetEnvUIConfigItem))]
      public ArrayList ConfigItems = new ArrayList();

      /// <summary>
      /// Load a configuration file, and return an instance of this class.
      /// </summary>
      /// <param name="filename"></param>
      /// <returns></returns>
      public static SetEnvUIConfig LoadFromFile(string filename)
      {
         SetEnvUIConfig config = null;

         XmlSerializer serializer = new XmlSerializer(typeof(SetEnvUIConfig));
         using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
         {
             config = (SetEnvUIConfig)serializer.Deserialize(fs);
         }
         return config;
      }

      /// <summary>
      /// Saves class instance to a configuration file
      /// </summary>
      /// <param name="filename"></param>
      /// <returns></returns>
      public void SaveToFile(string filename)
      {
         // Don't persist passwords.
         foreach(SetEnvUIConfigItem configItem in ConfigItems)
         {
            if(configItem.ValueType == SetEnvUIValueType.Password)
               configItem.PromptValue = string.Empty;
            else if(configItem.PersistValue == false)
               configItem.PromptValue = string.Empty;
         }

         XmlSerializer serializer = new XmlSerializer(typeof(SetEnvUIConfig));
         TextWriter writer = new StreamWriter(filename);
         serializer.Serialize(writer, this);
         writer.Close();
      }

      /// <summary>
      /// Strongly typed indexer for SetEnvUIConfigItems
      /// </summary>
      [XmlIgnore]
      public SetEnvUIConfigItem this[int index]
      {
         get
         {
            return (SetEnvUIConfigItem)ConfigItems[index];
         }
      }
   }

   /// <summary>
   /// Used in the SetEnvUIConfig class to represent individual configuration items.
   /// </summary>
   public class SetEnvUIConfigItem
   {
      /// <summary>
      /// Text to appear above the entry control
      /// </summary>
      public string PromptText;

      /// <summary>
      /// Text to appear after the control if a checkbox, etc. 
      /// </summary>
      public string Caption;

      /// <summary>
      /// Default value (or last saved value)  
      /// </summary>
      public string PromptValue;

      /// <summary>
      /// True to indicate the value should be saved back to the configuration
      /// file.  Ignored for password types.
      /// </summary>
      [System.ComponentModel.DefaultValueAttribute (true)]
      public bool PersistValue = true;

      /// <summary>
      /// One of the SetEnvUIValueType enumeration members.
      /// </summary>
      public SetEnvUIValueType ValueType;

      /// <summary>
      /// The environment variable name that will be set to be
      /// equal to PromptValue for the application that is launched
      /// once the wizard completes.
      /// </summary>
      public string EnvironmentVarName;

      /// <summary>
      /// Used for radio buttons.  Prompts refer to caption text per radio button - up to 5.
      /// </summary>
      public string[] RadioPrompts;

      /// <summary>
      /// Used for radio buttons.  Values refer to what should be put in EnvironmentVarName when
      /// a particular radio button is checked.
      /// </summary>
      public string[] RadioValues;
   }
}
