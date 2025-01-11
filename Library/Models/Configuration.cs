namespace PostgresKeyValueStore.Library
{
    public class Configuration
    {
        public string Key { get; private set; }
        public string Value { get; private set; }


        private Configuration() { }
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
