
using DSharpPlus.Entities;

namespace RickAstleyProject.VoiceRelated
{
    //NOT IN USE

    public class RickRollerEvents
    {
        protected virtual void OnStopRickRoll(StopRickRollEventArgs e)
        {
            StopRickRoll?.Invoke(this, e);
        }

        public event EventHandler<StopRickRollEventArgs>? StopRickRoll;
    }

    public class StopRickRollEventArgs : EventArgs
    {
        public DiscordChannel? Channel { get; set; }
    }
}
