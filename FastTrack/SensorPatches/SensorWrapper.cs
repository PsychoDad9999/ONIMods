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

using KSerialization;

namespace PeterHan.FastTrack.SensorPatches {
	/// <summary>
	/// Wraps several sensors that were removed from the Sensors class, and only invokes them
	/// when required.
	/// </summary>
	[SerializationConfig(MemberSerialization.OptIn)]
	public sealed class SensorWrapper : KMonoBehaviour, ISlicedSim1000ms {
		/// <summary>
		/// The sensor used to find a balloon stand location.
		/// </summary>
		private BalloonStandCellSensor balloonSensor;

		/// <summary>
		/// The sensor used to find food to eat.
		/// </summary>
		private ClosestEdibleSensor edibleSensor;

		/// <summary>
		/// The sensor used to find a cell to Idle.
		/// </summary>
		private IdleCellSensor idleSensor;

		/// <summary>
		/// The sensor used to find a cell to Mingle.
		/// </summary>
		private MingleCellSensor mingleSensor;

		/// <summary>
		/// The sensor used to set up FetchManager state for ClosestEdibleSensor and
		/// PickupableSensor.
		/// </summary>
		private PathProberSensor pathSensor;

		/// <summary>
		/// The sensor used to find reachable debris items.
		/// </summary>
		private PickupableSensor pickupSensor;

		/// <summary>
		/// The sensor used to find a "safe" cell to move if becoming idle in a dangerous
		/// area.
		/// </summary>
		private SafeCellSensor safeSensor;

		/// <summary>
		/// The sensor used to find available bathrooms.
		/// </summary>
		private ToiletSensor toiletSensor;

#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable CS0649
		[MyCmpGet]
		private KPrefabID id;

		[MyCmpReq]
		private Sensors sensors;

		[MyCmpReq]
		private Klei.AI.Traits traits;
#pragma warning restore CS0649
#pragma warning restore IDE0044

		protected override void OnCleanUp() {
			SlicedUpdaterSim1000ms<SensorWrapper>.instance.UnregisterUpdate1000ms(this);
			base.OnCleanUp();
		}

		protected override void OnSpawn() {
			var opts = FastTrackOptions.Instance;
			base.OnSpawn();
			if (opts.SensorOpts) {
				balloonSensor = sensors.GetSensor<BalloonStandCellSensor>();
				idleSensor = sensors.GetSensor<IdleCellSensor>();
				mingleSensor = sensors.GetSensor<MingleCellSensor>();
				safeSensor = sensors.GetSensor<SafeCellSensor>();
				toiletSensor = sensors.GetSensor<ToiletSensor>();
			}
			if (opts.PickupOpts) {
				edibleSensor = sensors.GetSensor<ClosestEdibleSensor>();
				pathSensor = sensors.GetSensor<PathProberSensor>();
				pickupSensor = sensors.GetSensor<PickupableSensor>();
			}
			RunUpdate();
			SlicedUpdaterSim1000ms<SensorWrapper>.instance.RegisterUpdate1000ms(this);
		}

		/// <summary>
		/// Updates the sensors only once a second, as opposed to every frame.
		/// </summary>
		private void RunUpdate() {
			if (id != null && !id.HasTag(GameTags.Dead)) {
				// The order of sensors matters here
				if (pathSensor != null)
					PathProberSensorUpdater.Update(pathSensor);
				if (pickupSensor != null)
					PickupableSensorUpdater.Update(pickupSensor);
				if (edibleSensor != null)
					ClosestEdibleSensorUpdater.Update(edibleSensor);
				if (balloonSensor != null && traits.HasTrait("BalloonArtist"))
					BalloonStandCellSensorUpdater.Update(balloonSensor);
				if (idleSensor != null)
					IdleCellSensorUpdater.Update(idleSensor);
				if (mingleSensor != null)
					MingleCellSensorUpdater.Update(mingleSensor);
				if (safeSensor != null)
					SafeCellSensorUpdater.Update(safeSensor);
				if (toiletSensor != null)
					ToiletSensorUpdater.Update(toiletSensor);
				// AssignableReachabilitySensor and BreathableAreaSensor are pretty cheap
			}
		}

		public void SlicedSim1000ms(float _) {
			RunUpdate();
		}
	}
}
