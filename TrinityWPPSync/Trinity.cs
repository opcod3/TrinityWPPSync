using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace TrinityWPPSync
{
    class Trinity
    {
        public static Regex OpcodeRgx = new Regex(@"\s+((CMSG|SMSG)\w+)\s+=\s*0x(\w+),", RegexOptions.IgnoreCase);
        private static string FilePath = @"https://raw.githubusercontent.com/TrinityCore/TrinityCore/6.x/src/server/game/Server/Protocol/Opcodes.h";

        public static void Sync()
        {
            Console.WriteLine("Syncing with TrinityCore...");
            try
            {
                Stream stream;
                if (Config.localTC)
                    stream = File.Open(Config.localPathTC, FileMode.Open);
                else
                {
                    WebClient client = new WebClient();
                    stream = client.OpenRead(FilePath);
                }

                StreamReader reader = new StreamReader(stream);

                var content = reader.ReadToEnd().Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                stream.Close();

                StreamWriter writer = new StreamWriter(Config.outputPath, false);
                foreach (var line in content)
                {
                    var rgxResult = OpcodeRgx.Match(line);
                    if (rgxResult.Success)
                    {
                        string opcodeName = rgxResult.Groups[1].Value;
                        int opcodeValue = Convert.ToInt32(rgxResult.Groups[3].Value, 16);
                        
                        switch (rgxResult.Groups[2].Value)
                        {
                            case "CMSG":
                                if(WPP.CMSG.ContainsKey(opcodeName) && !Config.noCMSG)
                                {
                                    int wppVal = WPP.CMSG[opcodeName];
                                    writer.WriteLine("    {0}= 0x{1:X4},", opcodeName.PadRight(50), wppVal);
                                }
                                else
                                    writer.WriteLine(line);
                                break;

                            case "SMSG":
                                if (WPP.SMSG.ContainsKey(opcodeName) && !Config.noSMSG)
                                {
                                    int wppVal = WPP.SMSG[opcodeName];
                                    writer.WriteLine("    {0}= 0x{1:X4},", opcodeName.PadRight(50), wppVal);
                                }
                                else
                                    writer.WriteLine(line);
                                break;
                        }
                    }
                    else
                    {
                        writer.WriteLine(line);
                    }
                }
                writer.Close();
            }
            catch (WebException /*whatever*/) // Haha so funny I is.
            {
                Console.WriteLine("Unable to query opcodes. Exiting. Try again.");
                return;
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message, "Exiting...");
                return;
            }

            Console.WriteLine("Done!");
        }
    }
}
