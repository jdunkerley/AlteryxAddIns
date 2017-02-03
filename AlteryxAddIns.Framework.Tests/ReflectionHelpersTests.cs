namespace JDunkerley.AlteryxAddIns.Framework.Tests
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;

    using Attributes;

    using JetBrains.Annotations;

    using Xunit;

    public class ReflectionHelpersTests
    {
        static ReflectionHelpersTests()
        {
            // This initializers the missing Alteryx DLLs
            TestHelper.InitResolver();
        }

        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        private class TestClass
        {
            public int Integer { get; set; }

            [Ordering(2)]
            [Optional]
            public List<string> ListA { get; set; }

            [CharLabel('B')]
            [Ordering(1)]
            public List<string> ListB { get; set; }
        }

        [Fact]
        public void GetPropertiesThrowsArgumentNullExceptionIfNullPassed()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => ((Type)null).GetProperties<int>());
            Assert.Equal("type", ex.ParamName);
        }

        [Fact]
        public void GetPropertiesReturnsCorrectTypeProperties()
        {
            var result = typeof(TestClass).GetProperties<int>();
            Assert.Equal(1, result.Count);
            Assert.True(result.ContainsKey(nameof(TestClass.Integer)));
        }

        [Fact]
        public void GetPropertiesReturnsDerivedTypeProperties()
        {
            var result = typeof(TestClass).GetProperties<IEnumerable<string>>();
            Assert.Equal(2, result.Count);
            Assert.True(result.ContainsKey(nameof(TestClass.ListA)));
            Assert.True(result.ContainsKey(nameof(TestClass.ListB)));
        }

        [Fact]
        public void GetPropertiesReturnsIncorrectTypesPropertiesSkipped()
        {
            var result = typeof(TestClass).GetProperties<double>();
            Assert.Equal(0, result.Count);
        }

        [Fact]
        public void GetAttribThrowsArgumentNullExceptionIfNullPassed()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => ((PropertyInfo)null).GetAttrib<CharLabelAttribute>());
            Assert.Equal("propertyInfo", ex.ParamName);
        }

        [Fact]
        public void GetAttribReturnsNullIfNoAttribute()
        {
            var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.Integer));
            var result = propertyInfo.GetAttrib<CharLabelAttribute>();
            Assert.Null(result);
        }

        [Fact]
        public void GetAttribReturnsAttributeIfPresent()
        {
            var propertyInfo = typeof(TestClass).GetProperty(nameof(TestClass.ListB));
            var result = propertyInfo.GetAttrib<CharLabelAttribute>();
            Assert.NotNull(result);
            Assert.Equal('B', result.Label);
        }

        [Fact]
        public void ToConnectionsThrowsArgumentNullExceptionIfNullPassed()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => ((Dictionary<string, PropertyInfo>)null).ToConnections());
            Assert.Equal("properties", ex.ParamName);
        }

        [Fact]
        public void ToConnetionsMakesAnEmptyArrayIfEmptyDictionary()
        {
            var result = new Dictionary<string, PropertyInfo>().ToConnections();
            Assert.Empty(result);
        }

        [Fact]
        public void ToConnetionsPopulatesListACorrectly()
        {
            var properties = new Dictionary<string, PropertyInfo>
                                 {
                                     [nameof(TestClass.ListA)] = typeof(TestClass).GetProperty(nameof(TestClass.ListA))
                                 };

            var result = properties.ToConnections().ToArray();
            Assert.Equal(1, result.Length);
            Assert.Equal(nameof(TestClass.ListA), result[0].m_strName);
            Assert.False(result[0].AllowMultiple);
            Assert.Null(result[0].Label);
            Assert.True(result[0].Optional);
        }

        [Fact]
        public void ToConnetionsPopulatesListBCorrectly()
        {
            var properties = new Dictionary<string, PropertyInfo>
                                 {
                                     [nameof(TestClass.ListB)] = typeof(TestClass).GetProperty(nameof(TestClass.ListB))
                                 };

            var result = properties.ToConnections().ToArray();
            Assert.Equal(1, result.Length);
            Assert.Equal(nameof(TestClass.ListB), result[0].m_strName);
            Assert.False(result[0].AllowMultiple);
            Assert.Equal('B', result[0].Label);
            Assert.False(result[0].Optional);
        }

        [Fact]
        public void ToConnetionsPopulatesListInCorrectOrder()
        {
            var properties = new Dictionary<string, PropertyInfo>
                                 {
                                     [nameof(TestClass.ListA)] = typeof(TestClass).GetProperty(nameof(TestClass.ListA)),
                                     [nameof(TestClass.ListB)] = typeof(TestClass).GetProperty(nameof(TestClass.ListB))
                                 };

            var result = properties.ToConnections().ToArray();
            Assert.Equal(2, result.Length);
            Assert.Equal(nameof(TestClass.ListB), result[0].m_strName);
            Assert.Equal(nameof(TestClass.ListA), result[1].m_strName);
        }

        [Fact]
        public void GetAttribFromCollectionThrowsArgumentNullExceptionIfNullPassed()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => ((AttributeCollection)null).GetAttrib<CharLabelAttribute>());
            Assert.Equal("attributeCollection", ex.ParamName);
        }

        [Fact]
        public void GetAttribFromCollectionReturnsNullIfNoAttributes()
        {
            var collection = new AttributeCollection();
            var result = collection.GetAttrib<CharLabelAttribute>();
            Assert.Null(result);
        }

        [Fact]
        public void GetAttribFromCollectionReturnsNullIfNoMatchingAttributes()
        {
            var collection = new AttributeCollection(new OptionalAttribute());
            var result = collection.GetAttrib<CharLabelAttribute>();
            Assert.Null(result);
        }

        [Fact]
        public void GetAttribFromCollectionReturnsAttributeIfPresent()
        {
            var collection = new AttributeCollection(new CharLabelAttribute('B'));
            var result = collection.GetAttrib<CharLabelAttribute>();
            Assert.NotNull(result);
            Assert.Equal('B', result.Label);
        }
    }
}