
using System.Media;
using System.Runtime.InteropServices;
using System.Text;

namespace VoicevoxCapiCsTest
{
    class Program
    {
        enum VoicevoxResultCode
        {
            // 成功
            VOICEVOX_RESULT_SUCCEED = 0,
            // OpenJTalk初期化に失敗した
            VOICEVOX_RESULT_NOT_INITIALIZE_OPEN_JTALK_ERR = 1,
        }

#if WINDOWS
        const string dllName = "core_cpu_x64.dll";
#else
        const string dllName = "libcore_cpu_universal2.dylib";
#endif
        const string openJTalkDictName = "open_jtalk_dic_utf_8-1.11";

        [DllImport(dllName)]
        static extern bool initialize(byte[] root_dir_path, bool use_gpu, int cpu_num_threads = 0);

        [DllImport(dllName)]
        static extern void finalize();

        /**
         * @fn
         * open jtalkを初期化する
         * @return 結果コード
         */
        [DllImport(dllName, CharSet = CharSet.Ansi)]
        static extern VoicevoxResultCode voicevox_initialize_openjtalk(string dict_path);

        /**
         * @fn
         * text to spearchを実行する
         * @param text 音声データに変換するtextデータ
         * @param speaker_id 話者番号
         * @param output_binary_size 音声データのサイズを出力する先のポインタ
         * @param output_wav 音声データを出力する先のポインタ。使用が終わったらvoicevox_wav_freeで開放する必要がある
         * @return 結果コード
         */
        [DllImport(dllName)]
        static extern VoicevoxResultCode voicevox_tts(byte[] text, long speaker_id, out int output_binary_size, IntPtr output_wav);

        /**
         * @fn
         * voicevox_ttsで生成した音声データを開放する
         * @param wav 開放する音声データのポインタ
         */
        [DllImport(dllName, CharSet = CharSet.Auto)]
        static extern void voicevox_wav_free(IntPtr wav);

        /**
         * @fn
         * エラーで返ってきた結果コードをメッセージに変換する
         * @return エラーメッセージ文字列
         */
        [DllImport(dllName, CharSet = CharSet.Unicode)]
        static extern IntPtr voicevox_error_result_to_message(VoicevoxResultCode result_code);

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("initialize");
                byte[] rootDirPathByteArr = Encoding.UTF8.GetBytes(GetThisAppDirectory());
                initialize(rootDirPathByteArr, false);

                Console.WriteLine("OpenJTalkの初期化");
                VoicevoxResultCode resultCode;
                resultCode = voicevox_initialize_openjtalk(Path.Combine(GetThisAppDirectory(), openJTalkDictName));
                ThrowVoicevoxError(resultCode);

                Console.WriteLine("喋らせる文章の入力");
                string talkString = Console.ReadLine() ?? "";

                byte[] waveArray;
                unsafe
                {
                    byte* output_wav = null;
                    try
                    {
                        byte[] textByteArr = Encoding.UTF8.GetBytes(talkString);

                        resultCode = voicevox_tts(textByteArr, 0, out int size, (IntPtr)(&output_wav));
                        waveArray = new byte[size];
                        if (output_wav != null)
                        {
                            Marshal.Copy((IntPtr)output_wav, waveArray, 0, size);
                        }
                        ThrowVoicevoxError(resultCode);
                    }
                    finally
                    {
                        if (output_wav != null)
                        {
                            voicevox_wav_free((IntPtr)output_wav);
                        }
                    }
                }

                using (MemoryStream waveStream = new MemoryStream(waveArray))
                using (FileStream fileStream = new FileStream(Path.Combine(GetThisAppDirectory(), "test.wav"), FileMode.Create))
                {
#if false
                    //なぜか再生されない
                    Console.WriteLine("再生中");
                    //読み込む
                    SoundPlayer player = new SoundPlayer(waveStream);
                    //非同期再生する
                    player.PlaySync();
                    Console.WriteLine("再生終了");
#endif
                    Console.WriteLine("保存");
                    fileStream.Write(waveArray, 0, waveArray.Length);
                    Console.WriteLine("保存完了");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.Read();
            }
            finally
            {
                finalize();
            }
        }

        /// <summary>
        /// VOICEVOXエラーを投げます。
        /// </summary>
        /// <param name="resultCode"></param>
        /// <exception cref="Exception"></exception>
        private static void ThrowVoicevoxError(VoicevoxResultCode resultCode)
        {
            if (resultCode != VoicevoxResultCode.VOICEVOX_RESULT_SUCCEED)
            {
                IntPtr errMessageByteArr = voicevox_error_result_to_message(resultCode);
                string errMessage = Marshal.PtrToStringAnsi(errMessageByteArr) ?? "";
                throw new Exception(errMessage);
            }
        }

        /// <summary>
        /// 実行中のコードを格納しているアセンブリのある場所を返します。
        /// </summary>
        /// <returns></returns>
        static public string GetThisAppDirectory()
        {
            string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            return Path.GetDirectoryName(appPath) ?? "";
        }

    }
}
