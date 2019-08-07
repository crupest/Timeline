namespace Timeline.Entities
{
    /// <summary>
    /// Represents the result of a "put" operation.
    /// </summary>
    public enum PutResult
    {
        /// <summary>
        /// Indicates the item did not exist and now is created.
        /// </summary>
        Created,
        /// <summary>
        /// Indicates the item exists already and is modified.
        /// </summary>
        Modified
    }
}
