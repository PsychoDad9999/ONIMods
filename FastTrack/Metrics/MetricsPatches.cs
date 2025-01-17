﻿/*
 * Copyright 2022 Peter Han
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software
 * and associated documentation files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or
 * substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using HarmonyLib;
using System.Diagnostics;

namespace PeterHan.FastTrack.Metrics {
#if true
	// Replace with method to patch
	// Brain#UpdateChores is shockingly cheap, just 10-20 ms/1000 ms
	// Game#LateUpdate is 100-200ms
	// Game#Update is 250-300ms
	// Game#SimEveryTick is most of Game#Update
	// StateMachineUpdater#AdvanceOneSimSubTick calls the ISim handlers (200ms)
	// Pathfinding#UpdateNavGrids is <20ms
	// StateMachineUpdater#Render calls the IRender handlers
	// StateMachineUpdater#RenderEveryTick calls the IRenderEveryTick handlers (80ms)
	[HarmonyPatch(typeof(StateMachineUpdater), "Render")]
	public static class TimePatch {
		internal static void Prefix(ref Stopwatch __state) {
			__state = Stopwatch.StartNew();
		}

		internal static void Postfix(Stopwatch __state) {
			DebugMetrics.TRACKED[0].Log(__state.ElapsedTicks);
		}
	}

	[HarmonyPatch(typeof(StateMachineUpdater), "RenderEveryTick")]
	public static class TimePatch2 {
		internal static void Prefix(ref Stopwatch __state) {
			__state = Stopwatch.StartNew();
		}

		internal static void Postfix(Stopwatch __state) {
			DebugMetrics.TRACKED[1].Log(__state.ElapsedTicks);
		}
	}
#endif

	/// <summary>
	/// Applied to Game to log Game update metrics if enabled.
	/// </summary>
	[HarmonyPatch(typeof(Game), "Update")]
	public static class Game_Update_Patch {
		internal static bool Prepare() => FastTrackOptions.Instance.Metrics;

		/// <summary>
		/// Applied before Update runs.
		/// </summary>
		internal static void Prefix(ref Stopwatch __state) {
			__state = Stopwatch.StartNew();
		}

		/// <summary>
		/// Applied after Update runs.
		/// </summary>
		internal static void Postfix(Stopwatch __state) {
			DebugMetrics.GAME_UPDATE.Log(__state.ElapsedTicks);
		}
	}

	/// <summary>
	/// Applied to Game to log Game late update metrics if enabled.
	/// </summary>
	[HarmonyPatch(typeof(Game), "LateUpdate")]
	public static class Game_LateUpdate_Patch {
		internal static bool Prepare() => FastTrackOptions.Instance.Metrics;

		/// <summary>
		/// Applied before Update runs.
		/// </summary>
		internal static void Prefix(ref Stopwatch __state) {
			__state = Stopwatch.StartNew();
		}

		/// <summary>
		/// Applied after Update runs.
		/// </summary>
		internal static void Postfix(Stopwatch __state) {
			DebugMetrics.GAME_LATEUPDATE.Log(__state.ElapsedTicks);
		}
	}

	/// <summary>
	/// Applied to Sensors to log sensor update metrics if enabled.
	/// </summary>
	[HarmonyPatch(typeof(Sensors), nameof(Sensors.UpdateSensors))]
	public static class Sensors_UpdateSensors_Patch {
		internal static bool Prepare() => FastTrackOptions.Instance.Metrics;

		/// <summary>
		/// Applied before UpdateSensors runs.
		/// </summary>
		internal static void Prefix(ref Stopwatch __state) {
			__state = Stopwatch.StartNew();
		}

		/// <summary>
		/// Applied after UpdateSensors runs.
		/// </summary>
		internal static void Postfix(Stopwatch __state) {
			DebugMetrics.SENSORS.Log(__state.ElapsedTicks);
		}
	}

	/// <summary>
	/// Applied to SimAndRenderScheduler.RenderEveryTickUpdater to log sim/render update
	/// metrics if enabled.
	/// 
	/// By far the worst: 4917/89,257us|1/18us.
	/// FishFeeder is cheap
	/// BrainScheduler is fairly expensive but covered elsewhere
	/// K*Collider2D and SpriteSheetAnimManager are necessary
	/// ColonyAchievementTracker is not easy to optimize any further
	/// BubbleManager is dead code
	/// TimerSideScreen, PlayerControlledToggleSideScreen, LogicBitSelectorSideScreen can be
	/// optimized
	/// PumpingStationGuide and LightSymbolTracker can be cut to 200ms
	/// </summary>
#if false
	[HarmonyPatch(typeof(SimAndRenderScheduler.RenderEveryTickUpdater),
		nameof(SimAndRenderScheduler.RenderEveryTickUpdater.Update))]
	public static class SimAndRenderScheduler_RenderEveryTickUpdater_Update_Patch {
		internal static bool Prepare() => FastTrackOptions.Instance.Metrics;

		/// <summary>
		/// Applied before Update runs.
		/// </summary>
		internal static void Prefix(ref Stopwatch __state) {
			__state = Stopwatch.StartNew();
		}

		/// <summary>
		/// Applied after Update runs.
		/// </summary>
		internal static void Postfix(IRenderEveryTick updater, Stopwatch __state) {
			DebugMetrics.SIMANDRENDER[(int)UpdateRate.RENDER_EVERY_TICK].AddSlice(updater.
				GetType().FullName, __state.ElapsedTicks);
		}
	}

	/// <summary>
	/// Applied to SimAndRenderScheduler.Render200ms to log sim/render update metrics if
	/// enabled.
	/// </summary>
	[HarmonyPatch(typeof(SimAndRenderScheduler.Render200ms),
		nameof(SimAndRenderScheduler.Render200ms.Update))]
	public static class SimAndRenderScheduler_Render200ms_Update_Patch {
		internal static bool Prepare() => FastTrackOptions.Instance.Metrics;

		/// <summary>
		/// Applied before Update runs.
		/// </summary>
		internal static void Prefix(ref Stopwatch __state) {
			__state = Stopwatch.StartNew();
		}

		/// <summary>
		/// Applied after Update runs.
		/// </summary>
		internal static void Postfix(IRender200ms updater, Stopwatch __state) {
			DebugMetrics.SIMANDRENDER[(int)UpdateRate.RENDER_200ms].AddSlice(updater.
				GetType().FullName, __state.ElapsedTicks);
		}
	}

	/// <summary>
	/// Applied to SimAndRenderScheduler.Render1000msUpdater to log sim/render update metrics
	/// if enabled.
	/// </summary>
	[HarmonyPatch(typeof(SimAndRenderScheduler.Render1000msUpdater),
		nameof(SimAndRenderScheduler.Render1000msUpdater.Update))]
	public static class SimAndRenderScheduler_Render1000msUpdater_Update_Patch {
		internal static bool Prepare() => FastTrackOptions.Instance.Metrics;

		/// <summary>
		/// Applied before Update runs.
		/// </summary>
		internal static void Prefix(ref Stopwatch __state) {
			__state = Stopwatch.StartNew();
		}

		/// <summary>
		/// Applied after Update runs.
		/// </summary>
		internal static void Postfix(IRender1000ms updater, Stopwatch __state) {
			DebugMetrics.SIMANDRENDER[(int)UpdateRate.RENDER_1000ms].AddSlice(updater.
				GetType().FullName, __state.ElapsedTicks);
		}
	}

	/// <summary>
	/// Applied to SimAndRenderScheduler.SimEveryTickUpdater to log sim/render update metrics
	/// if enabled.
	/// </summary>
	[HarmonyPatch(typeof(SimAndRenderScheduler.SimEveryTickUpdater),
		nameof(SimAndRenderScheduler.SimEveryTickUpdater.Update))]
	public static class SimAndRenderScheduler_SimEveryTickUpdater_Update_Patch {
		internal static bool Prepare() => FastTrackOptions.Instance.Metrics;

		/// <summary>
		/// Applied before Update runs.
		/// </summary>
		internal static void Prefix(ref Stopwatch __state) {
			__state = Stopwatch.StartNew();
		}

		/// <summary>
		/// Applied after Update runs.
		/// </summary>
		internal static void Postfix(ISimEveryTick updater, Stopwatch __state) {
			DebugMetrics.SIMANDRENDER[(int)UpdateRate.SIM_EVERY_TICK].AddSlice(updater.
				GetType().FullName, __state.ElapsedTicks);
		}
	}

	/// <summary>
	/// Applied to SimAndRenderScheduler.Sim33msUpdater to log sim/render update metrics
	/// if enabled.
	/// </summary>
	[HarmonyPatch(typeof(SimAndRenderScheduler.Sim33msUpdater),
		nameof(SimAndRenderScheduler.Sim33msUpdater.Update))]
	public static class SimAndRenderScheduler_Sim33msUpdater_Update_Patch {
		internal static bool Prepare() => FastTrackOptions.Instance.Metrics;

		/// <summary>
		/// Applied before Update runs.
		/// </summary>
		internal static void Prefix(ref Stopwatch __state) {
			__state = Stopwatch.StartNew();
		}

		/// <summary>
		/// Applied after Update runs.
		/// </summary>
		internal static void Postfix(ISim33ms updater, Stopwatch __state) {
			DebugMetrics.SIMANDRENDER[(int)UpdateRate.SIM_33ms].AddSlice(updater.
				GetType().FullName, __state.ElapsedTicks);
		}
	}

	/// <summary>
	/// Applied to SimAndRenderScheduler.Sim200msUpdater to log sim/render update metrics
	/// if enabled.
	/// </summary>
	[HarmonyPatch(typeof(SimAndRenderScheduler.Sim200msUpdater),
		nameof(SimAndRenderScheduler.Sim200msUpdater.Update))]
	public static class SimAndRenderScheduler_Sim200msUpdater_Update_Patch {
		internal static bool Prepare() => FastTrackOptions.Instance.Metrics;

		/// <summary>
		/// Applied before Update runs.
		/// </summary>
		internal static void Prefix(ref Stopwatch __state) {
			__state = Stopwatch.StartNew();
		}

		/// <summary>
		/// Applied after Update runs.
		/// </summary>
		internal static void Postfix(ISim200ms updater, Stopwatch __state) {
			DebugMetrics.SIMANDRENDER[(int)UpdateRate.SIM_200ms].AddSlice(updater.
				GetType().FullName, __state.ElapsedTicks);
		}
	}

	/// <summary>
	/// Applied to SimAndRenderScheduler.Sim1000msUpdater to log sim/render update metrics
	/// if enabled.
	/// </summary>
	[HarmonyPatch(typeof(SimAndRenderScheduler.Sim1000msUpdater),
		nameof(SimAndRenderScheduler.Sim1000msUpdater.Update))]
	public static class SimAndRenderScheduler_Sim1000msUpdater_Update_Patch {
		internal static bool Prepare() => FastTrackOptions.Instance.Metrics;

		/// <summary>
		/// Applied before Update runs.
		/// </summary>
		internal static void Prefix(ref Stopwatch __state) {
			__state = Stopwatch.StartNew();
		}

		/// <summary>
		/// Applied after Update runs.
		/// </summary>
		internal static void Postfix(ISim1000ms updater, Stopwatch __state) {
			DebugMetrics.SIMANDRENDER[(int)UpdateRate.SIM_1000ms].AddSlice(updater.
				GetType().FullName, __state.ElapsedTicks);
		}
	}

	/// <summary>
	/// Applied to SimAndRenderScheduler.Sim4000msUpdater to log sim/render update metrics
	/// if enabled.
	/// </summary>
	[HarmonyPatch(typeof(SimAndRenderScheduler.Sim4000msUpdater),
		nameof(SimAndRenderScheduler.Sim4000msUpdater.Update))]
	public static class SimAndRenderScheduler_Sim4000msUpdater_Update_Patch {
		internal static bool Prepare() => FastTrackOptions.Instance.Metrics;

		/// <summary>
		/// Applied before Update runs.
		/// </summary>
		internal static void Prefix(ref Stopwatch __state) {
			__state = Stopwatch.StartNew();
		}

		/// <summary>
		/// Applied after Update runs.
		/// </summary>
		internal static void Postfix(ISim4000ms updater, Stopwatch __state) {
			DebugMetrics.SIMANDRENDER[(int)UpdateRate.SIM_4000ms].AddSlice(updater.
				GetType().FullName, __state.ElapsedTicks);
		}
	}
#endif
}
