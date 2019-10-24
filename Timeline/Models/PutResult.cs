namespace Timeline.Models
{
    /// <summary>
    /// Represents the result of a "put" operation.
    /// </summary>
    public enum PutResult
    {
        /// <summary>
        /// Indicates the item did not exist and now is created.
        /// </summary>
        Create,
        /// <summary>
        /// Indicates the item exists already and is modified.
        /// </summary>
        Modify
    }
}
