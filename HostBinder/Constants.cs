namespace HostBinder
{
    public static class Constants
    {
        public const string HostLineRegEx = @"^([^#\s]*)?\s*([^#\s]+\s*)*(#.*)?$";
        public const string FavoriteTag = @"#!f!";
        public const string TargetTag = @"#!t!";
        public const char CommandDelimiter = '|';
        public const char CommentDelimiter = '#';
    }
}
