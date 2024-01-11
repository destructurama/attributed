namespace Destructurama.Attributed
{
    /// <summary>
    /// Global destucturing options
    /// </summary>
    public class AttributedDestructuringPolicyOptions
    {
        /// <summary>
        /// By setting IgnoreNullProperties to true no need to set [NotLoggedIfNull] for every logged property.
        /// Custom types implemenenting IEnumerable, will be destructed as StructureValue and affected by IgnoreNullProperties 
        /// only in case at least one property (or the type itself) has Destructurama attribute applied.
        /// </summary>
        public bool IgnoreNullProperties { get; set; }
    }
}
