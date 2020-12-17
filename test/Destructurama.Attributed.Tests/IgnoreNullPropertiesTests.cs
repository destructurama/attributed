using Destructurama.Attributed.Tests.Support;
using NUnit.Framework;
using Serilog;
using Serilog.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Destructurama.Attributed.Tests
{
    [TestFixture]
    public class IgnoreNullPropertiesTests
    {
        struct NotLoggedIfNullStruct
        {
            public int Integer { get; set; }

            public int? NullableInteger { get; set; }

            public DateTime DateTime { get; set; }

            public DateTime? NullableDateTime { get; set; }

            public object Object { get; set; }
        }

        class NotLoggedIfNull
        {
            public string String { get; set; }

            public int Integer { get; set; }

            public int? NullableInteger { get; set; }

            public object Object { get; set; }

            public object IntegerAsObject { get; set; }

            public DateTime DateTime { get; set; }

            public DateTime? NullableDateTime { get; set; }

            public NotLoggedIfNullStruct Struct { get; set; }

            public NotLoggedIfNullStruct? NullableStruct { get; set; }

            public NotLoggedIfNullStruct StructPartiallyInitialized { get; set; }
        }

        class NotLoggedIfNullAttributed
        {
            [NotLoggedIfNull]
            public string String { get; set; }

            [NotLoggedIfNull]
            public int Integer { get; set; }

            [NotLoggedIfNull]
            public int? NullableInteger { get; set; }

            [NotLoggedIfNull]
            public object Object { get; set; }

            [NotLoggedIfNull]
            public object IntegerAsObject { get; set; }

            [NotLoggedIfNull]
            public DateTime DateTime { get; set; }

            [NotLoggedIfNull]
            public DateTime? NullableDateTime { get; set; }

            [NotLoggedIfNull]
            public NotLoggedIfNullStruct Struct { get; set; }

            [NotLoggedIfNull]
            public NotLoggedIfNullStruct? NullableStruct { get; set; }

            [NotLoggedIfNull]
            public NotLoggedIfNullStruct StructPartiallyInitialized { get; set; }
        }

        class AttributedWithMask
        {
            [LogMasked(ShowFirst = 3)]
            public string String { get; set; }

            [LogMasked(ShowFirst = 3)]
            public int Integer { get; set; }

            [LogMasked(ShowFirst = 3)]
            public int? NullableInteger { get; set; }

            [LogMasked(ShowFirst = 3)]
            public object Object { get; set; }

            [LogMasked(ShowFirst = 3)]
            public object IntegerAsObject { get; set; }

            [LogMasked(ShowFirst = 7)]
            public DateTime DateTime { get; set; }

            [LogMasked(ShowFirst = 7)]
            public DateTime? NullableDateTime { get; set; }
        }

        class Dependency
        {
            public int Integer { get; set; }

            public int? NullableInteger { get; set; }
        }

        class CustomEnumerableDestructionIgnored : IEnumerable<int>
        {
            public int Integer { get; set; }
            
            public Dependency Dependency { get; set; }

            public IEnumerator<int> GetEnumerator()
            {
                yield return 1;
            }

            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumerator();
        }

        /// <summary>
        /// At least one attribute from Destructurma.Attributed is enough to ignore all default properties on IEnumerable,
        /// when IgnoreNullProperties is true.
        /// </summary>
        class CustomEnumerableAttributed : IEnumerable<int>
        {
            [NotLogged]
            public bool Dummy { get; set; }

            public int Integer { get; set; }

            public int? NullableInteger { get; set; }

            public Dependency Dependency { get; set; }

            public IEnumerator<int> GetEnumerator()
            {
                yield return 1;
            }

            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumerator();
        }

        [Test]
        public void NotLoggedIfNull_Uninitialized()
        {
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            AttributedDestructuringPolicy.IgnoreNullProperties = true;
            try
            {
                var customized = new NotLoggedIfNull();

                log.Information("Here is {@Customized}", customized);

                var sv = (StructureValue)evt.Properties["Customized"];
                var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

                Assert.IsTrue(props.ContainsKey("Integer"));
                Assert.IsTrue(props.ContainsKey("DateTime"));
                Assert.IsTrue(props.ContainsKey("Struct"));
                Assert.IsTrue(props.ContainsKey("StructPartiallyInitialized"));

                Assert.IsFalse(props.ContainsKey("String"));
                Assert.IsFalse(props.ContainsKey("NullableInteger"));
                Assert.IsFalse(props.ContainsKey("IntegerAsObject"));
                Assert.IsFalse(props.ContainsKey("Object"));
                Assert.IsFalse(props.ContainsKey("NullableDateTime"));
                Assert.IsFalse(props.ContainsKey("NullableStruct"));
            }
            finally
            {
                AttributedDestructuringPolicy.IgnoreNullProperties = false;
            }
        }

        [Test]
        public void NotLoggedIfNull_Initialized()
        {
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            AttributedDestructuringPolicy.IgnoreNullProperties = true;
            try
            {

                var dateTime = DateTime.UtcNow;
                var theStruct = new NotLoggedIfNullStruct
                {
                    Integer = 20,
                    NullableInteger = 15,
                    DateTime = dateTime,
                    NullableDateTime = dateTime,
                    Object = "Bar",
                };

                var theStructPartiallyUnitialized = new NotLoggedIfNullStruct
                {
                    Integer = 20,
                    NullableInteger = 15,
                    DateTime = dateTime,
                    NullableDateTime = dateTime,
                    Object = null,
                };

                var customized = new NotLoggedIfNull
                {
                    String = "Foo",
                    Integer = 10,
                    NullableInteger = 5,
                    Object = "Bar",
                    IntegerAsObject = 0,
                    DateTime = dateTime,
                    NullableDateTime = dateTime,
                    Struct = theStruct,
                    NullableStruct = theStruct,
                    StructPartiallyInitialized = theStructPartiallyUnitialized,
                };

                log.Information("Here is {@Customized}", customized);

                var sv = (StructureValue)evt.Properties["Customized"];
                var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

                Assert.IsTrue(props.ContainsKey("String"));
                Assert.IsTrue(props.ContainsKey("Integer"));
                Assert.IsTrue(props.ContainsKey("NullableInteger"));
                Assert.IsTrue(props.ContainsKey("Object"));
                Assert.IsTrue(props.ContainsKey("IntegerAsObject"));
                Assert.IsTrue(props.ContainsKey("DateTime"));
                Assert.IsTrue(props.ContainsKey("NullableDateTime"));
                Assert.IsTrue(props.ContainsKey("Struct"));
                Assert.IsTrue(props.ContainsKey("NullableStruct"));
                Assert.IsTrue(props.ContainsKey("StructPartiallyInitialized"));

                Assert.AreEqual("Foo", props["String"].LiteralValue());
                Assert.AreEqual(10, props["Integer"].LiteralValue());
                Assert.AreEqual(5, props["NullableInteger"].LiteralValue());
                Assert.AreEqual("Bar", props["Object"].LiteralValue());
                Assert.AreEqual(0, props["IntegerAsObject"].LiteralValue());
                Assert.AreEqual(dateTime, props["DateTime"].LiteralValue());
                Assert.AreEqual(dateTime, props["NullableDateTime"].LiteralValue());
                Assert.IsInstanceOf<StructureValue>(props["Struct"]);
                Assert.IsInstanceOf<StructureValue>(props["NullableStruct"]);
                Assert.IsInstanceOf<StructureValue>(props["StructPartiallyInitialized"]);

                var structProps = ((StructureValue)props["Struct"]).Properties
                    .ToDictionary(p => p.Name, p => p.Value);

                Assert.IsTrue(structProps.ContainsKey("Integer"));
                Assert.IsTrue(structProps.ContainsKey("NullableInteger"));
                Assert.IsTrue(structProps.ContainsKey("DateTime"));
                Assert.IsTrue(structProps.ContainsKey("NullableDateTime"));
                Assert.IsTrue(structProps.ContainsKey("Object"));
                Assert.AreEqual(20, structProps["Integer"].LiteralValue());
                Assert.AreEqual(15, structProps["NullableInteger"].LiteralValue());
                Assert.AreEqual(dateTime, structProps["DateTime"].LiteralValue());
                Assert.AreEqual(dateTime, structProps["NullableDateTime"].LiteralValue());
                Assert.AreEqual("Bar", structProps["Object"].LiteralValue());

                var partiallyItitializedProps = ((StructureValue)props["StructPartiallyInitialized"]).Properties
                    .ToDictionary(p => p.Name, p => p.Value);

                Assert.IsTrue(partiallyItitializedProps.ContainsKey("Integer"));
                Assert.IsTrue(partiallyItitializedProps.ContainsKey("NullableInteger"));
                Assert.IsTrue(partiallyItitializedProps.ContainsKey("DateTime"));
                Assert.IsTrue(partiallyItitializedProps.ContainsKey("NullableDateTime"));
                Assert.IsFalse(partiallyItitializedProps.ContainsKey("Object"));
                Assert.AreEqual(20, partiallyItitializedProps["Integer"].LiteralValue());
                Assert.AreEqual(15, partiallyItitializedProps["NullableInteger"].LiteralValue());
                Assert.AreEqual(dateTime, partiallyItitializedProps["DateTime"].LiteralValue());
                Assert.AreEqual(dateTime, partiallyItitializedProps["NullableDateTime"].LiteralValue());

            }
            finally
            {
                AttributedDestructuringPolicy.IgnoreNullProperties = false;
            }
        }

        [Test]
        public void WithMask_NotLoggedIfNull_Uninitialized()
        {
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            AttributedDestructuringPolicy.IgnoreNullProperties = true;
            try
            {
                var customized = new AttributedWithMask();

                log.Information("Here is {@Customized}", customized);

                var sv = (StructureValue)evt.Properties["Customized"];
                var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

                Assert.IsTrue(props.ContainsKey("Integer"));
                Assert.IsTrue(props.ContainsKey("DateTime"));

                Assert.IsFalse(props.ContainsKey("String"));
                Assert.IsFalse(props.ContainsKey("NullableInteger"));
                Assert.IsFalse(props.ContainsKey("IntegerAsObject"));
                Assert.IsFalse(props.ContainsKey("Object"));
                Assert.IsFalse(props.ContainsKey("NullableDateTime"));
            }
            finally
            {
                AttributedDestructuringPolicy.IgnoreNullProperties = false;
            }
        }

        [Test]
        public void WithMask_NotLoggedIfNull_Initialized()
        {
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            AttributedDestructuringPolicy.IgnoreNullProperties = true;
            try
            {

                var dateTime = new DateTime(2000, 1, 2, 3, 4, 5);
                var customized = new AttributedWithMask
                {
                    String = "Foo[Masked]",
                    Integer = 1000,
                    NullableInteger = 2000,
                    Object = "Bar[Masked]",
                    IntegerAsObject = 3000,
                    DateTime = dateTime,
                    NullableDateTime = dateTime,
                };

                log.Information("Here is {@Customized}", customized);

                var sv = (StructureValue)evt.Properties["Customized"];
                var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

                Assert.IsTrue(props.ContainsKey("String"));
                Assert.IsTrue(props.ContainsKey("Integer"));
                Assert.IsTrue(props.ContainsKey("NullableInteger"));
                Assert.IsTrue(props.ContainsKey("Object"));
                Assert.IsTrue(props.ContainsKey("IntegerAsObject"));
                Assert.IsTrue(props.ContainsKey("DateTime"));
                Assert.IsTrue(props.ContainsKey("NullableDateTime"));

                Assert.AreEqual("Foo***", props["String"].LiteralValue());
                Assert.AreEqual("100***", props["Integer"].LiteralValue());
                Assert.AreEqual("200***", props["NullableInteger"].LiteralValue());
                Assert.AreEqual("Bar***", props["Object"].LiteralValue());
                Assert.AreEqual("300***", props["IntegerAsObject"].LiteralValue());
                Assert.AreEqual("01/02/2***", props["DateTime"].LiteralValue());
                Assert.AreEqual("01/02/2***", props["NullableDateTime"].LiteralValue());
            }
            finally
            {
                AttributedDestructuringPolicy.IgnoreNullProperties = false;
            }
        }

        [Test]
        public void EnumerableIgnored()
        {
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            AttributedDestructuringPolicy.IgnoreNullProperties = true;
            try
            {
                var customized = new CustomEnumerableDestructionIgnored()
                {
                    Integer = 0,
                    Dependency = new Dependency
                    {
                        Integer = 0,
                    }
                };

                log.Information("Here is {@Customized}", customized);

                var sv = evt.Properties["Customized"];
                Assert.IsInstanceOf<SequenceValue>(sv);
            }
            finally
            {
                AttributedDestructuringPolicy.IgnoreNullProperties = false;
            }
        }

        [Test]
        public void EnumerableDestructedAsStruct()
        {
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            AttributedDestructuringPolicy.IgnoreNullProperties = true;
            try
            {
                var customized = new CustomEnumerableAttributed
                {
                    Integer = 0,
                    NullableInteger = null,
                    Dependency = new Dependency
                    {
                        Integer = 0,
                    },
                };

                log.Information("Here is {@Customized}", customized);

                var sv = (StructureValue)evt.Properties["Customized"];
                var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

                Assert.IsTrue(props.ContainsKey("Integer"));
                Assert.IsFalse(props.ContainsKey("NullableInteger"));
                Assert.IsTrue(props.ContainsKey("Dependency"));

                var dependencyProps = ((StructureValue)props["Dependency"]).Properties
                    .ToDictionary(p => p.Name, p => p.Value);

                Assert.IsTrue(dependencyProps.ContainsKey("Integer"));
                Assert.IsFalse(dependencyProps.ContainsKey("NullableInteger"));
            }
            finally
            {
                AttributedDestructuringPolicy.IgnoreNullProperties = false;
            }
        }

        [Test]
        public void NotLoggedIfNullAttribute_Uninitialized()
        {
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var customized = new NotLoggedIfNullAttributed();

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("Integer"));
            Assert.IsTrue(props.ContainsKey("DateTime"));
            Assert.IsTrue(props.ContainsKey("Struct"));
            Assert.IsTrue(props.ContainsKey("StructPartiallyInitialized"));

            Assert.IsFalse(props.ContainsKey("String"));
            Assert.IsFalse(props.ContainsKey("NullableInteger"));
            Assert.IsFalse(props.ContainsKey("IntegerAsObject"));
            Assert.IsFalse(props.ContainsKey("Object"));
            Assert.IsFalse(props.ContainsKey("NullableDateTime"));
            Assert.IsFalse(props.ContainsKey("NullableStruct"));
        }

        [Test]
        public void NotLoggedIfNullAttribute_Initialized()
        {
            LogEvent evt = null;

            var log = new LoggerConfiguration()
                .Destructure.UsingAttributes()
                .WriteTo.Sink(new DelegatingSink(e => evt = e))
                .CreateLogger();

            var dateTime = DateTime.UtcNow;
            var theStruct = new NotLoggedIfNullStruct
            {
                Integer = 20,
                NullableInteger = 15,
                DateTime = dateTime,
                NullableDateTime = dateTime,
                Object = "Bar",
            };

            var theStructPartiallyUnitialized = new NotLoggedIfNullStruct
            {
                Integer = 20,
                NullableInteger = 15,
                DateTime = dateTime,
                NullableDateTime = dateTime,
                Object = null,
            };

            var customized = new NotLoggedIfNullAttributed
            {
                String = "Foo",
                Integer = 10,
                NullableInteger = 5,
                Object = "Bar",
                IntegerAsObject = 0,
                DateTime = dateTime,
                NullableDateTime = dateTime,
                Struct = theStruct,
                NullableStruct = theStruct,
                StructPartiallyInitialized = theStructPartiallyUnitialized,
            };

            log.Information("Here is {@Customized}", customized);

            var sv = (StructureValue)evt.Properties["Customized"];
            var props = sv.Properties.ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(props.ContainsKey("String"));
            Assert.IsTrue(props.ContainsKey("Integer"));
            Assert.IsTrue(props.ContainsKey("NullableInteger"));
            Assert.IsTrue(props.ContainsKey("Object"));
            Assert.IsTrue(props.ContainsKey("IntegerAsObject"));
            Assert.IsTrue(props.ContainsKey("DateTime"));
            Assert.IsTrue(props.ContainsKey("NullableDateTime"));
            Assert.IsTrue(props.ContainsKey("Struct"));
            Assert.IsTrue(props.ContainsKey("NullableStruct"));
            Assert.IsTrue(props.ContainsKey("StructPartiallyInitialized"));

            Assert.AreEqual("Foo", props["String"].LiteralValue());
            Assert.AreEqual(10, props["Integer"].LiteralValue());
            Assert.AreEqual(5, props["NullableInteger"].LiteralValue());
            Assert.AreEqual("Bar", props["Object"].LiteralValue());
            Assert.AreEqual(0, props["IntegerAsObject"].LiteralValue());
            Assert.AreEqual(dateTime, props["DateTime"].LiteralValue());
            Assert.AreEqual(dateTime, props["NullableDateTime"].LiteralValue());
            Assert.IsInstanceOf<StructureValue>(props["Struct"]);
            Assert.IsInstanceOf<StructureValue>(props["NullableStruct"]);
            Assert.IsInstanceOf<StructureValue>(props["StructPartiallyInitialized"]);

            var structProps = ((StructureValue)props["Struct"]).Properties
                .ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(structProps.ContainsKey("Integer"));
            Assert.IsTrue(structProps.ContainsKey("NullableInteger"));
            Assert.IsTrue(structProps.ContainsKey("DateTime"));
            Assert.IsTrue(structProps.ContainsKey("NullableDateTime"));
            Assert.IsTrue(structProps.ContainsKey("Object"));
            Assert.AreEqual(20, structProps["Integer"].LiteralValue());
            Assert.AreEqual(15, structProps["NullableInteger"].LiteralValue());
            Assert.AreEqual(dateTime, structProps["DateTime"].LiteralValue());
            Assert.AreEqual(dateTime, structProps["NullableDateTime"].LiteralValue());
            Assert.AreEqual("Bar", structProps["Object"].LiteralValue());

            var partiallyItitializedProps = ((StructureValue)props["StructPartiallyInitialized"]).Properties
                .ToDictionary(p => p.Name, p => p.Value);

            Assert.IsTrue(partiallyItitializedProps.ContainsKey("Integer"));
            Assert.IsTrue(partiallyItitializedProps.ContainsKey("NullableInteger"));
            Assert.IsTrue(partiallyItitializedProps.ContainsKey("DateTime"));
            Assert.IsTrue(partiallyItitializedProps.ContainsKey("NullableDateTime"));
            Assert.IsTrue(partiallyItitializedProps.ContainsKey("Object"));
            Assert.AreEqual(20, partiallyItitializedProps["Integer"].LiteralValue());
            Assert.AreEqual(15, partiallyItitializedProps["NullableInteger"].LiteralValue());
            Assert.AreEqual(dateTime, partiallyItitializedProps["DateTime"].LiteralValue());
            Assert.AreEqual(dateTime, partiallyItitializedProps["NullableDateTime"].LiteralValue());

        }

    }
}


