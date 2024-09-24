using BepInEx;
using BepInEx.Configuration;
using JetBrains.Annotations;
using System;
using System.Timers;
using UnityEngine;

namespace CactusPie.RamCleanerInterval
{
	[BepInPlugin("com.cactuspie.ramcleanerinterval", "CactusPie.RamCleanerInterval", "1.0.0")]
	public class CustomRamCleanerIntervalPlugin : BaseUnityPlugin
	{
		private Timer timer;

		public static ConfigEntry<string> CleanNow { get; set; }

		internal static ConfigEntry<int> RamCleanerInterval { get; set; }

		internal static ConfigEntry<bool> IntervalEnabled { get; set; }

		internal static ConfigEntry<bool> OnlyInRaid { get; set; }

		[UsedImplicitly]
		internal void Start()
		{
			const string sectionName = "Override RAM cleaner interval";

			IntervalEnabled = Config.Bind
			(
				sectionName,
				"Interval enabled",
				true,
				new ConfigDescription
				(
					"Whether or not we should use the custom RAM cleaner interval",
					null,
					new ConfigurationManagerAttributes
					{
						Order = 4,
					}
				)
			);

			RamCleanerInterval = Config.Bind
			(
				sectionName,
				"Interval (seconds)",
				300,
				new ConfigDescription
				(
					"Number of seconds between each RAM cleaner execution. Changing this setting resets the interval",
					new AcceptableValueRange<int>(30, 900),
					new ConfigurationManagerAttributes
					{
						Order = 3,
					}
				)
			);

			CleanNow = Config.Bind(
				sectionName,
				"Clean now",
				"Execute the RAM cleaner now",
				new ConfigDescription(
					"Execute the RAM cleaner now",
					null,
					new ConfigurationManagerAttributes
					{
						CustomDrawer = CleanNowButtonDrawer,
						Order = 2,
					}
				));

			OnlyInRaid = Config.Bind
			(
				sectionName,
				"Only in raid",
				true,
				new ConfigDescription
				(
					"Only run the RAM cleaner in raid",
					null,
					new ConfigurationManagerAttributes
					{
						Order = 1,
					}
				)
			);

			timer = new Timer
			{
				AutoReset = true,
				Interval = RamCleanerInterval.Value * 1000,
			};

			timer.Elapsed += TimerOnElapsed;

			RamCleanerInterval.SettingChanged += RamCleanerIntervalOnSettingChanged;
			IntervalEnabled.SettingChanged += IntervalEnabledOnSettingChanged;

			if (IntervalEnabled.Value)
			{
				timer.Start();
			}
			else
			{
				timer.Stop();
			}
		}

		private void IntervalEnabledOnSettingChanged(object sender, EventArgs e)
		{
			if (IntervalEnabled.Value)
			{
				timer.Start();
			}
			else
			{
				timer.Stop();
			}
		}

		private void RamCleanerIntervalOnSettingChanged(object sender, EventArgs e)
		{
			ChangeInterval(RamCleanerInterval.Value);
		}

		private void TimerOnElapsed(object sender, ElapsedEventArgs e)
		{
			ExecuteCleaner();
		}

		/// <summary>
		/// Execute the ram cleaner
		/// </summary>
		/// <param name="force">If the ram cleaner should be executed even when not in raid</param>
		private void ExecuteCleaner(bool force = false)
		{
			if (!force && OnlyInRaid.Value && !GameHelper.IsInGame())
			{
				return;
			}

			Logger.LogInfo("Executing the RAM cleaner");

			GClass813.EmptyWorkingSet();
		}

		private void CleanNowButtonDrawer(ConfigEntryBase entry)
		{
			bool button = GUILayout.Button("Clean now", GUILayout.ExpandWidth(true));
			if (button)
			{
				ExecuteCleaner(true);
			}
		}

		private void ChangeInterval(int interval)
		{
			if (timer.Enabled)
			{
				timer.Stop();
				timer.Interval = interval * 1000;
				timer.Start();
			}
			else
			{
				timer.Interval = interval * 1000;
			}
		}
	}
}
