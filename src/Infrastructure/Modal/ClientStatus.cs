namespace Infrastructure.Modal;

public class ClientStatus
{
    public Guid ClientId { get; }
    private bool Dangerous { get; set; }

    public ClientStatus(Guid clientId)
    {
        ClientId = clientId;
    }

    public void SetDangerous(bool dangerous = true)
    {
        Dangerous = dangerous;
    }

    public bool IsDangerous()
    {
        return Dangerous;
    }
}