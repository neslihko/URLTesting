namespace URLTesting.Framework
{
    public class TestConfig
    {
        /// <summary>
        /// Should we use a static image URL for testing
        /// </summary>
        public bool UseStaticImage { get; set; }

        /// <summary>
        /// URL of the static image
        /// </summary>
        public string? StaticImageURL { get; set; }

        /// <summary>
        /// If UseStaticImage is false, shows the file with the list of image URLs.
        /// </summary>
        public string? PathForURLs { get; set; }

        /// <summary>
        /// How many total batches should be executed?
        /// </summary>
        public int SampleCount { get; set; }

        /// <summary>
        /// The number of slowest batches to ignore.
        /// </summary>
        public int MinSamplesToRemove { get; set; }

        /// <summary>
        /// Max number of threads to use in load testing.
        /// </summary>
        public int ThreadCount { get; set; }

        /// <summary>
        /// How many total requests should be made, in each batch?
        /// </summary>
        public int BatchCount { get; set; }

        /// <summary>
        /// Validates the configuration.
        /// </summary>
        /// <returns>The success and the invalid reason.</returns>
        public (bool isValid, string invalidReason) Validate()
        {
            if (UseStaticImage)
            {
                if (string.IsNullOrEmpty(StaticImageURL))
                {
                    return (false, $"{nameof(StaticImageURL)} can't be empty: {StaticImageURL}");
                }
            }
            else if (string.IsNullOrWhiteSpace(PathForURLs) || !File.Exists(PathForURLs))
            {
                return (false, $"{nameof(PathForURLs)} file doesn't exist: {PathForURLs}");
            }

            if (SampleCount <= 0)
            {
                return (false, $"{nameof(SampleCount)} must be positive: {SampleCount}");
            }

            if (ThreadCount <= 0)
            {
                return (false, $"{nameof(ThreadCount)} must be positive: {ThreadCount}");
            }

            if (BatchCount <= 0)
            {
                return (false, $"{nameof(BatchCount)} must be positive: {BatchCount}");
            }

            if (MinSamplesToRemove < 0)
            {
                return (false, $"{nameof(MinSamplesToRemove)} must be zero / positive: {MinSamplesToRemove}");
            }

            if (MinSamplesToRemove >= BatchCount)
            {
                return (false, $"{nameof(MinSamplesToRemove)} should be less than {nameof(BatchCount)}");
            }

            return (true, "OK");
        }
    }
}
