using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.Drawing.Drawing2D;
using System.IO;
using System.Diagnostics;
using SwfDotNet.IO;
using SwfDotNet.IO.ByteCode;
using SwfDotNet.IO.ByteCode.Actions;
using SwfDotNet.IO.Tags;

namespace DWE.MapEditor.CL
{

    class SwfFile
    {
        public string File
        {
            get;
            set;
        }

        public string[] ConstantsPool
        {
            get
            {
                string[] lines = GetBytesCode().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                return lines.First().Replace("constantPool", "").Replace("'", "").Split(',');
            }
        }

        public SwfFile(string file)
        {
            this.File = file;
        }

        public string GetBytesCode()
        {
            StringBuilder code = new StringBuilder();
            SwfReader Reader = new SwfReader(this.File);
            Swf swf = null;
            try
            {
                swf = Reader.ReadSwf();
            }
            catch (Exception Ex){ throw new Exception(Ex.ToString()); }

            IEnumerator enumerator = swf.Tags.GetEnumerator();
            while (enumerator.MoveNext())
            {
                BaseTag current = (BaseTag)enumerator.Current;
                if (current.ActionRecCount != 0)
                {
                    IEnumerator currentenumerator = current.GetEnumerator();
                    while (currentenumerator.MoveNext())
                    {
                        Decompiler decompiler = new Decompiler(swf.Version);
                        foreach (BaseAction action in decompiler.Decompile((byte[])currentenumerator.Current))
                        {
                            code.AppendLine(action.ToString());
                        }
                    }
                }
            }

            return code.ToString();
        }

        private static string PushLineValue(string push)
        {
            return push.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).GetValue(1).ToString();
        }

        private static string PushLineType(string push)
        {
            return push.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Last();
        }

        public string GetPushValue(string name)
        {
            string[] lines = GetBytesCode().Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            string[] variables = this.ConstantsPool;

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("push"))
                {
                    if (PushLineType(lines[i]) == "var")
                    {
                        string pushvariable = variables[int.Parse(PushLineValue(lines[i]))];

                        if (pushvariable == name)
                        {
                            if (lines.Length >= i + 1 && lines[i + 1].StartsWith("push"))
                            {
                                if (PushLineType(lines[i + 1]) == "var")
                                    return variables[int.Parse(PushLineValue(lines[i + 1]))];
                                return PushLineValue(lines[i + 1]);
                            }
                            else return "";
                        }
                    }
                }
            }

            return "";
        }
    }
}
