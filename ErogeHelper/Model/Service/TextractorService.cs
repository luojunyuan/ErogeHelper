using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ErogeHelper.Common.Entity;
using ErogeHelper.Function;
using ErogeHelper.Model.Service.Interface;

namespace ErogeHelper.Model.Service
{
    public class TextractorService : ITextractorService
    {
        public event Action<HookParam>? DataEvent;
        public event Action<HookParam>? SelectedDataEvent;

        public bool HasInjected { get; set; }

        public void InjectProcesses(IEnumerable<Process> processEnumerable)
        {
            _processes = processEnumerable;

            // Current texthook.dll version 4.15
            string textractorPath = Directory.GetCurrentDirectory() + @"\libs\texthost.dll";
            if (!File.Exists(textractorPath))
                throw new FileNotFoundException(textractorPath);

            _createThread = CreateThreadHandle;
            _output = OutputHandle;
            _removeThread = RemoveThreadHandle;
            _callback = OnConnectCallBackHandle;

            _ = TextHostDll.TextHostInit(_callback, _ => { }, _createThread, _removeThread, _output);

            foreach (Process p in _processes)
            {
                _ = TextHostDll.InjectProcess((uint)p.Id);
                Log.Info($"attach to PID {p.Id}.");
            }

            HasInjected = true;
        }

        private IEnumerable<Process> _processes = Array.Empty<Process>();

        public void InsertHook(string hookcode)
        {
            if (!_processes.Any())
                throw new ArgumentException(_processes.ToString());

            if (hookcode.StartsWith('/'))
                hookcode = hookcode[1..];

            string engineName;
            if (hookcode.StartsWith('R'))
            {
                // engineName = "READ";
                if (_threadHandleDict.Any(hcodeItem => hookcode.Equals(hcodeItem.Value.Hookcode)))
                {
                    DataEvent?.Invoke(new HookParam
                    {
                        Name = "Console",
                        Hookcode = "HB0@0",
                        Text = "ErogeHelper: The Read-Code has already insert"
                    });
                    return;
                }
            }
            else
            {
                // HCode
                engineName = hookcode[(hookcode.LastIndexOf(':') + 1)..];

                // 重复插入相同的code(可能)会导致产生很高位的Context
                if (_threadHandleDict.Any(hcodeItem => 
                    engineName.Equals(hcodeItem.Value.Name) || hookcode.Equals(hcodeItem.Value.Hookcode)))
                {
                    DataEvent?.Invoke(new HookParam
                    {
                        Name = "Console",
                        Hookcode = "HB0@0",
                        Text = "ErogeHelper: The Hook-Code has already insert"
                    });
                    return;
                }
            }

            foreach (Process p in _processes)
            {
                _ = TextHostDll.InsertHook((uint)p.Id, hookcode);
                Log.Info($"Try insert hook {hookcode} to PID {p.Id}");
            }
        }

        public void SearchRCode(string text)
        {
            if (!_processes.Any())
                throw new ArgumentException(_processes.ToString());

            throw new NotImplementedException();
        }

        public void UpdateSelectedThreadSetting(HookParam setting)
        {
            _setting = setting;
        }

        #region TextHost Callback Implement

        private TextHostDll.OnOutputText? _output;
        private TextHostDll.ProcessCallback? _callback;
        private TextHostDll.OnCreateThread? _createThread;
        private TextHostDll.OnRemoveThread? _removeThread;

        private readonly Dictionary<long, HookParam> _threadHandleDict = new();

        private void CreateThreadHandle(
            long threadId,
            uint processId,
            ulong address,
            ulong context,
            ulong subContext,
            string name,
            string hookcode)
        {
            _threadHandleDict[threadId] = new HookParam
            {
                Handle = threadId,
                Pid = processId,
                Address = (long)address,
                Ctx = (long)context,
                Ctx2 = (long)subContext,
                Name = name,
                Hookcode = hookcode
            };
        }

        private HookParam _setting = new();

        private void OutputHandle(long threadId, string opData, uint length)
        {
            if (length > 500)
                return;

            HookParam hp = _threadHandleDict[threadId];
            hp.Text = opData;

            DataEvent?.Invoke(hp);

            if (!string.IsNullOrWhiteSpace(_setting.Hookcode)
                && _setting.Hookcode == hp.Hookcode
                && (_setting.Ctx & 0xFFFF) == (hp.Ctx & 0xFFFF)
                && _setting.Ctx2 == hp.Ctx2)
            {
                Log.Debug(hp.Text);
                SelectedDataEvent?.Invoke(hp);
            }
            else if (_setting.Hookcode.StartsWith('R')
                     && hp.Name.Equals("READ"))
            {
                Log.Debug(hp.Text);
                SelectedDataEvent?.Invoke(hp);
            }
        }

        private void RemoveThreadHandle(long threadId) { }

        private void OnConnectCallBackHandle(uint processId)
        {
            if (_setting.Hookcode != string.Empty)
            {
                InsertHook(_setting.Hookcode);
            }
        }

        #endregion
    }
}