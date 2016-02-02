﻿/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using FSO.SimAntics.Engine.Utils;
using FSO.SimAntics.Primitives;
using FSO.Files.Formats.IFF.Chunks;
using FSO.SimAntics.Model;

namespace FSO.SimAntics.Engine
{
    public static class VMDialogHandler
    {
        //should use a Trie for this in future, for performance reasons
        private static string[] valid = {
            "Object", "Me", "TempXL:", "Temp:", "$", "Attribute:", "DynamicStringLocal:", "Local:", "NameLocal:", "DynamicObjectName", "\r\n"
        };

        public static void ShowDialog(VMStackFrame context, VMDialogOperand operand, STR source)
        {
            VMDialogInfo info = new VMDialogInfo
            {
                Block = (operand.Flags & VMDialogFlags.Continue) == 0,
                Caller = context.Caller,
                Icon = context.StackObject,
                Operand = operand,
                Message = ParseDialogString(context, source.GetString(Math.Max(0, operand.MessageStringID - 1)), source),
                Title = (operand.TitleStringID == 0) ? "" : ParseDialogString(context, source.GetString(operand.TitleStringID - 1), source),
                IconName = (operand.IconNameStringID == 0) ? "" : ParseDialogString(context, source.GetString(operand.IconNameStringID - 1), source),

                Yes = (operand.YesStringID == 0) ? null : ParseDialogString(context, source.GetString(operand.YesStringID - 1), source), 
                No = (operand.NoStringID == 0) ? null : ParseDialogString(context, source.GetString(operand.NoStringID - 1), source),
                Cancel = (operand.CancelStringID == 0) ? null : ParseDialogString(context, source.GetString(operand.CancelStringID - 1), source),
            };
            context.VM.SignalDialog(info);
        }

        private static bool CommandSubstrValid(string command)
        {
            for (int i = 0; i < valid.Length; i++)
            {
                if (command.Length <= valid[i].Length && command.Equals(valid[i].Substring(0, command.Length))) return true;
            }
            return false;
        }

        public static string ParseDialogString(VMStackFrame context, string input, STR source)
        {
            int state = 0;
            StringBuilder command = new StringBuilder();
            StringBuilder output = new StringBuilder();

            for (int i = 0; i < input.Length; i++)
            {
                if (state == 0)
                {
                    if (input[i] == '$')
                    {
                        state = 1; //start parsing string
                        command.Clear();
                    } else {
                        output.Append(input[i]);
                    }
                }
                else
                {
                    command.Append(input[i]);
                    if (i == input.Length - 1 || !CommandSubstrValid(command.ToString()))
                    {
                        if (i != input.Length - 1 || char.IsDigit(input[i]))
                        {
                            command.Remove(command.Length - 1, 1);
                            i--;
                        }

                        var cmdString = command.ToString();
                        short[] values = new short[3];
                        if (cmdString.Length > 1 && cmdString[cmdString.Length - 1] == ':')
                        {
                            try
                            {
                                if (cmdString == "DynamicStringLocal:")
                                {
                                    values[1] = -1;
                                    values[2] = -1;
                                    for (int j=0; j<3; j++)
                                    {
                                        char next = input[++i];
                                        string num = "";
                                        while (char.IsDigit(next))
                                        {
                                            num += next;
                                            next = (++i == input.Length) ? '!': input[i];
                                        }
                                        if (num == "")
                                        {
                                            values[j] = -1;
                                            if (j == 1) values[2] = -1;
                                            break;
                                        }
                                        values[j] = short.Parse(num);
                                        if (i == input.Length || next != ':') break;
                                    }
                                }
                                else
                                {
                                    char next = input[++i];
                                    string num = "";
                                    while (char.IsDigit(next))
                                    {
                                        num += next;
                                        next = (++i == input.Length) ? '!' : input[i];
                                    }
                                    values[0] = short.Parse(num);
                                }
                                i--;
                            }
                            catch (FormatException)
                            {

                            }
                        }
                        switch (cmdString)
                        {
                            case "Object":
                            case "DynamicObjectName":
                                output.Append(context.StackObject.ToString()); break;
                            case "Me":
                                output.Append(context.Caller.ToString()); break;
                            case "TempXL:":
                                output.Append(VMMemory.GetBigVariable(context, Scopes.VMVariableScope.TempXL, values[0]).ToString()); break;
                            case "Temp:":
                                output.Append(VMMemory.GetBigVariable(context, Scopes.VMVariableScope.Temps, values[0]).ToString()); break;
                            case "$":
                                output.Append("$"); i--; break;
                            case "Attribute:":
                                output.Append(VMMemory.GetBigVariable(context, Scopes.VMVariableScope.MyObjectAttributes, values[0]).ToString()); break;
                            case "DynamicStringLocal:":
                                STR res = null;
                                if (values[2] != -1 && values[1] != -1)
                                {
                                    VMEntity obj = context.VM.GetObjectById((short)context.Locals[values[2]]);
                                    if (obj == null) break;
                                    ushort tableID = (ushort)context.Locals[values[1]];
                                    
                                    {//local
                                        if (obj.SemiGlobal != null) res = obj.SemiGlobal.Resource.Get<STR>(tableID);
                                        if (res == null) res = obj.Object.Resource.Get<STR>(tableID);
                                        if (res == null) res = context.Global.Resource.Get<STR>(tableID);
                                    }
                                } else if (values[1] != -1)
                                {
                                    //global table
                                    ushort tableID = (ushort)context.Locals[values[1]];
                                    res = context.Global.Resource.Get<STR>(tableID);

                                } else
                                {
                                    res = source;
                                }
                                
                                ushort index = (ushort)context.Locals[values[0]];
                                if (res != null) output.Append(res.GetString(index));
                                break;
                            case "Local:": 
                                output.Append(VMMemory.GetBigVariable(context, Scopes.VMVariableScope.Local, values[0]).ToString()); break;
                            case "NameLocal:":
                                output.Append("(NameLocal)"); break;
                            default:
                                output.Append(cmdString);
                                break;
                        }
                        state = 0;
                    }
                }
            }
            output.Replace("\r\n", "\r\n\r\n");
            return output.ToString();
        }
    }
}
