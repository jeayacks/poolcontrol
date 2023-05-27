using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Pool.Control.Store;
using Pool.Hardware;

namespace Pool.Control.Tests
{
    [TestClass]
    public class WateringControlTests
    {
        private Mock<IHardwareManager> hardwareManager;
        private PoolSettings poolSettings;
        private SystemState systemState;
        private bool wateringState;

        [TestInitialize]
        public void Init()
        {
            this.hardwareManager = new Mock<IHardwareManager>();
            this.hardwareManager.Setup(s => s.Write(It.IsAny<PinName>(), It.IsAny<bool>())).Callback<PinName, bool>((pin, state) =>
            {
                if (pin != PinName.Watering)
                {
                    throw new NotSupportedException();
                }

                wateringState = state;
            });

            this.hardwareManager.Setup(s => s.GetOutput(It.IsAny<PinName>())).Returns<PinName>((pin) =>
            {
                if (pin != PinName.Watering)
                {
                    throw new NotSupportedException();
                }

                return new HardwareOutputState(pin, pin.ToString())
                {
                    State = wateringState,
                };
            });

            this.poolSettings = new PoolSettings();
            this.poolSettings.WateringScheduleTime = TimeSpan.FromHours(7);

            this.systemState = new SystemState();
        }

        [TestMethod]
        public void CheckInitilizationBeforeScheduleTime()
        {
            using (var systemTime = new SystemTimeMock())
            {
                var time = DateTime.Now.Date;
                systemTime.Set(new DateTime(2020, 06, 01, 4, 0, 0));

                var wateringControl = new WateringControl(
                    this.poolSettings,
                    this.systemState,
                    this.hardwareManager.Object);

                Assert.AreEqual(new DateTime(2020, 06, 01, 7, 0, 0), wateringControl.NextSchedule);
            }
        }

        [TestMethod]
        public void CheckInitilizationAfterScheduleTime()
        {
            using (var systemTime = new SystemTimeMock())
            {
                var time = DateTime.Now.Date;
                systemTime.Set(new DateTime(2020, 06, 01, 8, 0, 0));

                var wateringControl = new WateringControl(
                    this.poolSettings,
                    this.systemState,
                    this.hardwareManager.Object);

                Assert.AreEqual(new DateTime(2020, 06, 02, 7, 0, 0), wateringControl.NextSchedule);
            }
        }

        [TestMethod]
        public void AutomaticSchedule()
        {
            using (var systemTime = new SystemTimeMock())
            {
                var time = new DateTime(2020, 06, 01, 6, 0, 0);
                systemTime.Set(time);

                var wateringControl = new WateringControl(
                    this.poolSettings,
                    this.systemState,
                    this.hardwareManager.Object);

                wateringControl.Process();
                Assert.IsFalse(wateringState);

                this.systemState.WateringScheduleEnabled.UpdateValue(true);
                this.systemState.WateringScheduleDuration.UpdateValue(20);

                wateringControl.Process();
                Assert.IsFalse(wateringState);

                systemTime.Set(time.AddHours(1));
                wateringControl.Process();
                Assert.IsTrue(wateringState);

                systemTime.Set(time.AddHours(1).AddMinutes(19));
                wateringControl.Process();
                Assert.IsTrue(wateringState);

                systemTime.Set(time.AddHours(1).AddMinutes(21));
                wateringControl.Process();
                Assert.IsFalse(wateringState);

                systemTime.Set(time.AddDays(1).AddHours(1));
                wateringControl.Process();
                Assert.IsTrue(wateringState);
                systemTime.Set(time.AddDays(1).AddHours(1).AddMinutes(21));
                wateringControl.Process();
                Assert.IsFalse(wateringState);
            }
        }

        [TestMethod]
        public void ManualSchedule()
        {
            using (var systemTime = new SystemTimeMock())
            {
                var time = DateTime.Now.Date;
                systemTime.Set(new DateTime(2020, 06, 01, 8, 0, 0));

                var wateringControl = new WateringControl(
                    this.poolSettings,
                    this.systemState,
                    this.hardwareManager.Object);

                this.systemState.WateringManualDuration.UpdateValue(25);
                this.systemState.WateringManualOn.UpdateValue(true);
                wateringControl.Process();
                Assert.IsTrue(wateringState);

                time = time.AddMinutes(1);
                systemTime.Set(time);
                wateringControl.Process();
                Assert.IsTrue(wateringState);

                time = time.AddMinutes(26);
                systemTime.Set(time);
                wateringControl.Process();
                Assert.IsFalse(wateringState);
                Assert.IsFalse(this.systemState.WateringManualOn.Value);
            }
        }
    }
}