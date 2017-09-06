namespace NGuava.Base
{
    public static class StringHelper
    {
        public static string ValueOf(object obj)
        {
            return (obj == null) ? "null" : obj.ToString();
        }
    }
}