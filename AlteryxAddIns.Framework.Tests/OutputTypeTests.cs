namespace JDunkerley.AlteryxAddIns.Framework.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using JDunkerley.AlteryxAddIns.Framework;

    using Xunit;

    public class OutputTypeTests
    {
        static OutputTypeTests()
        {
            // This initializers the missing Alteryx DLLs
            TestHelper.InitResolver();
        }

        public static IEnumerable<object[]> AlteryxTypes
            =>
            Enum.GetValues(typeof(AlteryxRecordInfoNet.FieldType))
                .Cast<AlteryxRecordInfoNet.FieldType>()
                .Where(
                    f =>
                        !new[]
                             {
                                 AlteryxRecordInfoNet.FieldType.E_FT_Unknown,
                                 AlteryxRecordInfoNet.FieldType.E_FT_Blob,
                                 AlteryxRecordInfoNet.FieldType.E_FT_SpatialObj,
                                 AlteryxRecordInfoNet.FieldType.E_FT_FixedDecimal
                             }.Contains(f))
            .Select(f => new object[] { f });

        [Theory]
        [MemberData(nameof(AlteryxTypes))]
        public void CheckThatAlteryxTypesMap(AlteryxRecordInfoNet.FieldType alteryxFieldType)
        {
            var expected = alteryxFieldType.ToString().Substring(5).Replace("_", String.Empty);
            var outputType = (OutputType)Enum.Parse(typeof(OutputType), expected);
            Assert.Equal((int)alteryxFieldType, (int)outputType);
        }
    }
}
