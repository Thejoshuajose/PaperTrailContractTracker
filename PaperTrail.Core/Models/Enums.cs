namespace PaperTrail.Core.Models;

public enum ContractStatus
{
    Draft,
    Active,
    ExpiringSoon,
    Terminated,
    Archived
}

public enum ReminderType
{
    Renewal,
    Termination,
    Notice,
    Custom
}
