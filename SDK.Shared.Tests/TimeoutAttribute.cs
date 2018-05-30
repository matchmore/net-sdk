using System;
namespace Matchmore.Tests
{

#if NETCOREAPP2_0 || NETSTANDARD1_0 || NETSTANDARD1_1 || NETSTANDARD1_2 || NETSTANDARD1_3 || NETSTANDARD1_4 || NETSTANDARD1_5 || NETSTANDARD1_6 || NETSTANDARD2_0
    /// <summary>
    /// Polyfil for platforms which don't have this attribute in NUNit
    /// </summary>
	public class TimeoutAttribute : Attribute
	{
		public TimeoutAttribute(long l) { }
	}
#endif
}
