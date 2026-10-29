namespace iMasjid.Application.DTOs.Requests.Actions;

public abstract class BusinessActionBaseDto
{
    /// <example>Export PDF</example>
    public string Name { get; set; } = string.Empty;

    /// <example>Allows exporting records to PDF format.</example>
    public string? Description { get; set; }

    /// <example>true</example>
    public bool IsActive { get; set; } = true;
}

public class CreateBusinessActionRequest : BusinessActionBaseDto
{
    /// <example>EXPORT_PDF</example>
    public string Code { get; set; } = string.Empty;
}

public class UpdateBusinessActionRequest : BusinessActionBaseDto
{
}
