using Average.Client.Framework.Diagnostics;
using Average.Client.Framework.Extensions;
using Average.Client.Framework.Interfaces;
using System;
using System.Runtime.InteropServices;
using System.Security;
using static Average.Client.Framework.GameAPI;
using static CitizenFX.Core.Native.API;

namespace Average.Client.Framework.Services
{
    internal class AudioService : IService
    {
        public AudioService()
        {

        }

        [StructLayout(LayoutKind.Explicit, Size = 128)]
        [SecurityCritical]
        internal struct SoundStruct
        {
            [FieldOffset(0)] internal long soundName;
            [FieldOffset(8)] internal long soundRef;
            [FieldOffset(16)] internal int speechLine;
            [FieldOffset(24)] internal long speechParams;
            [FieldOffset(32)] internal int unk1;
            [FieldOffset(40)] internal int unk2;
            [FieldOffset(48)] internal int unk3;

            internal SoundStruct(long soundName, long soundRef, int speechLine, long speechParams, int unk1, int unk2, int unk3)
            {
                this.soundName = soundName;
                this.soundRef = soundRef;
                this.speechLine = speechLine;
                this.speechParams = speechParams;
                this.unk1 = unk1;
                this.unk2 = unk2;
                this.unk3 = unk3;
            }
        }

        //[StructLayout(LayoutKind.Explicit, Size = 128)]
        //[SecurityCritical]
        //internal struct SoundStruct2
        //{
        //    [FieldOffset(0)] internal long soundName;
        //    [FieldOffset(8)] internal long soundRef;
        //    [FieldOffset(16)] internal int speechLine;
        //    [FieldOffset(24)] internal long speechParams;
        //    [FieldOffset(32)] internal int unk1;
        //    [FieldOffset(40)] internal int unk2;
        //    [FieldOffset(48)] internal int unk3;
        //    [FieldOffset(56)] internal int unk4;

        //    internal SoundStruct2(long soundName, long soundRef, int speechLine, long speechParams, int unk1, int unk2, int unk3, int unk4)
        //    {
        //        this.soundName = soundName;
        //        this.soundRef = soundRef;
        //        this.speechLine = speechLine;
        //        this.speechParams = speechParams;
        //        this.unk1 = unk1;
        //        this.unk2 = unk2;
        //        this.unk3 = unk3;
        //        this.unk4 = unk4;
        //    }
        //}

        //[StructLayout(LayoutKind.Explicit, Size = 16)]
        //[SecurityCritical]
        //public struct SoundNameBigInt
        //{
        //    [FieldOffset(0)] public long soundName;

        //    public SoundNameBigInt(long soundName)
        //    {
        //        this.soundName = soundName;
        //    }
        //}

        //[StructLayout(LayoutKind.Explicit, Size = 16)]
        //[SecurityCritical]
        //public struct SoundRefBigInt
        //{
        //    [FieldOffset(0)] public long soundRef;

        //    public SoundRefBigInt(long soundRef)
        //    {
        //        this.soundRef = soundRef;
        //    }
        //}

        //[StructLayout(LayoutKind.Explicit, Size = 16)]
        //[SecurityCritical]
        //public struct SpeechParamsBigInt
        //{
        //    [FieldOffset(0)] public long speechParams;

        //    public SpeechParamsBigInt(long speechParams)
        //    {
        //        this.speechParams = speechParams;
        //    }
        //}

        internal void PlayAmbientSpeechFromEntity(int entityId, string soundRefString, string soundNameString, string speechParamsString, int speechLine)
        {
            unsafe
            {
                var soundStruct = new SoundStruct(
                    Call<long>(0xFA925AC00EB830B9, 10, "LITERAL_STRING", soundNameString),
                    Call<long>(0xFA925AC00EB830B9, 10, "LITERAL_STRING", soundRefString), speechLine,
                    (long)GetHashKey("speech_params_force_normal_clear"), PlayerPedId(), 1, 1);

                Call(0x8E04FEDD28D42462, entityId, ((IntPtr)(&soundStruct)).ToInt64());

                // ----------------------------

                //var soundName = Call<long>(0xFA925AC00EB830B9, 10, "LITERAL_STRING", soundNameString);
                //var soundRef = Call<long>(0xFA925AC00EB830B9, 10, "LITERAL_STRING", soundRefString);

                //var speechParams = GetHashKey(speechParamsString);

                //var soundNameBigInt = new SoundNameBigInt(soundName);
                //var soundRefBigInt = new SoundRefBigInt(soundRef);
                //var speechParamsBigInt = new SpeechParamsBigInt(speechParams);

                //var soundNameResult = ((IntPtr)(&soundNameBigInt)).ToInt64();
                //var soundRefResult = ((IntPtr)(&soundRefBigInt)).ToInt64();
                //var speechParamsResult = ((IntPtr)(&speechParamsBigInt)).ToInt64();

                //var soundStruct2 = new SoundStruct2(soundNameResult, soundRefResult, speechLine, speechParamsResult, PlayerPedId(), 1, 1, 1);
                //var result = ((IntPtr)(&soundStruct2)).ToInt64();

                //Logger.Debug("Speech: " + soundName + ", " + soundRef + ", " + speechParams + ", " + soundNameResult + ", " + soundRefResult + ", " + speechParamsResult + ", " + result);

                //Call(0x8E04FEDD28D42462, entityId, result);
            }
        }
    }
}
