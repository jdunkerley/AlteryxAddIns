namespace OmniBus
{
    /// <summary>
    /// Date To Return
    /// </summary>
    public enum DateTimeInputValueToReturn
    {
        /// <summary>
        /// Current Time
        /// </summary>
        Now,
        /// <summary>
        /// Current Date
        /// </summary>
        Today,
        /// <summary>
        /// Yesterday
        /// </summary>
        Yesterday,
        /// <summary>
        /// Start Of The Week (Sunday)
        /// </summary>
        StartOfWeek,
        /// <summary>
        /// First Day In The Month
        /// </summary>
        StartOfMonth,
        /// <summary>
        /// First Day In The Quarter
        /// </summary>
        StartOfQuarter,
        /// <summary>
        /// First Day In The Year
        /// </summary>
        StartOfYear,
        /// <summary>
        /// Last Day In The Previous Month
        /// </summary>
        PreviousMonthEnd,
        /// <summary>
        /// Last Day In The Previous Quarter
        /// </summary>
        PreviousQuarterEnd,
        /// <summary>
        /// Last Day In The Previous Year
        /// </summary>
        PreviousYearEnd
    }
}