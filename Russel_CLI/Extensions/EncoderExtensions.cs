using System.Text;

namespace Russel_CLI.Extensions;

public static class EncoderExtensions
{
    /// <summary>
    /// decode base 64 to string
    /// </summary>
    /// <param name="base64String"></param>
    /// <returns></returns>
    public static string DecodeBase64ToString(this string base64String)
    {
        // Decode the Base64 string to a byte array
        byte[] decodedBytes = Convert.FromBase64String(base64String);

        // Convert the byte array to a UTF-8 string
        string decodedString = Encoding.UTF8.GetString(decodedBytes);

        return decodedString;
    }
}

