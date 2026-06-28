namespace BaroquenMelody.Library.Motifs.Enums;

/// <summary>
///     How widely a developed restatement is applied across the voices of a recurring phrase.
/// </summary>
public enum MotifDevelopmentScope : byte
{
    /// <summary> Develop a single voice; the other voices of the phrase stay verbatim. </summary>
    SingleVoice = 0,

    /// <summary> Develop every voice of the phrase. </summary>
    AllVoices
}
