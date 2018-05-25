using NUnit.Framework;
using System;
using System.Linq;
using static Matchmore.Tests.Utils;
using Matchmore.SDK;
using System.Threading;

namespace Matchmore.Tests
{
	[TestFixture]
	public class CRUDTest : TestBase
    {
		[SetUp]
        public void Setup()
        {
            Matchmore.SDK.Matchmore.Reset();
            SetupDevInstance();
            _instance = Matchmore.SDK.Matchmore.Instance;
            Assert.NotNull(_instance);
        }

		[TearDown]
        public void Tear()
        {
            Matchmore.SDK.Matchmore.Reset();
        }

        [Test]
		public void DeviceCRUDTest()
        {
			var beacon = new IBeaconDevice
			{
				Major = 13,
				Minor = 3,
				ProximityUUID = Guid.NewGuid().ToString(),
				Name = "bacon"
			};

			var createdId = RunSync(() => _instance.CreateDeviceAsync(beacon)).Id;

			var device = _instance.Devices.FirstOrDefault(d => d.Id == createdId) as IBeaconDevice;
			Assert.NotNull(device);

			Assert.AreEqual(createdId, device.Id);
			Assert.AreEqual(beacon.Major, device.Major);
			Assert.AreEqual(beacon.Minor, device.Minor);
			Assert.AreEqual(beacon.ProximityUUID, device.ProximityUUID);

			RunSync(() => _instance.DeleteDeviceAsync(createdId));

			var deletedDevice = _instance.Devices.FirstOrDefault(d => d.Id == createdId);

			Assert.IsNull(deletedDevice);         
        }

		[Test]
        public void PublicationCRUDTest()
        {
			var _pub = RunSync(() =>_instance.CreatePublicationAsync(pub));
			Assert.AreEqual(1, _instance.ActivePublications.Count());
			var persistedPub = _instance.ActivePublications.FirstOrDefault(p => p.Id == _pub.Id);
			Assert.NotNull(persistedPub);

			RunSync(() => _instance.DeletePublicationAsync(persistedPub.Id));

			persistedPub = _instance.ActivePublications.FirstOrDefault(p => p.Id == _pub.Id);
			Assert.IsNull(persistedPub);
        }

		[Test]
        public void PublicationTimeoutTest()
        {
			pub.Duration = 5;
            var _pub = RunSync(() => _instance.CreatePublicationAsync(pub));
            Assert.AreEqual(1, _instance.ActivePublications.Count());
            var persistedPub = _instance.ActivePublications.FirstOrDefault(p => p.Id == _pub.Id);
            Assert.NotNull(persistedPub);

			Thread.Sleep(6000);

            persistedPub = _instance.ActivePublications.FirstOrDefault(p => p.Id == _pub.Id);
            Assert.IsNull(persistedPub);
        }      

		[Test]
        public void SubscriptionCRUDTest()
        {
			var _sub = RunSync(() =>_instance.CreateSubscriptionAsync(sub));
			Assert.AreEqual(1, _instance.ActiveSubscriptions.Count());
			var persistedSub = _instance.ActiveSubscriptions.FirstOrDefault(p => p.Id == _sub.Id);
            Assert.NotNull(persistedSub);

			RunSync(() => _instance.DeleteSubscriptionAsync(persistedSub.Id));

			persistedSub = _instance.ActiveSubscriptions.FirstOrDefault(p => p.Id == _sub.Id);
			Assert.IsNull(persistedSub);
        }

		[Test]
        public void SubscriptionTimeoutTest()
        {
			sub.Duration = 5;
            var _sub = RunSync(() => _instance.CreateSubscriptionAsync(sub));
            Assert.AreEqual(1, _instance.ActiveSubscriptions.Count());
            var persistedSub = _instance.ActiveSubscriptions.FirstOrDefault(p => p.Id == _sub.Id);
            Assert.NotNull(persistedSub);

			Thread.Sleep(6000);

            persistedSub = _instance.ActiveSubscriptions.FirstOrDefault(p => p.Id == _sub.Id);
            Assert.IsNull(persistedSub);
        }
    }
}
