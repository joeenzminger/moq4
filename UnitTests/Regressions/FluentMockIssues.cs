﻿using System;
using System.Runtime.Serialization;

using Xunit;


namespace Moq.Tests.Regressions
{
	public class FluentMockIssues
	{
		public interface IOne
		{
			ITwo Two { get; }
		}

		public interface ITwo
		{
			IThree Three { get; }
		}

		public interface IThree
		{
			ITwo LoopBack { get; }
			string SomeString { get; }
		}

		[Fact]
		public void CyclesInThePropertyGraphAreHandled()
		{
			var foo = new Mock<IOne> {DefaultValue = DefaultValue.Mock};
			foo.SetupGet(m => m.Two.Three.SomeString).Returns("blah");

			// the default value of the loopback property is not mocked
			Assert.Null(foo.Object.Two.Three.LoopBack);
			foo.SetupGet(m => m.Two.Three.LoopBack).Returns(Mock.Of<ITwo>());
			Assert.NotNull(foo.Object.Two.Three.LoopBack);
		}

		[Fact]
		public void SerializableTypesNotImplementingISerializableProperlyNotMockable()
		{
			var mock = new Mock<IContainingSerializableProperties> {DefaultValue = DefaultValue.Mock};
			// c.Serializable can't be mocked in a standard way as it doesn't implement the ISerializable properly
			Assert.Throws<ArgumentException>(() => mock.SetupGet(c => c.Serializable.SomeString).Returns("blah"));
		}

		[Fact]
		public void SerializableTypesNotImplementingISerializableProperlySetToDefaultValue()
		{
			var mock = new Mock<IContainingSerializableProperties> {DefaultValue = DefaultValue.Mock};
			mock.SetupGet(c => c.SomeString).Returns("blah");

			Assert.Equal("blah", mock.Object.SomeString);
			Assert.Throws<ArgumentException>(() => mock.Object.Serializable);
		}

		public interface IContainingSerializableProperties
		{
			SerializableWithoutDeserializationConstructor Serializable { get; }
			string SomeString { get; }
		}

		[Serializable]
		public abstract class SerializableWithoutDeserializationConstructor : ISerializable
		{
			public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
			{
			}

			public virtual string SomeString { get; set; }
		}
	}
}
