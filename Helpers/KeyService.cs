namespace vkr_bank.Helpers
{
    public interface IKeyService
    {
        string K_s { get; set; }
    }

    public class KeyService : IKeyService
    {
        public string K_s { get; set; }
    }
}
