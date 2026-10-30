namespace iMasjid.Application.DTOs.Requests.Actions;

public abstract class BusinessActionBaseDto
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}

public class CreateBusinessActionRequest : BusinessActionBaseDto
{
    public string Code { get; set; } = string.Empty;
}

public class UpdateBusinessActionRequest : BusinessActionBaseDto
{
}
