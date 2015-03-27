using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace TrinityWPPSync
{
    /// <summary>
    /// Parses command line arguments.
    /// </summary>
    public static class Config
    {
        [ConfigKey("-o", "Path of output file.")]
        public static string outputPath = "Opcodes_synced.h";

        [ConfigKey("-localwpp", "Path to local WPP Opcodes.cs")]
        public static string localPathWPP = string.Empty;
        public static bool localWPP = false;

        [ConfigKey("-localtc", "Path to local TrinityCore Opcodes.h")]
        public static string localPathTC = string.Empty;
        public static bool localTC = false;

        [ConfigKey("-nosmsg", "Do not update SMSGs.")]
        public static bool noSMSG = false;

        [ConfigKey("-nocmsg", "Do not update CMSGs.")]
        public static bool noCMSG = false;

        [ConfigKey("-help", "Show help.")]
        public static bool help = false;

        public static bool Load(string[] args)
        {
            localWPP = TryGet<string>(args, "-localwpp", ref localPathWPP, AppDomain.CurrentDomain.BaseDirectory + "./Opcodes.cs");
            localTC = TryGet<string>(args, "-localtc", ref localPathTC, AppDomain.CurrentDomain.BaseDirectory + "./Opcodes.h");
            TryGet<string>(args, "-o", ref outputPath, "Opcodes_synced.h");
            TryGet<bool>(args, "-nosmsg", ref noSMSG, false);
            TryGet<bool>(args, "-nocmsg", ref noCMSG, false);
            TryGet<bool>(args, "-help", ref help, false);

            return help ? ShowHelp() : true;
        }

        public static bool ShowHelp()
        {
            Console.WriteLine("Optional arguments:");

            var fields = typeof(Config).GetFields(BindingFlags.Static | BindingFlags.Public);
            var keys = new List<string>();
            var desc = new List<string>();

            foreach (var f in typeof(Config).GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                ConfigKey attr = null;
                if ((attr = (ConfigKey)f.GetCustomAttribute(typeof(ConfigKey), false)) != null)
                {
                    keys.Add(attr.Key);
                    desc.Add(attr.Description);
                }
            }

            var padLength = keys.Max(s => s.Length);
            for (var i = 0; i < keys.Count; ++i)
                Console.WriteLine(" {0} : {1}", keys[i].PadRight(padLength), desc[i]);
            return false;
        }

        public static bool TryGet<T>(string[] args, string argName, ref T dest, T defaultValue)
        {
            var keyType = Type.GetTypeCode(typeof(T));

            var index = Array.IndexOf(args, argName);
            if (keyType == TypeCode.Boolean || index == -1)
            {
                if (index == -1)
                    dest = defaultValue;
                else if (keyType == TypeCode.Boolean)
                    using (var box = new ValueBox<T>())
                    {
                        box.ObjectValue = index != -1;
                        dest = box.GetValue();
                    }
                return false;
            }
            else
            {
                try
                {
                    var val = args[index + 1];
                    using (var box = new ValueBox<T>())
                    {
                        switch (keyType)
                        {
                            case TypeCode.UInt32:
                                box.ObjectValue = Convert.ToUInt32(val);
                                break;
                            case TypeCode.Int32:
                                box.ObjectValue = Convert.ToInt32(val);
                                break;
                            case TypeCode.UInt16:
                                box.ObjectValue = Convert.ToUInt16(val);
                                break;
                            case TypeCode.Int16:
                                box.ObjectValue = Convert.ToInt16(val);
                                break;
                            case TypeCode.Byte:
                                box.ObjectValue = Convert.ToByte(val);
                                break;
                            case TypeCode.SByte:
                                box.ObjectValue = Convert.ToSByte(val);
                                break;
                            case TypeCode.Single:
                                box.ObjectValue = Convert.ToSingle(val);
                                break;
                            case TypeCode.String:
                                box.ObjectValue = Convert.ToString(val);
                                break;
                            default:
                                Console.WriteLine("Unable to read value for argument {0}", argName);
                                break;
                        }
                        dest = box.GetValue();
                    }
                }
                catch (IndexOutOfRangeException /*e*/)
                {
                    dest = defaultValue;
                }
                return true;

            }
        }
    }

    public sealed class ValueBox<T> : IDisposable
    {
        public object ObjectValue;

        public T GetValue()
        {
            return (T)ObjectValue;
        }

        public void Dispose() { }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class ConfigKey : Attribute
    {
        public string Description;
        public string Key;

        public ConfigKey(string key, string attr, params object[] obj)
        {
            Key = key;
            Description = String.Format(attr, obj);
        }
    }
}
