using static Matchmore.Tests.Utils;
using NUnit.Framework;
using System;
using Matchmore.SDK;
#if __ANDROID__ || __IOS__
using Matchmore.SDK.Xamarin.Shared;
#endif

namespace Matchmore.Tests
{
	[TestFixture]
	public class InitTests : TestBase
	{
		[SetUp]
		public void Setup()
		{
			Matchmore.SDK.Matchmore.Reset();
#if __ANDROID__ || __IOS__
			ConfigBuilder.SetMobileConfig(new MobileConfig()
			{

			});
#endif
		}

		[Test]
		public void InitWithInvalidProdKey()
		{
			RunSync(async () =>
			{
				await Matchmore.SDK.Matchmore.ConfigureAsync(apiKey);
				Assert.NotNull(Matchmore.SDK.Matchmore.Instance);
				Matchmore.SDK.Matchmore.Instance.WipeData();
				var threw = false;
				try
				{
					await Matchmore.SDK.Matchmore.Instance.SetupMainDeviceAsync();
				}
				catch (SwaggerException ex)
				{
					threw = true;
					Assert.AreEqual(403, ex.StatusCode);
				}
				Assert.IsTrue(threw);

			});
		}

		[TearDown]
		public void Tear()
		{
			Matchmore.SDK.Matchmore.Instance.WipeData();
			Matchmore.SDK.Matchmore.Reset();
		}
	}

}
