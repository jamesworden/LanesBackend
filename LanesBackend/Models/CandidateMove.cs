namespace LanesBackend.Models
{
    public class CandidateMove
    {
        public Move Move { get; set; }

        public bool IsValid { get; set; }

        public string? InvalidReason { get; set; }

        public CandidateMove(Move move, bool isValid, string? invalidReason)
        {
            Move = move;
            IsValid = isValid;
            InvalidReason = invalidReason;
        }
    }
}
