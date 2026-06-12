namespace BaroquenMelody.Library.Scoring.Enums;

public enum ScoringRule : byte
{
    /// <summary>
    ///     Prefer the "law of the shortest way": voices should retain common tones and otherwise move as little as possible.
    /// </summary>
    PreferShortestVoiceMovement = 0,

    /// <summary>
    ///     Prefer contrary or oblique motion between the outer voices, unless the bass leaps a perfect fourth or fifth.
    /// </summary>
    PreferContraryOuterVoiceMotion,

    /// <summary>
    ///     Prefer that melodic leaps recover with a stepwise move in the opposite direction.
    /// </summary>
    PreferLeapRecovery
}
