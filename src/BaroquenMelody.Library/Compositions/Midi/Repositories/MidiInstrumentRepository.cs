using BaroquenMelody.Infrastructure.Extensions;
using BaroquenMelody.Library.Compositions.Midi.Enums;
using Melanchall.DryWetMidi.Standards;

namespace BaroquenMelody.Library.Compositions.Midi.Repositories;

internal sealed class MidiInstrumentRepository : IMidiInstrumentRepository
{
    private static readonly IEnumerable<GeneralMidi2Program> Keyboard = new List<GeneralMidi2Program>
    {
        GeneralMidi2Program.AcousticGrandPiano,
        GeneralMidi2Program.AcousticGrandPianoWide,
        GeneralMidi2Program.AcousticGrandPianoDark,
        GeneralMidi2Program.BrightAcousticPiano,
        GeneralMidi2Program.BrightAcousticPianoWide,
        GeneralMidi2Program.ElectricGrandPiano,
        GeneralMidi2Program.ElectricGrandPianoWide,
        GeneralMidi2Program.HonkyTonkPiano,
        GeneralMidi2Program.HonkyTonkPianoWide,
        GeneralMidi2Program.ElectricPiano1,
        GeneralMidi2Program.DetunedElectricPiano1,
        GeneralMidi2Program.ElectricPiano1VelocityMix,
        GeneralMidi2Program.SixtiesElectricPiano,
        GeneralMidi2Program.ElectricPiano2,
        GeneralMidi2Program.DetunedElectricPiano2,
        GeneralMidi2Program.ElectricPiano2VelocityMix,
        GeneralMidi2Program.EpLegend,
        GeneralMidi2Program.EpPhase,
        GeneralMidi2Program.Harpsichord,
        GeneralMidi2Program.HarpsichordOctaveMix,
        GeneralMidi2Program.HarpsichordWide,
        GeneralMidi2Program.HarpsichordWithKeyOff,
        GeneralMidi2Program.Clavi,
        GeneralMidi2Program.PulseClavi
    };

    private static readonly IEnumerable<GeneralMidi2Program> ChromaticPercussion = new List<GeneralMidi2Program>
    {
        GeneralMidi2Program.Celesta,
        GeneralMidi2Program.Glockenspiel,
        GeneralMidi2Program.MusicBox,
        GeneralMidi2Program.Vibraphone,
        GeneralMidi2Program.VibraphoneWide,
        GeneralMidi2Program.Marimba,
        GeneralMidi2Program.MarimbaWide,
        GeneralMidi2Program.Xylophone,
        GeneralMidi2Program.TubularBells,
        GeneralMidi2Program.ChurchBell,
        GeneralMidi2Program.Carillon,
        GeneralMidi2Program.Dulcimer,
        GeneralMidi2Program.YangChin,
        GeneralMidi2Program.Timpani,
        GeneralMidi2Program.Kalimba
    };

    private static readonly IEnumerable<GeneralMidi2Program> Organ = new List<GeneralMidi2Program>
    {
        GeneralMidi2Program.DrawbarOrgan,
        GeneralMidi2Program.DetunedDrawbarOrgan,
        GeneralMidi2Program.ItalianSixtiesOrgan,
        GeneralMidi2Program.DrawbarOrgan2,
        GeneralMidi2Program.PercussiveOrgan,
        GeneralMidi2Program.DetunedPercussiveOrgan,
        GeneralMidi2Program.PercussiveOrgan2,
        GeneralMidi2Program.RockOrgan,
        GeneralMidi2Program.ChurchOrgan,
        GeneralMidi2Program.ChurchOrganOctaveMix,
        GeneralMidi2Program.DetunedChurchOrgan,
        GeneralMidi2Program.ReedOrgan,
        GeneralMidi2Program.PuffOrgan,
        GeneralMidi2Program.Accordion,
        GeneralMidi2Program.Accordion2,
        GeneralMidi2Program.Harmonica,
        GeneralMidi2Program.TangoAccordion
    };

    private static readonly IEnumerable<GeneralMidi2Program> Guitar = new List<GeneralMidi2Program>
    {
        GeneralMidi2Program.AcousticGuitarNylon,
        GeneralMidi2Program.Ukulele,
        GeneralMidi2Program.AcousticGuitarNylonKeyOff,
        GeneralMidi2Program.AcousticGuitarNylon2,
        GeneralMidi2Program.AcousticGuitarSteel,
        GeneralMidi2Program.TwelveStringsGuitar,
        GeneralMidi2Program.Mandolin,
        GeneralMidi2Program.SteelGuitarWithBodySound,
        GeneralMidi2Program.ElectricGuitarJazz,
        GeneralMidi2Program.ElectricGuitarPedalSteel,
        GeneralMidi2Program.ElectricGuitarClean,
        GeneralMidi2Program.ElectricGuitarDetunedClean,
        GeneralMidi2Program.MidToneGuitar,
        GeneralMidi2Program.ElectricGuitarMuted,
        GeneralMidi2Program.ElectricGuitarFunkyCutting,
        GeneralMidi2Program.ElectricGuitarMutedVeloSw,
        GeneralMidi2Program.JazzMan,
        GeneralMidi2Program.OverdrivenGuitar,
        GeneralMidi2Program.GuitarPinch,
        GeneralMidi2Program.DistortionGuitar,
        GeneralMidi2Program.DistortionGuitarWithFeedback,
        GeneralMidi2Program.DistortedRhythmGuitar,
        GeneralMidi2Program.GuitarHarmonics,
        GeneralMidi2Program.GuitarFeedback
    };

    private static readonly IEnumerable<GeneralMidi2Program> Bass = new List<GeneralMidi2Program>
    {
        GeneralMidi2Program.AcousticBass,
        GeneralMidi2Program.ElectricBassFinger,
        GeneralMidi2Program.FingerSlapBass,
        GeneralMidi2Program.ElectricBassPick,
        GeneralMidi2Program.FretlessBass,
        GeneralMidi2Program.SlapBass1,
        GeneralMidi2Program.SlapBass2,
        GeneralMidi2Program.SynthBass1,
        GeneralMidi2Program.SynthBassWarm,
        GeneralMidi2Program.SynthBass3Resonance,
        GeneralMidi2Program.ClaviBass,
        GeneralMidi2Program.Hammer,
        GeneralMidi2Program.SynthBass2,
        GeneralMidi2Program.SynthBass4Attack,
        GeneralMidi2Program.SynthBassRubber,
        GeneralMidi2Program.AttackPulse
    };

    private static readonly IEnumerable<GeneralMidi2Program> Strings = new List<GeneralMidi2Program>
    {
        GeneralMidi2Program.Violin,
        GeneralMidi2Program.ViolinSlowAttack,
        GeneralMidi2Program.Viola,
        GeneralMidi2Program.Cello,
        GeneralMidi2Program.Contrabass,
        GeneralMidi2Program.TremoloStrings,
        GeneralMidi2Program.PizzicatoStrings,
        GeneralMidi2Program.OrchestralHarp,
        GeneralMidi2Program.Sitar,
        GeneralMidi2Program.Sitar2Bend,
        GeneralMidi2Program.Banjo,
        GeneralMidi2Program.Shamisen,
        GeneralMidi2Program.Koto,
        GeneralMidi2Program.TaishoKoto,
        GeneralMidi2Program.Fiddle
    };

    private static readonly IEnumerable<GeneralMidi2Program> Ensemble = new List<GeneralMidi2Program>
    {
        GeneralMidi2Program.StringEnsembles1,
        GeneralMidi2Program.StringsAndBrass,
        GeneralMidi2Program.SixtiesStrings,
        GeneralMidi2Program.StringEnsembles2,
        GeneralMidi2Program.SynthStrings1,
        GeneralMidi2Program.SynthStrings3,
        GeneralMidi2Program.SynthStrings2,
        GeneralMidi2Program.OrchestraHit,
        GeneralMidi2Program.BassHitPlus,
        GeneralMidi2Program.SixthHit,
        GeneralMidi2Program.EuroHit
    };

    private static readonly IEnumerable<GeneralMidi2Program> Voice = new List<GeneralMidi2Program>
    {
        GeneralMidi2Program.ChoirAahs,
        GeneralMidi2Program.ChoirAahs2,
        GeneralMidi2Program.VoiceOohs,
        GeneralMidi2Program.Humming,
        GeneralMidi2Program.SynthVoice,
        GeneralMidi2Program.AnalogVoice
    };

    private static readonly IEnumerable<GeneralMidi2Program> Brass = new List<GeneralMidi2Program>
    {
        GeneralMidi2Program.Trumpet,
        GeneralMidi2Program.DarkTrumpetSoft,
        GeneralMidi2Program.Trombone,
        GeneralMidi2Program.Trombone2,
        GeneralMidi2Program.BrightTrombone,
        GeneralMidi2Program.Tuba,
        GeneralMidi2Program.MutedTrumpet,
        GeneralMidi2Program.MutedTrumpet2,
        GeneralMidi2Program.FrenchHorn,
        GeneralMidi2Program.FrenchHorn2Warm,
        GeneralMidi2Program.BrassSection,
        GeneralMidi2Program.BrassSection2OctaveMix,
        GeneralMidi2Program.SynthBrass1,
        GeneralMidi2Program.SynthBrass3,
        GeneralMidi2Program.AnalogSynthBrass1,
        GeneralMidi2Program.JumpBrass,
        GeneralMidi2Program.SynthBrass2,
        GeneralMidi2Program.SynthBrass4,
        GeneralMidi2Program.AnalogSynthBrass2
    };

    private static readonly IEnumerable<GeneralMidi2Program> Woodwind = new List<GeneralMidi2Program>
    {
        GeneralMidi2Program.SopranoSax,
        GeneralMidi2Program.AltoSax,
        GeneralMidi2Program.TenorSax,
        GeneralMidi2Program.BaritoneSax,
        GeneralMidi2Program.Oboe,
        GeneralMidi2Program.EnglishHorn,
        GeneralMidi2Program.Bassoon,
        GeneralMidi2Program.Clarinet,
        GeneralMidi2Program.Piccolo,
        GeneralMidi2Program.Flute,
        GeneralMidi2Program.Recorder,
        GeneralMidi2Program.PanFlute,
        GeneralMidi2Program.BlownBottle,
        GeneralMidi2Program.Shakuhachi,
        GeneralMidi2Program.Whistle,
        GeneralMidi2Program.Ocarina,
        GeneralMidi2Program.Shanai,
        GeneralMidi2Program.BagPipe
    };

    private static readonly IEnumerable<GeneralMidi2Program> Synth = new List<GeneralMidi2Program>
    {
        GeneralMidi2Program.Lead1Square,
        GeneralMidi2Program.Lead1ASquare2,
        GeneralMidi2Program.Lead1BSine,
        GeneralMidi2Program.Lead2Sawtooth,
        GeneralMidi2Program.Lead2ASawtooth2,
        GeneralMidi2Program.Lead2BSawPulse,
        GeneralMidi2Program.Lead2CDoubleSawtooth,
        GeneralMidi2Program.Lead2DSequencedAnalog,
        GeneralMidi2Program.Lead3Calliope,
        GeneralMidi2Program.Lead4Chiff,
        GeneralMidi2Program.Lead5Charang,
        GeneralMidi2Program.Lead5AWireLead,
        GeneralMidi2Program.Lead6Voice,
        GeneralMidi2Program.Lead7Fifths,
        GeneralMidi2Program.Lead8BassLead,
        GeneralMidi2Program.Lead8ASoftWrl
    };

    private static readonly IEnumerable<GeneralMidi2Program> All = Keyboard
        .Concat(ChromaticPercussion)
        .Concat(Organ)
        .Concat(Guitar)
        .Concat(Bass)
        .Concat(Strings)
        .Concat(Ensemble)
        .Concat(Voice)
        .Concat(Brass)
        .Concat(Woodwind)
        .Concat(Synth)
        .OrderBy(instrument => instrument.ToSpaceSeparatedString(), StringComparer.OrdinalIgnoreCase)
        .ToList();

    public IEnumerable<GeneralMidi2Program> GetMidiInstruments(MidiInstrumentType midiInstrumentType)
    {
        var midiInstruments = new List<GeneralMidi2Program>();

        if (midiInstrumentType.HasFlag(MidiInstrumentType.Keyboard))
        {
            midiInstruments.AddRange(Keyboard);
        }

        if (midiInstrumentType.HasFlag(MidiInstrumentType.ChromaticPercussion))
        {
            midiInstruments.AddRange(ChromaticPercussion);
        }

        if (midiInstrumentType.HasFlag(MidiInstrumentType.Organ))
        {
            midiInstruments.AddRange(Organ);
        }

        if (midiInstrumentType.HasFlag(MidiInstrumentType.Guitar))
        {
            midiInstruments.AddRange(Guitar);
        }

        if (midiInstrumentType.HasFlag(MidiInstrumentType.Bass))
        {
            midiInstruments.AddRange(Bass);
        }

        if (midiInstrumentType.HasFlag(MidiInstrumentType.Strings))
        {
            midiInstruments.AddRange(Strings);
        }

        if (midiInstrumentType.HasFlag(MidiInstrumentType.Ensemble))
        {
            midiInstruments.AddRange(Ensemble);
        }

        if (midiInstrumentType.HasFlag(MidiInstrumentType.Voice))
        {
            midiInstruments.AddRange(Voice);
        }

        if (midiInstrumentType.HasFlag(MidiInstrumentType.Brass))
        {
            midiInstruments.AddRange(Brass);
        }

        if (midiInstrumentType.HasFlag(MidiInstrumentType.Woodwind))
        {
            midiInstruments.AddRange(Woodwind);
        }

        if (midiInstrumentType.HasFlag(MidiInstrumentType.Synth))
        {
            midiInstruments.AddRange(Synth);
        }

        return midiInstruments;
    }

    public IEnumerable<GeneralMidi2Program> GetAllMidiInstruments() => All;
}
