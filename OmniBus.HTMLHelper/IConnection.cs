namespace OmniBus.HTMLHelper
{
    /// <summary>
    /// Connection Interface
    /// </summary>
    public interface IConnection
    {
        /// <summary>
        /// Serialise the connection to Xml
        /// </summary>
        /// <returns>Xml String</returns>
        string MakeXml();
    }
}