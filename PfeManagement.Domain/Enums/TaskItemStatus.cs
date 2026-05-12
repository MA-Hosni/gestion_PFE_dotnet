namespace PfeManagement.Domain.Enums
{
    public enum TaskItemStatus
    {
        ToDo,
        InProgress,
        /// <summary>Previously Standby; same numeric value for existing database rows.</summary>
        Blocked,
        Done
    }
}
