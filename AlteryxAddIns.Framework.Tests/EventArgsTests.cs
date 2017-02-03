namespace JDunkerley.AlteryxAddIns.Framework.Tests
{
    using System;

    using Interfaces;

    using Xunit;

    public class EventArgsTests
    {
        private static readonly Random Random;

        static EventArgsTests()
        {
            // This initializers the missing Alteryx DLLs
            TestHelper.InitResolver();

            Random = new Random(DateTime.Now.Millisecond);
        }

        [Fact]
        public void ProgressUpdatedEventArgsSetsProgress()
        {
            var progress = Random.Next();
            var eventArgs = new ProgressUpdatedEventArgs(progress);
            var setProgress = eventArgs.Progress;
            Assert.Equal(progress, setProgress, 8);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-1e-14)]
        [InlineData(1 + 1e-14)]
        public void ProgressUpdatedEventArgsRejectsInvalidValues(double progress)
        {
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new ProgressUpdatedEventArgs(progress));
            Assert.Equal("progress", exception.ParamName);
            Assert.False(string.IsNullOrWhiteSpace(exception.Message));
        }
    }
}