using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using ErogeHelper.Common;
using ErogeHelper.Common.Entity;
using ErogeHelper.Model.Service;
using ErogeHelper.ViewModel.Page;

namespace ErogeHelper.Model.Repository
{
    public class EhGlobalValueRepository
    {
        /// <summary>
        /// <para>Current game's md5</para>
        /// <para>1. Use for Textractor Output cut-down (not necessary)</para>
        /// <para>2. Use for game data query and access</para>
        /// </summary>
        public string Md5 { get; set; } = string.Empty;

        /// <summary>
        /// For <see cref="GameWindowHooker"/> and <see cref="TextractorService"/>
        /// </summary>
        public IEnumerable<Process> GameProcesses { get; set; } = new List<Process>();

        /// <summary>
        /// Only read in <see cref="GameWindowHooker"/> (not necessary)
        /// <para>A simplification of <b>GameProcesses.FirstOrDefault(p =&gt; p.MainWindowHandle != IntPtr.Zero)</b>
        /// </para>
        /// <para>just for easy use</para>
        /// </summary>
        public Process MainProcess { get; set; } = new();

        /// <summary>
        /// For helping find game information, with <see cref="Utils.GetGameNamesByPath"/>
        /// </summary>
        public string GamePath { get; set; } = string.Empty;

        // (INSTEAD: use message)
        /// <summary>
        /// <para>1. Use for turning off InsideView focus</para>
        /// <para>2. Use for brightness module initialize</para>
        /// </summary>
        public IntPtr GameInsideViewHandle { get; set; } = IntPtr.Zero;

        /// <summary>
        /// Used in Textractor and <see cref="HookViewModel"/>, especially <see cref="HookViewModel.SubmitSetting"/>
        /// </summary>
        public GameTextSetting TextractorSetting { get; set; } = new();

        public static string AppVersion => Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "error";
    }
}