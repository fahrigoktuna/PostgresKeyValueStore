namespace PostgresKeyValueStore.Library
{
    public class Configuration
    {
        public string Key { get; private set; }
        public string Value { get; private set; }


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private Configuration() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public static Configuration Create(
            string key,
            string value)
        {

            return new Configuration
            {
                Key = key,
                Value = value
            };
        }

        public void ChangeValue(string value) => this.Value = value;
    }
}
