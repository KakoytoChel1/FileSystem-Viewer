namespace FileSystem_Viewer.Models
{
    public interface IPauseResetTokenSource
    {
        public PauseResetToken Token { get; set; }
        public bool IsPauseRequested { get; }
        public void Pause();
        public void Reset();
    }
}
