namespace PfeManagement.WebApi.Models
{
    public enum UserRole
    {
        Student,
        CompSupervisor,
        UniSupervisor
    }

    public enum Degree
    {
        Bachelor,
        Master,
        Engineer
    }

    public enum DegreeType
    {
        // Engineer
        INLOG,
        INREV,

        // Master
        Pro_IM,
        Pro_DCA,
        Pro_PAR,
        R_DISR,
        R_TMAC,

        // Bachelor
        AV,
        CMM,
        IMM,
        BD,
        MIME,
        Coco_JV,
        Coco_3D
    }

    public enum TaskItemStatus
    {
        ToDo,
        InProgress,
        Standby,
        Done
    }

    public enum Priority
    {
        Highest,
        High,
        Medium,
        Low,
        Lowest
    }

    public enum ValidationStatus
    {
        Pending,
        InProgress,
        Valid,
        Invalid
    }

    public enum MeetingType
    {
        Reunion,
        HorsReunion
    }

    public enum MeetingReferenceType
    {
        UserStory,
        Task,
        Report
    }
}
