using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace TrinityWPPSync
{
    class WPP
    {
        public static Regex OpcodeRgx = new Regex(@"Opcode\.(.+), 0x([A-Z0-9]+)(?: | 0x[0-9A-Z]+)?", RegexOptions.IgnoreCase);

        private static string FilePath = @"https://raw.githubusercontent.com/TrinityCore/WowPacketParser/master/WowPacketParser/Enums/Version/V6_1_2_19802/Opcodes.cs";
        public static bool TryPopulate(bool smsg = true)
        {
            if ((smsg ? SMSG : CMSG).Count != 0)
                return true;

            Console.WriteLine("Loading opcodes from GitHub, build 19802...");
            try
            {
                Stream stream;
                if(Config.noGit != string.Empty)
                {
                    stream = File.OpenRead(Config.noGit);
                }
                else
                {
                    WebClient client = new WebClient();
                    stream = client.OpenRead(FilePath);
                }

                StreamReader reader = new StreamReader(stream);
                var content = reader.ReadToEnd().Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                stream.Close();
                foreach (var line in content)
                {
                    var rgxResult = OpcodeRgx.Match(line);
                    if (!rgxResult.Success)
                        continue;

                    var opcodeName = rgxResult.Groups[1].Value;
                    var opcodeValue = Convert.ToInt32(rgxResult.Groups[2].Value, 16);
                    if (opcodeName.Contains("CMSG"))
                        CMSG.Add(opcodeName, opcodeValue);
                    else
                        SMSG.Add(opcodeName, opcodeValue);
                }
            }
            catch (WebException /*whatever*/) // Haha so funny I is.
            {
                Console.WriteLine("Unable to query opcodes. Exiting. Try again.");
                return false;
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message, "Exiting...");
            }

            return (smsg ? SMSG : CMSG).Count != 0;
        }

        public static Dictionary<string, int> CMSG = new Dictionary<string, int>();
        public static Dictionary<string, int> SMSG = new Dictionary<string, int>();
    }
}
