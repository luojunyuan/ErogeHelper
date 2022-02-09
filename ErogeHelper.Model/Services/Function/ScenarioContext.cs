using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using ErogeHelper.Model.Services.Interface;
using ErogeHelper.Shared;

namespace ErogeHelper.Model.Services.Function
{
    public class ScenarioContext : IDisposable
    {
        private static long[] Hashes = new long[HASH_CAPACITY];
        private static List<string> SavedText = new();

        private readonly Subject<long> _scenarioHash = new();
        private readonly IDisposable _textractorDisposable;

        public ScenarioContext(ITextractorService? textractorService = null)
        {
            textractorService ??= DependencyResolver.GetService<ITextractorService>();
            _textractorDisposable = textractorService
                .SelectedData
                .Select(hp => hp.Text)
                .Do(UpdateCurrentSavedText)
                .Select(text => GenerateScenarioContext(text).Item1)
                .Subscribe(hash => _scenarioHash.OnNext(hash));
        }

        public IObservable<long> ScenarioHash => _scenarioHash;

        private void UpdateCurrentSavedText(string text)
        {
            SavedText.Add(text);
            if (SavedText.Count > CONTEXT_CAPACITY)
            {
                SavedText.RemoveAt(0);
            }
        }

        public void Dispose()
        {
            _textractorDisposable.Dispose();
            Hashes = new long[HASH_CAPACITY];
            SavedText = new();
        }

        private static readonly Encoding CP932 = Encoding.GetEncoding(932); // Shift-JIS

        private const int HASH_CAPACITY = 4;
        private const int CONTEXT_CAPACITY = HASH_CAPACITY - 1;

        /// <returns>
        /// Item1: Hash value of current context (long). <para/>
        /// Item2: Suggested sentences amount.
        /// </returns>
        private static (long, int) GenerateScenarioContext(string inputText)
        {
            byte[] bytes = CP932.GetBytes(inputText);
            Array.Copy(Hashes, 0, Hashes, 1, CONTEXT_CAPACITY);

            Hashes[0] = Djb2Hash(bytes);
            for (int i = 1; i < HASH_CAPACITY; i++)
            {
                Hashes[i] = (Hashes[i] != 0) ? Djb2Hash(bytes, Hashes[i]) : 0;
            }

            int contextSize = SuggestedContextSize(SavedText.ToArray());
            int hashIndex = contextSize - 1;

            return (Hashes[hashIndex], contextSize);
        }

        private const int THRESHOLD = 14;
        private const int MAX_TEXT_LENGTH = 300;

        /// <summary>
        /// Indicate how many sentences should be used in current situation.
        /// </summary>
        /// <param name="savedText">The array of saved text</param>
        /// <returns>1, 2 or 3</returns>
        private static int SuggestedContextSize(in string[] savedText)
        {
            int count = savedText.Length;

            if (count == 1 || savedText[^1].Length >= THRESHOLD)
            {
                return 1;
            }

            if (count == 2 || savedText[^2].Length >= THRESHOLD)
            {
                if (savedText[^2].Length < MAX_TEXT_LENGTH)
                {
                    return 2;
                }
                else
                {
                    return 1;
                }
            }

            if (savedText[^3].Length < MAX_TEXT_LENGTH)
            {
                return 3;
            }
            else
            {
                return 2;
            }
        }

        private static long Djb2Hash(in byte[] data, long hash = 5381) => 
            data.Aggregate(hash, (current, cur) => (current << 5) + current + cur);
    }
}
